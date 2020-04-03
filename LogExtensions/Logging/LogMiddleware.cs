using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PenguinSoft.CoreExtensions.Extensions;
using PenguinSoft.ProxyLogger.Logger;

namespace PenguinSoft.LogExtensions.Logging
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger _log;

        private readonly List<string> _filterUrl;

        public LogMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _filterUrl = "/swagger,/favicon".Split(',').Select(x => x).ToList();
            var filters = configuration["Logging:FilterUrls"];
            if (!string.IsNullOrWhiteSpace(filters))
            {
                foreach (var s in filters.Split(','))
                {
                    _filterUrl.Add(s.Trim());
                }
            }
        }

        public async Task Invoke(HttpContext context, ILogger logger)
        {
            _log = logger;
            try
            {
                _log.Info(FormatRequest(context));
                await _next(context);
                _log.Info(FormatResponse(context));
            }
            catch (Exception e)
            {
                _log.Error(e, $"Exception caught '{e.StackTrace}'");
                _log.Error(FormatResponse(context));
                throw;
            }
        }

        private bool ShouldLog(HttpContext context)
        {
            if (_filterUrl == null)
                return true;

            return !_filterUrl.Any(x => context.Request.Path.ToString().ContainsIgnoreCase(x));
        }

        private string FormatRequest(HttpContext context)
        {
            if (!ShouldLog(context))
                return null;

            var request = context.Request;
            return $"Request {request.Scheme} {request.Method} {request.Host}{request.PathBase}{request.Path}";
        }

        private string FormatResponse(HttpContext context)
        {
            if (!ShouldLog(context))
                return null;

            var request = context.Request;
            var response = context.Response;
            return $"Response {response.StatusCode} {request.Method} {request.Host}{request.PathBase}{request.Path} {response.ContentType}";
        }
    }
}
