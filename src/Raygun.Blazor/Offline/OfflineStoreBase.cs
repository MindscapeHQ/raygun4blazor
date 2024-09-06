using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Logging;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline.SendStrategy;

namespace Raygun.Blazor.Offline;

/// <summary>
/// Base class for all offline store implementations.
/// </summary>
internal abstract class OfflineStoreBase
{
    private readonly IBackgroundSendStrategy _backgroundSendStrategy;

    /// <summary>
    /// Internal send handler.
    /// </summary>
    protected internal SendHandler? SendCallback { get; set; }

    /// <summary>
    /// Raygun logger for the offline store.
    /// </summary>
    protected readonly IRaygunLogger? RaygunLogger;

    /// <summary>
    /// Creates a new instance of the <see cref="OfflineStoreBase"/> class.
    /// </summary>
    /// <param name="backgroundSendStrategy">Decides when to attempt to send stored reports</param>
    /// <param name="raygunLogger">Internal logger</param>
    protected OfflineStoreBase(IBackgroundSendStrategy backgroundSendStrategy,
        IRaygunLogger? raygunLogger = null)
    {
        _backgroundSendStrategy =
            backgroundSendStrategy ?? throw new ArgumentNullException(nameof(backgroundSendStrategy));
        _backgroundSendStrategy.OnSendAsync += ProcessOfflineCrashReports;
        RaygunLogger = raygunLogger;
    }

    /// <summary>
    /// Process the offline crash reports. Called by the background send strategy.
    /// </summary>
    private async Task ProcessOfflineCrashReports()
    {
        if (SendCallback is null)
        {
            return;
        }

        RaygunLogger?.Verbose("[OfflineStoreBase] Processing offline crash reports");
        var cachedCrashReports = await GetAll(CancellationToken.None);
        RaygunLogger?.Verbose($"[OfflineStoreBase] Found {cachedCrashReports.Count} offline crash reports");
        foreach (var crashReport in cachedCrashReports)
        {
            var result = await SendCallback(crashReport.RaygunRequest, CancellationToken.None);
            if (result)
            {
                RaygunLogger?.Verbose($"[OfflineStoreBase] Sent offline crash report: {crashReport.Id}");
                await Remove(crashReport.Id, CancellationToken.None);
            }
            else
            {
                RaygunLogger?.Warning($"[OfflineStoreBase] Failed to send offline crash report: {crashReport.Id}");
                // Stop processing if we fail to send a report
                break;
            }
        }
    }

    /// <summary>
    /// Obtain all the stored crash reports.
    /// </summary>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>List of stored crash reports</returns>
    protected abstract Task<List<CrashReportStoreEntry>> GetAll(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the crash report to be sent later.
    /// </summary>
    /// <param name="crashPayload">Raygun error request to store</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>true if saved correctly</returns>
    internal abstract Task<bool> Save(RaygunRequest crashPayload, CancellationToken cancellationToken);

    /// <summary>
    /// Remove stored crash report from store.
    /// </summary>
    /// <param name="cacheEntryId">Stored entry id</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns></returns>
    protected abstract Task<bool> Remove(Guid cacheEntryId, CancellationToken cancellationToken);
}