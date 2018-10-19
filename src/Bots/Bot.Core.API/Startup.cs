// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.eShopOnContainers.Bot.API;
using Microsoft.eShopOnContainers.Bot.API.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Services.Attachment;
using Microsoft.eShopOnContainers.Bot.API.Services.Basket;
using Microsoft.eShopOnContainers.Bot.API.Services.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Services.LUIS;
using Microsoft.eShopOnContainers.Bot.API.Services.Order;
using Microsoft.eShopOnContainers.Bot.API.Services.ProductSearchImage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore_SimplePrompt_Bot
{
    public class Startup
    {
        private ILoggerFactory loggerFactory;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        /// <summary>/
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">Specifies the contract for a <see cref="IServiceCollection"/> of service descriptors.</param>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration);
            services.AddHttpClient();

            services.AddTransient<HttpClient, HttpClient>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<ICatalogAIService, CatalogAIService>();
            services.AddTransient<IProductSearchImageService, ProductSearchImageService>();
            services.AddTransient<ICatalogService, CatalogService>();
            services.AddTransient<IBasketService, BasketService>();
            services.AddTransient<IOrderingService, OrderingService>();
            services.AddTransient<ILuisService, Microsoft.eShopOnContainers.Bot.API.Services.LUIS.LuisService>();
            services.AddTransient<ICatalogFilterDialogService, CatalogFilterDialogService>();
            services.AddTransient<IAttachmentService, AttachmentService>();
            services.AddTransient<IDialogFactory, DialogFactory>();

            var appSettings = services.BuildServiceProvider().GetService<IOptions<AppSettings>>();
            UIHelper.ImageUrl = appSettings.Value.ImageUrl;

            // The Memory Storage used here is for local bot debugging only. When the bot
            // is restarted, anything stored in memory will be gone. 
            IStorage dataStore = new MemoryStorage();
            // For production bots use the Azure Blob or
            // Azure CosmosDB storage providers. For the Azure
            // based storage providers, add the Microsoft.Bot.Builder.Azure
            // Nuget package to your solution. That package is found at:
            // https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/
            // Un-comment the following lines to use Azure Blob Storage
            // // Storage configuration name or ID from the .bot file.
            // const string StorageConfigurationId = "<STORAGE-NAME-OR-ID-FROM-BOT-FILE>";
            // var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
            // if (!(blobConfig is BlobStorageService blobStorageConfig))
            // {
            //    throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
            // }
            // // Default container name.
            // const string DefaultBotContainer = "<DEFAULT-CONTAINER>";
            // var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
            // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);

            services.AddSingleton(dataStore);
            var userState = new UserState(dataStore);
            var conversationState = new ConversationState(dataStore);

            services.AddSingleton(userState)
                    .AddSingleton(conversationState)
                    // The BotStateSet enables read() and write() in parallel on multiple BotState instances.
                    .AddSingleton(new BotStateSet(userState, conversationState));

            services.AddSingleton<DomainPropertyAccessors>(sp => new DomainPropertyAccessors(userState, conversationState));

            var botConfig = BotConfiguration.Load(@"./Bot.Core.API.bot");
            services.AddSingleton<BotConfiguration>(sp => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded. ({botConfig})"));

            

            services.AddBot<eShopBot>(options =>
            {
                // Load the connected services from .bot file.
                var endpointService = botConfig.Services.FirstOrDefault(s => s.Type == ServiceTypes.Endpoint) as EndpointService;

                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Catches any errors that occur during a conversation turn and logs them to currently
                // configured ILogger.
                ILogger logger = loggerFactory.CreateLogger<eShopBot>();
                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };

                // Automatically save state at the end of a turn.
                options.Middleware
                    .Add(new AutoSaveStateMiddleware(userState, conversationState));

            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder. This provides the mechanisms to configure the application request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to create logger object for tracing.</param>
        /// <remarks>See <see cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-2.1"/> for
        /// more information how environments are detected.</remarks>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
