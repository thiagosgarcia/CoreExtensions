using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PenguinSoft.HttpManager
{
    public interface IHttpManager
    {
        string DefaultBaseUrl { get; }

        Task<HttpResponseMessage> GetAsync(string resource, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<T> GetAsync<T>(string resource, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null) where T : class;

        Task<Stream> GetStreamAsync(string resource, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<T> PostAsync<T>(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null) where T : class;

        Task<Stream> PostStreamAsync(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<HttpResponseMessage> PostFileAsync(string resource, IEnumerable<IFormFile> files, string baseUrl = null,
            Dictionary<string, string> queryParams = null, IEnumerable<HttpContent> additionalContent = null);

        Task<HttpResponseMessage> PostMultipartAsync(string resource,
            IEnumerable<HttpContent> multipartContent, string baseUrl = null,
            Dictionary<string, string> queryParams = null);

        Task<HttpResponseMessage> PostAsync(
            string resource, dynamic obj, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<T> PutAsync<T>(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null) where T : class;

        Task<Stream> PutStreamAsync(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<HttpResponseMessage> PutAsync(
            string resource, dynamic obj, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<T> PatchAsync<T>(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null) where T : class;

        Task<Stream> PatchStreamAsync(string resource, dynamic obj,
            IDictionary<string, string> headers = null, string baseUrl = null,
            IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<HttpResponseMessage> PatchAsync(
            string resource, dynamic obj, IDictionary<string, string> headers = null,
            string baseUrl = null, IDictionary<string, string> queryParams = null, string mediaType = null);

        Task<HttpResponseMessage> DeleteAsync(string resource, string baseUrl = null, IDictionary<string, string> headers = null);

        Task<HttpResponseMessage> OptionsAsync(string resource, string baseUrl = null,
            IDictionary<string, string> headers = null);
        Task<HttpResponseMessage> HeadAsync(string resource, string baseUrl = null,
            IDictionary<string, string> headers = null);
        Task<HttpResponseMessage> TraceAsync(string resource, string baseUrl = null,
            IDictionary<string, string> headers = null);

    }
}
