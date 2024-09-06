using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Raygun.Blazor.Logging;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline.SendStrategy;

namespace Raygun.Blazor.Offline.Storage;

/// <summary>
/// File system based crash report store.
/// </summary>
internal class FileSystemCrashReportStore : OfflineStoreBase
{
    internal const int MaxOfflineFiles = 50;
    private const string CacheFileExtension = "rgcrash";
    private readonly string _storageDirectory;
    private readonly int _maxOfflineFiles;
    private readonly ConcurrentDictionary<Guid, string> _cacheLocationMap = new();

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

    /// <summary>
    /// Creates a new instance of the <see cref="FileSystemCrashReportStore"/> class.
    /// </summary>
    /// <param name="backgroundSendStrategy">Send strategy</param>
    /// <param name="storageDirectory">Storage directory</param>
    /// <param name="maxOfflineFiles">Maximum amount of stored reports</param>
    /// <param name="raygunLogger">Internal logger class</param>
    internal FileSystemCrashReportStore(IBackgroundSendStrategy backgroundSendStrategy, string storageDirectory,
        int maxOfflineFiles = MaxOfflineFiles, IRaygunLogger? raygunLogger = null)
        : base(backgroundSendStrategy, raygunLogger)
    {
        _storageDirectory = storageDirectory;
        _maxOfflineFiles = maxOfflineFiles;
    }

    /// <summary>
    /// Obtain all stored crash reports.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<List<CrashReportStoreEntry>> GetAll(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_storageDirectory))
        {
            return [];
        }

        var crashFiles = Directory.GetFiles(_storageDirectory, $"*.{CacheFileExtension}");
        var errorRecords = new List<CrashReportStoreEntry>();

        foreach (var crashFile in crashFiles)
        {
            try
            {
                using var fileStream = new FileStream(crashFile, FileMode.Open, FileAccess.Read);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream, Encoding.UTF8);

               RaygunLogger?.Verbose(
                    $"[FileSystemCrashReportStore] Attempting to load offline crash at {crashFile}");
                var jsonString = await reader.ReadToEndAsync(cancellationToken);
                var errorRecord =
                    JsonSerializer.Deserialize<CrashReportStoreEntry>(jsonString, _jsonSerializerOptions)!;

                errorRecords.Add(errorRecord);
                _cacheLocationMap[errorRecord.Id] = crashFile;
            }
            catch (Exception ex)
            {
                RaygunLogger?.Error($"[FileSystemCrashReportStore] Error deserializing offline crash: {ex}");
                File.Move(crashFile, $"{crashFile}.failed");
            }
        }

        return errorRecords;
    }

    /// <summary>
    /// Store a report
    /// </summary>
    /// <param name="raygunRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal override async Task<bool> Save(RaygunRequest raygunRequest, CancellationToken cancellationToken)
    {
        var cacheEntryId = Guid.NewGuid();
        try
        {
            Directory.CreateDirectory(_storageDirectory);

            var crashFiles = Directory.GetFiles(_storageDirectory, $"*.{CacheFileExtension}");
            if (crashFiles.Length >= _maxOfflineFiles)
            {
                RaygunLogger?.Warning(
                    $"[FileSystemCrashReportStore] Maximum offline files of [{_maxOfflineFiles}] has been reached");
                return false;
            }

            var cacheEntry = new CrashReportStoreEntry(cacheEntryId, raygunRequest);
            var filePath = GetFilePathForCacheEntry(cacheEntryId);
            var jsonContent = JsonSerializer.Serialize(cacheEntry, _jsonSerializerOptions);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            using var writer = new StreamWriter(gzipStream, Encoding.UTF8);

            RaygunLogger?.Verbose($"[FileSystemCrashReportStore] Saving crash {cacheEntry.Id} to {filePath}");
            await writer.WriteAsync(jsonContent);
            await writer.FlushAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            RaygunLogger?.Error(
                $"[FileSystemCrashReportStore] Error adding crash [{cacheEntryId}] to store: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Removes a stored crash report, should be called after being sent.
    /// </summary>
    /// <param name="cacheId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task<bool> Remove(Guid cacheId, CancellationToken cancellationToken)
    {
        try
        {
            if (_cacheLocationMap.TryGetValue(cacheId, out var filePath))
            {
                var result = RemoveFile(filePath);
                _cacheLocationMap.TryRemove(cacheId, out _);
                return Task.FromResult(result);
            }
        }
        catch (Exception ex)
        {
            RaygunLogger?.Error(
                $"[FileSystemCrashReportStore] Error remove crash [{cacheId}] from store: {ex}");
        }

        return Task.FromResult(false);
    }

    private static bool RemoveFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    private string GetFilePathForCacheEntry(Guid cacheId)
    {
        return Path.Combine(_storageDirectory, $"{cacheId:N}.{CacheFileExtension}");
    }
}