using Microsoft.eShopOnContainers.Services.Identity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopOnContainers.Services.Identity.API.Extensions
{
    internal static class StartupExtensions
    {
        public static IServiceCollection AddAIServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "eShopOnContainers - Identity HTTP API",
                    Version = "v1",
                    Description = "The Identity Microservice HTTP API.",
                    TermsOfService = "Terms Of Service"
                });
            });

            return services;
        }

        public static IApplicationBuilder UseAIServices(this IApplicationBuilder app, IConfiguration configuration)
        {
            var pathBase = configuration["PATH_BASE"];

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Identity.API V1");
               });

            return app;
        }
    }
}
