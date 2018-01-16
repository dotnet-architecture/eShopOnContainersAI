using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bots.Bot.API.Models.Catalog;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    public static class EntityExtensions
    {
        public static string NormalizeEntity(this string self)
        {
            return self.Replace(". ", ".");
        }
    }

    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        private readonly IDialogFactory dialogFactory;

        private readonly ICatalogService catalogService;

        public RootDialog(ICatalogService catalogService, IDialogFactory dialogFactory,
            ILuisService luis) : base(luis) {
            this.dialogFactory = dialogFactory;
            this.catalogService = catalogService;
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type '/help' if you need assistance.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        
        [LuisIntent("Login")]
        public Task Login(IDialogContext context, LuisResult result)
        {
            context.Call(dialogFactory.CreateLoginDialog(), ResumeAfterLoginDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Catalog")]
        public async Task Catalog(IDialogContext context, LuisResult result)
        {
            var catalogDialog = dialogFactory.CreateCatalogDialog();
            if (result.Entities.Count > 0)
            {
                await SetCatalogFilter(result, catalogDialog);
            }
            context.Call(catalogDialog, ResumeAfterDialog);
        }

        private async Task SetCatalogFilter(LuisResult result, CatalogDialog catalogdialog)
        {
            CatalogFilter filter = new CatalogFilter();
            var brand = result.Entities.SingleOrDefault(e => e.Type.Equals("brand"));
            if (brand != null)
            {
                var brands = await catalogService.GetBrands();
                var brandSelected = brands.FirstOrDefault(b => b.Text.Equals(brand.Entity.NormalizeEntity(), StringComparison.OrdinalIgnoreCase));
                if (brandSelected != null)
                {
                    filter.Brand = Convert.ToInt32(brandSelected.Id);
                }
            }

            var type = result.Entities.SingleOrDefault(e => e.Type.Equals("type"));
            if (type != null)
            {
                var types = await catalogService.GetTypes();
                var typeSelected = types.FirstOrDefault(b => b.Text.Equals(type.Entity.NormalizeEntity(), StringComparison.OrdinalIgnoreCase));
                if (typeSelected != null)
                {
                    filter.Type = Convert.ToInt32(typeSelected.Id);
                }
            }
            if (filter.Brand != null || filter.Type != null)
                catalogdialog._filter = filter;
        }

        [LuisIntent("Basket")]
        public Task Basket(IDialogContext context, LuisResult result)
        {
            context.Call(dialogFactory.CreateBasketDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Orders")]
        public Task Orders(IDialogContext context, LuisResult result)
        {
            context.Call(dialogFactory.CreateMyOrdersDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Order")]
        public Task Order(IDialogContext context, LuisResult result)
        {
            var ordersDialog = dialogFactory.CreateMyOrdersDialog(latestOrder:true);
            context.Call(ordersDialog, ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Hello")]
        public async Task Hello(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi. How can I help you?");
            context.Wait(MessageReceived);
        }


        [LuisIntent("Bye")]
        public async Task Bye(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Bye bye, remember I am here to help you when ever you need me.");
            context.Wait(MessageReceived);
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