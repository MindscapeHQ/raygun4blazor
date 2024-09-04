using System;
using Raygun.Blazor.Logging;
using Raygun.Blazor.Queue;

namespace Raygun.Blazor
{
    /// <summary>
    /// Represents the different configuration options for the Raygun client.
    /// </summary>
    public class RaygunSettings
    {
        #region Private Members

        /// <summary>
        /// Represents the endpoint for the Raygun API that is used to send crash reports.
        /// </summary>
        public const string EntriesEndpoint = "/entries";

        #endregion

        #region Public Properties

        /// <summary>
        /// The API Key assigned by Raygun for this specific application
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// The logical version of the application using this Raygun Provider.
        /// </summary>
        /// <remarks>
        /// Setting this value takes priority over the AssemblyVersion in the RaygunBlazorClient.
        /// </remarks>
        public string? ApplicationVersion { get; set; }

        /// <summary>
        /// Specifies whether or not unhandled exceptions should be caught and sent to Raygun. Defaults to true
        /// </summary>
        public bool CatchUnhandledExceptions { get; set; } = true;

        /// <summary>
        /// The endpoint for the Raygun API. Defaults to "https://api.raygun.com".
        /// </summary>
        public string Endpoint { get; set; } = "https://api.raygun.com";

        /// <summary>
        /// Specifies whether or not the RaygunClient should throw exceptions if there are problems. Defaults to true.
        /// </summary>
        public bool HideRaygunClientExceptions { get; set; } = true;

        /// <summary>
        /// The human-readable name of this Application.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Internal logging level for the Raygun client. Defaults to Warning.
        /// </summary>
        public RaygunLogLevel LogLevel { get; set; } = RaygunLogLevel.Warning;

        /// <summary>
        /// The maximum queue size for background exceptions
        /// </summary>
        public int BackgroundMessageQueueMax { get; } = ushort.MaxValue;

        /// <summary>
        /// Controls the maximum number of background threads used to process the raygun message queue
        /// </summary>
        /// <remarks>
        /// Defaults to Environment.ProcessorCount * 2 &gt;= 8 ? 8 : Environment.ProcessorCount * 2
        /// </remarks>
        public int BackgroundMessageWorkerCount { get; set; } =
            Environment.ProcessorCount * 2 >= ThrottledBackgroundMessageProcessor.MaxWorkerCountDefault
                ? ThrottledBackgroundMessageProcessor.MaxWorkerCountDefault
                : Environment.ProcessorCount * 2;

        /// <summary>
        /// Used to determine how many messages are in the queue before the background processor will add another worker to help process the queue.
        /// </summary>
        /// <remarks>
        /// Defaults to 25, workers will be added for every 25 messages in the queue, until the BackgroundMessageWorkerCount is reached.
        /// </remarks>
        public int BackgroundMessageWorkerBreakpoint { get; set; } =
            ThrottledBackgroundMessageProcessor.WorkerQueueBreakpointDefaultValue;

        /// <summary>
        /// Specifies the use of a background queue for sending messages to Raygun.
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        public bool UseBackgroundQueue { get; set; } = false;

        /// <summary>
        /// Specifies the use of offline storage for messages that failed to be sent to Raygun.
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        public bool UseOfflineStore { get; set; } = false;

        /// <summary>
        /// Specifies the directory name to use for offline storage.
        /// </summary>
        /// <remarks>
        /// If not set, it will default to the application name, otherwise it will be randomly generated.
        /// </remarks>
        public string? DirectoryName { get; set; }

        /// <summary>
        /// Specifies the maximum number of offline files to store.
        /// </summary>
        /// <remarks>
        /// Defaults to 50 if not set.
        /// </remarks>
        public int? MaxOfflineFiles { get; set; }

        #endregion
    }
}