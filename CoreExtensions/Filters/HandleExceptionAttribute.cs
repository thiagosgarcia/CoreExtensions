using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PenguinSoft.CoreExtensions.ExceptionHandlers;

namespace PenguinSoft.CoreExtensions.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HandleExceptionAttribute : TypeFilterAttribute
    {
        public HandleExceptionAttribute() : base(typeof(HandleExceptionAttributeImpl))
        {
        }
        public HandleExceptionAttribute(Type t) : base(t)
        {
        }

        public class HandleExceptionAttributeImpl : ExceptionFilterAttribute
        {
            protected bool SupressExceptionResponse;

            public HandleExceptionAttributeImpl(IConfiguration config)
            {
                SupressExceptionResponse = bool.Parse(config["SupressExceptionResponse"] ?? "false");
            }
            
            public override void OnException(ExceptionContext context)
            {
                context.Result = GenerateErrorResult(context);
            }

            private ObjectResult GenerateErrorResult(ExceptionContext context)
            {
                if (context.Exception is AuthorizationException exp)
                    return GenerateAuthExceptionResult(exp);

                if (context.Exception is FaultException)
                    return FaultExceptionResult(context.Exception);

                return GenerateExceptionResult(ExtractStatusCode(context.Exception), context.Exception);
            }

            private int ExtractStatusCode(Exception contextException)
            {
                if (contextException is UnauthorizedException)
                    return (int)HttpStatusCode.Unauthorized;

                if (contextException is AuthorizationException)
                    return (int)HttpStatusCode.Forbidden;

                if (contextException is AggregateException)
                    return (int)HttpStatusCode.InternalServerError;

                if (contextException is HttpRequestException ||
                    contextException is FaultException ||
                    contextException is ArgumentOutOfRangeException)
                    return (int)HttpStatusCode.NotFound;

                if (contextException is ValidationException)
                    return (int)HttpStatusCode.NotAcceptable;

                return (int)HttpStatusCode.NotFound;
            }

            private ObjectResult FaultExceptionResult(Exception contextException)
            {
                return GenerateExceptionResult((int)HttpStatusCode.NotAcceptable, ((dynamic)contextException).Detail ?? contextException);
            }

            protected ObjectResult GenerateExceptionResult(int statusCode, object value)
            {
                var result = new ObjectResult(null)
                {
                    StatusCode = statusCode,
                    Value = value
                };
                return GenerateExceptionResult(result);
            }
            protected ObjectResult GenerateAuthExceptionResult(AuthorizationException exception)
            {
                var result = new ObjectResult(null)
                {
                    StatusCode = ExtractStatusCode(exception),
                    Value = new
                    {
                        exception.Login,
                        exception.RequestPath,
                        exception.StringArguments,
                        exception.Status
                    }
                };
                return GenerateExceptionResult(result);
            }
            protected ObjectResult GenerateExceptionResult(ObjectResult result)
            {
                if (SupressExceptionResponse &&
                        IsException(result.Value))
                    result.Value = null;

                return result;
            }

            private bool IsException(object value)
            {
                return value.GetType().IsAssignableFrom(typeof(Exception)) ||
                       value.GetType().IsAssignableFrom(typeof(AggregateException));
            }
        }
    }
}
