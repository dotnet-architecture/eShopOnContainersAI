using System.IO;
using System.Transactions;
using eShopDashboard.Infraestructure.Data;
using eShopDashboard.Infraestructure.Setup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                var logger = services.GetService<ILogger<CatalogContextSetup>>();
                var dbContext = services.GetService<CatalogContext>();

                dbContext.Database.Migrate();

                new CatalogContextSetup(dbContext, env, logger)
                    .SeedAsync()
                    .Wait();
            }
        }
    }
}