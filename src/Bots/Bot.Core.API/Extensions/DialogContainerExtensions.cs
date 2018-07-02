using Microsoft.Bot.Builder.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Dialogs;

namespace Microsoft.eShopOnContainers.Bot.API.Extensions
{
    public static class DialogContainerExtensions
    {
        public static void AddCatalogDialog(this DialogSet self, IDialogFactory dialogFactory)
        {
            self.AddCatalogDialog(dialogFactory.CatalogDialog);
        }

        public static void AddCatalogDialog(this DialogSet self, CatalogDialog catalogDialog)
        {
            self.Add(CatalogDialog.Id, catalogDialog);
        }

        public static void AddCatalogFilterDialog(this DialogSet self, IDialogFactory dialogFactory)
        {
            self.AddCatalogFilterDialog(dialogFactory.CatalogFilterDialog);
        }

        public static void AddCatalogFilterDialog(this DialogSet self, CatalogFilterDialog catalogFilterDialog)
        {
            self.Add(CatalogFilterDialog.Id, catalogFilterDialog);
        }

    }
}
