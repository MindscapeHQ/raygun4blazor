using Raygun.Blazor.Models;
using System.ComponentModel;

namespace Raygun.Blazor.Events
{
    /// <summary>
    /// Can be used to modify the request before sending, or to cancel the send operation.
    /// </summary>
    public class RaygunRequestSendEventArgs : CancelEventArgs
    {

        /// <summary>
        /// Creates a new instance of the <see cref="RaygunRequestSendEventArgs" /> class.
        /// </summary>
        /// <param name="request"></param>
        public RaygunRequestSendEventArgs(RaygunRequest request)
        {
            Request = request;
        }

        /// <summary>
        /// RaygunRequest object that is about to be sent.
        /// </summary>
        public RaygunRequest Request { get; private set; }
    }
}
