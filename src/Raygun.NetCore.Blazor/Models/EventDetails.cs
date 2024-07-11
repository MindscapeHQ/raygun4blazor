using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// 
    /// </summary>
    internal record EventDetails
    {

        #region Public Properties

        /// <summary>
        /// The version number of your application.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("version")]
        public string ApplicationVersion { get; set; }

        /// <summary>
        /// A trail of breadcrumbs leading up to this event.
        /// </summary>
        [JsonInclude] 
        public List<BreadcrumbDetails> Breadcrumbs { get; set; }

        /// <summary>
        /// Information about the client library you are using for talking to the Raygun API.
        /// </summary>
        [JsonInclude] 
        public ClientDetails Client { get; set; }

        /// <summary>
        /// Information about the environment at the time of the event.
        /// </summary>
        [JsonInclude] 
        public EnvironmentDetails Environment { get; set; }

        /// <summary>
        /// Information about the error associated with the event.
        /// </summary>
        [JsonInclude]
        public ErrorDetails Error { get; set; }

        /// <summary>
        /// Client defined error grouping key.
        /// </summary>
        /// <remarks>
        /// Must be 1-100 chars, ideally the result of a hash function e.g MD5
        /// </remarks>
        [JsonInclude] 
        public string GroupingKey { get; set; }

        /// <summary>
        /// The name of machine this event occurred on
        /// </summary>
        [JsonInclude] 
        public string MachineName { get; set; }

        /// <summary>
        /// Information about the HTTP request being processed when the error occurred.
        /// </summary>
        [JsonInclude] 
        public RequestDetails Request { get; set; }

        /// <summary>
        /// User-specified tags that should be applied to the event.
        /// </summary>
        /// <remarks>
        /// These will be searchable and filterable on the dashboard.
        /// </remarks>
        [JsonInclude] 
        public List<string> Tags { get; set; }

        /// <summary>
        /// Information about the user that caused the error.
        /// </summary>
        [JsonInclude]
        public UserDetails User { get; set; }

        /// <summary>
        /// User-specified custom data you would like to attach to this event.
        /// </summary>
        /// <remarks>
        /// These will be searchable on the dashboard.
        /// </remarks>
        [JsonInclude] 
        public Dictionary<string, string> UserCustomData { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="EventDetails" /> class.
        /// </summary>
        /// <param name="applicationVersion">The logical version of your application.</param>
        public EventDetails(string applicationVersion = null)
        {
            Breadcrumbs = [];
            Tags = [];
            UserCustomData = [];
            Client = new();
            ApplicationVersion = applicationVersion;
        }

        #endregion

    }

}
