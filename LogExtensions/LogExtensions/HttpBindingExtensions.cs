using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PenguinSoft.CoreExtensions.Extensions;
using PenguinSoft.LogExtensions.Logging;
using PenguinSoft.ProxyLogger.Logger;

namespace PenguinSoft.LogExtensions.LogExtensions
{
    public static class LogExtensions
    {

        public static IServiceCollection AddWcfClientWithLog<I, T>(this IServiceCollection services, string key)
            where I : class
            where T : class, I
                => services.AddSingleton<I>(x =>
                    {
                        var instance = HttpBindingExtensions.GetWcfInstance<I, T>(key, x);
                        return DispatchLoggingProxy<I>.Create(instance, x);
                    });


        public static IApplicationBuilder AddLogMiddleware(this IApplicationBuilder app, IConfiguration configuration)
        {
            if (bool.Parse(configuration["Logging:SerializeHttp"] ?? "false"))
                return app.UseMiddleware<DetailedLogMiddleware>();

            return app.UseMiddleware<LogMiddleware>();
        }
    }
}
