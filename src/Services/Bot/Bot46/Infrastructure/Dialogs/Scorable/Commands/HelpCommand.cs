using System.Threading;
using System.Threading.Tasks;
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
            var  response = GetHelp(message);
            await BotToUser.PostAsync(response);
        }

        private IMessageActivity GetHelp(IMessageActivity message)
        {
            // Here you should get the help info for the user
            // You can inspace the message to search for specific help like 'help products'
            // and provide help related to the products you have 
            var reply = BotToUser.MakeMessage();
            reply.Text = "Help Command";
            return reply;
        }
    }
}