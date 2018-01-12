using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Bot46.API.Infrastructure.Modules
{
    public class Settings : Module
    {
        /// <summary>
        /// These are the services (and their dependency structure) for bot.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BotSettings>()
                    .Keyed<BotSettings>(FiberModule.Key_DoNotSerialize)
                    .AsSelf()
                    .SingleInstance();
        }
    }
}