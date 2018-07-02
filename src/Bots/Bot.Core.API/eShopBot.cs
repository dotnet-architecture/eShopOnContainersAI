using Bot.Core.API;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.eShopOnContainers.Bot.API.LUIS;
using Microsoft.eShopOnContainers.Bot.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API
{
    public class eShopBot : IBot
    {
        private readonly string host;
        private readonly ILuisService luisService;
        private readonly IDialogFactory dialogFactory;

        public eShopBot(IHttpContextAccessor httpContextAccessor,             
            ILuisService luisService, 
            IDialogFactory dialogFactory)
        {
            host = httpContextAccessor.HttpContext.Request.AbsoluteHost();
            this.luisService = luisService;
            this.dialogFactory = dialogFactory;
        }

        public async Task OnTurn(ITurnContext context)
        {
            switch (context.Activity.Type)
            {
                case ActivityTypes.Message:
                    string utterance = context.Activity.Text;

                    var (userState, dialogCtx) = CreateContext(context);

                    await dialogCtx.Continue();

                    if (context.Responded)
                        return;

                    var result = await luisService.GetResultAsync(utterance);

                    var topIntent = result != null ? result.TopIntent().intent : eShopLuisResult.Intent.None;

                    switch (topIntent)
                    {
                        case eShopLuisResult.Intent.Catalog:
                            if (result.Entities.brand != null || result.Entities.type != null)
                            {
                                userState.CatalogFilter = ConvertToConvesationInfo(result);
                                await dialogCtx.Begin(CatalogDialog.Id);
                            }
                            else
                                await dialogCtx.Begin(CatalogFilterDialog.Id);
                            break;
                        case eShopLuisResult.Intent.Login:
                            await context.SendActivity("No Login yet, sorry. Come back later");
                            break;
                        case eShopLuisResult.Intent.Hello:
                            await context.SendActivity("Hi. How can I help you?");
                            break;
                        case eShopLuisResult.Intent.Bye:
                            await context.SendActivity("Bye bye, remember I am here to help you when ever you need me.");
                            break;
                        default:
                            await context.SendActivity($"Sorry, I did not understand '{utterance}'. Type '/help' if you need assistance.");
                            break;
                    }
                    break;

                default:
                    await HandleSystemMessage(context);
                    break;
            }
        }

        private (UserInfo, DialogContext) CreateContext (ITurnContext context)
        {
            var dialogs = new DialogSet();
            dialogs.AddCatalogDialog(dialogFactory);
            dialogs.AddCatalogFilterDialog(dialogFactory);

            var userState = context.GetUserState<UserInfo>();
            var state = context.GetConversationState<eShopBotState>();
            var dialogCtx = dialogs.CreateContext(context, state);
            return (userState, dialogCtx);
        }

        private CatalogFilterData ConvertToConvesationInfo(eShopLuisResult result)
        {
            var filter = new CatalogFilterData();
            filter.Brand = result.Entities.brand != null ? result.Entities.brand.SingleOrDefault() : String.Empty;
            filter.Type = result.Entities.type != null ? result.Entities.type.SingleOrDefault() : String.Empty;
            return filter;
        }

        private Task HandleSystemMessage(ITurnContext context)
        {
            var activity = context.Activity;
            switch (activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    if (activity.IsFirstTime())
                    {
                        context.SendActivities(new[] {
                                    MessageFactory.Attachment(new HeroCard()
                                        {
                                            Title = $"Welcome {activity.MembersAdded.FirstOrDefault()?.Name}!",
                                            Images = new List<CardImage>() { new CardImage() { Alt = "eShop Logo", Url = $"{host}/images/brand.png" } }
                                        }.ToAttachment()),
                                    MessageFactory.Text("Howdy! - I am eShopAI-Bot."),
                                    MessageFactory.Text("I can show you the eShopAI Catalog, add items to your shopping cart, place a new order and explore your order's status."),
                                    MessageFactory.Text("Just type whatever you want to do, for example: *show me the product catalog*") }
                        );
                    }
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }

    }
}
