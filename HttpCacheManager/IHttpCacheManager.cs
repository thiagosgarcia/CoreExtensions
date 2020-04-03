using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PenguinSoft.HttpCacheManager
{
    public interface IHttpCacheManager
    {
        ServerPool ServerPool { get; set; }

        Task<string> Get(string id, string key, List<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null);

        Task<T> Get<T>(string id, string key, List<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null) where T : class;

        void Put(string id, string key, dynamic value, List<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null);

        Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory, List<string> compositeKey = null,
            TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, bool? performAsync = false);

        Task<string> GetOrPut(string id, string key, Func<Task<string>> factory, List<string> compositeKey = null,
            TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, bool? performAsync = false);

        Task<HttpResponseMessage> PutAndWait(string id, string key, dynamic value, List<string> compositeKey = null,
            TimeSpan? expireTime = null, TimeSpan? idleTime = null, bool? performAsync = false);

        void Delete(string id, string key = null, List<string> compositeKey = null);
        Task<HttpResponseMessage> DeleteAndWait(string id, string key = null, List<string> compositeKey = null, bool? performAsync = false);
    }
}