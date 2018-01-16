using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Microsoft.Bots.Bot.API.Dialogs.Scorable.Commands
{
    public class CancelCommand : CommandScorable
    {
        public CancelCommand(IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {
        }

        public override string Command
        {
            get{
                return "/cancel";
            }
        }

        protected override  Task PostAsync(IActivity item, bool state, CancellationToken token)
        {
            this.task.Reset();
            return Task.CompletedTask;
        }
    }
}