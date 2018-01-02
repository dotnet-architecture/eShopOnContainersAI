using Catalog.API.ServicesAI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IAzureMachineLearningService azureMachineLearningService;

        public RecommendationController(IAzureMachineLearningService azureMachineLearningService)
        {
            this.azureMachineLearningService = azureMachineLearningService;
        }

        [HttpGet]
        [Route("for/product/{productId}/customer/{customerId}")]
        public async Task<IActionResult> RecommendProduct(string productId, string customerId)
        {
            var results = await azureMachineLearningService.RecommendationsAsync(productId, customerId);
            return Ok(results);
        }
    }
}
