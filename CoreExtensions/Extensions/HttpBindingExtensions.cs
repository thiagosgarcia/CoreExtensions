using System;
using System.ServiceModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class HttpBindingExtensions
    {
        public static BasicHttpBinding Https => new BasicHttpBinding
        {
            MaxReceivedMessageSize = int.MaxValue,
            MaxBufferSize = int.MaxValue,
            SendTimeout = TimeSpan.FromMinutes(20),
            OpenTimeout = TimeSpan.FromMinutes(20),
            CloseTimeout = TimeSpan.FromMinutes(20),
            ReceiveTimeout = TimeSpan.FromMinutes(20),
            Security = new BasicHttpSecurity()
            {
                Mode = BasicHttpSecurityMode.Transport
            }
        };
        public static BasicHttpBinding Http => new BasicHttpBinding
        {
            SendTimeout = TimeSpan.FromMinutes(20),
            OpenTimeout = TimeSpan.FromMinutes(20),
            CloseTimeout = TimeSpan.FromMinutes(20),
            ReceiveTimeout = TimeSpan.FromMinutes(20),
            MaxReceivedMessageSize = int.MaxValue,
            MaxBufferSize = int.MaxValue
        };

        public static IServiceCollection AddWcfClient<I, T>(this IServiceCollection services, string key)
            where I : class
            where T : class, I
                => services.AddSingleton<I>(x => GetWcfInstance<I, T>(key, x));
        
        public static T GetWcfInstance<I, T>(string key, IServiceProvider x) where I : class where T : class, I
        {
            var type = typeof(T);
            var ctorInfo = type.GetConstructor(new[] { typeof(BasicHttpBinding), typeof(EndpointAddress) });

            var config = (IConfiguration)x.GetService(typeof(IConfiguration));
            var instance = (T)ctorInfo?.Invoke(new object[] { config.GetHttpBinding(key), config.GetEndpointAddress(key) });
            return instance;
        }

        public static T GetWcfClientInstance<T>(this IConfiguration config, string key)
            where T : class
        {
            var type = typeof(T);
            var ctorInfo = type.GetConstructor(new[] { typeof(BasicHttpBinding), typeof(EndpointAddress) });
            return (T)ctorInfo?.Invoke(new object[] { config.GetHttpBinding(key), config.GetEndpointAddress(key) });
        }

        public static EndpointAddress GetEndpointAddress(this IConfiguration config, string key)
        {
            return new EndpointAddress(config[key]);
        }
        public static BasicHttpBinding GetHttpBinding(this IConfiguration config, string key)
        {
            return GetHttpBinding(config[key]);
        }
        public static BasicHttpBinding GetHttpBinding(string uri)
        {
            return uri.StartsWithIgnoreCase("https") ? Https : Http;
        }
    }
}
