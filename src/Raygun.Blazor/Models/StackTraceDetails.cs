﻿using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Raygun.Blazor.Extensions;

namespace Raygun.Blazor.Models
{

    /// <summary>
    /// 
    /// </summary>
    internal class StackTraceDetails
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
        /// Creates a new instance of the <see cref="StackTraceDetails" /> class from a JavaScript stack trace.
        /// </summary>
        /// <param name="frame">JavaScript stack trace</param>
        internal StackTraceDetails(string frame)
        {
            // MB: TODO: Properly parse JavaScript stacktrace lines
            FileName = frame;
        }

        #endregion

    }

}