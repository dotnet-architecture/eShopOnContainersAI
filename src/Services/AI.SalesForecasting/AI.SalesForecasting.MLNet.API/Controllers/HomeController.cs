using Microsoft.AspNetCore.Mvc;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}
