using Bot46.API.Infrastructure.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class HelpDialog : IDialog<object>
    {
        private const string Catalog = "Catalog";
        private const string Orders = "Orders";
        private const string Cart = "Cart";
        private const string Back = "Back";
        private const string Login = "Login.";

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(await GetHelp(context));
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> messageActivity)
        {
            var message = await messageActivity;
            if (message != null && message.Type == ActivityTypes.Message && !string.IsNullOrEmpty(message.Text))
            {
                switch(message.Text)
                {
                    case Catalog:
                        await context.PostAsync("To show you the catalog you can type for example:");
                        await context.PostAsync("*Show me the shop catalog.*");                    
                        context.Done<object>(null);
                        break;
                    case Orders:
                        await context.PostAsync("To show yout orders can type for example:");
                        await context.PostAsync("*Show me my orders.*");
                        context.Done<object>(null);
                        break;
                    case Cart:
                        await context.PostAsync("To show your current cart type for example:");
                        await context.PostAsync("*Show me my cart.*");
                        context.Done<object>(null);
                        break;
                    case Login:
                        await context.PostAsync("To login on the service you must type:");
                        await context.PostAsync("*/login*");
                        context.Done<object>(null);
                        break;
                    case Back:
                        await context.PostAsync("Please type what do you want to do.");
                        context.Done<object>(null);
                        break;
                    default:
                        await context.PostAsync("I did not understand in which you need help, please select one.");
                        await context.PostAsync(await GetHelp(context));
                        context.Wait(MessageReceivedAsync);
                        break;
                }
            }
            else
            {

                await context.PostAsync("I did not understand in which you need help.");
                await context.PostAsync(await GetHelp(context));
            }
        }

        private async Task<IMessageActivity> GetHelp(IDialogContext context)
        {
            // ToDo help Dialog
            // should launch other Dialogs
            var reply = context.MakeMessage();
            reply.Text = "I can perform several tasks: show the catalog, your current cart or your orders.";
            var options = new List<CardAction>() {

                    new CardAction(){
                        Title = "Catalog",
                        Value = Catalog,
                        Type = ActionTypes.ImBack
                    },
                    new CardAction(){
                        Title = "Orders",
                        Value = Orders,
                        Type = ActionTypes.ImBack
                    },
                    new CardAction(){
                        Title = "Cart",
                        Value = Cart,
                        Type = ActionTypes.ImBack
                    },
                    new CardAction(){
                        Title = "Back",
                        Value = Back,
                        Type = ActionTypes.ImBack
                    }
                };

            if (! await context.IsAuthenticated())
            {
                options.Add(new CardAction()
                {
                    Title = "Login",
                    Value = Login,
                    Type = ActionTypes.ImBack
                });
            }

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = options

            };


            return reply;
        }
    }
}