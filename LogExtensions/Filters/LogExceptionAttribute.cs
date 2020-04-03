using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PenguinSoft.ProxyLogger.Logger;

namespace PenguinSoft.LogExtensions.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogExceptionAttribute : TypeFilterAttribute
    {
        public LogExceptionAttribute() : base(typeof(LogExceptionAttributeImpl))
        {
        }
        public class LogExceptionAttributeImpl : ExceptionFilterAttribute
        {
            private readonly ILogger _logger;
            public LogExceptionAttributeImpl(ILogger logger)
            {
                _logger = logger;
            }

                        public override void OnException(ExceptionContext context)
            {
                var prefix = "UHC.Common.HandleExceptionAttribute::";
                if (_logger != null)
                    _logger.Error(context.Exception, prefix);
                else
                    Serilog.Log.Logger.Error(context.Exception, prefix);
            }
        }
    }
}
