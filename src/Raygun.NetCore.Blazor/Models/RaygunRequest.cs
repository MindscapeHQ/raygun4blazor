using System;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// Represents the root object in a Raygun error report.
    /// </summary>
    internal record RaygunRequest
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        [JsonInclude]
        public EventDetails Details { get; set; }

        /// <summary>
        /// Date and time that the error occurred in ISO-8601 format.
        /// </summary>
        [JsonInclude]
        public DateTime OccurredOn { get; set; }

        /// <summary>
        /// Tracks whether or not this request has been fully-populated and is ready to be sent.
        /// </summary>
        [JsonIgnore]
        public RequestQueueStatus QueueStatus { get; set; }

        #endregion
        
        #region Constructors

        /// <summary>
        /// The default constructor for a new <see cref="RaygunRequest" /> instance.
        /// </summary>
        public RaygunRequest()
        {
            OccurredOn = DateTime.UtcNow;
        }

        #endregion

    }

}