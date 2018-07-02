using Microsoft.eShopOnContainers.Bot.API.Services;

namespace Microsoft.eShopOnContainers.Bot.API.Dialogs
{
    public class DialogFactory : IDialogFactory
    {
        public DialogFactory(ICatalogService catalogService, ICatalogAIService catalogAIService,
            IProductSearchImageService productSearchImageService)
        {
            this.catalogService = catalogService;
            this.catalogAIService = catalogAIService;
            this.productSearchImageService = productSearchImageService;
        }

        private readonly ICatalogService catalogService;
        private readonly ICatalogAIService catalogAIService;
        private readonly IProductSearchImageService productSearchImageService;
        private CatalogDialog catalogDialog;
        private CatalogFilterDialog catalogFilterDialog;

        public CatalogDialog CatalogDialog { get { return catalogDialog ?? (catalogDialog = new CatalogDialog(catalogAIService, catalogService) ); } }
        public CatalogFilterDialog CatalogFilterDialog { get { return catalogFilterDialog ?? (catalogFilterDialog = new CatalogFilterDialog(catalogService, productSearchImageService, this) ); } }
    }

    public interface IDialogFactory
    {
        CatalogDialog CatalogDialog { get; }
        CatalogFilterDialog CatalogFilterDialog { get; }
    }
}
