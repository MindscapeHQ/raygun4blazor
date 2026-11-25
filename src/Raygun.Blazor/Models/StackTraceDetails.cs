using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Raygun.Blazor.Extensions;

namespace Raygun.Blazor.Models
{
    /// <summary>
    /// 
    /// </summary>
    public partial class StackTraceDetails
    {
        #region Public Properties

        /// <summary>
        /// The name of the class this stack frame is in.
        /// </summary>
        [JsonInclude]
        public string? ClassName { get; set; }

        /// <summary>
        /// The column of the file that this stack frame is in.
        /// </summary>
        [JsonInclude]
        public int ColumnNumber { get; set; }

        /// <summary>
        /// The name of the file this stack frame is in.
        /// </summary>
        [JsonInclude]
        public string? FileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public int ILOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public string? ImageSignature { get; set; }

        /// <summary>
        /// The line number of this stack frame.
        /// </summary>
        [JsonInclude]
        public int LineNumber { get; set; }

        /// <summary>
        /// The name of the method this stack frame is in.
        /// </summary>
        [JsonInclude]
        public string? MethodName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public int? MethodToken { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="StackTraceDetails" /> class.
        /// </summary>
        /// <remarks>
        /// This is an optimized code path for JSON deserialization and should not be used directly.
        /// </remarks>
        [JsonConstructor]
        internal StackTraceDetails()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        internal StackTraceDetails(StackFrame frame)
        {
            var names = frame.GetBlazorNames();

            ClassName = names.ClassName;
            ColumnNumber = frame.GetFileColumnNumber();
            FileName = frame.GetFileName();
            ILOffset = frame.GetILOffset();
            LineNumber = frame.GetFileLineNumber();
            MethodName = names.MethodName;
            MethodToken = frame.GetMethod()?.MetadataToken;

            // Attempt to get the image signature from the debug information
            var method = frame.GetMethod();
            if (method != null)
            {
                // Debug information gets cached
                var debugInfo = ErrorDetails.TryGetDebugInformation(method.Module.FullyQualifiedName);
                if (debugInfo != null)
                {
                    ImageSignature = debugInfo.Signature;
                }
            }

        }

        /// <summary>
        /// Creates a new instance of the <see cref="StackTraceDetails" /> class from one trimmed line of a
        /// JavaScript stack trace.
        /// Examples: "at causeErrors (http://localhost:5010/myfunctions.js:4:9)" (Chrome, Edge),
        /// "causeErrors@http://localhost:5010/myfunctions.js:4:9" (Firefox, Safari).
        /// </summary>
        /// <remarks>
        /// A frame with no line and column, such as "at Array.forEach (&lt;anonymous&gt;)" or
        /// "forEach@[native code]", keeps its method name and uses the location text as the file name.
        /// An anonymous function's method name is "?", as raygun4js sends it.
        /// A line that matches neither frame form is kept whole as the file name.
        /// </remarks>
        /// <param name="frame">One line of a JavaScript stack trace, without leading or trailing whitespace.</param>
        internal StackTraceDetails(string frame)
        {
            var match = V8FrameRegex().Match(frame);
            if (!match.Success)
            {
                match = GeckoFrameWithUrlRegex().Match(frame);
            }

            if (!match.Success)
            {
                match = GeckoFrameRegex().Match(frame);
            }

            if (!match.Success)
            {
                FileName = frame;
                return;
            }

            var functionName = match.Groups["functionName"].Value;
            var location = match.Groups["location"].Value;
            var locationMatch = LocationRegex().Match(location);

            MethodName = functionName.Length > 0 ? functionName : AnonymousFunctionName;

            if (locationMatch.Success
                && int.TryParse(locationMatch.Groups["lineNumber"].Value, out var lineNumber)
                && int.TryParse(locationMatch.Groups["columnNumber"].Value, out var columnNumber))
            {
                FileName = locationMatch.Groups["fileName"].Value;
                LineNumber = lineNumber;
                ColumnNumber = columnNumber;
            }
            else
            {
                FileName = location.Length > 0 ? location : null;
            }
        }

        // What raygun4js sends as the method name of an anonymous function (TraceKit's UNKNOWN_FUNCTION).
        private const string AnonymousFunctionName = "?";

        // Every pattern uses NonBacktracking, because the frame text can come from a client. Under a
        // backtracking engine the V8 pattern takes seconds on a long line that does not match; the
        // others stay linear, but the same engine keeps a later edit from reintroducing the problem.

        // Chrome and Edge: "at name (location)", "at location", or "at async location" for an
        // awaited anonymous function, where "async" is not part of the URL.
        [GeneratedRegex(@"^at\s+(?:(?<functionName>.+?)\s+\((?<location>.+)\)|(?:async\s+)?(?<location>.+))$", RegexOptions.NonBacktracking)]
        private static partial Regex V8FrameRegex();

        // Firefox and Safari: "name@location", where name may be empty. Both name and URL can contain
        // "@" (Safari names a function after its object key, e.g. "./node_modules/@scope/pkg/index.js",
        // and URLs can be "https://unpkg.com/react@18.2.0/..."), so this splits at the first "@"
        // followed by a URL scheme or "[", as in "forEach@[native code]".
        [GeneratedRegex(@"^(?<functionName>.*?)@(?<location>(?:[A-Za-z][A-Za-z0-9+.\-]*:|\[).*)$", RegexOptions.NonBacktracking)]
        private static partial Regex GeckoFrameWithUrlRegex();

        // Firefox and Safari, when the location has no URL scheme: splits at the first "@".
        [GeneratedRegex(@"^(?<functionName>[^@]*)@(?<location>.*)$", RegexOptions.NonBacktracking)]
        private static partial Regex GeckoFrameRegex();

        // "file:line:column". [0-9] rather than \d, which also matches non-ASCII digits.
        [GeneratedRegex(@"^(?<fileName>.+):(?<lineNumber>[0-9]+):(?<columnNumber>[0-9]+)$", RegexOptions.NonBacktracking)]
        private static partial Regex LocationRegex();

        #endregion
    }
}