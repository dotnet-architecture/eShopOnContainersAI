using eShopDashboard.Extensions;
using eShopDashboard.Infrastructure.Data.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Controllers
{
    [Produces("application/json")]
    [Route("api/catalog")]
    public class CatalogController : Controller
    {
        private readonly CatalogContext _catalogContext;

        public CatalogController(CatalogContext catalogContext)
        {
            _catalogContext = catalogContext;
        }

        // GET: api/Catalog
        [HttpGet("productSetDetailsByDescription")]
        public async Task<IActionResult> SimilarProducts([FromQuery]string description)
        {
            if (string.IsNullOrEmpty(description))
                return BadRequest();

            var itemList = await _catalogContext.CatalogItems
                .Where(c => c.Description.Contains(description))
                .ToListAsync();

            if (itemList.Any())
            {
                var productList = itemList.Select(ci => new
                {
                    ci.Id,
                    ci.CatalogBrandId,
                    ci.Description,
                    ci.Price,
                    ci.PictureUri,
                    color = ci.Tags.Color.JoinTags(),
                    size = ci.Tags.Size.JoinTags(),
                    shape = ci.Tags.Shape.JoinTags(),
                    quantity = ci.Tags.Quantity.JoinTags(),
                    ci.Tags.agram,
                    ci.Tags.bgram,
                    ci.Tags.abgram,
                    ci.Tags.ygram,
                    ci.Tags.zgram,
                    ci.Tags.yzgram
                }).ToList();

                return Ok(productList);
            }

            return Ok();
        }
    }
}