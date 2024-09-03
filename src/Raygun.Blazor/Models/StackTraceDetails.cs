using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Raygun.Blazor.Extensions;

namespace Raygun.Blazor.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StackTraceDetails
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
            //ImageSignature = frame.ImageSignature;
            LineNumber = frame.GetFileLineNumber();
            MethodName = names.MethodName;
            MethodToken = frame.GetMethod()?.MetadataToken;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StackTraceDetails" /> class from a JavaScript stack trace frame.
        /// Example: "causeErrors@http://localhost:5010/myfunctions.js:4:9"
        /// </summary>
        /// <param name="frame">JavaScript stack trace</param>
        internal StackTraceDetails(string frame)
        {
            var regex = new Regex(
                @"(?<functionName>.+)@(?<fileName>.+):(?<lineNumber>\d+):(?<columnNumber>\d+)");
            var match = regex.Match(frame);
            if (match.Success)
            {
                MethodName = match.Groups["functionName"].Value;
                FileName = match.Groups["fileName"].Value;
                LineNumber = int.Parse(match.Groups["lineNumber"].Value);
                ColumnNumber = int.Parse(match.Groups["columnNumber"].Value);
            }
            else
            {
                FileName = frame;
            }
        }

        #endregion
    }
}