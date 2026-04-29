using System;

namespace Raygun.Blazor
{
    /// <summary>
    /// Represents an unhandled JavaScript exception captured from the browser and
    /// forwarded to .NET for reporting to Raygun.
    /// </summary>
    /// <remarks>
    /// Replaces the previous dependency on KristofferStrube.Blazor.WebIDL's
    /// WebIDLException so Raygun owns the JS-error contract end-to-end.
    /// </remarks>
    public class JsUnhandledException : Exception
    {
        /// <summary>
        /// The JavaScript error name (e.g. "TypeError"). May be null when the
        /// browser dispatches an ErrorEvent without a backing Error object.
        /// </summary>
        public string? JsName { get; }

        /// <summary>
        /// The script URL the error originated from, when available.
        /// </summary>
        public string? FileName { get; }

        /// <summary>
        /// The line number the error originated from, when available.
        /// </summary>
        public int? LineNumber { get; }

        /// <summary>
        /// The column number the error originated from, when available.
        /// </summary>
        public int? ColumnNumber { get; }

        private readonly string? _stackTrace;

        /// <inheritdoc />
        public override string? StackTrace => _stackTrace ?? base.StackTrace;

        /// <summary>
        /// Creates a new instance of the <see cref="JsUnhandledException" /> class.
        /// </summary>
        public JsUnhandledException(string? name, string? message, string? stack,
            string? fileName = null, int? lineNumber = null, int? columnNumber = null)
            : base(string.IsNullOrEmpty(message) ? (name ?? "JavaScript error") : message)
        {
            JsName = name;
            _stackTrace = stack;
            FileName = fileName;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }
    }
}
