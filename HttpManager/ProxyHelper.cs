using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace HttpManager
{
    public static class ProxyHelper
    {
        public static int GetClientTimeout(IConfiguration config)
        {
            var clientTimeout = int.Parse(config["HttpClientTimeout"] ?? "1");
            return clientTimeout < 1 ? 1 : clientTimeout > 20 ? 20 : clientTimeout;
        }

        public static HttpClientHandler CreateSimpleHttpClientHandler(IConfiguration config)
        {
            var allowAutoRedirect = bool.Parse(config["HttpManager:AllowAutoRedirect"] ?? "false");
            var useCookies = bool.Parse(config["HttpManager:UseCookies"] ?? "false");
            var ignoreCertificateValidation =
                bool.Parse(config["HttpManager:IgnoreCertificateValidation"] ?? "false");

            var handler = new HttpClientHandler { AllowAutoRedirect = allowAutoRedirect, UseCookies = useCookies };
            if (ignoreCertificateValidation)
            {
                handler.CheckCertificateRevocationList = false;
                handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
            }

            return handler;
        }
        public static HttpClientHandler CreateProxiedHttpClientHandler(IConfiguration config)
        {
            var allowAutoRedirect = bool.Parse(config["Proxy:AllowAutoRedirect"] ?? "false");
            var useCookies = bool.Parse(config["Proxy:UseCookies"] ?? "false");
            var proxyUrl = config["Proxy:Address"];
            var bypassLocal = bool.Parse(config["Proxy:BypassLocal"] ?? "false");
            var ignoreCertificateValidation =
                bool.Parse(config["Proxy:IgnoreCertificateValidation"] ?? "false");


            var handler = new HttpClientHandler { AllowAutoRedirect = allowAutoRedirect, UseCookies = useCookies };
            if (!string.IsNullOrWhiteSpace(proxyUrl))
            {
                handler.Proxy = new WebProxy(proxyUrl, bypassLocal);
                handler.UseProxy = true;
            }

            if (ignoreCertificateValidation)
            {
                handler.CheckCertificateRevocationList = false;
                handler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;
            }

            var credentialsEnabled = bool.Parse(config["Proxy:NetworkCredentials:Enabled"] ?? "false");
            if (credentialsEnabled)
            {
                var user = config["Proxy:NetworkCredentials:User"];
                var password = config["Proxy:NetworkCredentials:Password"];
                var domain = config["Proxy:NetworkCredentials:Domain"];
                handler.Credentials = new NetworkCredential(user, password, domain);
            }

            return handler;
        }

        public static bool ShouldBypassProxy(string baseUrl, IConfiguration configuration)
        {
            var exceptions = configuration["Proxy:Exceptions"];
            var exceptionList = exceptions.Split(',');

            foreach (var exp in exceptionList)
                if (Regex.IsMatch(baseUrl, exp.Trim(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    return true;

            return false;
        }

        public static bool IsProxyException(this Uri destinationUri, IConfiguration configuration)
            => ShouldBypassProxy(destinationUri.Host, configuration);
    }

}
