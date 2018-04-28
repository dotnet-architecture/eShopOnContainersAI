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

                var env = services.GetService<IHostingEnvironment>();

                var catalogLogger = services.GetService<ILogger<CatalogContextSetup>>();
                var catalogContext = services.GetService<CatalogContext>();

                catalogContext.Database.Migrate();

                new CatalogContextSetup(catalogContext, env, catalogLogger)
                    .SeedAsync()
                    .Wait();

                var orderingLogger = services.GetService<ILogger<OrderingContextSetup>>();
                var orderingContext = services.GetService<OrderingContext>();

                orderingContext.Database.Migrate();

                new OrderingContextSetup(orderingContext, env, orderingLogger)
                    .SeedAsync()
                    .Wait();
            }
        }
    }
}