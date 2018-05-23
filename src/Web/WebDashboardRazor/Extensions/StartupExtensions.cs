using Microsoft.AspNetCore.Http;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.eShopOnContainers.WebDashboardRazor.Infrastructure;
using Microsoft.eShopOnContainers.WebDashboardRazor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IOrderingService, OrderingService>();
            services.AddTransient<ICatalogService, CatalogService>();
            return services;
        }

        public static IServiceCollection AddResilienceHttp(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (configuration.GetValue<string>("UseResilientHttp") == bool.TrueString)
            {
                services.AddSingleton<IResilientHttpClientFactory, ResilientHttpClientFactory>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<ResilientHttpClient>>();
                    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

                    var retryCount = 6;
                    if (!string.IsNullOrEmpty(configuration["HttpClientRetryCount"]))
                    {
                        retryCount = int.Parse(configuration["HttpClientRetryCount"]);
                    }

                    var exceptionsAllowedBeforeBreaking = 5;
                    if (!string.IsNullOrEmpty(configuration["HttpClientExceptionsAllowedBeforeBreaking"]))
                    {
                        exceptionsAllowedBeforeBreaking = int.Parse(configuration["HttpClientExceptionsAllowedBeforeBreaking"]);
                    }

                    return new ResilientHttpClientFactory(logger, httpContextAccessor, exceptionsAllowedBeforeBreaking, retryCount);
                });
                services.AddSingleton<IHttpClient, ResilientHttpClient>(sp => sp.GetService<IResilientHttpClientFactory>().CreateResilientHttpClient());
            }
            else
            {
                services.AddSingleton<IHttpClient, StandardHttpClient>();
            }

            return services;
        }
    }
}
