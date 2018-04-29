using eShopDashboard.Extensions;
using eShopDashboard.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/ordering")]
    public class OrderingController : Controller
    {
        private readonly IOrderingQueries _queries;

        public OrderingController(IOrderingQueries queries)
        {
            _queries = queries;
        }

        [HttpGet("country/{country}/history")]
        public async Task<IActionResult> CountryHistory(string country)
        {
            if (country.IsBlank()) return BadRequest();

            IEnumerable<dynamic> items = await _queries.GetCountryHistoryAsync(country);

            return Ok(items);
        }

        [HttpGet("product/{productId}/history")]
        public async Task<IActionResult> ProductHistory(string productId)
        {
            if (productId.IsBlank() || productId.IsNotAnInt()) return BadRequest();

            IEnumerable<dynamic> items = await _queries.GetProductHistoryAsync(productId);

            return Ok(items);
        }

        [HttpGet("product/{productId}/stats")]
        public async Task<IActionResult> ProductStats(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return BadRequest();

            IEnumerable<dynamic> stats = await _queries.GetProductStatsAsync(productId);

            return Ok(stats);
        }
    }
}