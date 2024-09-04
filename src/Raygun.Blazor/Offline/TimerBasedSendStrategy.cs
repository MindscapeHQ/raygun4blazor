using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Raygun.Blazor.Offline;

/// <summary>
/// 
/// </summary>
public class TimerBasedSendStrategy : IBackgroundSendStrategy
{
  private static readonly TimeSpan DefaultInternal = TimeSpan.FromSeconds(30);

  private readonly Timer _backgroundTimer;
  
  /// <summary>
  /// 
  /// </summary>
  public event Func<Task>? OnSendAsync;

  /// <summary>
  /// 
  /// </summary>
  public TimeSpan Interval { get; }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="interval"></param>
  public TimerBasedSendStrategy(TimeSpan? interval = null)
  {
    Interval = interval ?? DefaultInternal;
    _backgroundTimer = new Timer(SendOfflineErrors);
    Start();
  }

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
  /// 
  /// </summary>
  public void Start()
  {
    // This sets the timer to trigger once at the interval, and then "never again".
    // This inherently prevents the timer from being re-entrant 
    _backgroundTimer.Change(Interval, TimeSpan.FromMilliseconds(int.MaxValue));
  }

  /// <summary>
  /// 
  /// </summary>
  public void Stop()
  {
    _backgroundTimer.Change(Timeout.Infinite, 0);
  }

  /// <summary>
  /// 
  /// </summary>
  public void Dispose()
  {
    _backgroundTimer?.Dispose();
  }
}