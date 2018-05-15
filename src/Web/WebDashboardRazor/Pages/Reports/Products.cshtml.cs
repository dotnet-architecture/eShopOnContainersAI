using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Pages.Reports
{
    public class ProductsModel : PageModel
    {
        public void OnGet()
        {
            ViewData.SetSelectedMenu(SelectedMenu.Reports_Product);
        }
    }
}