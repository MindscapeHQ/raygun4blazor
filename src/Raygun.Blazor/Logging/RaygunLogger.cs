using System;

namespace Raygun.Blazor.Logging
{
    /// <summary>
    /// Implementation of the Raygun logger.
    /// </summary>
    internal class RaygunLogger : IRaygunLogger
    {
        private readonly RaygunLogLevel _logLevel;
        private static RaygunLogger? _raygunLogger;

        /// <summary>
        /// Create or retrieve instance of IRaygunLogger.
        /// Returns null if the logLevel is None.
        /// </summary>
        internal static IRaygunLogger? Create(RaygunLogLevel logLevel)
        {
            _raygunLogger = logLevel == RaygunLogLevel.None ? null : _raygunLogger ?? new RaygunLogger(logLevel);
            return _raygunLogger;
        }

        private RaygunLogger(RaygunLogLevel logLevel)
        {
            _logLevel = logLevel;
            Warning("[RaygunLogger] Internal logger initialized with log level: " + _logLevel);
            Warning("[RaygunLogger] Disable internal logger by setting LogLevel to None in Raygun settings");
        }

        private const string RaygunPrefix = "Raygun:";

        /// <inheritdoc />
        public void Error(string message)
        {
            Log(RaygunLogLevel.Error, message);
        }

        /// <inheritdoc />
        public void Warning(string message)
        {
            Log(RaygunLogLevel.Warning, message);
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            Log(RaygunLogLevel.Info, message);
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            Log(RaygunLogLevel.Debug, message);
        }

        /// <inheritdoc />
        public void Verbose(string message)
        {
            Log(RaygunLogLevel.Verbose, message);
        }

        private void Log(RaygunLogLevel level, string message)
        {
            if (_logLevel == RaygunLogLevel.None)
            {
                return;
            }

            if (level > _logLevel) return;

            try
            {
                // Only log the first letter of the log level e.g. "I" for Info
                var initial = level.ToString()[0..1].ToUpper();
                Console.WriteLine($"{RaygunPrefix} [{initial}] {message}");
            }
            catch
            {
                // ignored
            }
        }
    }
}