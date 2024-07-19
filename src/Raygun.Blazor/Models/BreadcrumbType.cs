using System.Text.Json.Serialization;
using Raygun.Blazor.Converters;

namespace Raygun.Blazor.Models
{

    /// <summary>
    /// Specifies the different types of <see cref="BreadcrumbDetails" /> that are currently supported by the Raygun API.
    /// </summary>
    [JsonConverter(typeof(KebabCaseJsonStringEnumConverter<BreadcrumbType>))]
    public enum BreadcrumbType
    {

        /// <summary>
        /// 
        /// </summary>
        Manual,

        /// <summary>
        /// 
        /// </summary>
        ClickEvent,

        /// <summary>
        /// 
        /// </summary>
        Navigation,

        /// <summary>
        /// 
        /// </summary>
        Console,

        /// <summary>
        /// 
        /// </summary>
        Request

    }

}
