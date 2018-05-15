using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.eShopOnContainers.WebDashboardRazor.Infrastructure;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Pages
{
    public enum DataModelInformationCategories
    {
        OrderItems,
        Products,
        Users,
        OrderItemStats,
        CountryStats
    }

    public class DataModel : PageModel
    {
        private readonly AppSettings appSettings;

        public DataModel(IOptionsSnapshot<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public DataModelInformation Information { get; private set; }

        public void OnGet(string category)
        {
            if (!Enum.TryParse(category, true, out DataModelInformationCategories informationCategory))
                informationCategory = DataModelInformationCategories.OrderItems;

            UpdateSelectedMenu(informationCategory);

            Information = DataModelInformation.Create(informationCategory, Url, appSettings);
        }

        private void UpdateSelectedMenu(DataModelInformationCategories informationCategory)
        {
            switch (informationCategory)
            {
                case DataModelInformationCategories.OrderItems:
                    ViewData.SetSelectedMenu(SelectedMenu.Data_Sales);
                    break;
                case DataModelInformationCategories.Products:
                    ViewData.SetSelectedMenu(SelectedMenu.Data_Product);
                    break;
                case DataModelInformationCategories.Users:
                    ViewData.SetSelectedMenu(SelectedMenu.Data_Customer);
                    break;
                case DataModelInformationCategories.OrderItemStats:
                    ViewData.SetSelectedMenu(SelectedMenu.Data_SalesByProduct);
                    break;
                case DataModelInformationCategories.CountryStats:
                    ViewData.SetSelectedMenu(SelectedMenu.Data_SalesByCountry);
                    break;
            }

        }

        public class DataModelInformation
    {
        public static DataModelInformation Create (DataModelInformationCategories informationCategory, IUrlHelper urlHelper, AppSettings appSettings)
        {
            switch (informationCategory)
            {
                case DataModelInformationCategories.Products:
                    return new DataModelInformation
                    {
                        DownloadFileUrl = API.Catalog.ProductInfo(appSettings.WebShoppingUrl, "csv"),
                        FileName = "products.csv",
                        Description = $"The file products.csv contains the products stored in eShopOnContainersAI SqlServer database."
                    };
                case DataModelInformationCategories.Users:
                    return new DataModelInformation
                    {
                        DownloadFileUrl = API.Identity.UserInfo(appSettings.WebShoppingUrl),
                        FileName = "users.csv",
                        Description = $"The file users.csv contains the customers stored in eShopOnContainersAI SqlServer database."
                    };
                case DataModelInformationCategories.CountryStats:
                    return new DataModelInformation
                    {
                        DownloadFileUrl = API.Ordering.CountryStats(appSettings.WebShoppingUrl),
                        FileName = "countries.stats.csv",
                        Description = $"The file countries.stats.csv contains statistics about orders stored in eShopOnContainersAI SqlServer database."
                    };
                case DataModelInformationCategories.OrderItemStats:
                    return new DataModelInformation
                    {
                        DownloadFileUrl = urlHelper.Action("ProductStats", "Products"),
                        FileName = "orderItems.stats.csv",
                        Description = $"The file orderItems.stats.csv contains statistics about orders stored in eShopOnContainersAI SqlServer database."
                    };
                default:
                case DataModelInformationCategories.OrderItems:
                    return new DataModelInformation
                    {
                        DownloadFileUrl = API.Ordering.ProductSales(appSettings.WebShoppingUrl),
                        FileName = "orderItems.csv",
                        Description = $"The file orderItems.csv contains the order transactions stored in eShopOnContainersAI SqlServer database."
                    };
            }
        }

        public string DownloadFileUrl { get; private set; }
        public string FileName { get; private set; }
        public string Description { get; private set; }
    }
    }
}