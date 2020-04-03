using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Data.HashFunction.CityHash;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PenguinSoft.CoreExtensions.Extensions;
using PenguinSoft.HttpManager;

namespace PenguinSoft.HttpCacheManager
{
    public class HttpCacheManager : IHttpCacheManager
    {
        private readonly IHttpManager _httpManager;
        private readonly IConfiguration _configuration;
        private readonly IHashFunction _cityHash = CityHashFactory.Instance.Create();
        private string _prefixUrl;
        private readonly string _configSection;
        private readonly string _noKeyDefinedString = "no-key-defined";
        private int _cacheTimeout;
        public ServerPool ServerPool { get; set; } = new ServerPool();
        private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>()
        {
            {"Accept","text/plain"},
        };

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public HttpCacheManager(IHttpManager httpManager, IConfiguration configuration, string configSection)
        {
            _httpManager = httpManager;
            _configuration = configuration;
            _configSection = configSection;

            Setup();
        }

        private void Setup()
        {
            _prefixUrl = _configuration[$"{_configSection}:PrefixUrl"] ?? "rest/";
            _cacheTimeout = int.Parse(_configuration[$"{_configSection}:CacheTimeout"] ?? "1");

            var servers = _configuration[$"{_configSection}:Servers"] ?? string.Empty;
            var httpSchema = _configuration[$"{_configSection}:Schema"] ?? "http";

            var authentication = _configuration[$"{_configSection}:Authentication"] ?? string.Empty;
            _defaultHeaders.Add("Authorization", $"Basic {authentication.ToBase64()}");

            foreach (var server in servers.Split(';'))
                ServerPool.Add(new Uri($"{httpSchema}://{server.Trim()}"));
        }

        private string Urlfy(string fragment) => WebUtility.UrlEncode(fragment);
        private string Hash(string fragment) => Urlfy(_cityHash.ComputeHash(Encoding.UTF8.GetBytes(fragment), 256).AsHexString());

        public async Task<T> GetOrPut<T>(string id, string key, Func<Task<T>> factory, List<string> compositeKey = null,
            TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, bool? performAsync = false)
        {
            var existing = await Get(id, key, compositeKey, expireTime, idleTime);
            if (!string.IsNullOrEmpty(existing))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(existing, jsonProps ?? _jsonSettings);
                }
                catch (Exception)
                {
                    //Supress
                }
            }

            var newone = await factory();
            if (newone != null)
                if (performAsync.HasValue && performAsync.Value)
                    await PutAndWait(id, key, newone, compositeKey, expireTime, idleTime);
                else
                    Put(id, key, newone, compositeKey, expireTime, idleTime);

