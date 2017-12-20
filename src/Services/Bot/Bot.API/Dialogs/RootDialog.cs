using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot.API.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
         public Task StartAsync(IDialogContext context)
        {
            context.PostAsync($"[RootDialog] I am the root dialog.");

            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;
            if (activity != null && activity.Type == ActivityTypes.Message)
            {
                int length = (activity.Text ?? string.Empty).Length;
                await context.PostAsync($"[RootDialog] You sent {activity.Text} which was {length} characters");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}