using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.eShopOnContainers.Bot.API.Services.Order;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Order
{
    [Serializable]
    public class MyOrdersDialog : ComponentDialog
    {
        public const string Name = nameof(MyOrdersDialog) + ".MainDriver";

        private readonly AppSettings settings;
        private readonly DomainPropertyAccessors accessors;
        private readonly IDialogFactory dialogFactory;
        private readonly IOrderingService orderingService;
        private readonly ILogger<MyOrdersDialog> logger;

        private readonly SharedResponses sharedResponses = new SharedResponses();

        public MyOrdersDialog(DomainPropertyAccessors accessors, IDialogFactory dialogFactory, IOrderingService orderingService,
            IOptions<AppSettings> appSettings, ILogger<MyOrdersDialog> logger) : base(Name)
        {
            this.settings = appSettings.Value;
            this.accessors = accessors;
            this.dialogFactory = dialogFactory;
            this.orderingService = orderingService;
            this.logger = logger;
            AddDialog(new WaterfallDialog(Name, new WaterfallStep[] { ShowOrders, ProcessPromptOrder, ProcessPromptChoices }));
            AddDialog(new ChoicePrompt("orderId", OrderIdValidator));
            AddDialog(new ChoicePrompt("choices"));

            InitialDialogId = Name;
        }

        private Task<bool> OrderIdValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var isValidPromptOrJson = 
                promptContext.Recognized.Succeeded 
                || Int32.TryParse(promptContext.Context.Activity.Text, out int value)  
                || JObjectHelper.TryParse(promptContext.Context.Activity.Text, out JObject json, logger);
            return Task.FromResult(isValidPromptOrJson);
        }

        private async Task<DialogTurnResult> ShowOrders(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var authUser = await accessors.AuthUserProperty.GetAsync(dc.Context, () => new Models.User.AuthUser());

            if (authUser != null && !authUser.IsExpired)
            {
                var latestOrder = (bool)dc.Options;

                var context = dc.Context;
                var orders = await orderingService.GetMyOrders(authUser.UserId, authUser.AccessToken);
                if (latestOrder)
                    orders = orders.TakeLast(1).ToList();

                if (orders.Count > 1)
                {
                    var reply = ActivityFactory.OrdersCard(context, orders);
                    reply.AttachmentLayout = context.Activity.IsSkypeChannel() ? AttachmentLayoutTypes.List : AttachmentLayoutTypes.Carousel;
                    await context.SendActivityAsync(reply);

                    return await dc.PromptAsync("orderId", new PromptOptions() {
                        Prompt = MessageFactory.Text("Select an order item or go back"),
                        Choices = ChoiceFactory.ToChoices(new[] { "Home" })
                    });
                } if (orders.Count == 1)
                {
                    await PostOrderDetailAsync(context, orders.First().OrderNumber);
                }
                else
                {
                    await context.SendActivityAsync(MessageFactory.Text("You do not have any order."));
                }
            }
            else
            {
                await dc.Context.SendActivityAsync(MessageFactory.Text("You need to sign in to check your orders"));
            }
            await dc.Context.SendActivityAsync(MessageFactory.Text("Type what do you want to do."));
            return await dc.EndDialogAsync();
        }

        private async Task<DialogTurnResult> ProcessPromptOrder(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var context = dc.Context;
            var message = context.Activity.AsMessageActivity();
            if (message != null && JObjectHelper.TryParse(message.Text, out JObject json, logger))
            {
                var action = json.GetValue("ActionType");
                switch (action.ToString())
                {
                    case BotActionTypes.OrderDetail:
                        var OrderNumber = json.GetValue("OrderNumber").ToString();
                        await PostOrderDetailAsync(context, OrderNumber);
                        return await dc.PromptAsync("choices", new PromptOptions
                        {
                            Choices = ChoiceFactory.ToChoices(new[] { "Check more orders", "Home" }),
                            Prompt = MessageFactory.Text("Select what do you want to do")
                        });
                }
            }
            else if (message != null)
            {
                var result = message.Text;
                if (result != "Home" && Int32.TryParse(result, out int orderId))
                {
                    await PostOrderDetailAsync(context, result);
                    return await dc.PromptAsync("choices", new PromptOptions
                    {
                        Choices = ChoiceFactory.ToChoices(new[] { "Check more orders", "Home" })
                    });
                }
            }
            //await context.SendActivityAsync(MessageFactory.Text("Type what do you want to do"));
            await sharedResponses.ReplyWith(dc.Context, SharedResponses.TypeMore);
            return await dc.EndDialogAsync();
        }

        private async Task<DialogTurnResult> ProcessPromptChoices(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var foundChoice = (dc.Result as FoundChoice).Value;
            if (foundChoice != "Home")
            {
                return await dc.ReplaceDialogAsync(Name, dc.Options, cancellationToken: cancellationToken);
            } else
            {
                //await dc.Context.SendActivityAsync(MessageFactory.Text("Type what do you want to do"));
                await sharedResponses.ReplyWith(dc.Context, SharedResponses.TypeMore);
                return await dc.EndDialogAsync();
            }
        }

        private async Task PostOrderDetailAsync(ITurnContext context, string orderNumber)
        {
            var authUser = await accessors.AuthUserProperty.GetAsync(context);
            var order = await orderingService.GetOrder(orderNumber, authUser.AccessToken);
            var reply = ActivityFactory.OrderCard(context, order);
            await context.SendActivityAsync(reply);
        }
    }
}