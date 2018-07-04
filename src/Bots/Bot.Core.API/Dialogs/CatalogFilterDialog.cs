using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.eShopOnContainers.Bot.API.Services;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public class CatalogFilterDialog : DialogContainer
    {
        const string BrandsCacheKey = "brandsCache";
        const string TypesCacheKey = "typesCache";

        public const string Id = "PromptBrands";
        private const string PromptTypes = "PromptTypes";
        private readonly IOptions<AppSettings> appSettings;
        private readonly ICatalogService catalog;
        private readonly ICatalogFilterDialogService catalogFilterDialogService;

        public CatalogFilterDialog(IOptions<AppSettings> appSettings, ICatalogService catalogService, ICatalogFilterDialogService catalogFilterDialogService, IDialogFactory dialogFactory) : base(Id)
        {
            this.appSettings = appSettings;
            catalog = catalogService;
            this.catalogFilterDialogService = catalogFilterDialogService;

            this.Dialogs.AddCatalogDialog(dialogFactory);
            this.Dialogs.Add(Id, new WaterfallStep[]
            {
                AskBrandStep, ValidateBrandStep
            });
            this.Dialogs.Add(PromptTypes, new WaterfallStep[]
            {
                AskTypeStep, ValidateTypeStep
            });
        }

        private async Task AskBrandStep(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var userState = dc.Context.GetUserState<UserInfo>();
            userState.CatalogFilter = new CatalogFilterData();

            var brands = await catalog.GetBrandsAsync();
            dc.ActiveDialog.State[BrandsCacheKey] = brands;

            await dc.Context.SendActivities(new[] {
                        MessageFactory.Text("Please select a brand you'd like to search for"),
                        MessageFactory.SuggestedActions(
                            brands.Select(b => new CardAction() { Title = b.Text, Value = b.Text, Type = ActionTypes.ImBack }).ToList(),
                            "You can also upload an image/photo to search for similar products")
                    });

        }

        private async Task ValidateBrandStep(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var userState = dc.Context.GetUserState<UserInfo>();

            if (dc.Context.Activity.AttachmentContainsImageFile())
            {
                await catalogFilterDialogService.UpdateCatalogFilterUserStateWithTagsAsync(dc.Context);
                await dc.Replace(CatalogDialog.Id);
            }
            else
            {
                var selectedBrand = (args["Activity"] as Activity)?.Text?.Trim();
                var brands = dc.ActiveDialog.State[BrandsCacheKey] as IEnumerable<Models.Catalog.Brand>;

                if (brands.Select(b => b.Text).Contains(selectedBrand))
                {
                    userState.CatalogFilter.Brand = selectedBrand;
                    await dc.Replace(PromptTypes);
                }
                else
                {
                    await dc.Context.SendActivity("I didn't understand your response. Please select one brand.");
                    await dc.Replace(Id);
                }
            }
        }

        private async Task AskTypeStep(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var types = await catalog.GetTypesAsync();
            dc.ActiveDialog.State[TypesCacheKey] = types;

            await dc.Context.SendActivities(new[] {
                        MessageFactory.Text("Please select a type you'd like to search for"),
                        MessageFactory.SuggestedActions(
                            types.Select(t => new CardAction() { Title = t.Text, Value = t.Text, Type = ActionTypes.ImBack }).ToList(),
                            "You can also upload an image/photo to search for similar products"),
                    });
        }

        private async Task ValidateTypeStep(DialogContext dc, IDictionary<string, object> args, SkipStepFunction next)
        {
            var userState = dc.Context.GetUserState<UserInfo>();

            if (dc.Context.Activity.AttachmentContainsImageFile())
            {
                await catalogFilterDialogService.UpdateCatalogFilterUserStateWithTagsAsync(dc.Context);
                await dc.Replace(CatalogDialog.Id);
            }
            else
            {
                var selectedType = (args["Activity"] as Activity)?.Text?.Trim();

                var types = dc.ActiveDialog.State[TypesCacheKey] as IEnumerable<Models.Catalog.CatalogType>;

                if (types.Select(b => b.Text).Contains(selectedType))
                {
                    userState.CatalogFilter.Type = selectedType;
                    await dc.Replace(CatalogDialog.Id);
                }
                else
                {
                    await dc.Context.SendActivity("I didn't understand your response. Please select one type.");
                    await dc.Replace(PromptTypes);
                }
            }
        }
    }

}
