using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Help
{
    [Serializable]
    public class HelpDialog : ComponentDialog
    {
        public const string Name = nameof(HelpDialog);
        private const string HelpTOC = nameof(HelpTOC);

        private const string Catalog = "Products";
        private const string Orders = "Orders";
        private const string Basket = "Basket";
        private const string Back = "Back";
        private const string Login = "Login";
        private const string Logout = "Logout";
        private const string Cancel = "Cancel";
        private readonly DomainPropertyAccessors accessors;

        public HelpDialog(DomainPropertyAccessors accessors) : base(Name)
        {
            this.accessors = accessors;
            AddDialog(new ChoicePrompt(HelpTOC));
            AddDialog(new WaterfallDialog(Name, new WaterfallStep[] { ShowHelpChoices, ProcessHelpChoices }));
            InitialDialogId = Name;
        }

        private async Task<DialogTurnResult> ShowHelpChoices(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var authUser = await accessors.AuthUserProperty.GetAsync(dc.Context, () => new Models.User.AuthUser());
            return await dc.PromptAsync(HelpTOC, new PromptOptions {
                Choices = ChoiceFactory.ToChoices(HelpChoicesToList(dc.Context, authUser.IsAuthenticated)),
                Prompt = MessageFactory.Text("I can perform several tasks: show products list, your current cart or your past orders."),
                RetryPrompt = MessageFactory.Text("I did not understand in which area do you need help, please select one.")                
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessHelpChoices(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var helpTopic = (dc.Result as FoundChoice).Value;
            await HelpChoicesToFuncAsync[helpTopic](dc);
            return await dc.EndDialogAsync();
        }

        private static Dictionary<string, Func<WaterfallStepContext, Task>> HelpChoicesToFuncAsync = new Dictionary<string, Func<WaterfallStepContext, Task>>
            {
                {
                    Catalog,
                    async (step) =>
                    {
                        var context = step.Context;
                        await context.SendActivityAsync("To show you the catalog you can type for example:");
                        await context.SendActivityAsync(MessageFactory.SuggestedActions(new CardAction[] { new CardAction(type: ActionTypes.PostBack, title: "Test it", text: "Test it", value: "Show me the shop catalog") }, text: "*Show me the shop catalog.*"));
                    }
                },
                {
                    Orders,
                    async (step) =>
                    {
                        var context = step.Context;
                        await context.SendActivityAsync("To show your orders, you can type for example:");
                        await context.SendActivityAsync(MessageFactory.SuggestedActions(new CardAction[] { new CardAction(type: ActionTypes.PostBack, title: "Test it", text: "Test it", value: "Show me my orders") }, text: "*Show me my orders.*"));
                    }
                },
                {
                    Basket,
                    async (step) =>
                    {
                        var context = step.Context;
                        await context.SendActivityAsync("To show your current cart, you can type for example:");
                        await context.SendActivityAsync(MessageFactory.SuggestedActions(new CardAction[] { new CardAction(type: ActionTypes.PostBack, title: "Test it", text: "Test it", value: "Show me my cart") }, text: "*Show me my cart.*"));
                    }
                },
                {
                    Login,
                    async (step) =>
                    {
                        var context = step.Context;
                        await context.SendActivityAsync("To login on the service, you must type:");
                        await context.SendActivityAsync(MessageFactory.SuggestedActions(new CardAction[] { new CardAction(type: ActionTypes.PostBack, title: "Test it", text: "Test it", value: "login") }, text: "*login*"));
                    }
                },
                {
                    Logout,
                    async (step) =>
                    {
                        var context = step.Context;
                        await context.SendActivityAsync("To log out the service, you must type:");
                        await context.SendActivityAsync(MessageFactory.SuggestedActions(new CardAction[] { new CardAction(type: ActionTypes.PostBack, title: "Test it", text: "Test it", value: "bye") }, text: "*bye*"));
                    }
                },
                {
                    Cancel,
                    async (step) =>
                    {
                        var context = step.Context;
                        await context.SendActivityAsync("To cancel any operation, you must type:");
                        await context.SendActivityAsync("*/cancel*");
                    }
                }
            };

        private List<string> HelpChoicesToList(ITurnContext context, bool isAuthenticated)
        {
            var tocHelpList = new List<string> { Catalog, Orders, Basket };
            if (isAuthenticated)
                tocHelpList.Add(Logout);
            else
                tocHelpList.Add(Login);
            tocHelpList.Add(Cancel);
            return tocHelpList;
        }
    }
}