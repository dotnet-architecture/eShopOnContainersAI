using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Models;
using Microsoft.Bots.Bot.API.Properties;
using Microsoft.Bots.Bot.API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Order = Microsoft.Bots.Bot.API.Models.Order.Order;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    [Serializable]
    public class OrderDialog : IDialog<object>
    {
        private readonly IBasketService basketService;
        private readonly IOrderingService orderingService;
        private readonly IIdentityService identityService;
        private readonly BotSettings botSettings;

        private Order order;

        public OrderDialog(IBasketService basketService, IOrderingService orderingService, IIdentityService identityService, BotSettings botSettings)
        {
            this.basketService = basketService;
            this.orderingService = orderingService;
            this.identityService = identityService;
            this.botSettings = botSettings;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowOrderCheckout(context);

            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowOrderCheckout(IDialogContext context)
        {
            await GetOrder(context);

            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>
            {
                ShowOrder()
            };
            await context.PostAsync(reply);
        }

        private async Task GetOrder(IDialogContext context)
        {
            var botUserData = await  identityService.GetBotDataAsync(context);
            var authUser = botUserData.GetUserAuthData();
            //TODO recheck
            UserData userData = botUserData.GetUserData();

            var basket = await basketService.GetBasket(authUser.UserId, authUser.AccessToken);
            var orderFromBasket = basketService.MapBasketToOrder(basket);
            order = orderingService.MapUserInfoIntoOrder(userData, orderFromBasket);
            order.CardExpirationShortFormat();
            order.RequestId = Guid.NewGuid();
        }

        private Attachment ShowOrder()
        {
            List<CardImage> cardImages = new List<CardImage>();
             cardImages.Add(new CardImage(url: $"{botSettings.MvcUrl}/images/brand.png"));

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction plButton = new CardAction()
            {
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.OrderNow}'}}",
                Title = "Order Now"
            };
            cardButtons.Add(plButton);

            List<ReceiptItem> receiptList = new List<ReceiptItem>();
            foreach (var item in order.OrderItems)
            {
                ReceiptItem lineItem = new ReceiptItem()
                {
                    Title = item.ProductName,
                    Subtitle = null,
                    Text = null,
                    Image = new CardImage(url: $"{item.PictureUrl}"),
                    Price = $"{item.UnitPrice}$",
                    Quantity = $"{item.Units}",
                    Tap = null
                };
                receiptList.Add(lineItem);
            }

            decimal total = order.OrderItems.Sum(i => i.UnitPrice * i.Units);

            ReceiptCard plCard = new ReceiptCard()
            {
                Title = "eShopAI Order",
                Buttons = cardButtons,
                Items = receiptList,
                Total = $"{total} $"
            };

            return plCard.ToAttachment();
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message != null && message.Type == ActivityTypes.Message && !string.IsNullOrEmpty(message.Text))
            {
                try
                {
                    var json = JObject.Parse(message.Text);
                    var action = json.GetValue("ActionType");
                    switch (action.ToString())
                    {
                        case BotActionTypes.OrderNow:
                            await OrderNow(context);
                            await context.PostAsync(TextResources.Your_order_has_been_processed);
                            context.Done<object>(false);
                            break;
                    }
                }
                catch (JsonReaderException)
                {
                    // is not a Json
                    await context.PostAsync(TextResources.Please_make_a_selection);
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // file sent
                await context.PostAsync(TextResources.Please_make_a_selection);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task OrderNow(IDialogContext context)
        {
            var authUser = await identityService.GetAuthUserAsync(context);
            var basket = orderingService.MapOrderToBasket(order);
            await basketService.Checkout(basket, authUser.AccessToken);
        }
    }
}