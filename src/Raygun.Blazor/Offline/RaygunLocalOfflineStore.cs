using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline.SendStrategy;
using Raygun.Blazor.Offline.Storage;

namespace Raygun.Blazor.Offline;

/// <summary>
/// Local offline store for storing crash reports in the local application data folder.
/// </summary>
public class RaygunLocalOfflineStore : IRaygunOfflineStore
{
    /// <summary>
    /// Internal offline store implementation.
    /// </summary>
    private readonly OfflineStoreBase _offlineStore;

    /// <summary>
    /// Creates a new instance of the <see cref="RaygunLocalOfflineStore"/> class.
    /// </summary>
    public RaygunLocalOfflineStore(IBackgroundSendStrategy backgroundSendStrategy, IOptions<RaygunSettings> settings)
    {
        _offlineStore =
            new LocalApplicationDataCrashReportStore(backgroundSendStrategy, settings);
    }

    /// <summary>
    /// Sets the send callback.
    /// </summary>
    void IRaygunOfflineStore.SetSendCallback(SendHandler sendHandler)
    {
        _offlineStore.SendCallback = sendHandler;
    }

    /// <summary>
    /// Saves the crash report to be sent later.
    /// </summary>
    /// <param name="crashPayload">Raygun error request to store</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>true if saved correctly</returns>
    Task<bool> IRaygunOfflineStore.Save(RaygunRequest crashPayload, CancellationToken cancellationToken)
    {
        return _offlineStore.Save(crashPayload, cancellationToken);
    }
}