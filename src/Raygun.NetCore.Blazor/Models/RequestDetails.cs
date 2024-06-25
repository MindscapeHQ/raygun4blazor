using Raygun.NetCore.Blazor.Converters;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// 
    /// </summary>
    internal record RequestDetails
    {

        #region Public Properties

        /// <summary>
        /// The form parameters sent through with the request. Not form encoded.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, object> Form { get; set; }

        /// <summary>
        /// The HTTP Headers sent as part of the request.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The hostName portion of the URL being requested.
        /// </summary>
        [JsonInclude]
        public string HostName { get; set; }

        /// <summary>
        /// The HTTP method used to request the URL (GET, POST, PUT, etc).
        /// </summary>
        [JsonInclude]
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// The IP address of the client that initiated the request.
        /// </summary>
        [JsonPropertyName("iPAddress")]
        [JsonInclude]
        public string IPAddress { get; set; }

        /// <summary>
        /// The query string portion of the URL.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, string> QueryString { get; set; }

        /// <summary>
        /// The raw request body. Don't include form values here.
        /// </summary>
        [JsonInclude]
        public string RawData { get; set; }

        /// <summary>
        /// The path portion of the URL being requested
        /// </summary>
        [JsonInclude]
        public string Url { get; set; }

        #endregion

    }

}
