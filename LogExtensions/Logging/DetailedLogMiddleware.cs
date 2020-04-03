using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PenguinSoft.CoreExtensions.Extensions;
using PenguinSoft.ProxyLogger.Logger;

namespace PenguinSoft.LogExtensions.Logging
{
    public class DetailedLogMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger _log;

        private readonly List<string> _filterUrl;

        public DetailedLogMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _filterUrl = "/swagger,/favicon".Split(',').Select(x => x).ToList();
            var filters = configuration["Logging:FilterUrls"];
            if (!string.IsNullOrWhiteSpace(filters))
                foreach (var s in filters.Split(','))
                    _filterUrl.Add(s.Trim());
        }

        public async Task Invoke(HttpContext context, ILogger logger)
        {
            _log = logger;
            using (var responseBody = new MemoryStream())
            {
                var originalBodyStream = context.Response.Body;
                try
                {
                    _log.Info(await FormatRequest(context));

                    context.Response.Body = responseBody;
                    await _next(context);

                    _log.Info(await FormatResponse(context));
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Exception caught '{e.StackTrace}'");
                    _log.Error(await FormatResponse(context));
                    throw;
                }
                finally
                {
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
        }

        private bool ShouldLog(HttpContext context)
        {
            if (_filterUrl == null)
                return true;

            return !_filterUrl.Any(x => context.Request.Path.ToString().ContainsIgnoreCase(x));
        }

        private async Task<string> FormatRequest(HttpContext context)
        {
            var request = context.Request;
            var bodyAsText = await request.ExtractRequestBody();

            if (!ShouldLog(context))
                return null;
            var r = $"Request {request.Scheme} {request.Method} {request.Host}{request.PathBase}{request.Path}";
            if (_log.SerializeHttp)
                r += $" {request.ContentType} QueryString='{request.QueryString}' " +
                     $"Body='{bodyAsText}'";
            return r;
        }

        private async Task<string> FormatResponse(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var text = string.Empty;
            if (response.Body.CanSeek)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                text = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
            }

            if (!ShouldLog(context))
                return null;
            var r = $"Response {response.StatusCode} {request.Method} {request.Host}{request.PathBase}{request.Path} {response.ContentType}";
            if (_log.SerializeHttp)
                r += $" Body='{text}'";
            return r;
        }
    }
}
