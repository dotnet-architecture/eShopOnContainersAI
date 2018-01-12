using System;
using System.Linq;
using System.Threading.Tasks;
using Bot46.API.Infrastructure.Models;
using Bot46.API.Infrastructure.Modules;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class RootDialog : LuisDialog<object>
    {
        private static readonly ICatalogService serviceCatalog = ServiceResolver.Get<ICatalogService>();

        public RootDialog(ILuisService luis) : base(luis) {

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
            context.Call(new LoginDialog(), ResumeAfterLoginDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Catalog")]
        public async Task Catalog(IDialogContext context, LuisResult result)
        {
            var catalogdialog = new CatalogDialog();
            if(result.Entities.Count > 0)
            {
                await SetCatalogFilter(result, catalogdialog);
            }
            context.Call(catalogdialog, ResumeAfterDialog);
        }

        private static async Task SetCatalogFilter(LuisResult result, CatalogDialog catalogdialog)
        {
            CatalogFilter filter = new CatalogFilter();
            var brand = result.Entities.Where(e => e.Type.Equals("brand")).SingleOrDefault();
            if (brand != null)
            {
                var brands = await serviceCatalog.GetBrands();
                var brandSelected = brands.Where(b => b.Text.Equals(brand.Entity, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (brandSelected != null)
                {
                    filter.Brand = Convert.ToInt32(brandSelected.Id);
                }
            }

            var type = result.Entities.Where(e => e.Type.Equals("type")).SingleOrDefault();
            if (type != null)
            {
                var types = await serviceCatalog.GetTypes();
                var typeSelected = types.Where(b => b.Text.Equals(type.Entity, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
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
            context.Call(new BasketDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Orders")]
        public Task Orders(IDialogContext context, LuisResult result)
        {
            context.Call(new MyOrdersDialog(), ResumeAfterDialog);
            return Task.CompletedTask;
        }

        [LuisIntent("Order")]
        public Task Order(IDialogContext context, LuisResult result)
        {
            var ordersDialog = new MyOrdersDialog();
            ordersDialog.LatestOrder = true;
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