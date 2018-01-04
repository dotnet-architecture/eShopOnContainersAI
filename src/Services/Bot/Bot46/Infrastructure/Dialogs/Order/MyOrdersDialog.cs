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
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class MyOrdersDialog : IDialog<object>
    {
        private readonly IOrderingService service = ServiceResolver.Get<IOrderingService>();

        public async Task StartAsync(IDialogContext context)
        {
            await ShowOrders(context);
        }

        private async Task ShowOrders(IDialogContext context)
        {
            AuthUser authUser = await context.GetAuthUser();

            if (authUser != null && !authUser.IsExpired)
            {
                await ShowOrders(context, authUser);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Call(new LoginDialog(), ExecutedLoginAsync);
            }
        }

        private async Task ShowOrders(IDialogContext context, AuthUser authUser)
        {
            var reply = context.MakeMessage();
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            var orders = await service.GetMyOrders(authUser.UserId, authUser.AccessToken);
            reply.Attachments = ReceiptOrders(reply, orders);

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = CardBackAction()
            };
            await context.PostAsync(reply);
        }

        private static List<CardAction> CardBackAction()
        {
            var cardActions = new List<CardAction>();
            cardActions.Add(new CardAction()
            {
                Title = "🏠",
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.Back}'}}"
            });
            return cardActions;
        }

        private List<Attachment> ReceiptOrders(IMessageActivity reply, List<Order> orders)
        {
            var attachements = new List<Attachment>();
            foreach (var order in orders)
            {
                List<CardAction> cardButtons = new List<CardAction>();

                CardAction plButton = new CardAction()
                {
                    Type = ActionTypes.PostBack,
                    Value = $@"{{ 'ActionType': '{BotActionTypes.OrderDetail}', 'OrderNumber': '{order.OrderNumber}' }}",
                    Title = "Detail"
                };
                cardButtons.Add(plButton);

                List<Fact> facts = OrderFacts(order);

                ReceiptCard plCard = new ReceiptCard()
                {
                    Title = $"Order Details",
                    Facts = facts,
                    Buttons = cardButtons,
                    Total = $"{order.Total} $"
                };

                attachements.Add(plCard.ToAttachment());
            }
            return attachements;
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
                        case BotActionTypes.OrderDetail:
                            // Todo Show OrderDetail
                            var OrderNumber = json.GetValue("OrderNumber").ToString(); ;
                            await ShowOrderDetail(context, OrderNumber);
                            context.Wait(MessageReceivedAsync);
                            break;
                        case BotActionTypes.Back:
                            context.Done<object>(null);
                            break;

                        default:
                            IMessageActivity reply = ReplyNoSelecction(context);
                            await context.PostAsync(reply);
                            context.Wait(MessageReceivedAsync);
                            break;

                    }

                }
                catch (JsonReaderException)
                {
                    // is not a Json
                    IMessageActivity reply = ReplyNoSelecction(context);
                    await context.PostAsync(reply);
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                IMessageActivity reply = ReplyNoSelecction(context);
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task ShowOrderDetail(IDialogContext context, string orderNumber)
        {
            AuthUser authUser = await context.GetAuthUser();
            var order = await service.GetOrder(orderNumber, authUser.AccessToken);
            OrderReceipt(context, order);
        }

        private void OrderReceipt(IDialogContext context, Order order)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            

            List<Fact> facts = OrderFacts(order);

            List<ReceiptItem> receiptList = OrderItems(order);

            ReceiptCard plCard = new ReceiptCard()
            {
                Title = $"Order #{order.OrderNumber}",
                Facts = facts,
                Items = receiptList,
                Total = $"{order.Total} $"
            };

            reply.Attachments.Add(plCard.ToAttachment());
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = CardBackAction()
            };

            context.PostAsync($"{order.Description}");
            context.PostAsync(reply);
        }

        private List<Fact> OrderFacts(Order order)
        {
            List<Fact> facts = new List<Fact>
            {
                new Fact($"Order:", $"{order.OrderNumber}"),
                new Fact($"Ship to:", $"{order.Buyer}"),
                new Fact($"Date:", $"{order.Date.ToShortDateString()}"),
                new Fact($"Status:", $"{order.Status}")
            };
            return facts;
        }

        private List<ReceiptItem> OrderItems(Order order)
        {
            List<ReceiptItem> receiptList = new List<ReceiptItem>();
            foreach (var item in order.OrderItems)
            {
                ReceiptItem lineItem = new ReceiptItem()
                {
                    Title = item.ProductName,
                    Image = new CardImage(url: $"{item.PictureUrl}"),
                    Price = $"{item.UnitPrice}$",
                    Quantity = $"{item.Units}",
                    Tap = null
                };
                receiptList.Add(lineItem);
            }

            return receiptList;
        }

        private static IMessageActivity ReplyNoSelecction(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Text = "Please select an order.";

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
                await ShowOrders(context);
            }
            else {
                await context.PostAsync("You must be logged in to show your orders.");
                context.Done<object>(null);
            }
        }


    }
}