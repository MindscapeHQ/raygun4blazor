using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class UserDetails
    {

        #region Public Properties

        /// <summary>
        /// Unique Identifier for this user. 
        /// </summary>
        /// <remarks>
        /// Set this to the identifier you use internally to look up users, or a correlation id for anonymous users 
        /// if you have one. It doesn't have to be unique, but we will treat any duplicated values as the same user. 
        /// If you use the user's email address as the identifier, enter it here as well as the Email field.
        /// </remarks>
        [JsonPropertyName("identifier")]
        public string UserId { get; set; }

        /// <summary>
        /// Flag indicating whether or not a user is anonymous.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's full name.
        /// </summary>
        /// <remarks>
        /// If you are going to set any names, you should probably set this one too.
        /// </remarks>
        public string FullName { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The unique identifier for the device.
        /// </summary>
        /// <remarks>
        /// Could be used to identify users across devices, or machines that are breaking for many users.
        /// </remarks>
        [JsonPropertyName("uuid")]
        public string DeviceId { get; set; }

        #endregion

    }

}
