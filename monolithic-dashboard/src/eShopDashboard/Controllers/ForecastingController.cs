using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopDashboard.Forecasting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/forecasting")]
    public class ForecastingController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IProductSales productSales;
        private readonly ICountrySales countrySales;

        public ForecastingController(IOptionsSnapshot<AppSettings> appSettings, IProductSales productSales, ICountrySales countrySales)
        {
            this.appSettings = appSettings.Value;
            this.productSales = productSales;
            this.countrySales = countrySales;
        }

        // GET: api/Forecasting
        //[HttpGet("product/{productId}/unitdemandestimation")]
        //public IActionResult ProductUnitDemandEstimation(int productId, [FromQuery]int month)
        //{
        //    if (month == 10)
        //    {
        //        return Ok(153.410721);
        //    }

        //    return Ok(175.667068);
        //}

        [HttpGet]
        [Route("product/{productId}/unitdemandestimation")]
        public async Task <IActionResult> GetProductUnitDemandEstimation(string productId,
            [FromQuery]int year, [FromQuery]int month,
            [FromQuery]float units, [FromQuery]float avg,
            [FromQuery]int count, [FromQuery]float max,
            [FromQuery]float min, [FromQuery]float prev,
            [FromQuery]float price, [FromQuery]string color,
            [FromQuery]string size, [FromQuery]string shape,
            [FromQuery]string agram, [FromQuery]string bgram,
            [FromQuery]string ygram, [FromQuery]string zgram)
        {
            var nextMonthUnitDemandEstimation = await productSales.Predict($"{appSettings.AIModelsPath}/product_month_fastTreeTweedle.zip", productId, year, month, units, avg, count, max, min, prev, price, color, size, shape, agram, bgram, ygram, zgram);

            return Ok(nextMonthUnitDemandEstimation.Score);
        }

        [HttpGet]
        [Route("country/{country}/salesforecast")]
        public async Task<IActionResult> GetCountrySalesForecast(string country,
            [FromQuery]int year,
            [FromQuery]int month, [FromQuery]float avg,
            [FromQuery]float max, [FromQuery]float min,
            [FromQuery]float p_max, [FromQuery]float p_min,
            [FromQuery]float p_med,
            [FromQuery]float prev, [FromQuery]int count,
            [FromQuery]float sales, [FromQuery]float std)
        {
            var nextMonthSalesForecast = await countrySales.Predict($"{appSettings.AIModelsPath}/country_month_fastTreeTweedle.zip", country, year, month, sales, avg, count, max, min, p_max, p_med, p_min, std, prev);

            return Ok(nextMonthSalesForecast.Score);
        }
    }
}
