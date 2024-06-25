namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// 
    /// </summary>
    internal enum RequestQueueStatus
    {

        /// <summary>
        /// 
        /// </summary>
        NotQueued = 0,

        /// <summary>
        /// 
        /// </summary>
        Queued = 1,

        /// <summary>
        /// 
        /// </summary>
        Sending = 2,

        /// <summary>
        /// 
        /// </summary>
        Retrying = 90,

        /// <summary>
        /// 
        /// </summary>
        Failed = 99,

        /// <summary>
        /// 
        /// </summary>
        Completed = 100,

    }

}
