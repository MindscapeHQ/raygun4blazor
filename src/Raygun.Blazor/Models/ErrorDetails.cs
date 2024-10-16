using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;
using KristofferStrube.Blazor.WebIDL.Exceptions;
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
            if (ex is WebIDLException webIdlException)
            {
                // JS Exception
                ClassName = webIdlException.GetType().FullName;
                Data = webIdlException.Data;
                Message = webIdlException.Message;
                if (webIdlException.StackTrace != null)
                {
                    var frames = webIdlException.StackTrace.Split('\n')
                        .Where(frame => !string.IsNullOrWhiteSpace(frame));
                    StackTrace = frames.Select(frame => new StackTraceDetails(frame)).ToList();
                }

                if (webIdlException.InnerException is not null)
                {
                    InnerError = new ErrorDetails(webIdlException.InnerException);
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
            }
            
            if (StackTrace != null)
            {
                // If we have a stack trace then grab the debug info images, and put them into an array
                // for the outgoing payload
                Images = GetDebugInfoForStackFrames(StackTrace);
            }
        }
        
        #endregion
        
        #region Private Properties
        
        private static readonly ConcurrentDictionary<string, PEDebugDetails?> DebugInformationCache = new();
        private static Func<string, PEReader?> AssemblyReaderProvider { get; set; } = PortableExecutableReaderExtensions.GetFileSystemPEReader;
        
        #endregion

        #region Privatate Methods

        private static List<PEDebugDetails> GetDebugInfoForStackFrames(IEnumerable<StackTraceDetails> frames)
        {
            if (DebugInformationCache.IsEmpty)
            {
                return [];
            }
      
            var imageMap = DebugInformationCache.Values.Where(x => x?.Signature != null).ToDictionary(k => k!.Signature!);
            var imageSet = new HashSet<PEDebugDetails>();
      
            foreach (var stackFrame in frames)
            {
                if (stackFrame.ImageSignature != null && imageMap.TryGetValue(stackFrame.ImageSignature, out var image))
                {
                    if (image != null)
                    {
                        imageSet.Add(image);
                    }
                }
            }

            return imageSet.ToList();
        }

        private static PEDebugDetails? TryGetDebugInformation(string moduleName)
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