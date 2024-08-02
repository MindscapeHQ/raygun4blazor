namespace Raygun.Blazor.Logging
{
    /// <summary>
    /// Hierarchy level for internal logs
    /// </summary>
    public enum RaygunLogLevel
    {
        /// <summary>
        /// Use to disable console logs
        /// </summary>
        None = 0,
        /// <summary>
        /// Highest level of error
        /// </summary>
        Error,
        /// <summary>
        /// Warning level of error
        /// </summary>
        Warning,
        /// <summary>
        /// Information level of error
        /// </summary>
        Info,
        /// <summary>
        /// Debugging level of error
        /// </summary>
        Debug,
        /// <summary>
        /// Lowest level of error
        /// </summary>
        Verbose
    }
}