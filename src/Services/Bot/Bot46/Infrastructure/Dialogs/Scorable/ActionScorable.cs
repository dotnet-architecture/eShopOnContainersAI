using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot46.API.Infrastructure.Dialogs
{
    public abstract class ActionScorable : ScorableBase<IActivity, bool, double>
    {
        protected readonly IBotToUser BotToUser;
        protected readonly IDialogTask task;

        public ActionScorable(IBotToUser botToUser, IDialogTask task)
        {
            SetField.NotNull(out this.BotToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.task, nameof(task), task);
        }

        public abstract string Action { get; }

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
            bool result = false;
            var message = item.AsMessageActivity();
            if(message == null || string.IsNullOrEmpty(message.Text)){
                return Task.FromResult(result);
            }
            try
            {
                var json = JObject.Parse(message.Text);
                var actionType = json.GetValue("ActionType").ToString();
                result = actionType.ToLowerInvariant().Equals(Action.ToLowerInvariant());
            }
            catch (JsonReaderException e)
            {
                result = false;
            }
            return Task.FromResult(result);
        }
    }
}