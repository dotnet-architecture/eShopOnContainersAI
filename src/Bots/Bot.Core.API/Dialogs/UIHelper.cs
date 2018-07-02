using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Models.Catalog;
using System.Collections.Generic;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public static class UIHelper
    {
        //private static string _imageUrl;

        //static UIHelper()
        //{
        //    _imageUrl = ConfigurationManager.AppSettings["ImageUrl"];
        //}

        public static string ReplacePictureUri(string pictureUri)
        {
            return pictureUri;
            //if (string.IsNullOrEmpty(pictureUri)) return pictureUri;
            //var idx = pictureUri.IndexOf("/", 8);
            //return _imageUrl + pictureUri.Substring(idx);
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

        //public static ReceiptItem CreateOrderItemReceipt(OrderItem item)
        //{
        //    return CreateReceiptItemCard(item.ProductName, item.PictureUrl, item.UnitPrice, item.Units);
        //}

        //public static ReceiptItem CreateBasketItemReceipt(BasketItem basketItem)
        //{
        //    return CreateReceiptItemCard(basketItem.ProductName, basketItem.PictureUrl, basketItem.UnitPrice, basketItem.Quantity);
        //}

        //private static ReceiptItem CreateReceiptItemCard(string productName, string productImageUrl, 
        //    decimal unitPrice, int quantity)
        //{
        //    return new ReceiptItem()
        //    {
        //        Title = productName,
        //        Image = new CardImage(url: $"{ReplacePictureUri(productImageUrl)}"),
        //        Price = $"{unitPrice}$",
        //        Quantity = $"{quantity}",
        //        Tap = null
        //    };
        //}

        //public static List<ReceiptItem> CreateOrderItemListReceipt(IEnumerable<OrderItem> orderItems)
        //{
        //    var listReceipt = new List<ReceiptItem>();
        //    listReceipt.AddRange(orderItems.Select(c => CreateOrderItemReceipt(c)));
        //    return listReceipt;
        //}

        //public static List<ReceiptItem> CreateOrderBasketItemListReceipt(IEnumerable<BasketItem> orderItems)
        //{
        //    var listReceipt = new List<ReceiptItem>();
        //    listReceipt.AddRange(orderItems.Select(c => CreateBasketItemReceipt(c)));
        //    return listReceipt;
        //}

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