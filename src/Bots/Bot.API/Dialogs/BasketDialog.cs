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
using Basket = Microsoft.Bots.Bot.API.Models.Basket.Basket;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    [Serializable]
    public class BasketDialog : IDialog<object>
    {
        private readonly IBasketService basketService;
        private readonly IDialogFactory dialogFactory;
        private readonly IIdentityService identityService;

        public BasketDialog(IDialogFactory dialogFactory,IBasketService basketService, IIdentityService identityService)
        {
            this.basketService = basketService;
            this.dialogFactory = dialogFactory;
            this.identityService = identityService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowBasket(context);
        }

        private async Task ShowBasket(IDialogContext context)
        {
            var authUser = await identityService.GetAuthUserAsync(context);
            // TODO check Expired
            if (authUser != null)
            {
                var basket = await basketService.GetBasket(authUser.UserId, authUser.AccessToken);
                var reply = context.MakeMessage();
                reply.Attachments = new List<Attachment> {RecipeCard(context, basket)};
                await context.PostAsync(reply);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Call(dialogFactory.CreateLoginDialog(), ExecutedLoginAsync);
            }
        }

        private async Task ExecutedLoginAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var o = await result;
            await ShowBasket(context);
        }

        private Attachment RecipeCard(IDialogContext context, Basket basket)
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

            return basketCard.ToAttachment();
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
                        case BotActionTypes.ContinueShopping:
                            await context.PostAsync(TextResources.You_can_continue_shopping);
                            context.Done<object>(false);
                            break;
                        case BotActionTypes.BasketCheckout:
                            context.Call(dialogFactory.CreateOrderDialog(), AfterOrderAsync);
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

        private Task AfterOrderAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(false);
            return Task.CompletedTask;
        }
    }
}