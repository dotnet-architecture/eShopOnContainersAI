using Autofac;
using Bot46.API.Infrastructure.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Bot46.API.Infrastructure.Modules
{
    public class Dialogs : Module
    {
        /// <summary>
        /// These are the services (and their dependency structure) for bot.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            // register the top level dialog
            builder.RegisterType<RootDialog>()
                   .AsSelf()
                   .InstancePerDependency();

            // register other dialogs we use
            builder.RegisterType<LoginDialog>()
                    .AsSelf()
                    .InstancePerDependency();

            builder.RegisterType<CatalogDialog>()
                    .AsSelf()
                    .InstancePerDependency();

            builder.RegisterType<CatalogFilterDialog>()
                    .AsSelf()
                    .InstancePerDependency();

            builder.RegisterType<BasketDialog>()
                   .AsSelf()
                   .InstancePerDependency();

            builder.RegisterType<OrderDialog>()
               .AsSelf()
               .InstancePerDependency();

            // register scorables

            builder.Register(c => new CancelCommand(c.Resolve<IBotToUser>(), c.Resolve<IDialogTask>()))
                    .Keyed<CancelCommand>(typeof(CancelCommand).Name)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

            builder.Register(c => new HelpCommand(c.Resolve<IBotToUser>(), c.Resolve<IDialogTask>()))
                    .Keyed<HelpCommand>(typeof(HelpCommand).Name)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

            builder.Register(c => new LoginCommand(c.Resolve<IBotToUser>(), c.Resolve<IDialogTask>()))
                    .Keyed<LoginCommand>(typeof(LoginCommand).Name)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);


        }
    }
}