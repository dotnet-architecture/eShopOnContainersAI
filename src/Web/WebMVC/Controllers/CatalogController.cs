using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.ViewModels.Pagination;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels.CatalogViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Microsoft.eShopOnContainers.WebMVC.Controllers
{
    public class CatalogController : Controller
    {
        private ICatalogService _catalogSvc;
        private readonly ICatalogAIService _catalogAISvc;

        public CatalogController(ICatalogService catalogSvc, ICatalogAIService catalogAIService) {
            _catalogSvc = catalogSvc;
            _catalogAISvc = catalogAIService;
        }

        public async Task<IActionResult> Index(int? BrandFilterApplied, int? TypesFilterApplied, int? page, IFormFile ImageFilter)
        {
            var itemsPage = 12;
            var catalog = await _catalogSvc.GetCatalogItems(page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied);
            if (ImageFilter != null && ImageFilter.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await ImageFilter.CopyToAsync(ms);
                    var tags = await _catalogAISvc.AnalyzeImage(ms.ToArray());
                }
            }

            var vm = new IndexViewModel()
            {
                CatalogItems = catalog.Data,
                Brands = await _catalogSvc.GetBrands(),
                Types = await _catalogSvc.GetTypes(),
                BrandFilterApplied = BrandFilterApplied ?? 0,
                TypesFilterApplied = TypesFilterApplied ?? 0,
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

            return View(vm);
        }

        private async Task<byte[]> ProcessFormFile (IFormFile formFile)
        {
            if (formFile.Length == 0)
                return null;

            try
            {
                byte[] fileContents;

                using (var reader = new MemoryStream())
                {
                    await formFile.CopyToAsync(reader);
                    fileContents = reader.ToArray();

                    // Check the content length in case the file's only
                    // content was a BOM and the content is actually
                    // empty after removing the BOM.
                    if (fileContents.Length > 0)
                    {
                        return fileContents;
                    }
                }
            }
            catch (Exception ex)
            {
                //modelState.AddModelError(formFile.Name, $"The {fieldDisplayName}file ({fileName}) upload failed. Please contact the Help Desk for support.");
                // Log the exception
            }

            return null;
        }
    }
}