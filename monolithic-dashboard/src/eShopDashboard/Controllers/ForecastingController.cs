using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/forecasting")]
    public class ForecastingController : Controller
    {
        // GET: api/Forecasting
        [HttpGet("product/{productId}/unitdemandestimation")]
        public IActionResult ProductUnitDemandEstimation(int productId, [FromQuery]int month)
        {
            if (month == 10)
            {
                return Ok(153.410721);
            }

            return Ok(175.667068);
        }
    }
}
