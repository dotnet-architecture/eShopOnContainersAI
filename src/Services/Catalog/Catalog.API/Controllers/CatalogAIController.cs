using Catalog.API.AI;
using Catalog.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
        private readonly CatalogSettings _settings;

        public CatalogAIController(CatalogContext context, IAzureMachineLearningService amlService, IOptionsSnapshot<CatalogSettings> settings)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _amlService = amlService ?? throw new ArgumentNullException(nameof(amlService));
            _settings = settings.Value;
        }

        [HttpGet]
        [Route("[action]/product/{productId}/customer/{customerId}")]
        [ProducesResponseType(typeof(CatalogItem[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Recommendation(string productId, string customerId)
        {
            if (customerId == "null")
                customerId = String.Empty;

            var recommendations = await _amlService.Recommendations(productId, customerId);

            if (recommendations == null)
                return BadRequest();

            var recommendationsInt = recommendations
                .Select(c => Convert.ToInt32(c))
                .ToArray();

            var items = await _catalogContext.CatalogItems
                .Where(c => recommendationsInt.Contains(c.Id))
                .ToListAsync();

            items = ChangeUriPlaceholder(items);

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

        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
        {
            var baseUri = _settings.PicBaseUrl;

            items.ForEach(catalogItem =>
            {
                catalogItem.PictureUri = _settings.AzureStorageEnabled
                    ? baseUri + catalogItem.PictureFileName
                    : baseUri.Replace("[0]", catalogItem.Id.ToString());
            });

            return items;
        }

    }
}
