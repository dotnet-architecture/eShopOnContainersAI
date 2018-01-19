using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Models;
using Microsoft.Bots.Bot.API.Models.Basket;
using Microsoft.Bots.Bot.API.Models.Catalog;
using Microsoft.Bots.Bot.API.Properties;
using Microsoft.Bots.Bot.API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    [Serializable]
    public class CatalogDialog : IDialog<object>
    {
        private readonly IDialogFactory dialogFactory;
        private readonly ICatalogService catalogService;
        private readonly IBasketService basketService;
        private readonly ICatalogAIService catalogAIService;
        private readonly IIdentityService identityService;
        private readonly int _itemsPage = 10;
        private int _currentPage = 0;
        internal CatalogFilter _filter = null;

        public JObject ItemToBuy { get; private set; }

        public CatalogDialog(IDialogFactory dialogFactory, IBasketService basketService, 
            ICatalogService catalogService, ICatalogAIService catalogAIService, 
            IIdentityService identityService)
        {
            this.dialogFactory = dialogFactory;
            this.catalogService = catalogService;
            this.basketService = basketService;
            this.catalogAIService = catalogAIService;
            this.identityService = identityService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (_filter == null)
            {
                context.Call(dialogFactory.CreateCatalogFilterDialog(), ExecutedCatalogFilterAsync);
            }
            else
            {
                await ShowCatalog(context);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async  Task ExecutedCatalogFilterAsync(IDialogContext context, IAwaitable<CatalogFilter> result)
        {
            _filter = await result;            
            await ShowCatalog(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowCatalog(IDialogContext context)
        {
            var logged = await identityService.IsAuthenticatedAsync(context);
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            Catalog catalog;
            if(_filter.Tags != null)
            {
                if (_filter.Tags.Any())
                    catalog = await catalogAIService.GetCatalogItems(_currentPage, _itemsPage, _filter.Brand, _filter.Type, _filter.Tags);
                else
                    catalog = Catalog.Empty;
            }
            else
            {
                 catalog = await catalogService.GetCatalogItems(_currentPage, _itemsPage, _filter.Brand, _filter.Type);
            }

            int pageCount = (catalog.Count + _itemsPage - 1) / _itemsPage;
            if (catalog.Count != 0)
            {                 
                reply.Text = $"Page {_currentPage + 1} of {pageCount} ( {catalog.Count} items )";
                reply.Attachments = CatalogCarousel(catalog, logged);
            }
            else
            {
                reply.Text = TextResources.There_are_no_results_matching_your_search;
            }


            List<CardAction> cardActions = CardActions(pageCount, logged);
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = cardActions
            };

            await context.PostAsync(reply);
        }

        private List<CardAction> CardActions(int pageCount, bool logged)
        {
            var cardActions = new List<CardAction>();

            cardActions.Add(new CardAction()
            {
                Title = "🏠",
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.Back}'}}"
            });

            if (!logged)
            {
                cardActions.Add(new CardAction()
                {
                    Title = "👤",
                    Type = ActionTypes.PostBack,
                    Value = $@"{{ 'ActionType': '{BotActionTypes.Login}'}}"
                });
            }

            if (_currentPage + 1 < pageCount)
            {
                cardActions.Add(new CardAction()
                {
                    Title = "Show more",
                    Type = ActionTypes.PostBack,
                    Value = $@"{{ 'ActionType': '{BotActionTypes.NextPage}'}}"
                });
            }

            return cardActions;
        }

        private List<Attachment> CatalogCarousel(Catalog catalog, bool logged){
            var attachments = new List<Attachment>();
            foreach(var item in catalog.Data)
            {
                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url:item.PictureUri ));
                List<CardAction> cardButtons = new List<CardAction>();
                if (logged)
                {
                    CardAction plButton = new CardAction()
                    {
                        Value = $@"{{ 'ActionType': '{BotActionTypes.AddBasket}', 'ProductId': '{item.Id}' , 'ProductName': '{item.Name}', 'PictureUrl': '{item.PictureUri}', 'UnitPrice': '{item.Price}'}}",
                        Type = "postBack",
                        Title = "Add to cart"
                    };
                    cardButtons.Add(plButton);
                }
                ThumbnailCard  plCard = new ThumbnailCard ()
                {
                    Title = item.Name,
                    Subtitle = $"**{item.Price} $**",
                    Text = $"{item.Description}",
                    Images = cardImages,
                    Buttons = cardButtons
                };

                attachments.Add(plCard.ToAttachment());
            }

            List<CardImage> moreImages = new List<CardImage>();
            // Todo add more image
            //moreImage.Add(new CardImage(url: item.PictureUri));

            List<CardAction> moreButtons = new List<CardAction>();
            CardAction moreButton = new CardAction()
            {
                Value = $@"{{ 'ActionType': '{BotActionTypes.NextPage}'}}",
                Type = "postBack",
                Title = "More"
            };
            moreButtons.Add(moreButton);
            ThumbnailCard moreCard = new ThumbnailCard()
            {
                Title = "More",
                Text = $"Show more items",
                Images = moreImages,
                Buttons = moreButtons
            };


            return attachments;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            
                if (message.IsValidTextMessage())
                {
                    try
                    {
                        var json = JObject.Parse(message.Text);
                        var action = json.GetValue("ActionType");
                        switch (action.ToString())
                        {
                            case BotActionTypes.NextPage:
                                _currentPage++;
                                await ShowCatalog(context);
                                context.Wait(MessageReceivedAsync);
                                break;
                            case BotActionTypes.PreviousPage:
                                _currentPage--;
                                await ShowCatalog(context);
                                context.Wait(MessageReceivedAsync);
                                break;
                            case BotActionTypes.AddBasket:
                                await AskQuantity(context, json);
                                break;
                            case BotActionTypes.Login:
                                context.Call(dialogFactory.CreateLoginDialog(), LoginReceivedAsync);
                                break;
                            case BotActionTypes.Back:
                                await context.PostAsync(TextResources.Type_what_do_you_want_to_do);
                                context.Done<object>(null);
                                break;
                        }
                    }
                    catch (JsonReaderException)
                    {
                        // not valid JSON object
                        await context.PostAsync(TextResources.Please_make_a_selection);
                        await ShowCatalog(context);
                        context.Wait(MessageReceivedAsync);

                    }
                }
                else {
                    await context.PostAsync(TextResources.Please_make_a_selection);
                    await ShowCatalog(context);
                    context.Wait(MessageReceivedAsync);
                }
           
        }

        private async Task AskQuantity(IDialogContext context, JObject json)
        {
            ItemToBuy = json;
            var producName = json.GetValue("ProductName").ToString();
            await context.PostAsync(string.Format(TextResources.How_many_do_you_want_to_buy, producName));
            context.Wait(QuantityReceivedAsync);
        }

        private async Task QuantityReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            int quantity = 1;
            if (Int32.TryParse(message.Text, out quantity))
            {
                await AddToBasket(context, ItemToBuy, quantity);
            }
            else
            {
                await context.PostAsync(TextResources.Please_type_a_number);
                context.Wait(QuantityReceivedAsync);
            }
        }

        private async Task LoginReceivedAsync(IDialogContext context, IAwaitable<bool> result)
        {
            await ShowCatalog(context);
            context.Wait(MessageReceivedAsync);
        }

        private async Task AddToBasket(IDialogContext context, JObject json, int quantity)
        {
            BotData userData = await identityService.GetBotDataAsync(context);
            AuthUser authUser = userData.GetUserAuthData();
            // TODO Check Expired
            if (authUser != null)
            {
                var reply = context.MakeMessage();
                var producName = json.GetValue("ProductName").ToString();
                var product = new BasketItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = quantity,
                    ProductName = producName,
                    PictureUrl = json.GetValue("PictureUrl").ToString(),
                    UnitPrice = json.GetValue("UnitPrice").ToObject<decimal>(),
                    ProductId = json.GetValue("ProductId").ToString(),
                };
                await basketService.AddItemToBasket(authUser.UserId, product, authUser.AccessToken);
                reply.Text = string.Format(TextResources.You_have_added_to_your_basket, producName);
                context.Call(dialogFactory.CreateBasketDialog(), BasketAsync);
            }
        }

        private async Task BasketAsync(IDialogContext context, IAwaitable<object> result)
        {
            await ShowCatalog(context);
            context.Wait(MessageReceivedAsync);
        }
    }
}