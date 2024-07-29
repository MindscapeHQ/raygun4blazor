using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace Raygun.Blazor.Logging
{
    /// <summary>
    /// Implementation of the Raygun logger.
    /// </summary>
    public class RaygunLogger : IRaygunLogger
    {
        private readonly RaygunLogLevel _logLevel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raygunSettings"></param>
        public RaygunLogger(IOptions<RaygunSettings> raygunSettings)
        {
            _logLevel = raygunSettings.Value.LogLevel;
            Info("[RaygunLogger] Logger initialized with log level: " + _logLevel);
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
                var initial = level.ToString().Substring(0, 1).ToUpper();
                Console.WriteLine($"{RaygunPrefix} [{initial}] {message}");
            }
            catch
            {
                // ignored
            }
        }
    }
}