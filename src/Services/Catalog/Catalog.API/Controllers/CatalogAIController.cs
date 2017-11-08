using Catalog.API.AI;
using Catalog.API.Extensions;
using Catalog.API.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.eShopOnContainers.Services.Catalog.API.ViewModel;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IComputerVisionService _cvService;
        private readonly ICatalogTagsRepository _catalogTagsRepository;
        private readonly CatalogSettings _settings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public CatalogAIController(CatalogContext context, ICatalogTagsRepository catalogTagsRepository, IAzureMachineLearningService amlService, IComputerVisionService cvService, IOptionsSnapshot<CatalogSettings> settings, IHostingEnvironment hostingEnvironment)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _amlService = amlService ?? throw new ArgumentNullException(nameof(amlService));
            _cvService = cvService ?? throw new ArgumentNullException(nameof(cvService));
            _catalogTagsRepository = catalogTagsRepository;
            _settings = settings.Value;
            _hostingEnvironment = hostingEnvironment;
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

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Items(            
            [FromQuery]int? catalogTypeId, [FromQuery]int? catalogBrandId,
            [FromQuery]string tags,
            [FromQuery]int? pageIndex, [FromQuery]int? pageSize)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

            var validatedPageSize = pageSize ?? 10;
            var validatedPageIndex = pageIndex ?? 0;

            if (catalogTypeId.HasValue)
            {
                root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);
            }

            if (catalogBrandId.HasValue)
            {
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
            }

            if (!String.IsNullOrEmpty(tags))
            {
                var catalogTags = await _catalogTagsRepository.FindMatchingCatalogTagAsync(tags.Split(','));
                var catalogTagsIds = catalogTags.Select(x => x.ProductId);
                root = root.Where(ci => catalogTagsIds.Contains(ci.Id));
            }

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .OrderBy(c => c.Name)
                .Skip(validatedPageSize * validatedPageIndex)
                .Take(validatedPageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                validatedPageIndex, validatedPageSize, totalItems, itemsOnPage);

            return Ok(model);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                return NoContent();

            IEnumerable<string> tags;
            using (var image = new MemoryStream())
            {
                await imageFile.CopyToAsync(image);
                tags = await _cvService.AnalyzeImageAsync(image.ToArray());
            }

            return Ok(tags);
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
