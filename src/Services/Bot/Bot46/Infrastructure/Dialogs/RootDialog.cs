using System;
using System.Threading.Tasks;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        private readonly ICatalogService catalogService;
        private readonly IBasketService basketService;
        private readonly ICatalogAIService catalogAIService;
        private readonly IComputerVisionService computerVisionService;

        public RootDialog(ICatalogService catalogService, IBasketService basketService, 
            ICatalogAIService catalogAIService, IComputerVisionService computerVisionService, 
            ILuisService luis) : base(luis) {
            this.catalogService = catalogService;
            this.basketService = basketService;
            this.catalogAIService = catalogAIService;
            this.computerVisionService = computerVisionService;
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
            context.Call(new LoginDialog(), ResumeAfterLoginDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Catalog")]
        public Task Catalog(IDialogContext context, LuisResult result)
        {
            context.Call(new CatalogDialog(catalogService, basketService, catalogAIService, computerVisionService), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Basket")]
        public Task Basket(IDialogContext context, LuisResult result)
        {
            context.Call(new BasketDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Order")]
        public Task Order(IDialogContext context, LuisResult result)
        {
            context.Call(new MyOrdersDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        private Task ResumeAfterDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceived);
            return Task.CompletedTask;
        }

        private Task ResumeAfterLoginDialog(IDialogContext context, IAwaitable<bool> result)
        {
            context.Wait(MessageReceived);
            return Task.CompletedTask;
        }
    }
}