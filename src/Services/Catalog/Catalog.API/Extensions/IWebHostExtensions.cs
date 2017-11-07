using Catalog.API.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;

namespace Catalog.API.Extensions
{
    public static class IWebHostExtensions
    {
        public static IWebHost SeedDbContext<TContext>(this IWebHost webHost, Action<IServiceProvider> seeder) where TContext : CatalogTagsContext
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<TContext>>();

                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation($"Migrating database associated with context {typeof(TContext).Name}");

                    var retry = Policy.Handle<MongoDB.Driver.MongoException>()
                         .WaitAndRetry(new TimeSpan[]
                         {
                             TimeSpan.FromSeconds(5),
                             TimeSpan.FromSeconds(10),
                             TimeSpan.FromSeconds(15),
                         });

                    retry.Execute(() =>
                    {
                        seeder(services);
                    });


                    logger.LogInformation($"Database associated with context {typeof(TContext).Name} has been populated");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occurred while populating the database used on context {typeof(TContext).Name}");
                }
            }

            return webHost;
        }
    }
}
