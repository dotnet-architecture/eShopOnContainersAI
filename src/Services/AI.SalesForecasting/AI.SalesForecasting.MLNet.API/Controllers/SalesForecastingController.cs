using Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Forecasting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Controllers
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
        [Route("product/{productId}/unitdemandestimation")]
        public async Task<IActionResult> GetProductUnitDemandEstimation(string productId,
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev)
        {
            // next,productId,year,month,units,avg,count,max,min,prev
            var nextMonthUnitDemandEstimation = await productSales.Predict($"{appSettings.AIModelsPath}/product_month_fastTreeTweedie.zip", productId, year, month, units, avg, count, max, min, prev);

            return Ok(nextMonthUnitDemandEstimation.Score);
        }

        [HttpGet]
        [Route("country/{country}/salesforecast")]
        public async Task<IActionResult> GetCountrySalesForecast(string country,
            [FromQuery]int year,
            [FromQuery]int month, [FromQuery]float med,
            [FromQuery]float max, [FromQuery]float min,
            [FromQuery]float prev, [FromQuery]int count,
            [FromQuery]float sales, [FromQuery]float std)
        {
            // next,country,year,month,max,min,std,count,sales,med,prev
            var nextMonthSalesForecast = await countrySales.Predict($"{appSettings.AIModelsPath}/country_month_fastTreeTweedie.zip", country, year, month, max, min, std, count, sales, med, prev);

            return Ok(nextMonthSalesForecast.Score);
        }
    }
}
