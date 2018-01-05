using Autofac;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using System.Configuration;

namespace Bot46.API.Infrastructure.Modules
{
    public class Services : Module
    {
        /// <summary>
        /// These are the services (and their dependency structure) for bot.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CatalogService>()
                    .Keyed<ICatalogService>(FiberModule.Key_DoNotSerialize)
                    .AsImplementedInterfaces()
                    .SingleInstance();

            builder.RegisterType<BasketService>()
                    .Keyed<IBasketService>(FiberModule.Key_DoNotSerialize)
                    .AsImplementedInterfaces()
                    .SingleInstance();

            builder.RegisterType<OrderingService>()
                   .Keyed<IOrderingService>(FiberModule.Key_DoNotSerialize)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<ComputerVisionService>()
                   .Keyed<IComputerVisionService>(FiberModule.Key_DoNotSerialize)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<CatalogAIService>()
                  .Keyed<ICatalogAIService>(FiberModule.Key_DoNotSerialize)
                  .AsImplementedInterfaces()
                  .SingleInstance();

            builder.Register(c => new LuisModelAttribute(ConfigurationManager.AppSettings["luis:ModelId"],
                                                        ConfigurationManager.AppSettings["luis:SubscriptionId"]))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .SingleInstance();

            builder.RegisterType<LuisService>()
                .Keyed<ILuisService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

        }
    }
}