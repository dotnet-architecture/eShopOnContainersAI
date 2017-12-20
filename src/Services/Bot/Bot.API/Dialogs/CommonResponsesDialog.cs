using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.API.Dialogs
{
    public class CommonResponsesDialog : IDialog<object>
    {
        private readonly string _messageToSend;

        public CommonResponsesDialog(string message)
        {
            _messageToSend = message;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(_messageToSend);
            context.Done<object>(null);
        }
    }
}