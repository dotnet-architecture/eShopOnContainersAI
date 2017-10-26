using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.WebMVC.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddAIServices(this IServiceCollection services)
        {
            services.AddTransient<ICatalogAIService, CatalogAIService>();
            return services;
        }
    }
}
