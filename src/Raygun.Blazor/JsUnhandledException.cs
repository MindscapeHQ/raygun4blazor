using System;

namespace Raygun.Blazor
{
    /// <summary>
    /// Represents a JavaScript error captured from the browser and forwarded to .NET for
    /// reporting to Raygun.
    /// </summary>
    internal class JsUnhandledException : Exception
    {
        /// <summary>
        /// The JavaScript error name (e.g. "TypeError"). Null when the thrown value was not an Error.
        /// </summary>
        public string? JsName { get; }

        private readonly string? _stackTrace;

        /// <inheritdoc />
        public override string? StackTrace => _stackTrace ?? base.StackTrace;

        /// <summary>
        /// Creates a new instance of the <see cref="JsUnhandledException" /> class.
        /// </summary>
        public JsUnhandledException(string? name, string? message, string? stack)
            : base(message)
        {
            JsName = name;
            _stackTrace = stack;
        }
    }
}
