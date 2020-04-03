using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog.Events;

namespace PenguinSoft.ProxyLogger.Logger
{

    public interface ILogProxy<T>
    {
        T Decorated { get; set; }
        ILogger Logger { get; set; }
        bool SerializeData { get; set; }

        //Just a initial names to do not serialize at any times. More can be set in appsettings.json
        List<string> BlockedNames { get; set; }
        bool IsEnabled { get; set; }

        Predicate<MethodInfo> PredicateFilter { get; set; }
        void ConfigureHandlers();
        void Log(LogEventLevel type, string msg, object arg, object returnValue);
        void Log(LogEventLevel type, string msg, object arg, IEnumerable<object> parameters = null);
    }
}
