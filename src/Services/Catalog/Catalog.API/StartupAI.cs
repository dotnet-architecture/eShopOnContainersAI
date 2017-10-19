namespace Microsoft.eShopOnContainers.Services.Catalog.API
{
    using global::Catalog.API.AI;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    internal class StartupAI : Startup
    {
        public StartupAI(IConfiguration configuration) : base(configuration)
        {
        }

        public override IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAzureMachineLearningService, AzureMachineLearningService>();
            var baseProvider = base.ConfigureServices(services);
            return baseProvider;
        }
    }
}