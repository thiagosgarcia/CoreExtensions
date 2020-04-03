using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PenguinSoft.ProxyLogger.Helpers;
using Serilog.Events;

namespace PenguinSoft.ProxyLogger.Logger
{

    public class DispatchLoggingProxy<T> : DispatchProxy, ILogProxy<T>
    {
        public event EventHandler<DispatchBeforeExecutionArgs> BeforeExecute;
        public event EventHandler<DispatchAfterExecutionArgs> AfterExecute;
        public event EventHandler<DispatchExceptionArgs> ErrorExecuting;

        private void ConfigureSerializationFilters(IConfiguration configuration)
        {
            var serializationFilterConfig = configuration["Logging:SerializationFilterOverride"];
            BlockedNames = BlockedNames ?? new List<string>();

            Filter = m =>
            {
                foreach (var name in BlockedNames)
                {
                    if (m?.DeclaringType?.Name?.Contains(name) ?? false)
                        return false;
                    if (m?.DeclaringType?.Namespace?.Contains(name) ?? false)
                        return false;
                    if (m?.Name?.Contains(name) ?? false)
                        return false;
                }

                return true;
            };

            if (string.IsNullOrWhiteSpace(serializationFilterConfig))
                return;

            foreach (var name in serializationFilterConfig.Split(','))
                if (!BlockedNames.Contains(name.Trim()))
                    BlockedNames.Add(name.Trim());
        }

        // In future, more handlers can be added and removed via injection or configurations in order to do specific logging logic
        public void ConfigureHandlers()
        {
            BeforeExecute += BeforeHandler();
            AfterExecute += AfterHandler();
            ErrorExecuting += ErrorHandler();
        }

        private EventHandler<DispatchBeforeExecutionArgs> BeforeHandler() =>
            (sender, beforeExecutionArgs) =>
            {
                var className =
                    (SerializeData ? beforeExecutionArgs.MethodInfo.DeclaringType?.ToString() : beforeExecutionArgs.MethodInfo?.DeclaringType?.Name)
                    ?? string.Empty;

                Log(LogEventLevel.Information, "Executing method '{0}'",
                    $"{className}::{beforeExecutionArgs.MethodInfo?.Name}",
                    GetBeforeExecutionArgs(beforeExecutionArgs)
                );
            };

        private EventHandler<DispatchAfterExecutionArgs> AfterHandler() =>
            (sender, afterExecutionArgs) =>
            {
                var className =
                    (SerializeData ? afterExecutionArgs.MethodInfo?.DeclaringType?.ToString() :
                        afterExecutionArgs.MethodInfo?.DeclaringType?.Name)
                    ?? string.Empty;

                Log(LogEventLevel.Information, "Method executed '{0}'",
                    $"{className}::{afterExecutionArgs?.MethodInfo?.Name}",
                    GetAfterExecutionReturnValue(afterExecutionArgs)
                );
            };

        private EventHandler<DispatchExceptionArgs> ErrorHandler() =>
            (sender, exceptionArgs) =>
            {
                var className =
                    (SerializeData ? exceptionArgs.MethodInfo?.DeclaringType?.ToString() :
                        exceptionArgs.MethodInfo?.DeclaringType?.Name)
                    ?? string.Empty;

                Log(LogEventLevel.Error, "Exception caught '{0}'",
                    $"{className}::{exceptionArgs?.MethodInfo?.Name}",
                    GetErrorExecutingReturnValue(exceptionArgs)
                );
            };
        private void OnBeforeExecute(MethodInfo methodInfo, object[] args)
        {
            if (BeforeExecute != null)
                if (PredicateFilter(methodInfo))
                    BeforeExecute(this, new DispatchBeforeExecutionArgs(methodInfo, args));
        }
        private void OnAfterExecute(MethodInfo methodInfo, object result)
        {
            if (AfterExecute != null)
                if (PredicateFilter(methodInfo))
                    AfterExecute(this, new DispatchAfterExecutionArgs(methodInfo, result));
        }
        private void OnErrorExecuting(MethodInfo methodInfo, Exception ex)
        {
            if (ErrorExecuting != null)
                if (PredicateFilter(methodInfo))
                    ErrorExecuting(this, new DispatchExceptionArgs(methodInfo, ex));
        }

        private string GetErrorExecutingReturnValue(DispatchExceptionArgs exceptionArgs)
        {
            if (!SerializeData)
                return null;

            return JsonConvert.SerializeObject(exceptionArgs.Exception, LogHelper.JsonSettings);
        }

        private string GetAfterExecutionReturnValue(DispatchAfterExecutionArgs afterExecutionArgs)
        {
            if (!SerializeData)
                return null;

            var returnValue = afterExecutionArgs.ReturnValue;
            return JsonConvert.SerializeObject(FilterValueTypes(returnValue), LogHelper.JsonSettings);
        }

