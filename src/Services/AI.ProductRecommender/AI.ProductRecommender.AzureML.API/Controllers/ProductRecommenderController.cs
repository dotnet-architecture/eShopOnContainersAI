using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.AI.ProductRecommender.AzureML.API.Recommender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductRecommender.AzureML.API.Controllers
{
    [Route("api/v1/productRecommender")]
    public class ProductRecommenderController : ControllerBase
    {
        private readonly IAzureMLClient azureMLClient;

        public ProductRecommenderController(IAzureMLClient azureMachineLearningService)
        {
            this.azureMLClient = azureMachineLearningService;
        }

        [HttpGet]
        [Route("recommendProducts")]
        public async Task<IActionResult> RecommendProducts([FromQuery]string productId, [FromQuery]string customerId)
        {
            if (customerId == "null")
                customerId = String.Empty;

            var results = await azureMLClient.RecommendationsAsync(productId, customerId);
            return Ok(results);
        }
    }
}
