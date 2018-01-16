using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Infrastructure.Modules
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

            builder.RegisterType<API.Services.ProductSearchImageService>()
                   .Keyed<IProductSearchImageService>(FiberModule.Key_DoNotSerialize)
                   .AsImplementedInterfaces()
                   .SingleInstance();

            builder.RegisterType<CatalogAIService>()
                  .Keyed<ICatalogAIService>(FiberModule.Key_DoNotSerialize)
                  .AsImplementedInterfaces()
                  .SingleInstance();

            builder.RegisterType<IdentityService>()
                .Keyed<IIdentityService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<LuisService>()
                .Keyed<ILuisService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

        }
    }
}