using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Bot46.API.Infrastructure.Modules;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Bot46.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var config = GlobalConfiguration.Configuration;

            Conversation.UpdateContainer(
               builder =>
               {
                 //  builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));
                   builder.RegisterModule(new Infrastructure.Modules.Settings());
                   builder.RegisterModule(new Infrastructure.Modules.Infrastructure());
                   builder.RegisterModule(new Infrastructure.Modules.Services());
                   builder.RegisterModule(new Infrastructure.Modules.Dialogs());

                   // Bot Storage: Here we register the state storage for your bot. 
                   // Default store: volatile in-memory store - Only for prototyping!
                   // We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
                   // For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
                   // Other storage options
                   // var store = new TableBotDataStore("...DataStorageConnectionString..."); // requires Microsoft.BotBuilder.Azure Nuget package 
                   // var store = new DocumentDbBotDataStore("cosmos db uri", "cosmos db key"); // requires Microsoft.BotBuilder.Azure Nuget package 
                   // var store = new SqlBotDataStore(...DataStorageConnectionString...) // requires Microsoft.BotBuilder.Azure Nuget package 

                   var store = new InMemoryDataStore();
                   builder.Register(c => store)
                             .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                             .AsSelf()
                             .SingleInstance();

                   builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();


                   // Register Web API controller in executing assembly.
                   builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                   builder.RegisterWebApiFilterProvider(config);
                   // Register your MVC controllers.
                   builder.RegisterControllers(Assembly.GetExecutingAssembly());

               });

            // Set the dependency resolver to be Autofac.
            config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(Conversation.Container));
            ServiceResolver.Container = Conversation.Container;

        }
    }
}
