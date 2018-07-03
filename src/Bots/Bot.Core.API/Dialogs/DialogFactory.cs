using Microsoft.eShopOnContainers.Bot.API.Services;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public class DialogFactory : IDialogFactory
    {
        public DialogFactory(IOptions<AppSettings> appSettings, ICatalogService catalogService, ICatalogAIService catalogAIService,
            IProductSearchImageService productSearchImageService)
        {
            this.appSettings = appSettings;
            this.catalogService = catalogService;
            this.catalogAIService = catalogAIService;
            this.productSearchImageService = productSearchImageService;
        }

        private readonly IOptions<AppSettings> appSettings;
        private readonly ICatalogService catalogService;
        private readonly ICatalogAIService catalogAIService;
        private readonly IProductSearchImageService productSearchImageService;
        private CatalogDialog catalogDialog;
        private CatalogFilterDialog catalogFilterDialog;

        public CatalogDialog CatalogDialog { get { return catalogDialog ?? (catalogDialog = new CatalogDialog(catalogAIService, catalogService) ); } }
        public CatalogFilterDialog CatalogFilterDialog { get { return catalogFilterDialog ?? (catalogFilterDialog = new CatalogFilterDialog(appSettings, catalogService, productSearchImageService, this) ); } }
    }

    public interface IDialogFactory
    {
        CatalogDialog CatalogDialog { get; }
        CatalogFilterDialog CatalogFilterDialog { get; }
    }
}
