using eShopDashboard.Infrastructure.Data.Catalog;
using eShopDashboard.Infrastructure.Setup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using eShopDashboard.Infrastructure.Data.Ordering;

namespace eShopDashboard
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .Build();

        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            ConfigureDatabase(host);

            host.Run();
        }

        private static void ConfigureDatabase(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var catalogContext = services.GetService<CatalogContext>();
                var catalogContextSetup = services.GetService<CatalogContextSetup>();

                catalogContext.Database.Migrate();
                catalogContextSetup.SeedAsync().Wait();

                var orderingContext = services.GetService<OrderingContext>();
                var orderingContextSetup = services.GetService<OrderingContextSetup>();

                orderingContext.Database.Migrate();
                orderingContextSetup.SeedAsync().Wait();
            }
        }
    }
}