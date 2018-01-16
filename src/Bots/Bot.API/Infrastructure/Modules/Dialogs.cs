using System.Configuration;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bots.Bot.API.Dialogs;
using Microsoft.Bots.Bot.API.Dialogs.Scorable.Commands;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Infrastructure.Modules
{
    public class Dialogs : Module
    {
        /// <summary>
        /// These are the services (and their dependency structure) for bot.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new LuisModelAttribute(ConfigurationManager.AppSettings["luis:ModelId"],
                                                         ConfigurationManager.AppSettings["luis:SubscriptionId"]))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .SingleInstance();

            // register the top level dialog
            builder.RegisterType<RootDialog>()
                   .As<IDialog<object>>()
                   .InstancePerDependency();

            // register other dialogs we use
            builder.RegisterType<DialogFactory>()
                .Keyed<IDialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            // register scorables
            builder.Register(c => new CancelCommand(c.Resolve<IBotToUser>(), c.Resolve<IDialogTask>()))
                    .Keyed<CancelCommand>(typeof(CancelCommand).Name)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

            builder.Register(c => new HelpCommand(c.Resolve<IIdentityService>(), c.Resolve<IBotToUser>(), c.Resolve<IDialogTask>()))
                    .Keyed<HelpCommand>(typeof(HelpCommand).Name)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

            builder.Register(c => new LoginCommand(c.Resolve<IOIDCClient>(), c.Resolve<IIdentityService>(), c.Resolve<IBotToUser>(), c.Resolve<IDialogTask>()))
                    .Keyed<LoginCommand>(typeof(LoginCommand).Name)
                    .AsImplementedInterfaces()
                    .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);
        }
    }
}