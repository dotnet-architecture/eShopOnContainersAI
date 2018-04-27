using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eDashboard.Infrastructure.Data;
using eShopDashboard.Core.Entities;
using eShopDashboard.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
                    var productList = itemList.Select(ci =>
                        {
                            var tags = JsonConvert.DeserializeObject<CatalogFullTag>(ci.Tags);
                                
                            return new
                            {
                                ci.Id,
                                ci.CatalogBrandId,
                                ci.Description,
                                ci.Price,
                                ci.PictureUri,
                                color = tags.Color.JoinTags(),
                                size = tags.Size.JoinTags(),
                                shape = tags.Shape.JoinTags(),
                                quantity = tags.Quantity.JoinTags(),
                                tags.agram,
                                tags.bgram,
                                tags.abgram,
                                tags.ygram,
                                tags.zgram,
                                tags.yzgram
                            };
                        }).ToList();

                    return Ok(productList);
                }
                else
                    return Ok();
        }
    }
}
