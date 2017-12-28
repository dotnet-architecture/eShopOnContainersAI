using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Bot.API.Dialogs
{
    public class HelpCommand: CommandScorable
    {
        public HelpCommand(IBotToUser botToUser) : base(botToUser)
        {  
        }

        public override string Command
        {
            get{
                return "Help";
            }
        }

        protected override async Task PostAsync(IActivity item, bool state, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            var  response = await this.GetHelpAsync(message);
            await this.BotToUser.PostAsync(response);
        }

        private async Task<IMessageActivity> GetHelpAsync(IMessageActivity message)
        {
            // Here you should get the help info for the user
            // You can inspace the message to search for specific help like 'help products'
            // and provide help related to the products you have 
            var reply = this.BotToUser.MakeMessage();
            reply.Text = "Help Command";
            return reply;
        }
    }
}