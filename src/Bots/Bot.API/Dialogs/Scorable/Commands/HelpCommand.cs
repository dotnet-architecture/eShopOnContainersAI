using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;

namespace Microsoft.Bots.Bot.API.Dialogs.Scorable.Commands
{
    public class HelpCommand: CommandScorable
    {
        private readonly IDialogFactory dialogFactory;

        public HelpCommand(IDialogFactory dialogFactory, IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {
            this.dialogFactory = dialogFactory;
        }

        public override string Command
        {
            get{
                return "/help";
            }
        }

        protected override async Task PostAsync(IActivity item, bool state, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            if(message != null)
            {
                var helpDialog = dialogFactory.CreateHelpDialog();

                var interruption = helpDialog.Void<object, IMessageActivity>();

                task.Call(interruption, null);

                await task.PollAsync(token);                
            }
        }

    }
}