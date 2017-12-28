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
    public class BasketDialog : IDialog<object>
    {

        private readonly IBasketService service = ServiceResolver.Get<IBasketService>();

        public async Task StartAsync(IDialogContext context)
        {
            await ShowBasket(context);
        }

        private async Task ShowBasket(IDialogContext context)
        {         
            Basket basket = null;
            var state = context.Activity.GetStateClient();
            BotData userData = await state.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            AuthUser authUser = userData.GetProperty<AuthUser>("authUser");
            // Todo check Expired
            if (authUser != null)
            {
                basket = await service.GetBasket(authUser.UserId, authUser.AccessToken);
                var reply = context.MakeMessage();
                reply.Attachments = new List<Attachment>();
                reply.Attachments.Add(await RecipeCard(context, basket));
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Call(new LoginDialog(), ExecutedLoginAsync);
            }

        }

        private async Task ExecutedLoginAsync(IDialogContext context, IAwaitable<object> result)
        {
            var o = await result;
            await ShowBasket(context);
        }

        private async Task<Attachment> RecipeCard(IDialogContext context, Basket basket)
        {

            List<CardImage> cardImages = new List<CardImage>();
            // TODO EShop Logo
            // cardImages.Add(new CardImage(url: "https://<imageUrl1>"));

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction plButton = new CardAction()
            {
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.OrderNow}'}}",
                Title = "Order now"
            };
            cardButtons.Add(plButton);

            CardAction plButton2 = new CardAction()
            {
                Type = ActionTypes.PostBack,
                Value = $@"{{ 'ActionType': '{BotActionTypes.ContinueShopping}'}}",
                Title = "Continue shoping"
            };
            cardButtons.Add(plButton2);


            List<ReceiptItem> receiptList = new List<ReceiptItem>();
            foreach (var item in basket.Items)
            {
                ReceiptItem lineItem = new ReceiptItem()
                {
                    Title = item.ProductName,
                    Subtitle = null,
                    Text = null,
                    Image = new CardImage(url: $"{item.PictureUrl}"),
                    Price = $"{item.UnitPrice}$",
                    Quantity = $"{item.Quantity}",
                    Tap = null
                };
                receiptList.Add(lineItem);
            }

            decimal total = basket.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal tax = (total * 10) / 100;

            ReceiptCard plCard = new ReceiptCard()
            {
                Title = "EShop receipt",
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
                            //TODO mark order
                            context.Done<object>(true);
                            break;
                        case BotActionTypes.ContinueShopping:
                            await context.PostAsync("You can continue shopping.");
                            context.Done<object>(false);
                            break;
                    }
                }
                catch (JsonReaderException e)
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
    }
}