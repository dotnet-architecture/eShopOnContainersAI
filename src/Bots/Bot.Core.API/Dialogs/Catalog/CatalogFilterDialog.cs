using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Services.Catalog;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs.Catalog
{
    public class CatalogFilterDialog : ComponentDialog
    {
        const string BrandsCacheKey = "brandsCache";
        const string TypesCacheKey = "typesCache";

        public const string Name = nameof(CatalogFilterDialog) + ".MainDriver"; // CatalogFilterDialog.PromptProductBrands
        private const string PromptProductTypes = nameof(CatalogFilterDialog) + "." + nameof(PromptProductTypes);
        private const string PromptChoices = nameof(CatalogFilterDialog) + "." + nameof(PromptChoices);

        private readonly DomainPropertyAccessors eShopBotAccessors;
        private readonly IOptions<AppSettings> appSettings;
        private readonly ICatalogService catalog;
        private readonly ICatalogFilterDialogService catalogFilterDialogService;

        public CatalogFilterDialog(DomainPropertyAccessors eShopBotAccessors, IOptions<AppSettings> appSettings, ICatalogService catalogService, ICatalogFilterDialogService catalogFilterDialogService, IDialogFactory dialogFactory) : base(Name)
        {
            this.eShopBotAccessors = eShopBotAccessors;
            this.appSettings = appSettings;
            catalog = catalogService;
            this.catalogFilterDialogService = catalogFilterDialogService;

            AddDialog(new WaterfallDialog(Name, new WaterfallStep []
            {
                AskBrandStep, ValidateBrandStep
            }));
            AddDialog(new WaterfallDialog(PromptProductTypes, new WaterfallStep []
            {
                AskTypeStep, ValidateTypeStep
            }));
            AddDialog(new ChoicePrompt(PromptChoices));
            AddDialog(dialogFactory.CatalogDialog);

            InitialDialogId = Name;
        }

        private async Task<DialogTurnResult> AskBrandStep(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var brands = await catalog.GetBrandsAsync();
            dc.Values[BrandsCacheKey] = brands;

            await dc.Context.SendActivityAsync(MessageFactory.Text("Please select a brand you'd like to search for"));
            return await dc.PromptAsync(PromptChoices, new PromptOptions {
                Prompt = MessageFactory.Text("You can also upload an image/photo to search for similar products"),
                Choices =  brands.Select(b => new Microsoft.Bot.Builder.Dialogs.Choices.Choice(b.Text)).ToList() });
        }

        private async Task<DialogTurnResult> ValidateBrandStep(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var catalogFilter = await eShopBotAccessors.CatalogFilterProperty.GetAsync(dc.Context, () => new CatalogFilterData());

            var selectedBrand = (dc.Result as Microsoft.Bot.Builder.Dialogs.Choices.FoundChoice).Value;
            var brands = dc.Values[BrandsCacheKey] as IEnumerable<Models.Catalog.Brand>;

            if (brands.Select(b => b.Text).Contains(selectedBrand))
            {
                catalogFilter.Brand = selectedBrand;
                await eShopBotAccessors.CatalogFilterProperty.SetAsync(dc.Context, catalogFilter);
                return await dc.ReplaceDialogAsync(PromptProductTypes);
            }
            else
            {
                await dc.Context.SendActivityAsync("I didn't understand your response. Please select one brand.");
                return await dc.ReplaceDialogAsync(Id);
            }
        }

        private async Task<DialogTurnResult> AskTypeStep(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var types = await catalog.GetTypesAsync();
            dc.Values[TypesCacheKey] = types;

            await dc.Context.SendActivityAsync(MessageFactory.Text("Please select a type you'd like to search for"));
            return await dc.PromptAsync(PromptChoices, new PromptOptions
            {
                Prompt = MessageFactory.Text("You can also upload an image/photo to search for similar products"),
                Choices = types.Select(t => new Microsoft.Bot.Builder.Dialogs.Choices.Choice(t.Text)).ToList()
            });
        }

        private async Task<DialogTurnResult> ValidateTypeStep(WaterfallStepContext dc, CancellationToken cancellationToken)
        {
            var catalogFilter = await eShopBotAccessors.CatalogFilterProperty.GetAsync(dc.Context, () => new CatalogFilterData());

            var selectedType = (dc.Result as Microsoft.Bot.Builder.Dialogs.Choices.FoundChoice).Value;

            var types = dc.Values[TypesCacheKey] as IEnumerable<Models.Catalog.CatalogType>;

            if (types.Select(b => b.Text).Contains(selectedType))
            {
                catalogFilter.Type = selectedType;
                await eShopBotAccessors.CatalogFilterProperty.SetAsync(dc.Context, catalogFilter);
                return await dc.ReplaceDialogAsync(CatalogDialog.Name);
            }
            else
            {
                await dc.Context.SendActivityAsync("I didn't understand your response. Please select one type.");
                return await dc.ReplaceDialogAsync(PromptProductTypes);
            }
        }
    }
}
