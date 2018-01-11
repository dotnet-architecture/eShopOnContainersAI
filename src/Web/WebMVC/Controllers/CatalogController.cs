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

namespace Microsoft.eShopOnContainers.WebMVC.Controllers
{
    public class CatalogController : Controller
    {
        private ICatalogService _catalogSvc;
        private readonly ICatalogAIService _catalogAISvc;
        private readonly IProductSearchImageBasedService _productImageSearchService;

        public CatalogController(ICatalogService catalogSvc, ICatalogAIService catalogAIService, IProductSearchImageBasedService productSearchImageService) {
            _catalogSvc = catalogSvc;
            _catalogAISvc = catalogAIService;
            _productImageSearchService = productSearchImageService;
        }

		public async Task<IActionResult> Index(int? BrandFilterApplied, int? TypesFilterApplied, int? page, IFormFile ImageFilter, string Tags, [FromQuery]string errorMsg)
        {
            var itemsPage = 12;

            IEnumerable<string> tags = null;
            if (ImageFilter != null && ImageFilter.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await ImageFilter.CopyToAsync(ms);
                    tags = await _productImageSearchService.ClassifyImageAsync(ms.ToArray());
                }
            }
            else if (!String.IsNullOrEmpty(Tags))
                tags = Tags.Split(',');

            var catalog = await _catalogAISvc.GetCatalogItems(page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied, tags);
            var tagsAvailable = (tags == null || !tags.Any());

            var vm = new IndexViewModel()
            {
                CatalogItems = catalog.Data,
                Brands = await _catalogSvc.GetBrands(),
                Types = await _catalogSvc.GetTypes(),
                BrandFilterApplied = BrandFilterApplied ?? 0,
                TypesFilterApplied = TypesFilterApplied ?? 0,
                Tags = tagsAvailable ? String.Empty : String.Join(',',tags),
                TagsActive = tagsAvailable ? String.Empty : "active",
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page ?? 0,
                    ItemsPerPage = catalog.Data.Count,
                    TotalItems = catalog.Count, 
                    TotalPages = (int)Math.Ceiling(((decimal)catalog.Count / itemsPage))
                }
            };

            vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
            vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";

            ViewBag.BasketInoperativeMsg = errorMsg;

            return View(vm);
        }
    }
}