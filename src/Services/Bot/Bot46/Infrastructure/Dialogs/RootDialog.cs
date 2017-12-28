using System;
using System.Threading;
using System.Threading.Tasks;
using Bot46.API.Infrastructure.Helpers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        public RootDialog(ILuisService luis) : base(luis) {

        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type '/help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }
        
        [LuisIntent("Login")]
        public Task Login(IDialogContext context, LuisResult result)
        {
            context.Call(new LoginDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }
        
        [LuisIntent("Catalog")]
        public Task Catalog(IDialogContext context, LuisResult result)
        {
            context.Call(new CatalogDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Basket")]
        public Task Basket(IDialogContext context, LuisResult result)
        {
            context.Call(new BasketDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        private Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceived);
            return Task.CompletedTask;
        }
    }
}