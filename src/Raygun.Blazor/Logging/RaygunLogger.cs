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
        private readonly RaygunSettings _raygunSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raygunSettings"></param>
        public RaygunLogger(IOptions<RaygunSettings> raygunSettings)
        {
            _raygunSettings = raygunSettings.Value;
            Info("[RaygunLogger] Logger initialized with log level: " + _raygunSettings.LogLevel);
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
            if (_raygunSettings.LogLevel == RaygunLogLevel.None)
            {
                return;
            }

            if (level <= _raygunSettings.LogLevel)
            {
                try
                {
                    Console.WriteLine($"{RaygunPrefix} [{level}] {message}");
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}