            return newone;
        }
        public async Task<string> GetOrPut(string id, string key, Func<Task<string>> factory, List<string> compositeKey = null,
            TimeSpan? expireTime = null, TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null, bool? performAsync = false)
        {
            var existing = await Get(id, key, compositeKey, expireTime, idleTime);
            if (!string.IsNullOrEmpty(existing))
                return existing;

            var newone = await factory();
            if (newone != null)
                if (performAsync.HasValue && performAsync.Value)
                    await PutAndWait(id, key, newone, compositeKey, expireTime, idleTime);
                else
                    Put(id, key, newone, compositeKey, expireTime, idleTime);

            return newone;
        }

        public async Task<string> Get(string id, string key, List<string> compositeKey = null, TimeSpan? expireTime = null, TimeSpan? idleTime = null)
        {
            try
            {
                var getTask = _httpManager.GetAsync($"/{_prefixUrl}{Urlfy(id)}/{GenerateKey(key, compositeKey)}",
                    GetRequestHeaders(null, null),
                    ServerPool.Next().ToString(), mediaType: "text/plain");

                if (!getTask.Wait(TimeSpan.FromSeconds(_cacheTimeout)))
                    return string.Empty;

                var response = await getTask;
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                //Supress
            }

            return string.Empty;
        }

        public async Task<T> Get<T>(string id, string key, List<string> compositeKey = null, TimeSpan? expireTime = null,
            TimeSpan? idleTime = null, JsonSerializerSettings jsonProps = null) where T : class
        {

            var data = await Get(id, key, compositeKey, expireTime, idleTime);
            try
            {
                if (!string.IsNullOrEmpty(data))
                    return JsonConvert.DeserializeObject<T>(data, jsonProps ?? _jsonSettings);
            }
            catch (Exception)
            {
                //Supress
            }

            return null;
        }

        public void Put(string id, string key, dynamic value, List<string> compositeKey = null, TimeSpan? expireTime = null, TimeSpan? idleTime = null)
        {
            try
            {
                //CallPutAndWait and don't actually wait
                Task.Run(() => PutAndWait(id, key, value, compositeKey, expireTime, idleTime, true));
            }
            catch (Exception)
            {
                //Supress
            }
        }
        public async Task<HttpResponseMessage> PutAndWait(string id, string key, dynamic value, List<string> compositeKey = null, TimeSpan? expireTime = null, TimeSpan? idleTime = null, bool? performAsync = false)
        {
            //I have to convert the object now, because text/plain does not convert to json on httpManager. But if it's already a string, I'm not going to serialize
            string jsonValue = null;
            try
            {
                if (!(value is string))
                    jsonValue = JsonConvert.SerializeObject(value);
            }
            catch (Exception)
            {
                //supress
            }

            return await _httpManager.PutAsync(
                $"/{_prefixUrl}{Urlfy(id)}/{GenerateKey(key, compositeKey)}", jsonValue ?? value,
                GetRequestHeaders(expireTime, idleTime, performAsync),
                ServerPool.Next().ToString(), mediaType: "text/plain");
        }
        public void Delete(string id, string key = null, List<string> compositeKey = null)
        {
            try
            {
                //CallDeleteAndWait and don't actually wait
                Task.Run(() => DeleteAndWait(id, key, compositeKey, true));
            }
            catch (Exception)
            {
                //Supress
            }
        }
        public async Task<HttpResponseMessage> DeleteAndWait(string id, string key = null, List<string> compositeKey = null, bool? performAsync = false)
        {
            return await _httpManager.DeleteAsync(
                $"/{_prefixUrl}{Urlfy(id)}{GenerateDeleteKey(key, compositeKey)}",
                ServerPool.Next().ToString(), GetRequestHeaders(null, null, performAsync));
        }

        private string GenerateDeleteKey(string key, List<string> compositeKey)
        {
            var deleteKey = GenerateKey(key, compositeKey);
            deleteKey = deleteKey == _noKeyDefinedString ? string.Empty : $"/{deleteKey}";
            return deleteKey;
        }

        private string GenerateKey(string key, List<string> compositeKey)
        {
            var fullKey = key ?? string.Empty;
            if (compositeKey != null && compositeKey.Any())
                fullKey += "_" + string.Join("_", compositeKey.Select(x => x));

            if (string.IsNullOrEmpty(fullKey))
                return _noKeyDefinedString;

            return Hash(fullKey);
        }

        private Dictionary<string, string> GetRequestHeaders(TimeSpan? expireTime, TimeSpan? idleTime, bool? performAsync = null)
        {
            var headers = _defaultHeaders.CopyStringDictionary();

            if (idleTime != null)
                headers.Add("maxIdleTimeSeconds", idleTime?.TotalSeconds.ToString());
            //It can't have else clause here. I should be able to set both headers, but never none
            if (expireTime != null)
                headers.Add("timeToLiveSeconds", expireTime?.TotalSeconds.ToString());
            else if (idleTime == null) //In case both are null
                headers.Add("timeToLiveSeconds", TimeSpan.FromHours(1).TotalSeconds.ToString());

            if (performAsync.HasValue)
                headers.Add("performAsync", performAsync.Value.ToString());

            return headers;
        }

    }
}
