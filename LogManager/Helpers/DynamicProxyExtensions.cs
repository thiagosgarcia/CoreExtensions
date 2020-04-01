using Microsoft.Extensions.DependencyInjection;
using ProxyLogger.Logger;

namespace ProxyLogger.Helpers
{
    public static class DynamicProxyExtensions
    {
        public static IServiceCollection AddScopedWithLog<TInterface, TService>(this IServiceCollection services) where TInterface : class where TService : class, TInterface
        {
            return
                services.AddScoped<TService>()
                    .AddScoped(x => DispatchLoggingProxy<TInterface>.Create(x.GetService<TService>(), x));
        }
        public static IServiceCollection AddTransientWithLog<TInterface, TService>(this IServiceCollection services) where TInterface : class where TService : class, TInterface
        {
            return
                services.AddTransient<TService>()
                    .AddTransient(x => DispatchLoggingProxy<TInterface>.Create(x.GetService<TService>(), x));
        }
        public static IServiceCollection AddSingletonWithLog<TInterface, TService>(this IServiceCollection services) where TInterface : class where TService : class, TInterface
        {
            return
                services.AddSingleton<TService>()
                    .AddSingleton(x => DispatchLoggingProxy<TInterface>.Create(x.GetService<TService>(), x));
        }
    }
}
