using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Basket;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Help;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Login;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Order;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.eShopOnContainers.Bot.API.Services.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Services.LUIS;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Main
{
    public class MainDialog : RouterDialog
    {
        public const string Name = nameof(MainDialog);

        private readonly DomainPropertyAccessors accessors;
        private readonly ILuisService luisService;
        private readonly ICatalogFilterDialogService catalogFilterDialogService;
        private readonly AppSettings appSettings;
        private readonly string host;

        private Shared.SharedResponses sharedResponses = new Shared.SharedResponses();

        public MainDialog(DomainPropertyAccessors accessors, IOptions<AppSettings> appSettings, ILuisService luisService, IDialogFactory dialogFactory, ICatalogFilterDialogService catalogFilterDialogService, IHttpContextAccessor httpContextAccessor) : base(Name)
        {
            host = httpContextAccessor.HttpContext.Request.AbsoluteHost();
            AddDialog(dialogFactory.CatalogFilterDialog);
            AddDialog(dialogFactory.CatalogDialog);
            AddDialog(dialogFactory.LoginDialog);
            AddDialog(dialogFactory.BasketDialog);
            AddDialog(dialogFactory.MyOrdersDialog);
            AddDialog(dialogFactory.HelpDialog);
            this.accessors = accessors;
            this.luisService = luisService;
            this.catalogFilterDialogService = catalogFilterDialogService;
            this.appSettings = appSettings.Value;
        }

        protected override async Task RouteAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialogCtx = dc.Context;
            var result = await luisService.GetResultAsync(dialogCtx);
            var topIntent = result != null ? result.TopIntent().intent : eShopLuisResult.Intent.None;

            switch (topIntent)
            {
                case eShopLuisResult.Intent.Catalog:
                    if (result.Entities.brand != null || result.Entities.type != null)
                    {
                        var catalogFilter = await accessors.CatalogFilterProperty.GetAsync(dialogCtx, () => new CatalogFilterData());
                        ConvertToConvesationInfo(result, ref catalogFilter);
                        await accessors.CatalogFilterProperty.SetAsync(dialogCtx, catalogFilter);
                        await dc.BeginDialogAsync(CatalogDialog.Name);
                    }
                    else
                        await dc.BeginDialogAsync(CatalogFilterDialog.Name);

                    break;
                case eShopLuisResult.Intent.Login:
                    await dc.BeginDialogAsync(LoginDialog.Name);
                    break;
                case eShopLuisResult.Intent.Hello:
                    await sharedResponses.ReplyWith(dc.Context, Shared.SharedResponses.HowCanIHelp);
                    break;
                case eShopLuisResult.Intent.Basket:
                    await dc.BeginDialogAsync(BasketDialog.Name);
                    break;
                case eShopLuisResult.Intent.Orders:
                    await dc.BeginDialogAsync(MyOrdersDialog.Name, /*latestOrder*/ false);
                    break;
                case eShopLuisResult.Intent.Order:
                    await dc.BeginDialogAsync(MyOrdersDialog.Name, /*latestOrder*/ true);
                    break;
                case eShopLuisResult.Intent.Bye:
                    await Logout(dialogCtx, cancellationToken);
                    break;
                case eShopLuisResult.Intent.Help:
                    await dc.BeginDialogAsync(HelpDialog.Name);
                    break;
                default:
                    string utterance = dialogCtx.Activity.Text.ToLowerInvariant();
                    await dialogCtx.SendActivityAsync($"Sorry, I did not understand '{utterance}'. Type 'help' if you need assistance.");
                    break;
            }
        }

        private async Task Logout(ITurnContext dialogCtx, CancellationToken cancellationToken)
        {
            var botAdapter = (BotFrameworkAdapter)dialogCtx.Adapter;
            await botAdapter.SignOutUserAsync(dialogCtx, appSettings.AuthenticationConnectionName, cancellationToken: cancellationToken);
            await dialogCtx.SendActivityAsync("You have been signed out.", cancellationToken: cancellationToken);
            await dialogCtx.SendActivityAsync("Bye bye, remember I am here to help you when ever you need me.");
            var authUser = await accessors.AuthUserProperty.GetAsync(dialogCtx, () => new Models.User.AuthUser());
            authUser.Reset();
            await accessors.AuthUserProperty.SetAsync(dialogCtx, authUser);
        }

        private void ConvertToConvesationInfo(eShopLuisResult result, ref CatalogFilterData filter)
        {
            filter.Brand = result.Entities.brand != null ? result.Entities.brand.SingleOrDefault() : String.Empty;
            filter.Type = result.Entities.type != null ? result.Entities.type.SingleOrDefault() : String.Empty;
        }

        protected override async Task OnStartAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = innerDc.Context.Activity;
            var context = innerDc.Context;

            if (activity.IsFirstTime())
            {
                await context.SendActivitiesAsync(new[]
                {
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
        }

        protected override async Task<DialogTurnResult> OnMessageInterruptions(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var activity = innerDc.Context.Activity.AsMessageActivity();
            if (activity.Text == "/cancel" || activity.Text == "cancel")
            {
                await innerDc.CancelAllDialogsAsync(cancellationToken);
                await innerDc.Context.SendActivityAsync("Cancelling this operation", cancellationToken: cancellationToken);
                return EndOfTurn;
            }

            if (activity.AttachmentContainsImageFile())
            {
                await catalogFilterDialogService.UpdateCatalogFilterUserStateWithTagsAsync(innerDc.Context);
                return await innerDc.ReplaceDialogAsync(CatalogDialog.Name);
            }
            return null;
        }

        protected override Task<DialogTurnResult> OnMessageAuthentication(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            return innerDc.BeginDialogAsync(LoginDialog.Name, cancellationToken: cancellationToken);
        }
    }
}
