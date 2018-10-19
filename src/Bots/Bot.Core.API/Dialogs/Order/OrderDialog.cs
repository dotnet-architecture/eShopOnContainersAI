using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared;
using Microsoft.eShopOnContainers.Bot.API.Services.Basket;
using Microsoft.eShopOnContainers.Bot.API.Services.Order;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrderModels = Microsoft.eShopOnContainers.Bot.API.Models.Order;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Order
{
    public class OrderDialog : ComponentDialog
    {
        public const string Name = nameof(OrderDialog) + ".MainDriver";

        private const string OrderCacheKey = "OrderCache";

        private readonly AppSettings settings;
        private readonly DomainPropertyAccessors accessors;
        private readonly IBasketService basketService;
        private readonly IOrderingService orderingService;
        private readonly ILogger<OrderDialog> logger;

        public OrderDialog(DomainPropertyAccessors accessors, IBasketService basketService, IOrderingService orderingService, 
            IOptions<AppSettings> settings, ILogger<OrderDialog> logger) : base(Name)
        {
            this.settings = settings.Value;
            this.accessors = accessors;
            this.basketService = basketService;
            this.orderingService = orderingService;
            this.logger = logger;
            AddDialog(new WaterfallDialog(Name, new WaterfallStep[] { ShowOrderCheckout, ProcessOrderCheckout }));

            InitialDialogId = Name;
        }

        private async Task<DialogTurnResult> ShowOrderCheckout(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var order = await GetOrder(dc.Context);

            var reply = dc.Context.Activity.CreateReply();
            reply.Attachments = new List<Attachment>
            {
                BuildOrderCard(order)
            };
            await dc.Context.SendActivityAsync(reply);
            dc.Values[OrderCacheKey] = order;
            return EndOfTurn;
        }

        private async Task<OrderModels.Order> GetOrder(ITurnContext context)
        {
            var authUser = await accessors.AuthUserProperty.GetAsync(context);
            var userData = await accessors.UserDataProperty.GetAsync(context);

            var basket = await basketService.GetBasket(authUser.UserId, authUser.AccessToken);
            var orderFromBasket = basketService.MapBasketToOrder(basket);
            var order = orderingService.MapUserInfoIntoOrder(userData, orderFromBasket);
            order.CardExpirationShortFormat();
            order.RequestId = Guid.NewGuid();
            return order;
        }

        private Attachment BuildOrderCard(OrderModels.Order order)
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

        private async Task<DialogTurnResult> ProcessOrderCheckout(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var message = dc.Context.Activity.AsMessageActivity();
            if (message != null && JObjectHelper.TryParse(message.Text, out JObject json, logger))
            {
                var action = json.GetValue("ActionType");
                switch (action.ToString())
                {
                    case BotActionTypes.OrderNow:
                        var order = dc.Values[OrderCacheKey] as OrderModels.Order;
                        await OrderNowAsync(dc.Context, order);
                        await dc.Context.SendActivityAsync(MessageFactory.Text("Your order has been processed"));
                        break;
                }
            }
            return await dc.EndDialogAsync();
        }

        private async Task OrderNowAsync(ITurnContext context, OrderModels.Order order)
        {
            var authUser = await accessors.AuthUserProperty.GetAsync(context);
            var basket = orderingService.MapOrderToBasket(order);
            await basketService.Checkout(basket, authUser.AccessToken);
        }
    }
}