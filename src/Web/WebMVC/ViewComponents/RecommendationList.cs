using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
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

        public RecommendationList(ICatalogAIService catalogAIService) => _catalogAIService = catalogAIService;

        public async Task<IViewComponentResult> InvokeAsync(string productId)
        {
            const int maxProductRecommendations = 3;

            if (String.IsNullOrEmpty(productId))
                return View();

            try
            {
                var recommendations = await _catalogAIService.GetRecommendationsAsync(productId);
                return View(recommendations.Take(maxProductRecommendations).ToList());

            } catch (BrokenCircuitException)
            {
                TempData["RecommendationInoperativeMsg"] = "Recommendation Service is inoperative, please try later on. (Business Msg Due to Circuit-Breaker)";
            }

            return View();
        }
    }
}
