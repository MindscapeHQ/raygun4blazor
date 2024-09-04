using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Raygun.Blazor.Offline.Storage;

/// <summary>
/// Stores a cached copy of crash reports that failed to send in Local App Data
/// Creates a directory if specified, otherwise creates a unique directory based off the location of the application
/// </summary>
public sealed class LocalApplicationDataCrashReportStore : FileSystemCrashReportStore
{
    private const int DefaultMaxOfflineFiles = 50;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="backgroundSendStrategy"></param>
    /// <param name="settings"></param>
    public LocalApplicationDataCrashReportStore(IBackgroundSendStrategy backgroundSendStrategy, RaygunSettings settings)
        : base(backgroundSendStrategy, GetLocalAppDirectory(settings.DirectoryName),
            settings.MaxOfflineFiles ?? DefaultMaxOfflineFiles)
    {
    }

    private static string GetLocalAppDirectory(string? directoryName)
    {
        directoryName ??= CreateUniqueDirectoryName();
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directoryName);
    }

    private static string CreateUniqueDirectoryName()
    {
        // Try to generate a unique id, from the executable location
        var uniqueId = Assembly.GetEntryAssembly()?.Location ??
                       throw new ApplicationException("Cannot determine unique application id");

        var uniqueIdHash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(uniqueId));
        return BitConverter.ToString(uniqueIdHash).Replace("-", "").ToLowerInvariant();
    }
}