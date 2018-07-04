using Microsoft.eShopOnContainers.Bot.API.Services;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public class DialogFactory : IDialogFactory
    {
        public DialogFactory(IOptions<AppSettings> appSettings, ICatalogService catalogService, ICatalogAIService catalogAIService,
            ICatalogFilterDialogService catalogFilterDialogService)
        {
            this.appSettings = appSettings;
            this.catalogService = catalogService;
            this.catalogAIService = catalogAIService;
            this.catalogFilterDialogService = catalogFilterDialogService;
        }

        private readonly IOptions<AppSettings> appSettings;
        private readonly ICatalogService catalogService;
        private readonly ICatalogAIService catalogAIService;
        private readonly ICatalogFilterDialogService catalogFilterDialogService;

        private CatalogDialog catalogDialog;
        private CatalogFilterDialog catalogFilterDialog;

        public CatalogDialog CatalogDialog { get { return catalogDialog ?? (catalogDialog = new CatalogDialog(catalogAIService, catalogService, this) ); } }
        public CatalogFilterDialog CatalogFilterDialog { get { return catalogFilterDialog ?? (catalogFilterDialog = new CatalogFilterDialog(appSettings, catalogService, catalogFilterDialogService, this) ); } }
    }

    public interface IDialogFactory
    {
        CatalogDialog CatalogDialog { get; }
        CatalogFilterDialog CatalogFilterDialog { get; }
    }
}
