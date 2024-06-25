using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// Information about the environment the app is running in.
    /// </summary>
    internal record EnvironmentDetails
    {

        #region Public Properties

        /// <summary>
        /// CPU architecture (ARMv8, AMD64, etc).
        /// </summary>
        public string Architecture { get; set; }

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
        public string BrowserManufacturer { get; set; }

        /// <summary>
        /// The Product Name of the browser.
        /// </summary>
        [JsonPropertyName("browserName")]
        public string BrowserName { get; set; }

        /// <summary>
        /// The full user agent string.
        /// </summary>
        [JsonPropertyName("browser-Version")]
        public string BrowserVersion { get; set; }

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
        public string Cpu { get; set; }

        /// <summary>
        /// The orientation of the screen.
        /// </summary>
        public string CurrentOrientation { get; set; }

        /// <summary>
        /// The company who manufactured the device.
        /// </summary>
        public string DeviceManufacturer { get; set; }

        /// <summary>
        /// The model of the device.
        /// </summary>
        [JsonPropertyName("model")]
        public string DeviceModel { get; set; }

        /// <summary>
        /// Name of the device (phone name for instance).
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Free disk space in GB.
        /// </summary>
        public List<double> DiskSpaceFree { get; set; }

        /// <summary>
        /// Locale setting of the system.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// The version of the operating system this app is running on.
        /// </summary>
        public string OSVersion { get; set; }

        /// <summary>
        /// OS Name.
        /// </summary>
        public string Platform { get; set; }

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
        public int? UtcOffset { get; set; }

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
        internal EnvironmentDetails(BrowserSpecs specs, BrowserStats stats)
        {
            AvailableVirtualMemory = Convert.ToUInt64((stats.MemoryMaxSizeInBytes - stats.MemoryUsedSizeInBytes));
            BrowserHeight = stats.AppHeight;
            BrowserName = specs.CalculatedBrowserName;
            BrowserWidth = stats.AppWidth;
            ColorDepth = specs.ColorDepth;
            CurrentOrientation = stats.Orientation.ToString();
            DeviceManufacturer = !string.IsNullOrWhiteSpace(specs.DeviceManufacturer) ? specs.DeviceManufacturer : null;
            DeviceModel = !string.IsNullOrWhiteSpace(specs.DeviceModel) ? specs.DeviceModel: null;
            DeviceName = !string.IsNullOrWhiteSpace(specs.DeviceName) ? specs.DeviceName : null; ;
            Locale = specs.Locale;
            OSVersion = !string.IsNullOrWhiteSpace(specs.UAHints?.CalculatedOSVersion) ? specs.UAHints.CalculatedOSVersion : specs.CalculatedOSVersion;
            Platform = specs.UAHints.CalculatedPlatform ?? specs.Platform;
            ProcessorCount = specs.ProcessorCount;
            ResolutionScale = stats.DevicePixelRatio;
            ScreenHeight = specs.ScreenHeight;
            ScreenWidth = specs.ScreenWidth;
            TotalPhysicalMemory = Convert.ToUInt64(specs.DeviceMemoryInGB * 1024 * 1024 * 1024);
            TotalVirtualMemory = Convert.ToUInt64(stats.MemoryMaxSizeInBytes);
            UtcOffset = specs.UtcOffset;

            var uaBrowserVersionKey = specs.UAHints?.ComponentVersions?.Keys.Where(c => c.EndsWith(specs.CalculatedBrowserName)).FirstOrDefault();
            if (uaBrowserVersionKey is null) return;

            BrowserManufacturer = uaBrowserVersionKey.Split(' ')[0];
            BrowserVersion = specs.UAHints?.ComponentVersions?[uaBrowserVersionKey] ?? specs.CalculatedBrowserVersion;
        }

        #endregion

    }

}