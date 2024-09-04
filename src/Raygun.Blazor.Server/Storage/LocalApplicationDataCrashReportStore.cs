using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Raygun.Blazor.Offline;

namespace Raygun.Blazor.Server.Storage;

/// <summary>
/// Stores a cached copy of crash reports that failed to send in Local App Data
/// Creates a directory if specified, otherwise creates a unique directory based off the location of the application
/// </summary>
public sealed class LocalApplicationDataCrashReportStore : FileSystemCrashReportStore
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="backgroundSendStrategy"></param>
    /// <param name="directoryName"></param>
    /// <param name="maxOfflineFiles"></param>
    public LocalApplicationDataCrashReportStore(IBackgroundSendStrategy backgroundSendStrategy,
        string directoryName = null, int maxOfflineFiles = 50)
        : base(backgroundSendStrategy, GetLocalAppDirectory(directoryName), maxOfflineFiles)
    {
    }

    private static string GetLocalAppDirectory(string directoryName)
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