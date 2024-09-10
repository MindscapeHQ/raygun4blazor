using System.Threading;
using System.Threading.Tasks;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Interfaces;

/// <summary>
/// Task to send the crash report by the offline store.
/// </summary>
public delegate Task<bool> SendHandler(RaygunRequest messagePayload, CancellationToken cancellationToken);

/// <summary>
/// Interface for offline store.
/// </summary>
public interface IRaygunOfflineStore
{
    /// <summary>
    /// Set the send callback.
    /// </summary>
    public void SetSendCallback(SendHandler sendHandler);

    /// <summary>
    /// Saves the crash report to be sent later.
    /// </summary>
    /// <param name="crashPayload">Raygun error request to store</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>true if saved correctly</returns>
    public Task<bool> Save(RaygunRequest crashPayload, CancellationToken cancellationToken);
}