        private IEnumerable<string> GetBeforeExecutionArgs(DispatchBeforeExecutionArgs beforeExecutionArgs)
        {
            if (!SerializeData)
                return null;

            var r = new List<object>();
            if (beforeExecutionArgs.Args.Length == beforeExecutionArgs.MethodInfo.GetParameters().Length)
                for (var i = 0; i < beforeExecutionArgs.Args.Length; i++)
                    r.Add(JsonConvert.SerializeObject(FilterValueTypes(beforeExecutionArgs.Args[i], beforeExecutionArgs.MethodInfo.GetParameters().ElementAt(i).ToString()), LogHelper.JsonSettings));

            return r.Select(x => x.ToString());
        }

        private object FilterValueTypes(object value, string name = null)
        {
            if (!string.IsNullOrWhiteSpace(name))
                if (BlockedNames.Any(x => name.ToLower().Contains(x.ToLower())))
                    return null;

            //TODO Filter value types that doesn't make sense to serialize
            if (value == null
                || value is Stream
                || value is Task<Stream>
                || value is byte[]
                || value is Task<byte[]>
                || value is Task) //TODO  Improve this filter
                return null;

            if (!string.IsNullOrWhiteSpace(name))
                return new { paramName = name, value };

            return new { returnValue = value };
        }

        public Predicate<MethodInfo> Filter
        {
            get => PredicateFilter;
            set => PredicateFilter = value ?? (m => true); //TODO Filter by params names and method name
        }

        protected override object Invoke(MethodInfo methodInfo, object[] args)
        {
            try
            {
                OnBeforeExecute(methodInfo, args);
                var result = methodInfo.Invoke(Decorated, args);
                if (result != null)
                {
                    var type = result.GetType();
                    if ((type.IsAssignableFrom(typeof(Task))) ||
                        (type.IsGenericType &&
                         type.GetGenericTypeDefinition() == typeof(Task<>)))
                    {
                        ((Task)result).ContinueWith(x =>
                        {
                            if (x.IsFaulted)
                                OnErrorExecuting(methodInfo, x.Exception);
                            if (x.IsCanceled)
                                OnAfterExecute(methodInfo, "Task has been canceled");
                            if (x.IsCompleted)
                                OnAfterExecute(methodInfo, (result as dynamic)?.Result ?? result);
                        });
                        return result;
                    }
                }
                OnAfterExecute(methodInfo, result);
                return result;
            }
            catch (Exception ex)
            {
                var theException = ex.InnerException ?? ex;
                OnErrorExecuting(methodInfo, theException);
                throw theException;
            }
        }

        public void Log(LogEventLevel type, string msg, object arg, object returnValue)
        {
            Log(type, msg, arg, new[] { returnValue });
        }
        public void Log(LogEventLevel type, string msg, object arg, IEnumerable<object> parameters = null)
        {
            var args = parameters == null ? arg : new[] { arg }.Concat(parameters);
            switch (type)
            {
                case LogEventLevel.Debug:
                    Logger.Debug(msg, args);
                    break;
                case LogEventLevel.Warning:
                    Logger.Warn(msg, args);
                    break;
                case LogEventLevel.Error:
                    if (arg.GetType() == typeof(Exception))
                        Logger.Error(msg, (Exception)arg);
                    else
                        Logger.Error(msg, args);

                    break;
                default:
                    Logger.Info(msg, args);
                    break;
            }
        }

        public static T Create(T decorated, IServiceProvider services)
        {
            object proxy = Create<T, DispatchLoggingProxy<T>>();
            ((DispatchLoggingProxy<T>)proxy).SetParameters(decorated,
                (ILogger)services.GetService(typeof(ILogger)),
                (IConfiguration)services.GetService(typeof(IConfiguration)));

            return (T)proxy;
        }

        private void SetParameters(T decorated, ILogger logger, IConfiguration configuration)
        {
            if (decorated == null)
                throw new ArgumentNullException(nameof(decorated));

            Decorated = decorated;
            Logger = logger;
            IsEnabled = Convert.ToBoolean(configuration["Logging:Enabled"] ?? "false");
            SerializeData = Convert.ToBoolean(configuration["Logging:SerializeData"] ?? "false"); // We can turn off data serialization in appSettings

            if (!IsEnabled)
                return;

            ConfigureSerializationFilters(configuration);
            ConfigureHandlers();
        }

        public T Decorated { get; set; }
        public ILogger Logger { get; set; }
        public bool SerializeData { get; set; }
        public List<string> BlockedNames { get; set; }
        public bool IsEnabled { get; set; }
        public Predicate<MethodInfo> PredicateFilter { get; set; }
    }

}
