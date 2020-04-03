using System;
using System.Collections.Generic;
using System.Net;

namespace PenguinSoft.CoreExtensions.ExceptionHandlers
{
    public class AuthorizationException : Exception
    {
        public string Login { get; }
        public string RequestPath { get; }
        public Dictionary<string, string> StringArguments { get; }
        public string Status { get; }

        public AuthorizationException(string login, string requestPath, string businessValue, string controllerName)
        {
            Login = login;
            RequestPath = requestPath;
            StringArguments = new Dictionary<string, string>()
            {
                {nameof(businessValue), businessValue},
                {nameof(controllerName), controllerName }
            };
            Status = HttpStatusCode.Forbidden.ToString();
        }
        public AuthorizationException(string login, string requestPath, Dictionary<string, string> stringArguments, HttpStatusCode statusCode)
        {
            Login = login;
            RequestPath = requestPath;
            StringArguments = stringArguments;
            Status = statusCode.ToString();
        }
    }
}
