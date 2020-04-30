using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PenguinSoft.ProxyLogger.Logger;

namespace PenguinSoft.HttpManager
{
    public class HttpManager : IHttpManager
    {
        private static ConcurrentDictionary<string, HttpClient> _clientPool = new ConcurrentDictionary<string, HttpClient>();

        private HttpClient GetOrAddClient(string baseUrl)
        {
            if (_clientPool.TryGetValue(baseUrl, out var client))
                return client;

            var proxyEnabled = bool.Parse(_configuration["Proxy:Enabled"] ?? "false");

            //One HttpClient per instance int order to reuse it https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            var instance = proxyEnabled && !ProxyHelper.ShouldBypassProxy(baseUrl, _configuration) ?
                new HttpClient(ProxyHelper.CreateProxiedHttpClientHandler(_configuration)) :
                new HttpClient(ProxyHelper.CreateSimpleHttpClientHandler(_configuration));

            instance.Timeout = TimeSpan.FromMinutes(ProxyHelper.GetClientTimeout(_configuration));
            instance.BaseAddress = new Uri(baseUrl);

            return _clientPool.GetOrAdd(baseUrl, instance);
        }

        public string DefaultBaseUrl { get; }

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly JsonSerializerSettings _jsonExceptionSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        private readonly IHttpContextAccessor _contextAccessor;
        private IConfiguration _configuration;
        private IServiceProvider _provider;
        private readonly List<string> _globalHeaders = new List<string>()
            { "Authorization", "CorrelationId", "User-Agent", "X-Forwarded-For"};
        private ILogger _logger;

        public HttpManager()
        { }
        public HttpManager(string baseUrl)
        {
            DefaultBaseUrl = baseUrl;
        }
        public HttpManager(string baseUrl, IHttpContextAccessor contextAccessor)
        {
            DefaultBaseUrl = baseUrl;
            _contextAccessor = contextAccessor;
        }
        //Logging purposes
        public HttpManager(string baseUrl, IHttpContextAccessor contextAccessor, IConfiguration configuration, IServiceProvider provider)
        {
            DefaultBaseUrl = baseUrl;
            _contextAccessor = contextAccessor;

            InitializeLogger(configuration, provider);
        }

        public HttpManager(string baseUrl, IHttpContextAccessor contextAccessor, JsonSerializerSettings jsonSettings)
        {
            DefaultBaseUrl = baseUrl;
            _contextAccessor = contextAccessor;
            _jsonSettings = jsonSettings ?? _jsonSettings;
        }
        //Logging purposes
        public HttpManager(string baseUrl, IHttpContextAccessor contextAccessor, JsonSerializerSettings jsonSettings, IConfiguration configuration, IServiceProvider provider)
        {
            DefaultBaseUrl = baseUrl;
            _contextAccessor = contextAccessor;
            _jsonSettings = jsonSettings ?? _jsonSettings;

            InitializeLogger(configuration, provider);
        }

        private void InitializeLogger(IConfiguration configuration, IServiceProvider provider)
        {
            _configuration = configuration;
            _provider = provider;
            if (_configuration != null && _provider != null && _contextAccessor != null)
                _logger = new SeriLogger(_configuration, _contextAccessor);
        }

        private void Log(string msg, params object[] param)
        {
            if (!_logger?.SerializeHttp ?? true)
                return;

            _logger?.Info(msg, param);
        }

        private async Task<HttpResponseMessage> PerformAction(
            HttpMethod httpMethod, string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> queryParams, bool throwOnError, string mediaType)
        {
            var baseAddress = baseUrl ?? DefaultBaseUrl;

            var requestMessage = new HttpRequestMessage(httpMethod, BuildResource(resource, queryParams));
            SetHeaders(requestMessage, baseAddress, headers);

            if (!httpMethod.Equals(HttpMethod.Get))
                requestMessage.Content = CreateRequestContent(obj, mediaType);

            try
            {
                var result = await SendAsync(baseAddress, requestMessage);
                await HandleNonSuccess(throwOnError, result);
                return result;
            }
            catch (HttpRequestException ex)
            {
                Log($"HttpManager::Exception: {ex.StackTrace}");
                throw;
            }
        }

