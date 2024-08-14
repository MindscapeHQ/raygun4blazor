using System.Threading.Tasks;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Interfaces
{

    /// <summary>
    /// Defines the contract for a user manager for Raygun error reports.
    /// </summary>
    public interface IRaygunUserProvider
    {

        /// <summary>
        /// Get the current logged user to attach to error reports.
        /// </summary>
        /// <returns>UserDetails to attach to error report</returns>
        public Task<UserDetails?> GetCurrentUser();

    }

}
