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
    public class ForecastModel : PageModel
    {
        private readonly AppSettings appSettings;

        public ForecastModel(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public string CountryUrl { get; private set; }
        public string ProductUrl { get; private set; }

        public void OnGet()
        {
            ViewData.SetSelectedMenu(SelectedMenu.Data_SalesByProduct);

            CountryUrl = API.Ordering.CountryStats(appSettings.WebShoppingUrlExternal);
            ProductUrl = API.Ordering.ProductStats(appSettings.WebShoppingUrlExternal); //Url.Action("ProductStats", "Products");
        }
    }
}