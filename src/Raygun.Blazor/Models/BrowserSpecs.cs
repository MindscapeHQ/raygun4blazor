using System;
using MyCSharp.HttpUserAgentParser;

namespace Raygun.Blazor.Models
{
    /// <summary>
    /// Attributes about the Browser that are <i>highly unlikely</i> to change at runtime.
    /// </summary>
    /// <remarks>
    /// This exists because we need to parse some stuff from the browser to construct a proper <see cref="EnvironmentDetails" />
    /// instance. Also, the <see cref="EnvironmentDetails" /> is a hot mess of JavaScript serialization inconsistency.
    /// </remarks>
    internal record BrowserSpecs
    {
        #region Private Members

        private string? _calculatedBrowserVersion;
        private string? _calculatedBrowserName;
        private string? _calculatedOSVersion;

        #endregion

        #region Public Properties

        /// <summary>
        /// True if able to read and write cookies. From `navigator.cookieEnabled`.
        /// </summary>
        public bool AreCookiesEnabled { get; set; }

        /// <summary>
        /// Browser name as extracted from the User Agent string.
        /// </summary>
        public string? CalculatedBrowserName => _calculatedBrowserName;

        /// <summary>
        /// Browser version as extracted from the User Agent string.
        /// </summary>
        public string? CalculatedBrowserVersion => _calculatedBrowserVersion;

        /// <summary>
        /// Color depth of the screen. From `screen.colorDepth`.
        /// </summary>
        public int ColorDepth { get; set; }

        /// <summary>
        /// The company who manufactured the device.
        /// </summary>
        /// <remarks>
        /// Currently not set by the client.
        /// </remarks>
        public string? DeviceManufacturer { get; set; }

        /// <summary>
        /// The model of the device.
        /// </summary>
        /// <remarks>
        /// Currently not set by the client.
        /// </remarks>
        public string? DeviceModel { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        /// <remarks>
        /// Currently not set by the client.
        /// </remarks>
        public string? DeviceName { get; set; }

        /// <summary>
        /// Device memory in gigabytes. From `navigator.deviceMemory`.
        /// </summary>
        public decimal DeviceMemoryInGb { get; set; }

        /// <summary>
        /// If the device has multiple monitors. From `screen.isExtended`.
        /// </summary>
        /// <remarks>
        /// When null, the browser does not support the required API.
        /// </remarks>
        public bool? HasMultipleMonitors { get; set; }

        /// <summary>
        /// Device locale. From `navigator.language`.
        /// </summary>
        public string? Locale { get; set; }

        /// <summary>
        /// Operating system version as extracted from the User Agent string.
        /// </summary>
        public string? CalculatedOSVersion => _calculatedOSVersion;

        /// <summary>
        /// Pixel depth of the screen. From `screen.pixelDepth`.
        /// </summary>
        public int PixelDepth { get; set; }

        /// <summary>
        /// OS Name. From `navigator.platform`.
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// Number of logical processors available to run threads on the user's computer. From `navigator.hardwareConcurrency`.
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// Screen height in pixels. From `screen.height`.
        /// </summary>
        public int ScreenHeight { get; set; }

        /// <summary>
        /// Screen width in pixels. From `screen.width`.
        /// </summary>
        public int ScreenWidth { get; set; }

        /// <summary>
        /// User Agent Hints.
        /// </summary>
        public BrowserUserAgentData? UAHints { get; set; }

        /// <summary>
        /// User Agent string. From `navigator.userAgent`.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// UTC offset in minutes. From `new Date().getTimezoneOffset() / -60`.
        /// </summary>
        public int UtcOffset { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculate values from User Agent.
        /// </summary>
        internal void ParseUserAgent()
        {
            var result = HttpUserAgentParser.Parse(UserAgent!);
            _calculatedBrowserName = result.Name;
            _calculatedBrowserVersion = result.Version;
            _calculatedOSVersion = result.Platform?.Name;
            Console.WriteLine(result.MobileDeviceType);
        }

        #endregion
    }
}
