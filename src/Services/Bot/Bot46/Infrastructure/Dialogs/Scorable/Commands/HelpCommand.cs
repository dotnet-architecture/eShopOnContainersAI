using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Dialogs
{
    public class HelpCommand: CommandScorable
    {
        public HelpCommand(IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {  
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
                var helpDialog = new HelpDialog();

                var interruption = helpDialog.Void<object, IMessageActivity>();

                task.Call(interruption, null);

                await task.PollAsync(token);
                
            }
        }

    }
}