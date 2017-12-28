using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;

namespace Bot.API.Dialogs
{
    public abstract class CommandScorable : ScorableBase<IActivity, bool, double>
    {
        protected readonly IBotToUser BotToUser; 

        public CommandScorable(IBotToUser botToUser)
        {
            SetField.NotNull(out this.BotToUser, nameof(botToUser), botToUser);
        }

        public abstract string Command { get; }

        protected override Task DoneAsync(IActivity item, bool state, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override double GetScore(IActivity item, bool state)
        {
            return state ? 1 : 0; 
        }

        protected override bool HasScore (IActivity item, bool state)
        {
            return state; 
        }

        protected override Task<bool> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item.AsMessageActivity();

            if(message == null){
                return Task.FromResult(false);
            }

            return Task.FromResult(message.Text.ToLowerInvariant().Contains(this.Command.ToLowerInvariant()));
        }
    }
}