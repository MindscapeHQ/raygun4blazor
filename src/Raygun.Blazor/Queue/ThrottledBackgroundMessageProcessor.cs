using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raygun.Blazor.Logging;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Queue
{
    using RequestProcessCallback = Func<RaygunRequest, CancellationToken, Task>;

    internal sealed class ThrottledBackgroundMessageProcessor : IDisposable
    {
        // When the "BackgroundMessageWorkerBreakpoint" setting is not set, we default to 25.
        internal const int WorkerQueueBreakpointDefaultValue = 25;
        
        // When the "BackgroundMessageWorkerCount" setting is not set, we default to 8 max workers.
        internal const int MaxWorkerCountDefault = 8;
        
        // This was a BlockingCollection<T> which used .Take to dequeue items, but since we will have 0 workers when the queue is empty
        // we don't need to block the thread waiting for an item to be enqueued. A concurrent queue is more appropriate.
        private readonly ConcurrentQueue<RaygunRequest> _messageQueue;
        private readonly ConcurrentDictionary<Task, CancellationTokenSource> _workerTasks;

        private readonly CancellationTokenSource _globalCancellationSource;
        private readonly int _maxQueueSize;
        private readonly RequestProcessCallback _processCallback;
        private readonly int _maxWorkerTasks;
        private readonly int _workerQueueBreakpoint;
        private readonly object _workerTaskMutex = new();
        private readonly IRaygunLogger? _raygunLogger;

        private bool _drainingQueue;


        private bool _isDisposing;
        private readonly int _drainSize;

        public ThrottledBackgroundMessageProcessor(int maxQueueSize,
            int maxWorkerTasks,
            int workerQueueBreakpoint,
            RequestProcessCallback onProcessMessageFunc,
            IRaygunLogger? raygunLogger = null)
        {
            _raygunLogger = raygunLogger;
            _maxQueueSize = maxQueueSize;
            _workerQueueBreakpoint = workerQueueBreakpoint <= 0 ? WorkerQueueBreakpointDefaultValue : workerQueueBreakpoint;

            // Drain the queue when it reaches 90% of the max size
            _drainSize = Math.Max(maxQueueSize / 100 * 90, 1);
            _processCallback = onProcessMessageFunc ?? throw new ArgumentNullException(nameof(onProcessMessageFunc));
            _maxWorkerTasks = maxWorkerTasks;
            _messageQueue = new ConcurrentQueue<RaygunRequest>();
            _globalCancellationSource = new CancellationTokenSource();
            _workerTasks =
                new ConcurrentDictionary<Task, CancellationTokenSource>(Environment.ProcessorCount, _maxWorkerTasks);
            _raygunLogger?.Debug("[ThrottledBackgroundMessageProcessor] created");
        }

        public bool Enqueue(RaygunRequest message)
        {
            _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] Enqueueing message");
            if (_drainingQueue)
            {
                if (_messageQueue.Count >= _drainSize)
                {
                    _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] Draining queue");
                    return false;
                }

                _drainingQueue = false;
            }

            if (_messageQueue.Count >= _maxQueueSize)
            {
                _raygunLogger?.Verbose($"[ThrottledBackgroundMessageProcessor] Queue is full: {_messageQueue.Count}");
                _drainingQueue = true;
                return false;
            }

            _messageQueue.Enqueue(message);
            AdjustWorkers();
            _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] message enqueued");
            return true;
        }

        /// <summary>
        /// This method uses the queue size to determine the number of workers that should be processing messages.
        /// </summary>
        private void AdjustWorkers(bool breakAfterRun = false)
        {
            _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] Adjusting workers");
            // We only want one thread to adjust the number of workers at a time
            if (_isDisposing || !Monitor.TryEnter(_workerTaskMutex))
            {
                return;
            }

            try
            {
                // Remove any completed tasks, otherwise we might not have space to add new tasks that want to do work
                RemoveCompletedTasks();

                // Calculate the desired number of workers based on the queue size, this is so we don't end up creating
                // many workers that essentially do nothing, or if there's a small number of errors we don't have too many workers
                var currentWorkers = _workerTasks.Count(x => x.Key.Status == TaskStatus.Running);
                var desiredWorkers = CalculateDesiredWorkers(_messageQueue.Count);

                if (desiredWorkers > currentWorkers)
                {
                    for (var i = currentWorkers; i < desiredWorkers; i++)
                    {
                        CreateWorkerTask();
                    }
                }
                else if (desiredWorkers < currentWorkers)
                {
                    RemoveExcessWorkers(currentWorkers - desiredWorkers);
                }

                if (desiredWorkers > 0 && _messageQueue.Count > 0)
                {
                    // If we have messages to process but no workers, create a worker
                    CreateWorkerTask();
                }
            }
            finally
            {
                // Make sure we release the mutex otherwise we'll block the next thread that wants to adjust the number of workers
                Monitor.Exit(_workerTaskMutex);
            }

            if (breakAfterRun)
            {
                return;
            }

            // We only want 1 thread adjusting the workers at any given time, but there could be a race condition
            // where the queue is empty when we release the mutex, but there are 'completed' tasks, so we need to double-check and adjust.
            if (_messageQueue.Count > 0 && _workerTasks.All(x => x.Key.IsCompleted))
            {
                AdjustWorkers(true);
            }
        }

        /// <summary>
        /// A task may be in a completed state but not yet removed from the dictionary. This method removes any completed tasks.
        /// Completed means the task has finished executing, whether it was successful or not. (Failed, Successful, Faulted, etc.)
        /// </summary>
        private void RemoveCompletedTasks()
        {
            _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] Removing completed tasks");
            var count = _workerTasks.Count;
            var rentedArray = ArrayPool<KeyValuePair<Task, CancellationTokenSource>>.Shared.Rent(count);
            var completedCount = 0;

            foreach (var kvp in _workerTasks)
            {
                if (kvp.Key.IsCompleted)
                {
                    rentedArray[completedCount++] = kvp;
                }
            }

            for (var i = 0; i < completedCount; i++)
            {
                var kvp = rentedArray[i];
                if (_workerTasks.TryRemove(kvp.Key, out var cts))
                {
                    cts.Dispose();
                }
            }

            ArrayPool<KeyValuePair<Task, CancellationTokenSource>>.Shared.Return(rentedArray);
        }

        /// <summary>
        /// When the number of workers is greater than the desired number, remove the excess workers.
        /// We do this by taking the first N workers and cancelling them.
        /// Then we dispose of the CancellationTokenSource.
        /// </summary>
        /// <param name="count">Number of workers to kill off.</param>
        private void RemoveExcessWorkers(int count)
        {
            _raygunLogger?.Verbose($"[ThrottledBackgroundMessageProcessor] Removing {count} excess workers");
            var rentedArray = ArrayPool<KeyValuePair<Task, CancellationTokenSource>>.Shared.Rent(count);
            var index = 0;

            foreach (var kvp in _workerTasks)
            {
                if (index == count)
                {
                    break;
                }

                rentedArray[index++] = kvp;
            }

            for (var i = 0; i < index; i++)
            {
                var kvp = rentedArray[i];

                if (_workerTasks.TryRemove(kvp.Key, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
            }

            ArrayPool<KeyValuePair<Task, CancellationTokenSource>>.Shared.Return(rentedArray);
        }

        /// <summary>
        /// Spin up a new worker task to process messages. This method is called by AdjustWorkers when the number of workers is less than the desired number.
        /// When a task completes it will adjust the number of workers again in case the queue size has changed.
        /// </summary>
        private void CreateWorkerTask()
        {
            _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] Creating worker task");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_globalCancellationSource.Token);

            var task = Task.Run(() => RaygunMessageWorker(_messageQueue, _processCallback, cts.Token), cts.Token);
            _workerTasks[task] = cts;

            // When the worker task completes, adjust the number of workers
            task.ContinueWith(_ => AdjustWorkers(), TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Calculate the desired number of workers based on the queue size. This method is used by AdjustWorkers.
        /// </summary>
        private int CalculateDesiredWorkers(int queueSize)
        {
            // Should never have a _maxWorkerTasks of 0, but there's a couple of unit tests
            // which use 0 to determine if messages are discarded when the queue is full
            // so we need to allow for 0 workers to verify those tests.
            if (queueSize == 0 || _maxWorkerTasks == 0)
            {
                return 0;
            }

            if (queueSize <= _workerQueueBreakpoint)
            {
                return 1;
            }

            return Math.Min((queueSize + _workerQueueBreakpoint - 1) / _workerQueueBreakpoint, _maxWorkerTasks);
        }

        /// <summary>
        /// Actual task run by the worker. This method will take a message from the queue and process it.
        /// </summary>
        private static async Task RaygunMessageWorker(ConcurrentQueue<RaygunRequest> messageQueue,
            RequestProcessCallback callback,
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && messageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        await callback(message, cancellationToken);
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                }
            }
            catch (Exception cancelledEx) when (cancelledEx is ThreadAbortException
                                                    or OperationCanceledException
                                                    or TaskCanceledException)
            {
                // Task was cancelled, this is expected behavior
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in queue worker: {ex}");
            }
        }

        public void Dispose()
        {
            _raygunLogger?.Verbose("[ThrottledBackgroundMessageProcessor] Disposing");
            if (_isDisposing)
            {
                return;
            }

            _isDisposing = true;

            try
            {
                foreach (var kvp in _workerTasks)
                {
                    if (_workerTasks.TryRemove(kvp.Key, out var cts))
                    {
                        if (!cts.IsCancellationRequested)
                        {
                            cts.Cancel();
                        }

                        cts.Dispose();
                    }
                }

                _globalCancellationSource.Cancel();

                // If we're running in a browser, we can't wait for tasks to complete
                // as this is only called when the app is shutting down, and the browser
                // will kill the app before the tasks have a chance to complete.
                if (!OperatingSystem.IsBrowser())
                {
                    // WaitAll is not supported in Blazor WebAssembly
                    Task.WaitAll(_workerTasks.Keys.ToArray(), TimeSpan.FromSeconds(2));
                }

                _globalCancellationSource.Dispose();
            }
            catch (Exception ex)
            {
                _raygunLogger?.Error($"[ThrottledBackgroundMessageProcessor] Dispose error: {ex}");
                Debug.WriteLine($"Exception in ThrottledBackgroundMessageProcessor.Dispose: {ex}");
            }
        }
    }
}