        private HttpContent CreateRequestContent(dynamic obj, string mediaType)
        {
            mediaType = mediaType ?? "application/json";

            HttpContent requestContent;
            switch (mediaType)
            {
                case "application/x-www-form-urlencoded":
                    requestContent = new FormUrlEncodedContent(obj);
                    break;
                case "text/plain":
                    requestContent = new StringContent(obj, Encoding.UTF8, mediaType);
                    break;
                default:
                    var jsonParam = JsonConvert.SerializeObject(obj, _jsonSettings);
                    requestContent = new StringContent(jsonParam, Encoding.UTF8, mediaType);
                    break;
            }

            return requestContent;
        }

        private Task<HttpResponseMessage> SendAsync(string baseAddress, HttpRequestMessage requestMessage)
        {
            Log($"HttpManager::PreparingHttpClient: {baseAddress}");
            var client = GetOrAddClient(baseAddress);
            Log($"HttpManager::Send: {requestMessage.Method}, {baseAddress}, {requestMessage.RequestUri}");
            return client.SendAsync(requestMessage);
        }

        private async Task HandleNonSuccess(bool throwOnError, HttpResponseMessage result)
        {
            Log($"HttpManager::StatusCode: {result.StatusCode}");
            if (!result.IsSuccessStatusCode && throwOnError)
            {
                var content = await result.Content.ReadAsStringAsync();
                Log($"HttpManager::Content: {content}");
                Exception obj;
                try
                {
                    //TODO Limitar serialização DE EXCEPTIONS com contratos personalizados https://stackoverflow.com/q/10453127/2944723
                    obj = JsonConvert.DeserializeObject<Exception>(content, _jsonExceptionSettings);
                }
                catch (Exception e)
                {
                    throw new HttpRequestException($"StatusCode='{result.StatusCode}' Body='{content}'", e);
                }
                throw new HttpRequestException(obj?.Message ?? result.ReasonPhrase, obj);
            }
        }

        private void SetHeaders(HttpRequestMessage client, string baseAddress, IDictionary<string, string> headers = null)
        {
            if (headers != null)
                foreach (var header in headers)
                    client.Headers.Add(header.Key, header.Value);

            var accessorHeaders = _contextAccessor?.HttpContext?.Request?.Headers;
            foreach (var globalHeader in _globalHeaders)
                if (accessorHeaders != null && accessorHeaders.ContainsKey(globalHeader) && !client.Headers.Contains(globalHeader))
                    client.Headers.TryAddWithoutValidation(globalHeader, accessorHeaders[globalHeader].ToString());
        }

        private static string BuildResource(string resource, IDictionary<string, string> queryParams)
        {
            var query = string.Empty;
            if (queryParams != null)
                query += "?" + string.Join("&", queryParams.Where(x => x.Value != null)
                             .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
            resource += query;
            return resource;
        }

                private Task<HttpResponseMessage> InternalGet(string resource,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> queryParams, bool throwOnError, string mediaType)
                => PerformAction(HttpMethod.Get, resource, null, headers, baseUrl, queryParams, throwOnError, mediaType);

        public Task<HttpResponseMessage> GetAsync(string resource, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null)
                => InternalGet(resource, headers, baseUrl, queryParams, false, mediaType);

        public async Task<T> GetAsync<T>(string resource, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null) where T : class
        {
            var result = await InternalGet(resource, headers, baseUrl, queryParams, true, mediaType);
            var returnResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(returnResult, _jsonSettings);
        }
        public async Task<Stream> GetStreamAsync(string resource, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null)
        {
            var result = await InternalGet(resource, headers, baseUrl, queryParams, true, mediaType);
            return await result.Content.ReadAsStreamAsync();
        }
                
        private Task<HttpResponseMessage> InternalPost(string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> queryParams, bool throwOnError, string mediaType)
                => PerformAction(HttpMethod.Post, resource, obj, headers, baseUrl, queryParams, throwOnError, mediaType);

        public async Task<T> PostAsync<T>(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null) where T : class
        {
            var result = await InternalPost(resource, obj, headers, baseUrl, queryParams, true, mediaType);
            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stringResult, _jsonSettings);
        }
        public async Task<Stream> PostStreamAsync(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null)
        {
            var result = await InternalPost(resource, obj, headers, baseUrl, queryParams, true, mediaType);
            return await result.Content.ReadAsStreamAsync();
        }
        public Task<HttpResponseMessage> PostAsync(
            string resource, dynamic obj, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null)
                => InternalPost(resource, obj, headers, baseUrl, queryParams, false, mediaType);

                        private Task<HttpResponseMessage> InternalPut(string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> queryParams, bool throwOnError, string mediaType)
                => PerformAction(HttpMethod.Put, resource, obj, headers, baseUrl, queryParams, throwOnError, mediaType);

