using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.ViewModels.Pagination;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels.CatalogViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.WebMVC.Controllers
{
    public class CatalogController : Controller
    {
        private ICatalogService _catalogSvc;
        private readonly ICatalogAIService _catalogAISvc;
        private readonly IProductSearchImageBasedService _productImageSearchService;
        private readonly IOptionsSnapshot<AppSettings> settings;

        public CatalogController(ICatalogService catalogSvc, ICatalogAIService catalogAIService, 
            IProductSearchImageBasedService productSearchImageService, IOptionsSnapshot<AppSettings> settings) {
            _catalogSvc = catalogSvc;
            _catalogAISvc = catalogAIService;
            _productImageSearchService = productSearchImageService;
            this.settings = settings;
        }

		public async Task<IActionResult> Index(int? BrandFilterApplied, int? TypesFilterApplied, int? page, IFormFile ImageFilter, string Tags, [FromQuery]string errorMsg)
        {
            var itemsPage = 12;

            IEnumerable<string> tags = null;
            var imageFilterActive = ImageFilter != null && ImageFilter.Length > 0;
            if (imageFilterActive)
            {
                using (var ms = new MemoryStream())
                {
                    await ImageFilter.CopyToAsync(ms);
                    tags = await _productImageSearchService.ClassifyImageAsync(ms.ToArray());
                }
            }
            else if (!String.IsNullOrEmpty(Tags))
                tags = Tags.Split(',');

            var noTagsAvailable = (tags == null || !tags.Any());

            var catalog = imageFilterActive && noTagsAvailable ? 
                Catalog.Empty : 
                await _catalogAISvc.GetCatalogItems(page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied, tags);

            var vm = new IndexViewModel()
            {
                CatalogItems = catalog.Data,
                Brands = await _catalogSvc.GetBrands(),
                Types = await _catalogSvc.GetTypes(),
                BrandFilterApplied = BrandFilterApplied ?? 0,
                TypesFilterApplied = TypesFilterApplied ?? 0,
                Tags = noTagsAvailable ? String.Empty : String.Join(',',tags),
                TagsActive = noTagsAvailable ? String.Empty : "active",
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page ?? 0,
                    ItemsPerPage = catalog.Data.Count,
                    TotalItems = catalog.Count, 
                    TotalPages = (int)Math.Ceiling(((decimal)catalog.Count / itemsPage))
                }
            };

            if (catalog?.Data == null || !catalog.Data.Any())
            {
                ViewBag.EmptyCatalogMsg = imageFilterActive ? 
                    "Sorry, we could not find any product similar to your sample image" : 
                    "Sorry, we could not find any product";
            }

            vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
            vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";

            ViewBag.BasketInoperativeMsg = errorMsg;

            ViewBag.ProductSearchImageBasedLabel = $"product search image-based ({settings.Value.ProductSearchImageBased.Approach})";

            return View(vm);
        }
    }
}