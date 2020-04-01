using System;

namespace ProxyLogger.Logger
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogger
    {
        bool SerializeData { get; set; }
        bool SerializeCustom { get; set; }
        bool SerializeHttp { get; set; }
        void Info(string message);

        Guid CorrelationId { get; }

        void Info(string message, params object[] args);

        void Debug(string message);

        void Debug(string message, params object[] args);

        void Warn(string message);

        void Warn(string message, params object[] args);

        void Error(string message);

        void Error(string message, params object[] args);

        void Error(Exception ex, string message);
    }
}
