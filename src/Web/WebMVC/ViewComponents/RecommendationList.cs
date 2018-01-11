using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebMVC.ViewComponents
{
    public class RecommendationList : ViewComponent
    {
        private readonly ICatalogAIService _catalogAIService;
        private readonly IProductRecommenderService _productRecommenderService;

        public RecommendationList(ICatalogAIService catalogAIService, IProductRecommenderService productRecommenderService)
        {
            _catalogAIService = catalogAIService;
            _productRecommenderService = productRecommenderService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string productId, string customerId)
        {
            const int maxProductRecommendations = 9;

            if (String.IsNullOrEmpty(productId))
                return View();

            try
            {
                var recommendedProductsIDs = await _productRecommenderService.GetRecommendProductsAsync(productId, customerId);
                var recommendations = await _catalogAIService.GetRecommendationsAsync(productId, recommendedProductsIDs);
                return View(recommendations.Take(maxProductRecommendations).ToList());

            } catch (BrokenCircuitException)
            {
                TempData["RecommendationInoperativeMsg"] = "Recommendation Service is inoperative, please try later on. (Business Msg Due to Circuit-Breaker)";
            }

            return View();
        }
    }
}
