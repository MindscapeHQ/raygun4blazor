using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raygun.Blazor.Models;
using Raygun.Blazor.Queue;

namespace Raygun.Tests.Blazor.Queue
{
    [TestClass]
    public class ThrottledBackgroundMessageProcessorTests
    {
        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_WithQueueSpace_AcceptsMessages()
        {
            var cut = new ThrottledBackgroundMessageProcessor(1, 0, 25, (m, t) => Task.CompletedTask);
            var enqueued = cut.Enqueue(new RaygunRequest());

            enqueued.Should().BeTrue();

            cut.Dispose();
        }

        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_WithFullQueue_DropsMessages()
        {
            var cut = new ThrottledBackgroundMessageProcessor(1, 0, 25, (m, t) => Task.CompletedTask);
            cut.Enqueue(new RaygunRequest());
            var second = cut.Enqueue(new RaygunRequest());

            second.Should().BeFalse();

            cut.Dispose();
        }

        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_WithNoWorkers_DoesNotProcessMessages()
        {
            var processed = false;
            var cut = new ThrottledBackgroundMessageProcessor(1, 0, 25, (m, t) =>
            {
                processed = true;
                return Task.CompletedTask;
            });

            cut.Enqueue(new RaygunRequest());

            // This flushes the workers
            cut.Dispose();

            processed.Should().BeFalse();
        }

        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_WithAtLeastOneWorker_DoesProcessMessages()
        {
            var processed = false;
            var resetEventSlim = new ManualResetEventSlim();
            var cut = new ThrottledBackgroundMessageProcessor(1, 1, 25, (m, t) =>
            {
                processed = true;
                resetEventSlim.Set();
                return Task.CompletedTask;
            });

            cut.Enqueue(new RaygunRequest());

            resetEventSlim.Wait(TimeSpan.FromSeconds(5));

            // This flushes the workers
            cut.Dispose();

            processed.Should().BeTrue();
        }

        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_CallingDisposeTwice_DoesNotExplode()
        {
            var cut = new ThrottledBackgroundMessageProcessor(1, 0, 25, (m, t) => Task.CompletedTask);

            try
            {
                cut.Dispose();
                cut.Dispose();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_ExceptionInProcess_KillsWorkerThenCreatesAnother()
        {
            var shouldThrow = true;
            var secondMessageWasProcessed = false;
            var resetEventSlim = new ManualResetEventSlim();

            var cut = new ThrottledBackgroundMessageProcessor(1, 1, 25, (m, t) =>
            {
                if (shouldThrow)
                {
                    resetEventSlim.Set();
                    throw new Exception("Bad");
                }

                secondMessageWasProcessed = true;
                resetEventSlim.Set();
                return Task.CompletedTask;
            });

            cut.Enqueue(new RaygunRequest());

            resetEventSlim.Wait(TimeSpan.FromSeconds(5));
            resetEventSlim.Reset();

            shouldThrow = false;

            cut.Enqueue(new RaygunRequest());

            resetEventSlim.Wait(TimeSpan.FromSeconds(5));

            secondMessageWasProcessed.Should().BeTrue();
        }

        [TestMethod]
        public void ThrottledBackgroundMessageProcessor_CancellationRequested_IsCaughtAndKillsWorker()
        {
            var shouldThrow = true;
            var secondMessageWasProcessed = false;
            var resetEventSlim = new ManualResetEventSlim();

            var cut = new ThrottledBackgroundMessageProcessor(1, 1, 25, (m, t) =>
            {
                if (shouldThrow)
                {
                    resetEventSlim.Set();
                    throw new OperationCanceledException("Bad");
                }

                secondMessageWasProcessed = true;
                resetEventSlim.Set();
                return Task.CompletedTask;
            });

            cut.Enqueue(new RaygunRequest());

            resetEventSlim.Wait(TimeSpan.FromSeconds(5));
            resetEventSlim.Reset();

            shouldThrow = false;

            cut.Enqueue(new RaygunRequest());

            resetEventSlim.Wait(TimeSpan.FromSeconds(5));

            secondMessageWasProcessed.Should().BeTrue();
        }
    }
}