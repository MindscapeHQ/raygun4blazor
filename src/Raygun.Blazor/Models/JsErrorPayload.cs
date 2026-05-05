using System.Text.Json.Serialization;

namespace Raygun.Blazor.Models
{
    /// <summary>
    /// Plain DTO carrying details of a JavaScript error from the browser to .NET.
    /// </summary>
    /// <remarks>
    /// Populated by the Raygun JS handler in Raygun.Blazor.ts and passed to
    /// <see cref="RaygunBrowserInterop.RecordJsException(JsErrorPayload, System.Collections.Generic.List{string}?, System.Collections.Generic.Dictionary{string, object}?)" />.
    /// All fields are optional because browsers (notably iOS/Safari and cross-origin
    /// scripts) frequently dispatch ErrorEvents whose `error` property is null.
    /// </remarks>
    public class JsErrorPayload
    {
        /// <summary>
        /// The error name (e.g. "TypeError"). May be null when the source is a
        /// cross-origin "Script error." with no Error reference.
        /// </summary>
        [JsonInclude]
        public string? Name { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        [JsonInclude]
        public string? Message { get; set; }

        /// <summary>
        /// The raw JavaScript stack string, newline-separated frames.
        /// </summary>
        [JsonInclude]
        public string? Stack { get; set; }

        /// <summary>
        /// The script URL the error originated from, when available.
        /// </summary>
        [JsonInclude]
        public string? FileName { get; set; }

        /// <summary>
        /// The line number the error originated from, when available.
        /// </summary>
        [JsonInclude]
        public int? LineNumber { get; set; }

        /// <summary>
        /// The column number the error originated from, when available.
        /// </summary>
        [JsonInclude]
        public int? ColumnNumber { get; set; }
    }
}
