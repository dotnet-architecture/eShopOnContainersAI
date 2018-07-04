using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.eShopOnContainers.Bot.API.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public class CatalogDialog : DialogContainer
    {
        public const string Id = nameof(CatalogDialog);
        private readonly ICatalogAIService catalogAIService;
        private readonly ICatalogService catalogService;

        public CatalogDialog(ICatalogAIService catalogAIService, ICatalogService catalogService, IDialogFactory dialogFactory) : base(Id)
        {
            this.catalogAIService = catalogAIService;
            this.catalogService = catalogService;

            //this.Dialogs.AddCatalogFilterDialog(dialogFactory);
            this.Dialogs.Add(Id, new WaterfallStep[] {
                ShowProductCarouselStep, ProcessNextStep
            });
        }

        private async Task ShowProductCarouselStep(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var context = dc.Context;
            var catalogFilter = dc.Context.GetUserState<UserInfo>().CatalogFilter;

            var products = await GetProducts(catalogFilter, catalogAIService, catalogService);

            int pageCount = (products.Count + CatalogFilterData.PageSize - 1) / CatalogFilterData.PageSize;
            if (products.Count != 0)
            {
                const bool logged = false;
                const bool supportsMarkdown = false;
                var text = $"Page {catalogFilter.PageIndex + 1} of {pageCount} ( {products.Count} items )";
                var items = CatalogCarousel(products, logged, supportsMarkdown);
                await context.SendActivities(new[]
                {
                    MessageFactory.Carousel(items, text),
                    MessageFactory.SuggestedActions(CreateNextButtons(catalogFilter.PageIndex, pageCount, logged), "choose one option")
                });
            }
            else
            {
                await context.SendActivities(new[] {
                    MessageFactory.Text("There are no results matching your search"),
                    MessageFactory.Text("Type what do you want to do.")
                });
                await dc.End();
            }
        }

        private async Task ProcessNextStep(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var catalogFilter = dc.Context.GetUserState<UserInfo>().CatalogFilter;
            var selectedAction = (args["Activity"] as Activity)?.Text?.Trim();

            dynamic activityResponse;
            try
            {
                activityResponse = JsonConvert.DeserializeObject(selectedAction);
            } catch (Exception ex)
            {
                await dc.Context.SendActivity("I didn't understand your response. Please click on a button");
                await dc.Replace(Id);
                return;
            }
            if (activityResponse.ActionType == BotActionTypes.Back)
            {
                await dc.Context.SendActivity("Type what do you want to do.");
                await dc.End();
            }
            //else if (activityResponse.ActionType == BotActionTypes.CatalogFilter)
            //{
            //    await dc.Replace(CatalogFilterDialog.Id);
            //}
            else
            {
                catalogFilter.PageIndex++;
                await dc.Replace(Id);
            }
        }

        private async Task<Models.Catalog.Catalog> GetProducts(CatalogFilterData ci, ICatalogAIService catalogAIService, ICatalogService catalogService)
        {
            int? brandId = -1;
            if (!string.IsNullOrEmpty(ci.Brand))
            {
                var brands = await catalogService.GetBrandsAsync();
                brandId = Convert.ToInt32(brands.FirstOrDefault(b => b.Text.Equals(ci.Brand, StringComparison.InvariantCultureIgnoreCase))?.Id);
            }
            if (brandId < 1) brandId = null;

            int? typeId = -1;
            if (!string.IsNullOrEmpty(ci.Type))
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

        private List<CardAction> CreateNextButtons(int currentPage, int pageCount, bool logged = false)
        {
            var cardActions = new List<CardAction>();

            cardActions.Add(UIHelper.CreateHomeButton());

            if (currentPage + 1 < pageCount)
            {
                cardActions.Add(UIHelper.CreateShowMoreButton());
            }
            //else
            //{
            //    cardActions.Add(UIHelper.CreateAnotherQueryButton());
            //}

            return cardActions;
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
                    Value = $@"{{ 'ActionType': '{BotActionTypes.AddBasket}', 'ProductId': '{catalogItem.Id}' , 'ProductName': '{catalogItem.Name}', 'PictureUrl': '{catalogItem.PictureUri}', 'UnitPrice': '{catalogItem.Price}'}}",
                    Type = ActionTypes.PostBack,
                    Title = "Add to cart"
                };
                addToBasketAction.Add(addToBasketButton);
            }

            return UIHelper.CreateCatalogItemCard(catalogItem, addToBasketAction, isMarkdownSupported);
        }
    }

}
