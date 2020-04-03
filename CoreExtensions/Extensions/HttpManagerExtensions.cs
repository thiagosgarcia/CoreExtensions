using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PenguinSoft.HttpManager;
using Manager = PenguinSoft.HttpManager.HttpManager;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class HttpManagerExtensions
    {
        public static IServiceCollection AddHttpManager(this IServiceCollection services,
            string defaultBaseUrlConfig = null, bool enableServiceEndpointConfig = true)
        {
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IHttpManager>(x =>
                {
                    var config = (IConfiguration)x.GetService(typeof(IConfiguration));
                    return new Manager(defaultBaseUrlConfig == null ? null : config[defaultBaseUrlConfig], (IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor)), config, x);
                });
        }
        public static IServiceCollection AddHttpManager(this IServiceCollection services, JsonSerializerSettings jsonSettings, string defaultBaseUrlConfig = null, bool enableServiceEndpointConfig = true)
        {
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IHttpManager>(x =>
                {
                    var config = (IConfiguration)x.GetService(typeof(IConfiguration));
                    return new Manager(defaultBaseUrlConfig == null ? null : config[defaultBaseUrlConfig], (IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor)), jsonSettings, config, x);
                });
        }
        public static IServiceCollection AddHttpManagerWithoutLog(this IServiceCollection services, string defaultBaseUrlConfig = null, bool enableServiceEndpointConfig = true)
        {
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IHttpManager>(x =>
                {
                    var config = (IConfiguration)x.GetService(typeof(IConfiguration));
                    return new Manager(defaultBaseUrlConfig == null ? null : config[defaultBaseUrlConfig], (IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor)));
                });
        }
        public static IServiceCollection AddHttpManagerWithoutLog(this IServiceCollection services, JsonSerializerSettings jsonSettings, string defaultBaseUrlConfig = null, bool enableServiceEndpointConfig = true)
        {
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IHttpManager>(x =>
                {
                    var config = (IConfiguration)x.GetService(typeof(IConfiguration));
                    return new Manager(defaultBaseUrlConfig == null ? null : config[defaultBaseUrlConfig], (IHttpContextAccessor)x.GetService(typeof(IHttpContextAccessor)), jsonSettings);
                });
        }
    }
}
