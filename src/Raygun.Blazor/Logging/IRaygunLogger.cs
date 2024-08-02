namespace Raygun.Blazor.Logging
{
    /// <summary>
    /// Raygun logger interface.
    /// </summary>
    internal interface IRaygunLogger
    {
        /// <summary>
        /// Prints an error message with level Error.
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);
        /// <summary>
        /// Prints a warning message with level Warning.
        /// </summary>
        /// <param name="message"></param>
        void Warning(string message);
        /// <summary>
        /// Prints an information message with level Info.
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
        /// <summary>
        /// Prints a debug message with level Debug.
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);
        /// <summary>
        /// Prints a verbose message with level Verbose.
        /// </summary>
        /// <param name="message"></param>
        void Verbose(string message);
    }
}