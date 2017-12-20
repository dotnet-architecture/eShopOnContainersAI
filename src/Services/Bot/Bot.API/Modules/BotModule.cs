using Autofac;
using Bot.API.Dialogs;
using Bot.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;

namespace Bot.API.Modules
{
    public class BotModule : Module
    {
        /// <summary>
        /// These are the services (and their dependency structure) for bot.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
                base.Load(builder);

                // register the top level dialog
                 builder.RegisterType<RootDialog>()
                        .AsSelf()
                        .InstancePerLifetimeScope();

                // register other dialogs we use
                builder.RegisterType<CatalogDialog>()
                        .AsSelf()
                        .InstancePerLifetimeScope();

                builder.RegisterType<CatalogFilterDialog>()
                        .AsSelf()
                        .InstancePerLifetimeScope();

                builder.RegisterType<LoginDialog>()
                        .AsSelf()
                        .InstancePerLifetimeScope();
                        

                // register scorables
                builder.Register(c => new SampleScorable(c.Resolve<IDialogTask>()))
                        .As<IScorable<IActivity, double>>()
                        .InstancePerLifetimeScope();

                //  singleton services
                builder.RegisterType<CatalogService>()
                        .Keyed<ICatalogService>(FiberModule.Key_DoNotSerialize)
                        .AsImplementedInterfaces()
                        .SingleInstance();


                // other DI
                builder.RegisterType<HttpContextAccessor>()
                        .Keyed<IHttpContextAccessor>(FiberModule.Key_DoNotSerialize)
                        .AsImplementedInterfaces()
                        .SingleInstance();

                
                // TODO Resilience.Http
                builder.RegisterType<StandardHttpClient>()
                        .Keyed<IHttpClient>(FiberModule.Key_DoNotSerialize)
                        .AsImplementedInterfaces()
                        .SingleInstance();
            

        }
    }
}
