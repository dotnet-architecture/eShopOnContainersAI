using Catalog.API.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddAIServices(this IServiceCollection services)
        {
            services.AddTransient<IAzureMachineLearningService, AzureMachineLearningService>();
            return services;
        }
    }
}
