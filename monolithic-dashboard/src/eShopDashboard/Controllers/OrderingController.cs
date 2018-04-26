using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/ordering")]
    public class OrderingController : Controller
    {
        // GET: api/Ordering
        [HttpGet("product/{productId}/history")]
        public IActionResult ProductHistory(int productId)
        {
            return Content(ProductHistoryFake.Value(productId), "application/json");
        }
    }
}
