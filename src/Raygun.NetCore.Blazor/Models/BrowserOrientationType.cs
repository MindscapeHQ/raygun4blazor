using Raygun.NetCore.Blazor.Converters;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// Specifies the different types of device orientation reported by the browser.
    /// </summary>
    [JsonConverter(typeof(KebabCaseJsonStringEnumConverter<BrowserOrientationType>))]
    public enum BrowserOrientationType
    {

        /// <summary>
        /// Taller-than-wide with the top facing-up
        /// </summary>
        PortraitPrimary,

        /// <summary>
        /// Taller-than-wide with the top facing-down.
        /// </summary>
        PortraitSecondary,

        /// <summary>
        /// Wider-than-tall with the top facing-right.
        /// </summary>
        LandscapePrimary,

        /// <summary>
        /// Wider-than-tall with the top facing-left.
        /// </summary>
        LandscapeSecondary

    }

}
