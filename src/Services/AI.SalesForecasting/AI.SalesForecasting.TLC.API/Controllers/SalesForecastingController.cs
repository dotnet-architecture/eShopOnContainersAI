using Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Forecasting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Controllers
{
    [Route("api/v1/ForecastingAI")]
    public class SalesForecastingController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IProductSales productSales;
        private readonly ICountrySales countrySales;

        public SalesForecastingController(IOptionsSnapshot<AppSettings> appSettings, IProductSales productSales, ICountrySales countrySales)
        {
            this.appSettings = appSettings.Value;
            this.productSales = productSales;
            this.countrySales = countrySales;
        }

        [HttpGet]
        [Route("product/{productId}/forecast")]
        public IActionResult ForecastProduct(string productId, 
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev,
            [FromQuery]float price, [FromQuery]string color,
            [FromQuery]string size, [FromQuery]string shape,
            [FromQuery]string agram, [FromQuery]string bgram,
            [FromQuery]string ygram, [FromQuery]string zgram)
        {
            var results = productSales.Predict($"{appSettings.AIModelsPath}/product_ts_month.zip", productId, year, month, units, avg, count, max, min, prev, price, color, size, shape, agram, bgram, ygram, zgram);

            return Ok(results.Score);
        }

        [HttpGet]
        [Route("country/{country}/forecast")]
        public IActionResult ForecastProduct(string country,
            [FromQuery]int year,
            [FromQuery]int month, [FromQuery]float avg, 
            [FromQuery]int max, [FromQuery]int min,
            [FromQuery]int prev, [FromQuery]int count,
            [FromQuery]int units)
        {
            var results = countrySales.Predict($"{appSettings.AIModelsPath}/country_ts_month.zip", country, year, month, avg, max, min, prev, count, units);

            return Ok(results.Score);
        }
    }
}
