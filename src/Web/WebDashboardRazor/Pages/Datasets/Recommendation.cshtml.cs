using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopOnContainers.WebDashboardRazor.Infrastructure;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Pages.Datasets
{
    public class RecommendationModel : PageModel
    {
        private readonly AppSettings appSettings;

        public RecommendationModel(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public string CustomerUrl { get; private set; }
        public string ProductUrl { get; private set; }
        public string OrderItemsUrl { get; set; }

        public void OnGet()
        {
            ViewData.SetSelectedMenu(SelectedMenu.Data_SalesByCountry);

            CustomerUrl = API.Identity.UserInfo(appSettings.WebShoppingUrlExternal);
            ProductUrl = API.Catalog.ProductInfo(appSettings.WebShoppingUrlExternal, "csv");
            OrderItemsUrl = API.Ordering.ProductSales(appSettings.WebShoppingUrlExternal);
        }
    }
}