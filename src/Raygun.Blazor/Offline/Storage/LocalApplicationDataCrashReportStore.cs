using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Models;
using Raygun.Blazor.Offline.SendStrategy;

namespace Raygun.Blazor.Offline.Storage;

/// <summary>
/// Configures the FileSystemCrashReportStore to store crash reports in the local application data folder.
/// </summary>
internal sealed class LocalApplicationDataCrashReportStore : FileSystemCrashReportStore
{
    /// <summary>
    /// Creates a new instance of the <see cref="LocalApplicationDataCrashReportStore"/> class.
    /// Configuration is obtained from <see cref="RaygunSettings"/>.
    /// </summary>
    /// <param name="backgroundSendStrategy"></param>
    /// <param name="settings"></param>
    internal LocalApplicationDataCrashReportStore(IBackgroundSendStrategy backgroundSendStrategy,
        IOptions<RaygunSettings> settings)
        : base(backgroundSendStrategy, GetLocalAppDirectory(settings.Value.DirectoryName),
            settings.Value.MaxOfflineFiles,
            Logging.RaygunLogger.Create(settings.Value.LogLevel))
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