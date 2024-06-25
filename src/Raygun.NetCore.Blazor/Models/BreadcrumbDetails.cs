using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// A manually-reported event that happened during the execution of your application.
    /// </summary>
    /// <remarks>
    /// Breadcrumbs are used to help understand how your app was being used before an error happened. They are queued up by the
    /// <see cref="RaygunBlazorClient" /> and sent along with the next error report.
    /// </remarks>
    internal record BreadcrumbDetails
    {

        #region Public Properties

        /// <summary>
        /// A custom value used to arbitrarily group this Breadcrumb.
        /// </summary>
        [JsonInclude] 
        public string Category { get; set; }

        /// <summary>
        /// If relevant, a class name from where the breadcrumb was recorded.
        /// </summary>
        [JsonInclude]
        public string ClassName { get; internal set; }

        /// <summary>
        /// Any custom data you want to record about application state when the Breadcrumb was recorded.
        /// </summary>     
        [JsonInclude] 
        public Dictionary<string, object> CustomData { get; set; }

        /// <summary>
        /// If relevant, a line number from where the breadcrumb was recorded.
        /// </summary>
        [JsonInclude]
        public int LineNumber { get; internal set; }

        /// <summary>
        /// The message you want to record for this Breadcrumb.
        /// </summary>
        [JsonInclude] 
        public string Message { get; set; }

        /// <summary>
        /// If relevant, a method name from where the Breadcrumb was recorded.
        /// </summary>
        [JsonInclude]
        public string MethodName { get; internal set; }

        /// <summary>
        /// Specifies the platform that the breadcrumb was recorded on. Possible values are "DotNet", and "JavaScript.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Milliseconds since the Unix Epoch.
        /// </summary>
        [JsonPropertyName("timeStamp")]
        [JsonInclude]
        public long Timestamp { get; internal set; }

        /// <summary>
        /// The <see cref="BreadcrumbType"> for the message. Defaults to <see cref="BreadcrumbType.Manual"/>.
        /// </summary>
        [JsonInclude] 
        public BreadcrumbType Type { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="BreadcrumbDetails" /> class.
        /// </summary>
        /// <remarks>
        /// This is an optimized code path for JSON deserialization and should not be used directly.
        /// </remarks>
        [JsonConstructor]
        private BreadcrumbDetails()
        {
            CustomData = [];
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BreadcrumbDetails" /> class.
        /// </summary>
        /// <param name="message">The message you want to record for this breadcrumb.</param>
        /// <param name="type">The <see cref="BreadcrumbType"> for the message. Defaults to <see cref="BreadcrumbType.Manual"/>.</param>
        /// <param name="category">A custom value used to arbitrarily group this Breadcrumb.</param>
        /// <param name="customData">Any custom data you want to record about application state when the breadcrumb was recorded.</param>
        /// <param name="platform">Specifies the platform that the breadcrumb was recorded on. Possible values are "DotNet", and "JavaScript.</param>
        public BreadcrumbDetails(string message, BreadcrumbType type = BreadcrumbType.Manual, string category = null, Dictionary<string, object> customData = null, string platform = "DotNet")
        {
            Message = message;
            Type = type;
            Platform = platform;
            Category = category;
            CustomData = customData;

            Timestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
            var stackFrame = EnhancedStackTrace.Current().GetExternalFrames().FirstOrDefault();
            if (stackFrame is null) return;

            var names = stackFrame.GetBlazorNames();
            ClassName = names.ClassName;
            MethodName = names.MethodName;
            LineNumber = stackFrame.GetFileLineNumber();
        }

        #endregion

    }

}