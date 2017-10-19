using Catalog.API.AI;
using Catalog.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class CatalogAIController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly IAzureMachineLearningService _amlService;

        public CatalogAIController(CatalogContext context, IAzureMachineLearningService amlService)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _amlService = amlService ?? throw new ArgumentNullException(nameof(amlService));
        }

        [HttpGet]
        [Route("[action]/{productId}")]
        [ProducesResponseType(typeof(CatalogItem[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Recommendation(string productId)
        {
            var recommendations = await _amlService.Recommendations(productId);

            if (recommendations == null)
                return BadRequest();

            var recommendationsInt = recommendations
                .Select(c => Convert.ToInt32(c))
                .ToArray();

            var items = await _catalogContext.CatalogItems
                .Where(c => recommendationsInt.Contains(c.Id))
                .ToArrayAsync();

            return Ok(items);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Dump()
        {
            var catalog = await _catalogContext.CatalogItems
                .Select(c => new {c.Id, c.CatalogBrandId, c.CatalogTypeId, c.Description, c.Price })
                .ToListAsync();

            var csvFile = File(Encoding.UTF8.GetBytes(catalog.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "catalog.csv";
            return csvFile;
        }
    }
}
