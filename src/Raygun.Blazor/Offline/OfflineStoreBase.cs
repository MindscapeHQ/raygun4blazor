using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Offline;

/// <summary>
/// 
/// </summary>
public delegate Task SendHandler(RaygunRequest messagePayload, CancellationToken cancellationToken);

/// <summary>
/// 
/// </summary>
public abstract class OfflineStoreBase
{
    private readonly IBackgroundSendStrategy _backgroundSendStrategy;

    /// <summary>
    /// 
    /// </summary>
    protected internal SendHandler? SendCallback { get; set; }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="backgroundSendStrategy"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected OfflineStoreBase(IBackgroundSendStrategy backgroundSendStrategy)
    {
        _backgroundSendStrategy =
            backgroundSendStrategy ?? throw new ArgumentNullException(nameof(backgroundSendStrategy));
        _backgroundSendStrategy.OnSendAsync += ProcessOfflineCrashReports;
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task ProcessOfflineCrashReports()
    {
        if (SendCallback is null)
        {
            return;
        }

        var cachedCrashReports = await GetAll(CancellationToken.None);
        foreach (var crashReport in cachedCrashReports)
        {
            await SendCallback(crashReport.MessagePayload, CancellationToken.None);
            await Remove(crashReport.Id, CancellationToken.None);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<List<CrashReportStoreEntry>> GetAll(CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="crashPayload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal abstract Task<bool> Save(RaygunRequest crashPayload, CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cacheEntryId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<bool> Remove(Guid cacheEntryId, CancellationToken cancellationToken);
}