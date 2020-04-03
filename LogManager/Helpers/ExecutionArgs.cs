using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace PenguinSoft.ProxyLogger.Helpers
{
    public class ExceptionArgs : EventArgs
    {
        public ExceptionArgs(IMethodCallMessage methodCall, Exception exception)
        {
            MethodCall = methodCall;
            Exception = exception;
        }

        public IMethodCallMessage MethodCall { get; set; }
        public Exception Exception { get; set; }
    }
    public class AfterExecutionArgs : EventArgs
    {
        public AfterExecutionArgs(IMethodCallMessage methodCall, object returnValue, object[] outArgs)
        {
            MethodCall = methodCall;
            ReturnValue = returnValue;
            OutArgs = outArgs;
        }

        public IMethodCallMessage MethodCall { get; set; }
        public object ReturnValue { get; set; }
        public object[] OutArgs { get; set; }
    }

    public class DispatchExceptionArgs : EventArgs
    {
        public DispatchExceptionArgs(MethodInfo methodInfo, Exception exception)
        {
            MethodInfo = methodInfo;
            Exception = exception;
        }

        public MethodInfo MethodInfo { get; set; }
        public Exception Exception { get; set; }
    }
    public class DispatchAfterExecutionArgs : EventArgs
    {
        public DispatchAfterExecutionArgs(MethodInfo methodInfo, object returnValue)
        {
            MethodInfo = methodInfo;
            ReturnValue = returnValue;
        }

        public MethodInfo MethodInfo { get; set; }
        public object ReturnValue { get; set; }
    }

    public class DispatchBeforeExecutionArgs : EventArgs
    {
        public MethodInfo MethodInfo { get; }
        public object[] Args { get; }

        public DispatchBeforeExecutionArgs(MethodInfo methodInfo, object[] args)
        {
            MethodInfo = methodInfo;
            Args = args;
        }
    }



}
