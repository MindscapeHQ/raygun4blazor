using System.Text.Json.Serialization;
using Raygun.Blazor.Converters;

namespace Raygun.Blazor.Models
{

    /// <summary>
    /// Specifies the different types of <see cref="BreadcrumbLevel" /> that are currently supported by the Raygun API.
    /// </summary>
    [JsonConverter(typeof(KebabCaseJsonStringEnumConverter<BreadcrumbLevel>))]
    public enum BreadcrumbLevel
    {
        /// <summary>
        /// Debug level breadcrumb.
        /// </summary>
        Debug,
        
        /// <summary>
        /// Info level breadcrumb.
        /// </summary>
        Info,
        
        /// <summary>
        /// Warning level breadcrumb.
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error level breadcrumb.
        /// </summary>
        Error
    }
}