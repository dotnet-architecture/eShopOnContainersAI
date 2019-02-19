using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.eShopOnContainers.Bot.API.Models.Basket;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.TemplateManager;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Shared;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Basket;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Login;
using Microsoft.eShopOnContainers.Bot.API.Services.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Services.Basket;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Catalog
{
    public class CatalogDialog : ComponentDialog
    {
        public const string Name = nameof(CatalogDialog) + ".MainDriver";
        private const string PromptProductQuantity = nameof(CatalogDialog) + "." + nameof(PromptProductQuantity);
        private const string PromptChoices = nameof(CatalogDialog) + "." + nameof(PromptChoices);
        private const string PromptNumber = nameof(CatalogDialog) + "." + nameof(PromptNumber);
        private readonly DomainPropertyAccessors accessors;
        private readonly ICatalogAIService catalogAIService;
        private readonly ICatalogService catalogService;
        private readonly IBasketService basketService;
        private readonly ILogger<CatalogDialog> logger;
        private TemplateManager sharedResponses = new Shared.SharedResponses();

        public CatalogDialog(DomainPropertyAccessors accessors, ICatalogAIService catalogAIService, ICatalogService catalogService,
            IDialogFactory dialogFactory, IBasketService basketService, ILogger<CatalogDialog> logger) : base(Name)
        {
            this.accessors = accessors;
            this.catalogAIService = catalogAIService;
            this.catalogService = catalogService;
            this.basketService = basketService;
            this.logger = logger;

            AddDialog(new ChoicePrompt(PromptChoices, ChoiceValidator));
            AddDialog(new NumberPrompt<int>(PromptNumber));
            AddDialog(new WaterfallDialog(Name, new WaterfallStep[] {
                ShowProductCarouselStep, ProcessNextStep, ProcessChildDialogEnd
            }));
            AddDialog(new WaterfallDialog(PromptProductQuantity, new WaterfallStep[] { ShowProductPromptQuantity, ProcessProductPromptQuantity }));
            AddDialog(dialogFactory.LoginDialog);
            AddDialog(dialogFactory.BasketDialog);

            InitialDialogId = Name;
        }

        private Task<bool> ChoiceValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
        {
            var isValidChoiceOrJson = promptContext.Recognized.Succeeded || JObjectHelper.TryParse(promptContext.Context.Activity.Text, out JObject json);
            return Task.FromResult(isValidChoiceOrJson);
        }

        private Task<DialogTurnResult> ProcessChildDialogEnd(WaterfallStepContext stepContext, CancellationToken cancellationToken) => stepContext.ReplaceDialogAsync(Id);

        private async Task<DialogTurnResult> ShowProductPromptQuantity(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var productName = (dc.Options as BasketItem).ProductName;
            return await dc.PromptAsync(PromptNumber, new PromptOptions { Prompt = MessageFactory.Text($"How many '{productName}' do you want to buy?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessProductPromptQuantity(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var quantity = (int)dc.Result;
            var tempBasketItem = dc.Options as BasketItem;
            tempBasketItem.Quantity = quantity;

            var authUser = await accessors.AuthUserProperty.GetAsync(dc.Context);

            try
            {
                await basketService.AddItemToBasket(authUser.UserId, tempBasketItem, authUser.AccessToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }

            return await dc.BeginDialogAsync(BasketDialog.Name);
        }

        private async Task<DialogTurnResult> ShowProductCarouselStep(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var context = dc.Context;
            var catalogFilter = await accessors.CatalogFilterProperty.GetAsync(context);

            var products = await GetProducts(catalogFilter, catalogAIService, catalogService);

            int pageCount = (products.Count + CatalogFilterData.PageSize - 1) / CatalogFilterData.PageSize;
            if (products.Count != 0)
            {
                var authUser = await accessors.AuthUserProperty.GetAsync(dc.Context, () => new Models.User.AuthUser());
                bool logged = authUser.IsAuthenticated;
                const bool supportsMarkdown = false;
                var text = $"Page {catalogFilter.PageIndex + 1} of {pageCount} ( {products.Count} items )";
                var items = CatalogCarousel(products, logged, supportsMarkdown);

                await context.SendActivityAsync(MessageFactory.Carousel(items, text));

                var choices = CreateNextChoices(catalogFilter.PageIndex, pageCount, logged).ToArray();

                return await dc.PromptAsync(PromptChoices, new PromptOptions()
                {
                    Prompt = MessageFactory.Text("choose one option"),
                    Choices = choices.Select(c => c.Choice).ToList()
                });
            }
            else
            {
                await context.SendActivitiesAsync(new[] {
                    MessageFactory.Text("There are no results matching your search"),
                    MessageFactory.Text("Type what do you want to do.")
                });
                return await dc.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> ProcessNextStep(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var context = dc.Context;
            var catalogFilter = await accessors.CatalogFilterProperty.GetAsync(context);
            var message = dc.Context.Activity.AsMessageActivity();

            if (message != null && JObjectHelper.TryParse(message.Text, out JObject json, logger))
            {
                var action = json.GetValue("ActionType").ToString();
                if (action == BotActionTypes.AddBasket)
                {
                    var tempBasketItem = new BasketItem
                    {
                        ProductName = json.GetValue("ProductName").ToString(),
                        ProductId = json.GetValue("ProductId").ToString(),
                        PictureUrl = json.GetValue("PictureUrl").ToString(),
                        UnitPrice = json.GetValue("UnitPrice").ToObject<decimal>()
                    };

                    return await dc.BeginDialogAsync(PromptProductQuantity, tempBasketItem);
                }
            }

            var foundChoice = dc.Result as FoundChoice;

            var selectedType = (dc.Result as FoundChoice).Value.ToLowerInvariant();
            if (selectedType == "home")
            {
                await sharedResponses.ReplyWith(dc.Context, SharedResponses.TypeMore);
                return await dc.EndDialogAsync();
            }
            else if (selectedType.ToLowerInvariant() == "login")
            {
                return await dc.BeginDialogAsync(LoginDialog.Name);
            }
            else if (selectedType == "show previous")
            {
                catalogFilter.PageIndex--;
                await accessors.CatalogFilterProperty.SetAsync(context, catalogFilter);
                return await dc.ReplaceDialogAsync(Id);
            }
            else
            {
                catalogFilter.PageIndex++;
                await accessors.CatalogFilterProperty.SetAsync(context, catalogFilter);
                return await dc.ReplaceDialogAsync(Id);
            }
        }

        private async Task<Models.Catalog.Catalog> GetProducts(CatalogFilterData ci, ICatalogAIService catalogAIService, ICatalogService catalogService)
        {
            int? brandId = -1;
            if (ci != null && !string.IsNullOrEmpty(ci.Brand))
            {
                var brands = await catalogService.GetBrandsAsync();
                brandId = Convert.ToInt32(brands.FirstOrDefault(b => b.Text.Equals(ci.Brand, StringComparison.InvariantCultureIgnoreCase))?.Id);
            }
            if (brandId < 1) brandId = null;

            int? typeId = -1;
            if (ci != null && !string.IsNullOrEmpty(ci.Type))
            {
                var types = await catalogService.GetTypesAsync();
                typeId = Convert.ToInt32(types.FirstOrDefault(b => b.Text.Equals(ci.Type, StringComparison.InvariantCultureIgnoreCase))?.Id);
            }
            if (typeId < 1) typeId = null;

            var products =
                    ci.Tags != null && !ci.Tags.Any() ?
                        Models.Catalog.Catalog.Empty :
                        await catalogAIService.GetCatalogItems(ci.PageIndex, CatalogFilterData.PageSize, brandId, typeId, ci.Tags);

            return products;
        }

        private IEnumerable<(Choice Choice, Func<DialogContext, Task<DialogTurnResult>> ChoiceFunc)> CreateNextChoices(int currentPage, int pageCount, bool logged)
        {
            if (currentPage > 0)
                yield return (
                    new Choice("Show previous"),
                    async (DialogContext dc) =>
                    {
                        var catalogFilter = await accessors.CatalogFilterProperty.GetAsync(dc.Context);
                        catalogFilter.PageIndex--;
                        await accessors.CatalogFilterProperty.SetAsync(dc.Context, catalogFilter);
                        return await dc.ReplaceDialogAsync(Id);
                    }
                );

            yield return (
                new Choice("Home"),
                async (DialogContext dc) =>
                {
                    await sharedResponses.ReplyWith(dc.Context, SharedResponses.TypeMore);
                    return await dc.EndDialogAsync();
                }
            );

            if (!logged)
                yield return (
                    new Choice("Login"),
                    async (DialogContext dc) => await dc.BeginDialogAsync(LoginDialog.Name)
                );

            if (currentPage + 1 < pageCount)
                yield return (
                    new Choice("Show next"),
                    async (DialogContext dc) =>
                    {
                        var catalogFilter = await accessors.CatalogFilterProperty.GetAsync(dc.Context);
                        catalogFilter.PageIndex++;
                        await accessors.CatalogFilterProperty.SetAsync(dc.Context, catalogFilter);
                        return await dc.ReplaceDialogAsync(Id);
                    }
                );
        }

        private List<Attachment> CatalogCarousel(Models.Catalog.Catalog catalog, bool isAuthenticated, bool isMarkdownSupported)
        {
            var attachments = new List<Attachment>();
            foreach (var item in catalog.Data)
            {
                var productThumbnail = BuildCatalogItemCard(item, isAuthenticated, isMarkdownSupported);

                attachments.Add(productThumbnail.ToAttachment());
            }

            return attachments;
        }

        private ThumbnailCard BuildCatalogItemCard(Models.Catalog.CatalogItem catalogItem, bool isAuthenticated, bool isMarkdownSupported)
        {
            var addToBasketAction = new List<CardAction>();
            if (isAuthenticated)
            {
                CardAction addToBasketButton = new CardAction()
                {
                    Type = ActionTypes.PostBack,
                    Value = $@"{{ 'ActionType': '{BotActionTypes.AddBasket}', 'ProductId': '{catalogItem.Id}' , 'ProductName': '{catalogItem.Name}', 'PictureUrl': '{catalogItem.PictureUri}', 'UnitPrice': '{catalogItem.Price}'}}",
                    Title = "Add to cart",
                    //DisplayText = $"{catalogItem.Name} added to cart"
                };
                addToBasketAction.Add(addToBasketButton);
            }

            return UIHelper.CreateCatalogItemCard(catalogItem, addToBasketAction, isMarkdownSupported);
        }
    }

}
