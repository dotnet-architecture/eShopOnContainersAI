using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/catalog")]
    public class CatalogController : Controller
    {
        // GET: api/Catalog
        [HttpGet("productSetDetailsByDescription")]
        public IActionResult Get(string description)
        {
            return Content(ProductSetDetailsByDescription.Value, "application/json");
        }
    }
}
