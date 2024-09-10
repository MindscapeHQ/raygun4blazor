using System;
using System.Threading.Tasks;

namespace Raygun.Blazor.Offline.SendStrategy;

/// <summary>
/// Defines the interface for the background send strategy.
/// Implementations must call the <see cref="OnSendAsync"/> event when it is time to send the data.
/// </summary>
public interface IBackgroundSendStrategy : IDisposable
{
    /// <summary>
    /// Event to trigger the send of the data.
    /// </summary>
    public event Func<Task> OnSendAsync;

    /// <summary>
    /// Called when the send strategy should start.
    /// </summary>
    public void Start();

    /// <summary>
    /// Called when the send strategy should stop.
    /// </summary>
    public void Stop();
}