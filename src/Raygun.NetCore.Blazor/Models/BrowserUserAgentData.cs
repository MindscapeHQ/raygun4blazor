using System;
using System.Collections.Generic;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// Represents the information acquired by calling "navigator.userAgentData" in JavaScript.
    /// </summary>
    /// <remarks>
    /// Any value that is derived from the reported data is prefixed with the word "Calculated" to indicate
    /// that it is not directly reported by the browser.
    /// </remarks>
    internal record BrowserUserAgentData
    {

        #region Public Properties

        /// <summary>
        /// A string containing the platform architecture. For example, "x86".
        /// </summary>
        public string Architecture { get; set; }

        /// <summary>
        /// A string containing the architecture bitness. For example, "32" or "64".
        /// </summary>
        public string Bitness { get; set; }

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"> containing a key representing the brand name a value specifying the publicly reported version of the browser and it's underlying engine.
        /// </summary>
        /// <remarks>
        /// Please note that one object may intentionally contain invalid information to prevent sites from relying on a fixed list of browsers.
        /// </remarks>
        public Dictionary<string, string> BrandVersions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CalculatedOSVersion => $"{Platform} {(
            Platform == "Windows" && Decimal.Parse(PlatformVersion?.Split(".")?[0] ?? "0") >= 13 ? "11" : 
            Platform == "Windows" && Decimal.Parse(PlatformVersion?.Split(".")?[0] ?? "0") < 13 ? "10" :
            PlatformVersion)}";

        /// <summary>
        /// 
        /// </summary>
        public string CalculatedPlatform
        {
            get
            {
                return true switch
                {
                    true when Platform == "Windows" && Architecture == "x86" && Bitness == "64" => "x86_64",
                    true when Platform == "Windows" && Architecture == "x86" && Bitness == "32" => "x86",
                    true when Platform == "Windows" && Architecture == "arm" && Bitness == "64" => "ARM64",
                    true when Platform == "Windows" && Architecture == "arm" && Bitness == "32" => "ARM32",
                    _ => null
                };
            }
        }

        /// <summary>
        /// A <see cref="Dictionary{TKey, TValue}"> containing a key representing the brand name a value specifying the binary version of the browser and it's underlying engine.
        /// </summary>
        /// <remarks>
        /// Please note that one object may intentionally contain invalid information to prevent sites from relying on a fixed list of browsers.
        /// </remarks>
        public Dictionary<string, string> ComponentVersions { get; set; }

        /// <summary>
        /// A string containing the form-factor of a device. For example, "Tablet" or "VR".
        /// </summary>
        public string FormFactor { get; set; }

        /// <summary>
        /// A boolean indicating whether the user agent is running on a mobile device
        /// </summary>
        public bool IsMobile { get; set; }

        /// <summary>
        /// A boolean indicating whether the user agent's binary is running in 32-bit mode on 64-bit Windows.
        /// </summary>
        public bool IsWow64 { get; set; }

        /// <summary>
        /// A string containing the model of mobile device. For example, "Pixel 2XL".
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// A string describing the platform the user agent is running on. For example, "Windows".
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// A string containing the platform version.
        /// </summary>
        public string PlatformVersion { get; set; }

        #endregion

    }

}
