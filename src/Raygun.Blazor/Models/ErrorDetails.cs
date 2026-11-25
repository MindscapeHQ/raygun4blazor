using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;
using Raygun.Blazor.Extensions;
using NonGeneric = System.Collections;

namespace Raygun.Blazor.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorDetails
    {
        #region Public Properties

        /// <summary>
        /// The name of the <see langword="class" /> the error was produced in.
        /// </summary>
        [JsonInclude]
        public string? ClassName { get; set; }

        /// <summary>
        /// Data contained in the error object.
        /// </summary>
        [JsonInclude]
        public NonGeneric.IDictionary? Data { get; set; }

        /// <summary>
        /// Details about the symbol files related to the error.
        /// </summary>
        [JsonInclude]
        public List<PEDebugDetails>? Images { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public ErrorDetails? InnerError { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public List<ErrorDetails>? InnerErrors { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public string? Message { get; set; }

        /// <summary>
        /// The collection of stack traces.
        /// </summary>
        /// <remarks>
        /// The first one in the list should be the highest on the stack
        /// </remarks>
        [JsonInclude]
        public List<StackTraceDetails>? StackTrace { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorDetails" /> class.
        /// </summary>
        /// <remarks>
        /// This is an optimized code path for JSON deserialization and should not be used directly.
        /// </remarks>
        [JsonConstructor]
        private ErrorDetails()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        internal ErrorDetails(string? message)
        {
            Message = message;
            StackTrace = [new StackTraceDetails()];
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorDetails" /> class for a given <see cref="Exception" />.
        /// </summary>
        /// <param name="ex">The <see cref="Exception" /> to use to populate this <see cref="ErrorDetails" /> instance.</param>
        internal ErrorDetails(Exception ex)
        {
            if (ex is JsUnhandledException jsException)
            {
                // JS Exception
                ClassName = jsException.JsName ?? jsException.GetType().FullName;
                Data = jsException.Data;
                Message = jsException.Message;
                if (jsException.StackTrace != null)
                {
                    var frames = RemoveJsStackHeader(jsException.StackTrace, jsException.JsName, jsException.Message)
                        .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (frames.Length > 0)
                    {
                        StackTrace = frames.Select(frame => new StackTraceDetails(frame)).ToList();
                    }
                }

                if (jsException.InnerException is not null)
                {
                    InnerError = new ErrorDetails(jsException.InnerException);
                }
            }
            else
            {
                // Dotnet Exception
                var betterEx = ex.Demystify();
                ClassName = betterEx.GetType().FullName;
                Data = betterEx.Data;
                Message = betterEx.Message;
                StackTrace = new EnhancedStackTrace(ex).GetExternalFrames()
                    .Select(frame => new StackTraceDetails(frame)).ToList();

                if (betterEx is AggregateException aggregateEx)
                {
                    InnerErrors = aggregateEx.InnerExceptions.Select(innerEx => new ErrorDetails(innerEx)).ToList();
                }
                else if (betterEx.InnerException is not null)
                {
                    InnerError = new ErrorDetails(betterEx.InnerException);
                }

                Images = GetDebugInfoForStackFrames(new EnhancedStackTrace(ex).GetExternalFrames());
            }
        }

        #endregion

        #region Private Properties

        private static readonly ConcurrentDictionary<string, PEDebugDetails?> DebugInformationCache = new();

        internal static Func<string, PEReader?> AssemblyReaderProvider { get; set; } =
            PortableExecutableReaderExtensions.GetFileSystemPEReader;

        #endregion

        #region Privatate Methods

        /// <summary>
        /// Removes the header that Chrome and Edge put before the frames of a JavaScript stack.
        /// </summary>
        /// <remarks>
        /// The header is the error's "name: message" text, not a frame, and the message can span several
        /// lines (Blazor puts a whole .NET exception in it, and a message can contain another error's
        /// stack). It is matched exactly first. Chrome fixes the message in the header when the error is
        /// created (it reads the name later), so if the message was changed afterwards the exact match
        /// fails; the header is then everything before the first V8 frame line. Firefox and Safari stacks
        /// have no header and no V8 frame lines, so they are returned unchanged.
        /// </remarks>
        private static string RemoveJsStackHeader(string stack, string? name, string? message)
        {
            // V8 frame lines start with four spaces; .NET stack lines in a message start with three.
            const string V8FrameLineStart = "    at ";

            string? header;

            if (string.IsNullOrEmpty(name))
            {
                header = message;
            }
            else if (string.IsNullOrEmpty(message))
            {
                header = name;
            }
            else
            {
                header = $"{name}: {message}";
            }

            if (!string.IsNullOrEmpty(header) && stack.StartsWith(header, StringComparison.Ordinal))
            {
                var isWholeLine = stack.Length == header.Length
                    || stack[header.Length] == '\n'
                    || stack[header.Length] == '\r';

                if (isWholeLine)
                {
                    return stack[header.Length..];
                }
            }

            if (stack.StartsWith(V8FrameLineStart, StringComparison.Ordinal))
            {
                return stack;
            }

            var firstFrameLine = stack.IndexOf("\n" + V8FrameLineStart, StringComparison.Ordinal);

            return firstFrameLine >= 0 ? stack[firstFrameLine..] : stack;
        }

        private static List<PEDebugDetails> GetDebugInfoForStackFrames(IEnumerable<StackFrame> frames)
        {
            var imageSet = new HashSet<PEDebugDetails>();

            foreach (var stackFrame in frames)
            {
                var methodBase = stackFrame.GetMethod();
                if (methodBase == null) continue;
                var debugInformation = TryGetDebugInformation(methodBase.Module.FullyQualifiedName);
                if (debugInformation != null)
                {
                    imageSet.Add(debugInformation);
                }
            }

            return imageSet.ToList();
        }

        internal static PEDebugDetails? TryGetDebugInformation(string moduleName)
        {
            if (DebugInformationCache.TryGetValue(moduleName, out var cachedInfo))
            {
                return cachedInfo;
            }

            try
            {
                // Attempt to read out the Debug Info from the PE
                var peReader = AssemblyReaderProvider(moduleName);

                // If we got this far, the assembly/module exists, so whatever the result
                // put it in the cache to prevent reading the disk over and over
                peReader.TryGetDebugInformation(out var debugInfo);
                DebugInformationCache.TryAdd(moduleName, debugInfo);
                return debugInfo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not load debug information: {ex}");
            }

            return null;
        }

        #endregion
    }
}