        public async Task<T> PutAsync<T>(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null) where T : class
        {
            var result = await InternalPut(resource, obj, headers, baseUrl, queryParams, true, mediaType);
            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stringResult, _jsonSettings);
        }
        public async Task<Stream> PutStreamAsync(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null)
        {
            var result = await InternalPut(resource, obj, headers, baseUrl, queryParams, true, mediaType);
            return await result.Content.ReadAsStreamAsync();
        }
        public Task<HttpResponseMessage> PutAsync(
            string resource, dynamic obj, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null)
                => InternalPut(resource, obj, headers, baseUrl, queryParams, false, mediaType);

                        private Task<HttpResponseMessage> InternalPatch(string resource, dynamic obj,
            IDictionary<string, string> headers, string baseUrl,
            IDictionary<string, string> queryParams, bool throwOnError, string mediaType)
                => PerformAction(new HttpMethod("PATCH"), resource, obj, headers, baseUrl, queryParams, throwOnError, mediaType);

        public async Task<T> PatchAsync<T>(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null) where T : class
        {
            var result = await InternalPatch(resource, obj, headers, baseUrl, queryParams, true, mediaType);
            var stringResult = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stringResult, _jsonSettings);
        }
        public async Task<Stream> PatchStreamAsync(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null)
        {
            var result = await InternalPatch(resource, obj, headers, baseUrl, queryParams, true, mediaType);
            return await result.Content.ReadAsStreamAsync();
        }
        public Task<HttpResponseMessage> PatchAsync(
            string resource, dynamic obj, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null)
                => InternalPatch(resource, obj, headers, baseUrl, queryParams, false, mediaType);

                
        public Task<HttpResponseMessage> PostFileAsync(string resource, IEnumerable<IFormFile> files,
            string baseUrl = null, Dictionary<string, string> queryParams = null, IEnumerable<HttpContent> additionalContent = null)
        {
            var baseAddress = baseUrl ?? DefaultBaseUrl;

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, BuildResource(resource, queryParams));

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var content = new MultipartFormDataContent($"----------{Guid.NewGuid()}");

            if (files != null)
                foreach (var formFile in files)
                    content.Add(new StreamContent(formFile.OpenReadStream()), formFile.Name, formFile.FileName);

            if (additionalContent != null)
                foreach (var addContent in additionalContent)
                    content.Add(addContent);

            requestMessage.Content = content;
            SetHeaders(requestMessage, baseAddress);

            return SendAsync(baseAddress, requestMessage);
        }
        public Task<HttpResponseMessage> PostMultipartAsync(string resource, IEnumerable<HttpContent> multipartContent, string baseUrl = null,
            Dictionary<string, string> queryParams = null)
        {
            return PostFileAsync(resource, null, baseUrl, queryParams, multipartContent);
        }

                
        public Task<HttpResponseMessage> DeleteAsync(string resource, string baseUrl = null, IDictionary<string, string> headers = null)
        {
            var baseAddress = baseUrl ?? DefaultBaseUrl;
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, resource);
            SetHeaders(requestMessage, baseAddress, headers);
            return SendAsync(baseAddress, requestMessage);
        }
        
                public Task<HttpResponseMessage> OptionsAsync(string resource, string baseUrl = null, IDictionary<string, string> headers = null)
        {
            var baseAddress = baseUrl ?? DefaultBaseUrl;
            var requestMessage = new HttpRequestMessage(HttpMethod.Options, resource);
            SetHeaders(requestMessage, baseAddress, headers);
            return SendAsync(baseAddress, requestMessage);
        }
                        public Task<HttpResponseMessage> HeadAsync(string resource, string baseUrl = null, IDictionary<string, string> headers = null)
        {
            var baseAddress = baseUrl ?? DefaultBaseUrl;
            var requestMessage = new HttpRequestMessage(HttpMethod.Head, resource);
            SetHeaders(requestMessage, baseAddress, headers);
            return SendAsync(baseAddress, requestMessage);
        }
                        public Task<HttpResponseMessage> TraceAsync(string resource, string baseUrl = null, IDictionary<string, string> headers = null)
        {
            var baseAddress = baseUrl ?? DefaultBaseUrl;
            var requestMessage = new HttpRequestMessage(HttpMethod.Trace, resource);
            SetHeaders(requestMessage, baseAddress, headers);
            return SendAsync(baseAddress, requestMessage);
        }
            }
}
