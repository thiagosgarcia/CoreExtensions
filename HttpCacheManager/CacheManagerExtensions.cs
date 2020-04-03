using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PenguinSoft.HttpManager;

namespace PenguinSoft.HttpCacheManager
{
    public static class CacheManagerExtensions
    {
        public static IServiceCollection AddCacheManager(this IServiceCollection services, string configSection = "HttpCacheManager")
        {
            return services.AddSingleton<IHttpCacheManager>(x => new HttpCacheManager(x.GetService<IHttpManager>(), x.GetService<IConfiguration>(), configSection));
        }
    }
}