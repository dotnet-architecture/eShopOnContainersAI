using Bot46.API.Infrastructure.Extensions;
using Bot46.API.Infrastructure.Models;
using Bot46.API.Infrastructure.Modules;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class OrderDialog : IDialog<object>
    {
        private readonly IBasketService serviceBasket = ServiceResolver.Get<IBasketService>();
        private readonly IOrderingService serviceOrder = ServiceResolver.Get<IOrderingService>();

        private Order order;

        public async Task StartAsync(IDialogContext context)
        {
            await ShowOrderCheckout(context);

            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowOrderCheckout(IDialogContext context)
        {
            await GetOrder(context);

            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            reply.Attachments.Add(ShowOrder());
            await context.PostAsync(reply);
        }

        private async Task GetOrder(IDialogContext context)
        {
            var botUserData = await  context.GetUserData();
            AuthUser authUser = botUserData.GetProperty<AuthUser>("authUser");
            UserData userData = botUserData.GetProperty<UserData>("userData");

            var basket = await serviceBasket.GetBasket(authUser.UserId, authUser.AccessToken);
            var orderFromBasket = serviceBasket.MapBasketToOrder(basket);
            order = serviceOrder.MapUserInfoIntoOrder(userData, orderFromBasket);
            order.CardExpirationShortFormat();
            order.RequestId = Guid.NewGuid();
        }

        private Attachment ShowOrder()
        {
            List<CardImage> cardImages = new List<CardImage>();
            // TODO EShop Logo
            // cardImages.Add(new CardImage(url: "https://<imageUrl1>"));

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
                Title = "EShop Order",
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
                            await context.PostAsync("Your order has been processed.");
                            context.Done<object>(false);
                            break;
                    }
                }
                catch (JsonReaderException)
                {
                    // is not a Json
                    await context.PostAsync("Please make a selection");
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // file sent
                await context.PostAsync("Please make a selection");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task OrderNow(IDialogContext context)
        {
            AuthUser authUser = await context.GetAuthUser();
            var basket = serviceOrder.MapOrderToBasket(order);
            await serviceBasket.Checkout(basket, authUser.AccessToken);
        }
    }
}