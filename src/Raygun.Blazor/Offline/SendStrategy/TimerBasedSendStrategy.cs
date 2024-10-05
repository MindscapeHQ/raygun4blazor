using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raygun.Blazor.Offline.SendStrategy;

/// <summary>
/// Send strategy based on a background timer.
/// </summary>
public class TimerBasedSendStrategy : IBackgroundSendStrategy
{
    private static readonly TimeSpan DefaultInternal = TimeSpan.FromSeconds(30);

    private readonly Timer _backgroundTimer;

    /// <summary>
    /// Event to trigger send events.
    /// </summary>
    public event Func<Task>? OnSendAsync;

    /// <summary>
    /// Time interval between send events.
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Constructor for the timer based send strategy.
    /// </summary>
    /// <param name="interval">Time interval between send events</param>
    public TimerBasedSendStrategy(TimeSpan? interval = null)
    {
        Interval = interval ?? DefaultInternal;
        _backgroundTimer = new Timer(SendOfflineErrors);
        Start();
    }

    /// <summary>
    /// Class destructor. Disposes the timer.
    /// </summary>
    ~TimerBasedSendStrategy()
    {
        Dispose();
    }

    private async void SendOfflineErrors(object? state)
    {
        try
        {
            var invocationList = OnSendAsync?.GetInvocationList();
            if (invocationList != null)
            {
                var tasks = invocationList.OfType<Func<Task>>().Select(handler => handler());
                await Task.WhenAll(tasks);
            }
        }
        finally
        {
            Start();
        }
    }

    /// <summary>
    /// Called when the send strategy should start.
    /// </summary>
    public void Start()
    {
        // This sets the timer to trigger once at the interval, and then "never again".
        // This inherently prevents the timer from being re-entrant 
        _backgroundTimer.Change(Interval, TimeSpan.FromMilliseconds(int.MaxValue));
    }

    /// <summary>
    /// Called when the send strategy should stop.
    /// </summary>
    public void Stop()
    {
        _backgroundTimer.Change(Timeout.Infinite, 0);
    }

    /// <summary>
    /// The timer based send strategy is disposed.
    /// </summary>
    public void Dispose()
    {
        _backgroundTimer?.Dispose();
    }
}