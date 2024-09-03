#nullable enable
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
public delegate Task SendHandler(string messagePayload, string apiKey, CancellationToken cancellationToken);

/// <summary>
/// 
/// </summary>
public abstract class OfflineStoreBase
{
  private readonly IBackgroundSendStrategy _backgroundSendStrategy;
  private static readonly Regex HttpStatusCodeRegex = new("(?<statusCode>4[0-9]{2}) \\(.+\\)");
  
  /// <summary>
  /// 
  /// </summary>
  protected SendHandler? SendCallback { get; set; }
  

  /// <summary>
  /// 
  /// </summary>
  /// <param name="backgroundSendStrategy"></param>
  /// <exception cref="ArgumentNullException"></exception>
  protected OfflineStoreBase(IBackgroundSendStrategy backgroundSendStrategy)
  {
    _backgroundSendStrategy = backgroundSendStrategy ?? throw new ArgumentNullException(nameof(backgroundSendStrategy));
    _backgroundSendStrategy.OnSendAsync += ProcessOfflineCrashReports;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="sendHandler"></param>
  public virtual void SetSendCallback(SendHandler sendHandler)
  {
    SendCallback = sendHandler;
  }

  /// <summary>
  /// 
  /// </summary>
  protected virtual async Task ProcessOfflineCrashReports()
  {
    if (SendCallback is null)
    {
      return;
    }

    try
    {
      var cachedCrashReports = await GetAll(CancellationToken.None);
      foreach (var crashReport in cachedCrashReports)
      {
        try
        {
          await SendCallback(crashReport.MessagePayload, crashReport.ApiKey, CancellationToken.None);
          await Remove(crashReport.Id, CancellationToken.None);
        }
        catch (HttpRequestException hrex)
        {
          var statusCode = HttpStatusCodeRegex.Match(hrex.Message).Groups["statusCode"]?.Value ?? "0";
          
          if (int.TryParse(statusCode, out var code) && code is >= 400 and < 500)
          {
            Trace.WriteLine($"Crash report returned {code} error, removing from cache");
            // Client error, remove the crash report
            await Remove(crashReport.Id, CancellationToken.None);
            continue;
          }
          
          Trace.WriteLine($"Crash report returned {statusCode} error, keeping in cache for retry");
          throw;
        }
        catch (Exception ex)
        {
          Trace.WriteLine($"Exception sending offline error [{crashReport.Id}]: {ex}");
          throw;
        }
      }
    }
    catch (Exception ex)
    {
      Trace.WriteLine($"Exception sending offline errors: {ex}");
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public abstract Task<List<CrashReportStoreEntry>> GetAll(CancellationToken cancellationToken);

  /// <summary>
  /// 
  /// </summary>
  /// <param name="crashPayload"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public abstract Task<bool> Save(RaygunRequest crashPayload, CancellationToken cancellationToken);
  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="cacheEntryId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public abstract Task<bool> Remove(Guid cacheEntryId, CancellationToken cancellationToken);
}