using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.API.Services;
using Bot.API.ViewModels;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.API.Dialogs
{
    [Serializable]
    public class CatalogDialog : IDialog<object>
    {
  

        private static readonly ICatalogService service = ServiceResolver.Get<ICatalogService>();
        private readonly int _itemsPage = 10;
        private int _currentPage = 1;
        private readonly CatalogFilter _filter = new CatalogFilter(){ Brand = "-1", Type = "-1"};

        public CatalogDialog(){

        }

         public Task StartAsync(IDialogContext context)
        {
            context.PostAsync($"[CatalogDialog] I am the Catalog dialog.");
            context.PostAsync($"Filter Faked to Brand: All  and Type: All.");
            context.Call(new CatalogFilterDialog(), ExecutedCatalogFilterAsync);

            return Task.CompletedTask;
        }

        private async  Task ExecutedCatalogFilterAsync(IDialogContext context, IAwaitable<CatalogFilter> result)
        {
            //    var catalogFilter = await result;  
            
            await ShowCatalog(context);

            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowCatalog(IDialogContext context){
            int? brand = null;
            int? type = null;
            if(!_filter.Brand.Equals("-1"))
            {
                brand = Convert.ToInt32(_filter.Brand);
            }
            if(!_filter.Type.Equals("-1"))
            {              
                type = Convert.ToInt32(_filter.Type);
            }

            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            
            var catalog = await service.GetCatalogItems(_currentPage, _itemsPage, brand, type);
            int pageCount = (catalog.Count + _itemsPage - 1) / _itemsPage;

            reply.Text = $"Page {_currentPage} of {pageCount} ( {catalog.Count} items )";

            var cardActions = new List<CardAction>();
            if(_currentPage > 1)
            {
                cardActions.Add(new CardAction(){   Title = "◀ Previous",
                                        Type = ActionTypes.PostBack, 
                                        Value="{ 'ActionType': 'PreviousPage'}" });
            }
            if(_currentPage < pageCount)
            {
                cardActions.Add(new CardAction(){   Title = "Next ▶", 
                                        Type = ActionTypes.PostBack,
                                        Value ="{ 'ActionType': 'NextPage'}" });
            }

            reply.Attachments = CatalogCarousel(catalog);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = cardActions
            };

            await context.PostAsync(reply);
        }

        private List<Attachment> CatalogCarousel(Catalog catalog){
            var attachments = new List<Attachment>();
            foreach(var item in catalog.Data)
            {
                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url:item.PictureUri ));

                List<CardAction> cardButtons = new List<CardAction>();
                CardAction plButton = new CardAction()
                {
                    Value = $@"{{ 'ActionType': 'AddBasket', 'ItemId': '{item.Id}'  }}",
                    Type = "postBack",
                    Title = "Buy"
                };
                cardButtons.Add(plButton);

                ThumbnailCard  plCard = new ThumbnailCard ()
                {
                    Title = item.Name,
                    Subtitle = $"**{item.Price}$**",
                    Text = $"{item.Description}",
                    Images = cardImages,
                    Buttons = cardButtons
                };

                attachments.Add(plCard.ToAttachment());
            }
            return attachments;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            try
            {
                var json = JObject.Parse(message.Text);
                var action = json.GetValue("ActionType");
                switch(action.ToString())
                {
                    case "AddBasket":                
                        await context.PostAsync($"Action:{action} Item:{json.GetValue("ItemId")}");
                        break;
                    case "NextPage":  
                        _currentPage++;                       
                        await ShowCatalog(context);
                        break;
                    case "PreviousPage":               
                        _currentPage--;                      
                        await ShowCatalog(context);
                        break;
                }
            }
            catch(JsonReaderException e)
            {
                // is not a Json
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}