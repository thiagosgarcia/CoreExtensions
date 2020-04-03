using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PenguinSoft.ProxyLogger.Logger;

namespace PenguinSoft.ProxyLogger.Helpers
{
    public static class LogHelper
    {
        private static readonly object _locker = new object();
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Error = (sender, args) =>
            {
                //supress
                args.ErrorContext.Handled = true;
            }
        };
        public static void CustomLogTransaction(ILogger logger, string logText, string context)
        {
            if (!ShouldLog(logger, logText))
                return;

            logger.Info($"{context} {logText.Trim()}");
        }

        private static bool ShouldLog(ILogger logger, string dbLog)
        {
            dbLog = dbLog.Trim();

            if (string.IsNullOrEmpty(dbLog))
                return false;

            return logger.SerializeCustom || false; // TODO filters to match what can be logged here
        }

        public static Guid GetCorrelationId(this ILogger logger, IHttpContextAccessor contextAccessor)
        {
            lock (_locker)
            {
                var req = contextAccessor?.HttpContext;
                var containsHeader = req?.Request?.Headers?.ContainsKey("CorrelationId");

                if (containsHeader.HasValue && containsHeader.Value)
                {
                    var headerValue = req.Request.Headers["CorrelationId"];

                    if (Guid.TryParse(headerValue, out var parsedGuid))
                        return parsedGuid;
                }

                var headerCorrId = Guid.NewGuid();
                req?.Request?.Headers?.Add("CorrelationId", Guid.NewGuid().ToString());
                return headerCorrId;
            }

        }
    }
}
