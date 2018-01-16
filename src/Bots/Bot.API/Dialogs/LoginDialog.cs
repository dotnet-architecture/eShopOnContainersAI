using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    [Serializable]
    public class LoginDialog : IDialog<bool>
    {
        private readonly IOIDCClient oidcClient;
        private readonly IIdentityService identityService;

        public LoginDialog(IOIDCClient oidcClient, IIdentityService identityService)
        {
            this.oidcClient = oidcClient;
            this.identityService = identityService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (await identityService.IsAuthenticatedAsync(context))
            {
                context.Done(true);
            }
            else
            {
                await SendAuthCard(context);
            }
            context.Wait(MessageReceivedAsync);
        }

        private async Task SendAuthCard(IDialogContext context)
        {
            var message = context.MakeMessage();
            message.Attachments = new List<Attachment> {await context.Activity.CreateLoginCardAsync(oidcClient)};
            await context.PostAsync(message);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> messageActivity)
        {
            var message = await messageActivity;
            await HandleAttachmentsAsync(context, message);
            if(await identityService.IsAuthenticatedAsync(context))
            {
                context.Done(true);
            }
            else{
                await SendAuthCard(context);
                context.Wait(MessageReceivedAsync);
            }
        }

        private static async Task HandleAttachmentsAsync(IDialogContext context, IMessageActivity message)
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
