namespace Raygun.NetCore.Blazor
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
        public string ApiKey { get; set; }

        /// <summary>
        /// The logical version of the application using this Raygun Provider.
        /// </summary>
        /// <remarks>
        /// Setting this value takes priority over the AssemblyVersion in the RaygunBlazorClient.
        /// </remarks>
        public string ApplicationVersion { get; set; }

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
        public string Name { get; set; }

        #endregion

    }

}
