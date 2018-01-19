using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
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

        public OrderDialog(IBasketService basketService, IOrderingService orderingService, 
            IIdentityService identityService, BotSettings botSettings)
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
                BuildOrderCard()
            };
            await context.PostAsync(reply);
        }

        private async Task GetOrder(IDialogContext context)
        {
            var botUserData = await  identityService.GetBotDataAsync(context);
            var authUser = botUserData.GetUserAuthData();
            var userData = botUserData.GetUserData();

            var basket = await basketService.GetBasket(authUser.UserId, authUser.AccessToken);
            var orderFromBasket = basketService.MapBasketToOrder(basket);
            order = orderingService.MapUserInfoIntoOrder(userData, orderFromBasket);
            order.CardExpirationShortFormat();
            order.RequestId = Guid.NewGuid();
        }

        private Attachment BuildOrderCard()
        {
            var orderNowButton = new List<CardAction>
            {
                new CardAction()
                {
                    Type = ActionTypes.PostBack,
                    Value = $@"{{ 'ActionType': '{BotActionTypes.OrderNow}'}}",
                    Title = "Order Now"
                }
            };

            decimal total = order.OrderItems.Sum(i => i.UnitPrice * i.Units);

            var orderCard = new ReceiptCard()
            {
                Title = "eShopAI Order",
                Buttons = orderNowButton,
                Items = UIHelper.CreateOrderItemListReceipt(order.OrderItems),
                Total = $"{total} $"
            };

            return orderCard.ToAttachment();
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
                        case BotActionTypes.OrderNow:
                            await OrderNowAsync(context);
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

        private async Task OrderNowAsync(IDialogContext context)
        {
            var authUser = await identityService.GetAuthUserAsync(context);
            var basket = orderingService.MapOrderToBasket(order);
            await basketService.Checkout(basket, authUser.AccessToken);
        }
    }
}