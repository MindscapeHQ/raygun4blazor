using Raygun.Blazor.Models;

namespace Raygun.Blazor.Interfaces
{

    /// <summary>
    /// Defines the contract for a user manager for Raygun error reports.
    /// </summary>
    public interface IRaygunUserManager
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public UserDetails GetCurrentUser();

    }

}
