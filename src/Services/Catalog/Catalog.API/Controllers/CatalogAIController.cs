using Catalog.API.Extensions;
using Catalog.API.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.eShopOnContainers.Services.Catalog.API.ViewModel;
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
        private readonly ICatalogTagsRepository _catalogTagsRepository;
        private readonly CatalogSettings _settings;

        public CatalogAIController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings, ICatalogTagsRepository catalogTagsRepository)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _catalogTagsRepository = catalogTagsRepository;
            _settings = settings.Value;
        }

        [HttpGet]
        [Route("productSetDetailsByIDs")]
        [ProducesResponseType(typeof(CatalogItem[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RecommendProducts([FromQuery]string productId, [FromQuery]string productIDs)
        {
            if (productIDs == null)
                return BadRequest();

            var sortRecommendations = await SortRecommendations(productId, productIDs.Split(','));

            var items = await _catalogContext.CatalogItems
                .Where(c => sortRecommendations.Contains(c.Id))
                .ToListAsync();

            items = items.FollowsOrder(c => c.Id, sortRecommendations).ToList();

            items = items.ChangeUriPlaceholder(_settings);

            return Ok(items);
        }

        [HttpGet]
        [Route("dumpToCSV")]
        public async Task<IActionResult> DumpToCSV()
        {
            var catalog = await _catalogContext.CatalogItems
                .Select(c => new {c.Id, c.CatalogBrandId, c.CatalogTypeId, c.Description, c.Price })
                .ToListAsync();

            var tags = await _catalogTagsRepository.All;

            var join = catalog.Join(tags, a => a.Id, b => b.ProductId,
                (a,b) => new {
                    a.Id,
                    a.CatalogBrandId,
                    a.Description,
                    a.Price,
                    color = b.Color.JoinTags(),
                    size = b.Size.JoinTags(),
                    shape = b.Shape.JoinTags(),
                    quantity = b.Quantity.JoinTags(),
                    b.agram, b.bgram, b.abgram,
                    b.ygram, b.zgram, b.yzgram
                }).ToList();

            var csvFile = File(Encoding.UTF8.GetBytes(join.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "products.csv";
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
            var root = _catalogContext.CatalogItems.AsQueryable();

            if (!String.IsNullOrEmpty(tags))
            {
                var preTags = tags.Split(',').SelectMany(c => c.Permutation());
                var catalogTags = await _catalogTagsRepository.FindMatchingTagsAsync(preTags);
                var catalogTagsProductIds = catalogTags.Select(x => x.ProductId);
                if (catalogTagsProductIds.Any())
                    root = root.Where(ci => catalogTagsProductIds.Contains(ci.Id));
                else
                    return Ok(Enumerable.Empty<CatalogItem>());
            }

            if (catalogTypeId.HasValue)
            {
                root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);
            }

            if (catalogBrandId.HasValue)
            {
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
            }

            var totalItems = await root
                .LongCountAsync();

            var validatedPageSize = pageSize ?? 10;
            var validatedPageIndex = pageIndex ?? 0;

            var itemsOnPage = await root
                .OrderBy(c => c.Name)
                .Skip(validatedPageSize * validatedPageIndex)
                .Take(validatedPageSize)
                .ToListAsync();

            itemsOnPage = itemsOnPage.ChangeUriPlaceholder(_settings);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                validatedPageIndex, validatedPageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        private Task<IEnumerable<int>> SortRecommendations(string productId, IEnumerable<string> recommendations)
        {
            return SortRecommendations(productId, recommendations.Select(c => Convert.ToInt32(c)));
        }

        private async Task<IEnumerable<int>> SortRecommendations(string productId, IEnumerable<int> recommendations)
        {
            var refProductId = Convert.ToInt32(productId);
            var allRequests = recommendations.Concat(new[] { refProductId });
            // get tags from all catalog items
            var tags = await _catalogTagsRepository.FindMatchingProductsAsync(allRequests);

            // productId tags
            var refCatalogTag = tags.FirstOrDefault(c => c.ProductId == refProductId);

            // recommendations catalog items tags
            var recCatalogTags = tags.Except(new[] { refCatalogTag });


            var tagMatchingMatrix = new Dictionary<int, int>();
            foreach (var rec in recommendations)
            {
                tagMatchingMatrix.Add(rec, 0);
            }

            // for each match tag match, we add more weight to the recommended product
            foreach (var tag in refCatalogTag.Tagrams)
            {
                foreach (var rec in recCatalogTags)
                {
                    if (rec.Tagrams.Contains(tag))
                    {
                        tagMatchingMatrix[rec.ProductId]++;
                    }
                }
            }

            // we take three (recommended) items with the maximum weight
            var recByTag = tagMatchingMatrix
                .Where(c => c.Value > 0)
                .OrderByDescending(c => c.Value)
                .Take(3)
                .Select(c => c.Key);

            // and we take rest of items in the same order as specified in recommendations
            var recByRec = tagMatchingMatrix
                .Keys
                .Except(recByTag);

            return recByTag.Concat(recByRec);
        }
    }
}
