using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Order;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared;
using Microsoft.eShopOnContainers.Bot.API.Services.Basket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Basket
{
    [Serializable]
    public class BasketDialog : ComponentDialog
    {
        public const string Name = nameof(BasketDialog) + ".MainDriver";

        private readonly IBasketService basketService;
        private readonly ILogger<BasketDialog> logger;
        private readonly DomainPropertyAccessors accessors;
        private readonly IDialogFactory dialogFactory;

        public BasketDialog(DomainPropertyAccessors accessors, IDialogFactory dialogFactory,IBasketService basketService, ILogger<BasketDialog> logger) : base(Name)
        {
            this.basketService = basketService;
            this.logger = logger;
            this.accessors = accessors;
            this.dialogFactory = dialogFactory;

            AddDialog(new WaterfallDialog(Name, new WaterfallStep[] { ShowBasket, ProcessBasket }));
            AddDialog(dialogFactory.OrderDialog);

            InitialDialogId = Name;
        }

        private async Task<DialogTurnResult> ShowBasket(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var authUser = await accessors.AuthUserProperty.GetAsync(step.Context);
            if (authUser != null && !authUser.IsExpired)
            {
                var basket = await basketService.GetBasket(authUser.UserId, authUser.AccessToken);
                if (basket.Items.Count == 0)
                {
                    await step.Context.SendActivityAsync(MessageFactory.Text("There are no items in your basket"));
                    return await step.EndDialogAsync();
                }
                else
                {
                    var reply = ActivityFactory.RecipeCard(step.Context, basket);
                    await step.Context.SendActivityAsync(reply, cancellationToken);
                    return EndOfTurn;
                }
            }
            else
            {
                await step.Context.SendActivityAsync(MessageFactory.Text("You need to sign in to check your basket"));
                return await step.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> ProcessBasket(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var message = step.Context.Activity.AsMessageActivity();
            if (message != null && JObjectHelper.TryParse(message.Text, out JObject json, logger))
            {
                var action = json.GetValue("ActionType");
                switch (action.ToString())
                {
                    case BotActionTypes.ContinueShopping:
                        await step.Context.SendActivityAsync(MessageFactory.Text("You can continue shopping"));
                        break;
                    case BotActionTypes.BasketCheckout:
                        await step.Context.SendActivityAsync(MessageFactory.Text("Proceeding to ordering ..."));
                        return await step.BeginDialogAsync(OrderDialog.Name);
                }
            }
            return await step.EndDialogAsync();
        }
    }
}