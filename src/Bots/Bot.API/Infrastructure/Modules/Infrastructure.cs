using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Infrastructure.Modules
{
    public class Infrastructure : Module
    {
        /// <summary>
        /// These are the services (and their dependency structure) for bot.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            // TODO Resilience.Http
            builder.RegisterType<BasicHttpClient>()
                    .Keyed<IHttpClient>(FiberModule.Key_DoNotSerialize)
                    .AsImplementedInterfaces()
                    .SingleInstance();

            builder.RegisterType<OIDCClient>()
                .Keyed<IOIDCClient>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<BotDataRepository>()
                .Keyed<IBotDataRepository>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}