using Autofac;

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
                    .AsSelf()
                    .SingleInstance();
        }
    }
}