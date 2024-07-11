using System.Reflection;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// Specifies the details of the client library you are using for talking to the Raygun API.
    /// </summary>
    internal record ClientDetails
    {

        #region Public Properties

        /// <summary>
        /// The name of this Client library.
        /// </summary>
        [JsonInclude] 
        public string Name { get; set; }

        /// <summary>
        /// The version of this Client library.
        /// </summary>
        [JsonInclude] 
        public string Version { get; set; }

        /// <summary>
        /// The URL for the repository this Client library is maintained in.
        /// </summary>
        [JsonInclude] 
        public string ClientUrl { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ClientDetails" /> class.
        /// </summary>
        /// <remarks>
        /// This is an optimized code path for JSON deserialization and should not be used directly.
        /// </remarks>
        [JsonConstructor]
        private ClientDetails()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ClientDetails" /> <see langword="record"/>. 
        /// </summary>
        /// <param name="name">The name for this Client. Defaults to "Raygun4Blazor".</param>
        /// <remarks>
        /// Sets the Name to "Raygun4Blazor", the Version to the current version of the assembly, and the ClientUrl to the GitHub repository for this project.
        /// </remarks>
        internal ClientDetails(string name = "Raygun4Blazor")
        {
            Name = name;
            Version = Assembly.GetExecutingAssembly()
                              .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                              ?.InformationalVersion ?? "1.0.0";
            ClientUrl = "https://github.com/MindscapeHQ/raygun4blazor";
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ClientDetails" /> <see langword="record"/>.
        /// </summary>
        /// <param name="name">The name of this Client library.</param>
        /// <param name="version">The version of this Client library.</param>
        /// <param name="clientUrl">The URL for the repository this Client library is maintained in.</param>
        internal ClientDetails(string name, string version, string clientUrl)
        {
            Name = name;
            Version = version;
            ClientUrl = clientUrl;
        }

        #endregion

    }

}