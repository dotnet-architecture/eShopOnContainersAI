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
    public class MyOrdersDialog : IDialog<object>
    {
        private readonly IDialogFactory dialogFactory;
        private readonly IOrderingService orderingService;
        private readonly IIdentityService identityService;
        private readonly BotSettings botSettings;

        public bool LatestOrder { get; internal set; }

        public MyOrdersDialog(IDialogFactory dialogFactory, IOrderingService orderingService,
            IIdentityService identityService,
            BotSettings botSettings, bool latestOrder = false)
        {
            this.dialogFactory = dialogFactory;
            this.orderingService = orderingService;
            this.identityService = identityService;
            this.botSettings = botSettings;
            this.LatestOrder = latestOrder;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowOrders(context);
        }

        private async Task ShowOrders(IDialogContext context)
        {
            AuthUser authUser = await identityService.GetAuthUserAsync(context);

            if (authUser != null && !authUser.IsExpired)
            {
                await ShowDesired(context, authUser);
            }
            else
            {
                context.Call(dialogFactory.CreateLoginDialog(), ExecutedLoginAsync);
            }
        }

        private async Task ShowDesired(IDialogContext context, AuthUser authUser)
        {
            if (!LatestOrder)
            {
                await ShowOrders(context, authUser);
            }
            else
            {
                var orders = await orderingService.GetMyOrders(authUser.UserId, authUser.AccessToken);
                var order = orders.OrderByDescending(o => o.OrderNumber).FirstOrDefault();
                if (order != null)
                {
                    await PostOrderDetailAsync(context, order.OrderNumber);
                    await context.PostAsync(TextResources.Type_what_do_you_want_to_do);
                    context.Done<object>(null);

                }
                else
                {
                    await context.PostAsync(TextResources.You_do_not_have_any_orders);
                    await context.PostAsync(TextResources.Type_what_do_you_want_to_do);
                    context.Done<object>(null);
                }
            }
        }

        private async Task ShowOrders(IDialogContext context, AuthUser authUser)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = context.Activity.IsSkypeChannel() ? AttachmentLayoutTypes.List : AttachmentLayoutTypes.Carousel;
            var orders = await orderingService.GetMyOrders(authUser.UserId, authUser.AccessToken);
            if (orders.Count > 0)
            {
                reply.Attachments = ReceiptOrders(reply, orders);
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = CardBackAction()
                };
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                await context.PostAsync(TextResources.You_do_not_have_any_orders);
                await context.PostAsync(TextResources.Type_what_do_you_want_to_do);
                context.Done<object>(null);
            }
        }

        private static List<CardAction> CardBackAction()
        {
            var cardActions = new List<CardAction>
            {
                UIHelper.CreateHomeButton()
            };
            return cardActions;
        }

        private List<Attachment> ReceiptOrders(IMessageActivity reply, List<Order> orders)
        {
            var attachements = new List<Attachment>();
            foreach (var order in orders)
            {
                var orderDetailsButtons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Type = ActionTypes.PostBack,
                        Value = $@"{{ 'ActionType': '{BotActionTypes.OrderDetail}', 'OrderNumber': '{order.OrderNumber}' }}",
                        Title = "Detail"
                    }
                };

                var orderDetailsCard = new ReceiptCard()
                {
                    Title = $"Order Details",
                    Facts = BuildOrderFacts(order),
                    Buttons = orderDetailsButtons,
                    Total = $"{order.Total} $"
                };

                attachements.Add(orderDetailsCard.ToAttachment());
            }
            return attachements;
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
                        case BotActionTypes.OrderDetail:
                            var OrderNumber = json.GetValue("OrderNumber").ToString(); ;
                            await PostOrderDetailAsync(context, OrderNumber);
                            context.Wait(MessageReceivedAsync);
                            break;
                        case BotActionTypes.Back:
                            await context.PostAsync(TextResources.Type_what_do_you_want_to_do);
                            context.Done<object>(null);
                            break;

                        default:
                            IMessageActivity reply = ReplyNoSelection(context);
                            await context.PostAsync(reply);
                            context.Wait(MessageReceivedAsync);
                            break;

                    }

                }
                catch (JsonReaderException)
                {
                    // invalid json
                    IMessageActivity reply = ReplyNoSelection(context);
                    await context.PostAsync(reply);
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                IMessageActivity reply = ReplyNoSelection(context);
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task PostOrderDetailAsync(IDialogContext context, string orderNumber)
        {
            var authUser = await identityService.GetAuthUserAsync(context);
            var order = await orderingService.GetOrder(orderNumber, authUser.AccessToken);
            await OrderReceipt(context, order);
        }

        private async Task OrderReceipt(IDialogContext context, Order order)
        {
            if (!string.IsNullOrEmpty(order.Description))
            {
                await context.PostAsync($"{order.Description}");
            }

            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>
            {
                BuildOrderCard(order).ToAttachment()
            };
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = CardBackAction()
            };

            await context.PostAsync(reply);
        }

        private ReceiptCard BuildOrderCard(Order order)
        {
            return new ReceiptCard()
            {
                Title = $"Order #{order.OrderNumber}",
                Facts = BuildOrderFactsDetail(order),
                Items = UIHelper.CreateOrderItemListReceipt(order.OrderItems),
                Total = $"{order.Total} $"
            };
        }

        private List<Fact> BuildOrderFactsDetail(Order order)
        {
            return BuildOrderFacts(order, showOrder: false);
        }

        private List<Fact> BuildOrderFacts(Order order, bool showOrder = true)
        {
            List<Fact> facts = new List<Fact>
            {
                new Fact($"Date:", $"{order.Date.ToShortDateString()}"),
                new Fact($"Status:", $"{order.Status}")
            };

            if (showOrder)
                facts.Insert(0, new Fact($"Order:", $"{order.OrderNumber}"));

            return facts;
        }

        private static IMessageActivity ReplyNoSelection(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Text = TextResources.Please_select_an_order;

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = CardBackAction()
            };
            return reply;
        }

        private async Task ExecutedLoginAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var loginResult = await result;
            if (loginResult)
            {

                AuthUser authUser = await identityService.GetAuthUserAsync(context);
                await ShowDesired(context, authUser);
            }
            else
            {
                await context.PostAsync(TextResources.You_must_be_logged_to_ckeck_your_orders);
                context.Done<object>(null);
            }
        }
    }
}