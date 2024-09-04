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
/// 
/// </summary>
public class FileSystemCrashReportStore : OfflineStoreBase
{
    private const string CacheFileExtension = "rgcrash";
    private readonly string _storageDirectory;
    private readonly int _maxOfflineFiles;
    private readonly ConcurrentDictionary<Guid, string> _cacheLocationMap = new();
    private readonly IRaygunLogger? _raygunLogger;

    private readonly JsonSerializerOptions _jsonSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="backgroundSendStrategy"></param>
    /// <param name="storageDirectory"></param>
    /// <param name="maxOfflineFiles"></param>
    /// <param name="raygunLogger"></param>
    internal FileSystemCrashReportStore(IBackgroundSendStrategy backgroundSendStrategy, string storageDirectory,
        int maxOfflineFiles = 50, IRaygunLogger? raygunLogger = null)
        : base(backgroundSendStrategy)
    {
        _storageDirectory = storageDirectory;
        _maxOfflineFiles = maxOfflineFiles;
        _raygunLogger = raygunLogger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<List<CrashReportStoreEntry>> GetAll(CancellationToken cancellationToken)
    {
        var crashFiles = Directory.GetFiles(_storageDirectory, $"*.{CacheFileExtension}");
        var errorRecords = new List<CrashReportStoreEntry>();

        foreach (var crashFile in crashFiles)
        {
            try
            {
                using var fileStream = new FileStream(crashFile, FileMode.Open, FileAccess.Read);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream, Encoding.UTF8);

                _raygunLogger?.Verbose($"[FileSystemCrashReportStore] Attempting to load offline crash at {crashFile}");
                var jsonString = await reader.ReadToEndAsync(cancellationToken);
                var errorRecord =
                    JsonSerializer.Deserialize<CrashReportStoreEntry>(jsonString, _jsonSerializerOptions)!;

                errorRecords.Add(errorRecord);
                _cacheLocationMap[errorRecord.Id] = crashFile;
            }
            catch (Exception ex)
            {
                _raygunLogger?.Error($"[FileSystemCrashReportStore] Error deserializing offline crash: {ex}");
                File.Move(crashFile, $"{crashFile}.failed");
            }
        }

        return errorRecords;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal override async Task<bool> Save(RaygunRequest payload, CancellationToken cancellationToken)
    {
        var cacheEntryId = Guid.NewGuid();
        try
        {
            Directory.CreateDirectory(_storageDirectory);

            var crashFiles = Directory.GetFiles(_storageDirectory, $"*.{CacheFileExtension}");
            if (crashFiles.Length >= _maxOfflineFiles)
            {
                _raygunLogger?.Warning(
                    $"[FileSystemCrashReportStore] Maximum offline files of [{_maxOfflineFiles}] has been reached");
                return false;
            }

            var cacheEntry = new CrashReportStoreEntry(cacheEntryId, payload);
            var filePath = GetFilePathForCacheEntry(cacheEntryId);
            var jsonContent = JsonSerializer.Serialize(cacheEntry, _jsonSerializerOptions);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            using var writer = new StreamWriter(gzipStream, Encoding.UTF8);

            _raygunLogger?.Verbose($"[FileSystemCrashReportStore] Saving crash {cacheEntry.Id} to {filePath}");
            await writer.WriteAsync(jsonContent);
            await writer.FlushAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _raygunLogger?.Error($"[FileSystemCrashReportStore] Error adding crash [{cacheEntryId}] to store: {ex}");
            return false;
        }
    }

    /// <summary>
    /// 
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
            _raygunLogger?.Error($"[FileSystemCrashReportStore] Error remove crash [{cacheId}] from store: {ex}");
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