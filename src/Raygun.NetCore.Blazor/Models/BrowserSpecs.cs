using MyCSharp.HttpUserAgentParser;
using System;

namespace Raygun.NetCore.Blazor.Models
{
    /// <summary>
    /// Attributes about the Browser that are <i>highly unlikely</i> to change at runtime.
    /// </summary>
    /// <remarks>
    /// This exists because we need to parse some stuff from the browser to construct a proper <see cref="EnvironmentDetails" />
    /// instance. Also the <see cref="EnvironmentDetails" /> is a hot mess of JavaScript serialization inconsistency.
    /// </remarks>
    internal record BrowserSpecs
    {
        #region Private Members

        private string? _calculatedBrowserVersion;
        private string? _calculatedBrowserName;
        private string? _calculatedOSVersion;
        // private string? _browserManufacturer;

        #endregion

        #region Public Properties

        /// <summary>
        /// 
        /// </summary>
        public bool AreCookiesEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? CalculatedBrowserName => _calculatedBrowserName;

        /// <summary>
        /// 
        /// </summary>
        public string? CalculatedBrowserVersion => _calculatedBrowserVersion;

        /// <summary>
        /// 
        /// </summary>
        public int ColorDepth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? DeviceManufacturer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? DeviceModel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal DeviceMemoryInGb { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// When null, the browser does not support the required API.
        /// </remarks>
        public bool? HasMultipleMonitors { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Locale { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? CalculatedOSVersion => _calculatedOSVersion;

        /// <summary>
        /// 
        /// </summary>
        public int PixelDepth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ScreenHeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ScreenWidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BrowserUserAgentData? UAHints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int UtcOffset { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
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