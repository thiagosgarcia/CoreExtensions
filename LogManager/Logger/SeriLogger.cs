using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProxyLogger.Helpers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ILogger = ProxyLogger.Logger.ILogger;

namespace ProxyLogger.Logger
{
    /// <summary>
    /// SeriLogger logging implementation
    /// </summary>
    public class SeriLogger : ILogger, IDisposable
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private const string MessageFormat = "{0} - {1}";
        private readonly bool _isEnabled = false;

        public bool SerializeData { get; set; }
        public bool SerializeCustom { get; set; }
        public bool SerializeHttp { get; set; }

        public static LoggingLevelSwitch LoggingLevel { get; set; } = new LoggingLevelSwitch();
        public Guid CorrelationId => this.GetCorrelationId(_contextAccessor);

        public SeriLogger(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configuration)
                .MinimumLevel.ControlledBy(LoggingLevel)
                .CreateLogger();

            _isEnabled = Convert.ToBoolean(configuration["Logging:Enabled"] ?? "false");
            SerializeData = Convert.ToBoolean(configuration["Logging:SerializeData"] ?? "false");
            SerializeCustom = Convert.ToBoolean(configuration["Logging:SerializeCustom"] ?? "false");
            SerializeHttp = Convert.ToBoolean(configuration["Logging:SerializeHttp"] ?? "false");

            var configuredLevel = configuration["Logging:Level"] ?? "Information";
            if (Enum.TryParse(configuredLevel, out LogEventLevel level))
                LoggingLevel.MinimumLevel = level;
        }

        public void Debug(string message)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Debug(logMessage);
        }

        public void Debug(string message, params object[] args)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Debug(logMessage, args);
        }

        public void Error(string message)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Error(logMessage);
        }

        public void Error(string message, params object[] args)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Error(logMessage, args);
        }

        public void Error(Exception ex, string message)
        {
            if (!_isEnabled) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Error(ex, logMessage);
        }

        public void Info(string message)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Information(logMessage);
        }

        public void Info(string message, params object[] args)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Information(logMessage, args);
        }

        public void Warn(string message)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Warning(logMessage);
        }

        public void Warn(string message, params object[] args)
        {
            if (!_isEnabled || string.IsNullOrWhiteSpace(message)) return;
            var logMessage = string.Format(MessageFormat, CorrelationId != default(Guid) ? CorrelationId : Guid.NewGuid(), message);
            Log.Logger.Warning(logMessage, args);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Log.Logger != null && Log.Logger is IDisposable)
                {
                    (Log.Logger as IDisposable)?.Dispose();
                }
            }
        }
    }
}
