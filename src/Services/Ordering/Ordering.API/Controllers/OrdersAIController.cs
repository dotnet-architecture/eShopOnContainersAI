using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;
using Ordering.API.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class OrdersAIController : ControllerBase
    {
        private readonly IOrderQueries _orderQueries;

        public OrdersAIController(IOrderQueries orderQueries)
        {
            _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
        }

        [HttpGet]
        [Route("dumpToCSV")]
        public async Task<IActionResult> DumpToCSV()
        {
            var orderItems = await _orderQueries.GetOrderItemsAsync();

            var typedOrderItems = orderItems
                .Select(c => new { c.CustomerId, c.ProductId, c.Units })
                .ToList();

            var csvFile = File(Encoding.UTF8.GetBytes(typedOrderItems.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "orderItems.csv";
            return csvFile;
        }

        [HttpGet]
        [Route("product/{productId}/history")]
        public async Task<IActionResult> ProductHistory(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return BadRequest();

            var items = await _orderQueries.GetProductHistoryAsync(productId);
                
            return Ok(items);
        }

        [HttpGet]
        [Route("product/stats")]
        public async Task<IActionResult> ProductStatsDownload(string format = "csv")
        {
            var stats = await _orderQueries.GetProductStatsAsync(null);

            var typedStats = stats
                .Select(c => new { c.next, c.productId, c.year, c.month, c.units, c.avg, c.count, c.max, c.min, c.prev })
                .ToList();

            switch (format.ToLower())
            {
                case "csv":
                    var csvFile = File(Encoding.UTF8.GetBytes(typedStats.FormatAsCSV()), "text/csv");
                    csvFile.FileDownloadName = "products.stats.csv";
                    return csvFile;
                case "json":
                    return Ok(typedStats);
                default:
                    return BadRequest();
            }

        }

        [HttpGet]
        [Route("product/{productId}/stats")]
        public async Task<IActionResult> ProductStats(string productId)
        {
            if (string.IsNullOrEmpty(productId))
                return BadRequest();

            var stats = await _orderQueries.GetProductStatsAsync(productId);

            return Ok(stats);
        }

        [HttpGet]
        [Route("country/{country}/history")]
        public async Task<IActionResult> CountryHistory(string country)
        {
            if (string.IsNullOrEmpty(country))
                return BadRequest();

            var items = await _orderQueries.GetCountryHistoryAsync(country);

            return Ok(items);
        }

        [HttpGet]
        [Route("country/stats")]
        public async Task<IActionResult> CountryStats()
        {
            var stats = await _orderQueries.GetCountryStatsAsync(String.Empty);

            var typedStats = stats
                .Select(c => new { c.next, c.country, c.year, c.month, c.max, c.min, c.std, c.count, c.sales, c.med, c.prev })
                .ToList();

            var csvFile = File(Encoding.UTF8.GetBytes(typedStats.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "countries.stats.csv";
            return csvFile;
        }

        [HttpGet]
        [Route("country/{country}/stats")]
        public async Task<IActionResult> CountryStats(string country)
        {
            if (string.IsNullOrEmpty(country))
                return BadRequest();

            var stats = await _orderQueries.GetCountryStatsAsync(country);

            return Ok(stats);
        }

        [HttpGet]
        [Route("product/depth")]
        public async Task<IActionResult> ProductHistoryDepth([FromQuery]string products)
        {
            if (string.IsNullOrEmpty(products))
                return BadRequest();

            var depths = await _orderQueries.GetProductsHistoryDepthAsync(products.Split(',').Select(p => Int32.Parse(p)));

            return Ok(depths);
        }
    }
}
