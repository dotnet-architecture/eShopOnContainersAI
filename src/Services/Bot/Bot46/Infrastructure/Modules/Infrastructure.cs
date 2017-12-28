using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Bot46.API.Infrastructure.Modules
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
        }
    }
}