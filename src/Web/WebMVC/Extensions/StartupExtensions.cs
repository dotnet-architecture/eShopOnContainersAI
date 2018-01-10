using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopOnContainers.WebMVC.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddAIServices(this IServiceCollection services)
        {
            services.AddTransient<ICatalogAIService, CatalogAIService>();
            services.AddTransient<IProductRecommenderService, ProductRecommenderService>();
            return services;
        }
    }
}
