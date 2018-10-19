using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared;
using Microsoft.eShopOnContainers.Bot.API.Models.Catalog;
using BasketModels = Microsoft.eShopOnContainers.Bot.API.Models.Basket;
using OrderModels = Microsoft.eShopOnContainers.Bot.API.Models.Order;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public static class ActivityFactory
    {
        public static IMessageActivity RecipeCard(ITurnContext context, BasketModels.Basket basket)
        {
            var checkoutButton = new CardAction()
            {
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.BasketCheckout}'}}",
                Title = "Checkout"
            };

            var continueShoppingButton = new CardAction()
            {
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.ContinueShopping}'}}",
                Title = "Continue shopping"
            };
            var cardButtons = new List<CardAction>
            {
                checkoutButton,
                continueShoppingButton
            };

            decimal total = basket.Items.Sum(i => i.UnitPrice * i.Quantity);

            var basketCard = new ReceiptCard()
            {
                Title = "eShopAI receipt",
                Buttons = cardButtons,
                Items = UIHelper.CreateOrderBasketItemListReceipt(basket.Items),
                Total = $"{total} $"
            };

            var reply = context.Activity.CreateReply();
            reply.Attachments = new List<Attachment> { basketCard.ToAttachment() };
            return reply;
        }

        public static IMessageActivity OrdersCard(ITurnContext context, IList<OrderModels.Order> orders)
        {
            var reply = context.Activity.CreateReply();
            reply.Attachments = ReceiptOrders(reply, orders);
            //reply.SuggestedActions = new SuggestedActions()
            //{
            //    Actions = CardBackAction()
            //};

            return reply;
        }

        public static IMessageActivity OrderCard(ITurnContext context, OrderModels.Order order)
        {
            //if (!string.IsNullOrEmpty(order.Description))
            //{
            //    await context.SendActivityAsync($"{order.Description}");
            //}

            var reply = context.Activity.CreateReply();
            reply.Attachments = new List<Attachment>
            {
                BuildOrderCard(order).ToAttachment()
            };
            //reply.SuggestedActions = new SuggestedActions()
            //{
            //    Actions = CardBackAction()
            //};

            return reply;
        }

        public static IMessageActivity OrderNotSelectedCard(ITurnContext context)
        {
            var reply = context.Activity.CreateReply();
            reply.Text = "Please select an order";

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = CardBackAction()
            };
            return reply;
        }

        private static ReceiptCard BuildOrderCard(OrderModels.Order order)
        {
            return new ReceiptCard()
            {
                Title = $"Order #{order.OrderNumber}",
                Facts = BuildOrderFacts(order, showOrder: false),
                Items = UIHelper.CreateOrderItemListReceipt(order.OrderItems),
                Total = $"{order.Total} $"
            };
        }

        private static List<Attachment> ReceiptOrders(IMessageActivity reply, IList<OrderModels.Order> orders)
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

        private static List<CardAction> CardBackAction()
        {
            var cardActions = new List<CardAction>
            {
                UIHelper.CreateHomeButton()
            };
            return cardActions;
        }

        private static IList<Fact> BuildOrderFacts(OrderModels.Order order, bool showOrder = true)
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

    }

    public static class UIHelper
    {
        public static string ImageUrl { get; set; }

        public static string ReplacePictureUri(string pictureUri)
        {
            if (string.IsNullOrEmpty(pictureUri)) return pictureUri;
            var idx = pictureUri.IndexOf("/", 8);
            return ImageUrl + pictureUri.Substring(idx);
        }

        public static CardAction CreateHomeButton()
        {
            return new CardAction()
            {
                Title = "Home",
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.Back}'}}",
                DisplayText = "Home"
            };
        }

        public static CardAction CreateLoginButton()
        {
            return new CardAction()
            {
                Title = "Login",
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.Login}'}}",
                DisplayText = "Login"
            };
        }

        public static CardAction CreateShowMoreButton()
        {
            return new CardAction()
            {
                Title = "Show more",
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.NextPage}'}}",
                DisplayText = "Show more"
            };
        }

        public static CardAction CreateAnotherQueryButton()
        {
            return new CardAction()
            {
                Title = "Another query",
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.CatalogFilter}'}}",
                DisplayText = "Another query"
            };
        }

        public static ReceiptItem CreateOrderItemReceipt(Models.Order.OrderItem item)
        {
            return CreateReceiptItemCard(item.ProductName, item.PictureUrl, item.UnitPrice, item.Units);
        }

        public static ReceiptItem CreateBasketItemReceipt(Models.Basket.BasketItem basketItem)
        {
            return CreateReceiptItemCard(basketItem.ProductName, basketItem.PictureUrl, basketItem.UnitPrice, basketItem.Quantity);
        }

        private static ReceiptItem CreateReceiptItemCard(string productName, string productImageUrl,
            decimal unitPrice, int quantity)
        {
            return new ReceiptItem()
            {
                Title = productName,
                Image = new CardImage(url: $"{ReplacePictureUri(productImageUrl)}"),
                Price = $"{unitPrice}$",
                Quantity = $"{quantity}",
                Tap = null
            };
        }

        public static List<ReceiptItem> CreateOrderItemListReceipt(IEnumerable<Models.Order.OrderItem> orderItems)
        {
            var listReceipt = new List<ReceiptItem>();
            listReceipt.AddRange(orderItems.Select(c => CreateOrderItemReceipt(c)));
            return listReceipt;
        }

        public static List<ReceiptItem> CreateOrderBasketItemListReceipt(IEnumerable<Models.Basket.BasketItem> orderItems)
        {
            var listReceipt = new List<ReceiptItem>();
            listReceipt.AddRange(orderItems.Select(c => CreateBasketItemReceipt(c)));
            return listReceipt;
        }

        public static ThumbnailCard CreateCatalogItemCard(CatalogItem catalogItem, IList<CardAction> buttons, bool isMarkdownSupported)
        {
            return new ThumbnailCard()
            {
                Title = catalogItem.Name,
                Subtitle = isMarkdownSupported ? $"**{catalogItem.Price} $**" : $"{catalogItem.Price} $",
                Text = $"{catalogItem.Description}",
                Images = new List<CardImage>
                {
                    new CardImage(url: ReplacePictureUri(catalogItem.PictureUri))
                },
                Buttons = buttons
            };
        }

        public static IEnumerable<CardAction> BuildCardActions(IEnumerable<string> actions, string actionType)
        {
            foreach (var action in actions)
            {
                yield return new CardAction(actionType, title: action, value: action);
            }
        }
    }

}