using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.API.Extensions;

namespace Bot.API.Dialogs
{
    [Serializable]
    public class LoginDialog : IDialog<object>
    {
        private bool _isUserDataShown;
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task SendUserData(IDialogContext context){         
            var userid =  context.UserData.GetValue<string>("userAppId");
            var token =  context.UserData.GetValue<string>("access_token");
            var expires =  context.UserData.GetValue<string>("expires_at");
            await context.PostAsync($"**userid:** {userid} <br/> **token:** {token} <br/>  **expires (UTC):** {expires}");
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
            if(context.IsAuthenticated())
            {     
                if(!_isUserDataShown){          
                    await SendUserData(context);                  
                    _isUserDataShown = true;
                }          
                else{                  
                    await context.PostAsync($"Echo: {message.Text}"); 
                }        
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
