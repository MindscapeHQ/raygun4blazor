using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Raygun.Blazor.Models
{

    /// <summary>
    /// Information about the environment the app is running in.
    /// </summary>
    public record EnvironmentDetails
    {

        #region Public Properties

        /// <summary>
        /// CPU architecture (ARMv8, AMD64, etc).
        /// </summary>
        public string? Architecture { get; set; }

        /// <summary>
        /// Available RAM in MB.
        /// </summary>
        public ulong? AvailablePhysicalMemory { get; set; }

        /// <summary>
        /// Available Virtual Memory in MB - RAM plus swap space.
        /// </summary>
        public ulong? AvailableVirtualMemory { get; set; }

        /// <summary>
        /// The height of the browser window.
        /// </summary>
        [JsonPropertyName("browser-Height")]
        public int? BrowserHeight { get; set; }

        /// <summary>
        /// The company who manufactured the browser.
        /// </summary>
        [JsonPropertyName("browser")]
        public string? BrowserManufacturer { get; set; }

        /// <summary>
        /// The Product Name of the browser.
        /// </summary>
        [JsonPropertyName("browserName")]
        public string? BrowserName { get; set; }

        /// <summary>
        /// The full user agent string.
        /// </summary>
        [JsonPropertyName("browser-Version")]
        public string? BrowserVersion { get; set; }

        /// <summary>
        /// The width of the browser window.
        /// </summary>
        [JsonPropertyName("browser-Width")]
        public int? BrowserWidth { get; set; }

        /// <summary>
        /// Color depth of the screen.
        /// </summary>
        [JsonPropertyName("color-Depth")]
        public int? ColorDepth { get; set; }

        /// <summary>
        /// The type of CPU in the machine
        /// </summary>
        public string? Cpu { get; set; }

        /// <summary>
        /// The orientation of the screen.
        /// </summary>
        public string? CurrentOrientation { get; set; }

        /// <summary>
        /// The company who manufactured the device.
        /// </summary>
        public string? DeviceManufacturer { get; set; }

        /// <summary>
        /// The model of the device.
        /// </summary>
        [JsonPropertyName("model")]
        public string? DeviceModel { get; set; }

        /// <summary>
        /// Name of the device (phone name for instance).
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// Free disk space in GB.
        /// </summary>
        public List<double>? DiskSpaceFree { get; set; }

        /// <summary>
        /// Locale setting of the system.
        /// </summary>
        public string? Locale { get; set; }

        /// <summary>
        /// The version of the operating system this app is running on.
        /// </summary>
        public string? OSVersion { get; set; }

        /// <summary>
        /// OS Name.
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// The number of processors present on the target machine.
        /// </summary>
        public int? ProcessorCount { get; set; }

        /// <summary>
        /// The scale of the screen.
        /// </summary>
        public decimal? ResolutionScale { get; set; }

        /// <summary>
        /// The height of the screen.
        /// </summary>
        [JsonPropertyName("screen-Height")]
        public int? ScreenHeight { get; set; }

        /// <summary>
        /// The width of the screen.
        /// </summary>
        [JsonPropertyName("screen-Width")]
        public int? ScreenWidth { get; set; }

        /// <summary>
        /// Total RAM in MB.
        /// </summary>
        public ulong? TotalPhysicalMemory { get; set; }

        /// <summary>
        /// Total Virtual Memory in MB - RAM plus swap space.
        /// </summary>
        public ulong? TotalVirtualMemory { get; set; }

        /// <summary>
        /// The height of the window.
        /// </summary>
        public int? WindowBoundsHeight { get; set; }

        /// <summary>
        /// The width of the window.
        /// </summary>
        public int? WindowBoundsWidth { get; set; }

        /// <summary>
        /// Number of hours offset from UTC.
        /// </summary>
        public decimal? UtcOffset { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="EnvironmentDetails" /> class.
        /// </summary>
        /// <remarks>
        /// This is an optimized code path for JSON deserialization and should not be used directly.
        /// </remarks>
        [JsonConstructor]
        private EnvironmentDetails()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specs"></param>
        /// <param name="stats"></param>
        internal EnvironmentDetails(BrowserSpecs? specs, BrowserStats? stats)
        {
            AvailableVirtualMemory = stats != null ? Convert.ToUInt64((stats.MemoryMaxSizeInBytes - stats.MemoryUsedSizeInBytes)) : 0;
            BrowserHeight = stats?.AppHeight;
            BrowserName = specs?.CalculatedBrowserName;
            BrowserWidth = stats?.AppWidth;
            ColorDepth = specs?.ColorDepth;
            CurrentOrientation = stats?.Orientation.ToString();
            DeviceManufacturer = !string.IsNullOrWhiteSpace(specs?.DeviceManufacturer) ? specs.DeviceManufacturer : null;
            DeviceModel = !string.IsNullOrWhiteSpace(specs?.DeviceModel) ? specs.DeviceModel : null;
            DeviceName = !string.IsNullOrWhiteSpace(specs?.DeviceName) ? specs.DeviceName : null; ;
            Locale = specs?.Locale;
            OSVersion = !string.IsNullOrWhiteSpace(specs?.UAHints?.CalculatedOSVersion) ? specs.UAHints.CalculatedOSVersion : specs?.CalculatedOSVersion;
            Platform = specs?.UAHints?.CalculatedPlatform ?? specs?.Platform;
            ProcessorCount = specs?.ProcessorCount;
            ResolutionScale = stats?.DevicePixelRatio;
            ScreenHeight = specs?.ScreenHeight;
            ScreenWidth = specs?.ScreenWidth;
            TotalPhysicalMemory = specs != null ? Convert.ToUInt64(specs.DeviceMemoryInGb * 1024 * 1024 * 1024) : 0;
            TotalVirtualMemory = stats != null ? Convert.ToUInt64(stats.MemoryMaxSizeInBytes) : 0;
            UtcOffset = specs?.UtcOffset;

            var uaBrowserVersionKey = (specs?.UAHints?.ComponentVersions?.Keys)?.FirstOrDefault(c => c.EndsWith(specs.CalculatedBrowserName!));
            if (uaBrowserVersionKey is null) return;

            BrowserManufacturer = uaBrowserVersionKey.Split(' ')[0];
            BrowserVersion = specs?.UAHints?.ComponentVersions?[uaBrowserVersionKey] ?? specs?.CalculatedBrowserVersion;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EnvironmentDetails" /> using runtime information.
        /// </summary>
        /// <remarks>
        /// Some of the properties are not available in all environments.
        /// e.g. Browsers, Android and iOS cannot access Process information.
        /// </remarks>
        /// <returns>
        /// Environment details for the current runtime.
        /// </returns>
        static internal EnvironmentDetails FromRuntime() => new()
        {
            // In most cases the Architecture and the Cpu will be the same.
            // The "ProcessArchitecture" is defined at compile time.
            Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            // The "OSArchitecture" is taken from the OS.
            Cpu = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(),

            // The DeviceName is the machine name.
            // Couldn't find a way to obtain Model or Manufacturer.
            DeviceName = Environment.MachineName,

            DiskSpaceFree = GetDiskSpaceFree(),

            Locale = System.Globalization.CultureInfo.CurrentCulture.Name,
            OSVersion = Environment.OSVersion.Version.ToString(),
            Platform = Environment.OSVersion.Platform.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            UtcOffset = (int)DateTimeOffset.Now.Offset.TotalHours,

            // Disable warning on platform compatibility: Process not available on all platforms.
#pragma warning disable CA1416 // Validate platform compatibility
            // Memory values obtained in Bytes and must be converted to Megabytes
            // Working Set: The amount of physical memory, in bytes, allocated for the associated process.
            // See: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.workingset64?view=net-8.0&redirectedfrom=MSDN#System_Diagnostics_Process_WorkingSet64
            TotalPhysicalMemory = Convert.ToUInt64(Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024),
            // Virtual Memory Size: Gets the amount of the virtual memory, in bytes, allocated for the associated process.
            TotalVirtualMemory = Convert.ToUInt64(Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024),
#pragma warning restore CA1416 // Validate platform compatibility
        };

        static private List<double> GetDiskSpaceFree()
        {
            try
            {
                // Convert Bytes to Gygabytes
                // Each drive is listed individually
                return System.IO.DriveInfo.GetDrives().Select(d => Convert.ToDouble(d.TotalFreeSpace / 1024 / 1024 / 1024)).ToList();
            }
            catch (Exception)
            {
                // If we can't get the disk space, return an empty list.
                // e.g. "System.IO.IOException: The device is not ready" when running on CI.
                return [];
            }
        }

        #endregion

    }

}