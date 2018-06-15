using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.eShopOnContainers.WebDashboardRazor.Extensions;
using Microsoft.eShopOnContainers.WebDashboardRazor.Services;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Controllers
{
    [Route("api/v1/ProductsAI")]
    public class ProductsController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IOrderingService orderingService;
        private readonly ICatalogService catalogService;

        public ProductsController(IOptions<AppSettings> appSettings, IOrderingService orderingService, ICatalogService catalogService)
        {
            this.appSettings = appSettings.Value;
            this.orderingService = orderingService;
            this.catalogService = catalogService;
        }

        [HttpGet]
        [Route("product/stats")]
        public async Task<IActionResult> ProductStats(string format = "csv")
        {
            var productInfo = await catalogService.GetProductInfoAsync();

            var productSales = await orderingService.GetProductSalesAsync();

            var join = productInfo.Join(productSales, a => a.id, b => b.productId, (a, b) => new {
                b.next, b.productId, b.year, b.month, b.units, b.avg, b.count, b.max, b.min, b.prev,
                a.price, a.color, a.size, a.shape, a.agram, a.bgram, a.ygram, a.zgram
            }).ToList();

            switch (format.ToLower())
            {
                case "csv":
                    var csvFile = File(Encoding.UTF8.GetBytes(join.FormatAsCSV()), "text/csv");
                    csvFile.FileDownloadName = "orderItems.stats.csv";
                    return csvFile;
                case "json":
                    return Ok(join);
                default:
                    return BadRequest();
            }
        }

        [HttpGet]
        [Route("product/similar")]
        public async Task<IActionResult> SimilarProducts([FromQuery]string description)
        {
            // for product forecasting, we only take into consideration
            // products with more than 8 months of history
            const int minimalProductSalesHistoryDepth = 8;

            var products = await catalogService.GetSimilarProductsAsync(description);
            var productDepths = await orderingService.GetProductHistoryDepthAsync(products.Select(p => p.id));
            var productDepthIds = productDepths.Where(p => p.count > minimalProductSalesHistoryDepth).Select(p => p.productId.ToString());

            var validProducts = products
                .Where(p => productDepthIds.Contains(p.id))
                .Select(p => new { p.id, p.description, p.price, pictureUri = catalogService.GetProductPicture(p.id) });

            return Ok(validProducts);
        }
    }
}