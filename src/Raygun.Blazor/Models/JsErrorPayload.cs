namespace Raygun.Blazor.Models
{
    /// <summary>
    /// Plain DTO carrying details of a JavaScript error from the browser to .NET.
    /// </summary>
    /// <remarks>
    /// Populated by Raygun.Blazor.ts and passed to
    /// <see cref="RaygunBrowserInterop.RecordJsException(JsErrorPayload, System.Collections.Generic.List{string}?, System.Collections.Generic.Dictionary{string, object}?)" />.
    /// All fields are optional because browsers (notably iOS/Safari and cross-origin
    /// scripts) frequently dispatch ErrorEvents whose `error` property is null.
    /// </remarks>
    public class JsErrorPayload
    {
        /// <summary>
        /// The error name (e.g. "TypeError"). Null when the thrown value was not an Error.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The raw JavaScript stack string, newline-separated frames.
        /// </summary>
        public string? Stack { get; set; }
    }
}
