using Bot46.API.Infrastructure.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class LoginDialog : IDialog<object>
    {
        private bool _isUserDataShown;

        public async Task StartAsync(IDialogContext context)
        {
            if (await context.IsAuthenticated())
            {
                context.Done<object>(true);
            }
            else
            {
                await SendAuthCard(context);
            }
            context.Wait(MessageReceivedAsync);
        }

        private async Task SendUserData(IDialogContext context){
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>();
            message.Attachments.Add(await context.UserCard());
            await context.PostAsync(message);
        }

        private async Task SendAuthCard(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment>();
            message.Attachments.Add(context.LoginCard());
            await context.PostAsync(message);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> messageActivity)
        {
            var message = await messageActivity;
            await HandelAttachements(context, message);
            if(await context.IsAuthenticated())
            {
                context.Done<object>(true);
            }
            else{
                await SendAuthCard(context);
            }
            context.Wait(MessageReceivedAsync);
        }

        private static async Task HandelAttachements(IDialogContext context, IMessageActivity message)
        {
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];
                var client = new ConnectorClient(new Uri(context.Activity.ServiceUrl), new MicrosoftAppCredentials());
                var content = await client.HttpClient.GetByteArrayAsync(attachment.ContentUrl);
            }
        }
    }
}
