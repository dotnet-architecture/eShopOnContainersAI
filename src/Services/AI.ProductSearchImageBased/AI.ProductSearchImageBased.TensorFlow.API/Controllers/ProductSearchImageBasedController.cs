using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Classifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Controllers
{
    public enum AnalyzeImageModel
    {
        Default,
        TensorFlowInception,
        TensorFlowModel
    }

    [Route("api/v1/productSearchImage")]
    public class ProductSearchImageBasedController : Controller
    {
        private readonly ITensorFlowPredictionStrategy predictionServices;

        public ProductSearchImageBasedController(ITensorFlowPredictionStrategy predictionServices)
        {
            this.predictionServices = predictionServices;
        }

        [HttpPost]
        [Route("classifyImage")]
        public async Task<IActionResult> ClassifyImage(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                return NoContent();

            IEnumerable<string> tags = null;
            using (var image = new MemoryStream())
            {
                await imageFile.CopyToAsync(image);
                tags = await predictionServices.ClassifyImageAsync(image.ToArray());
            }

            return Ok(tags);
        }
    }
}
