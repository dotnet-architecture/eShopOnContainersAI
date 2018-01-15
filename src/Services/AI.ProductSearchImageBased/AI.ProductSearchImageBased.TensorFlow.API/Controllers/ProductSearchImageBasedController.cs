using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Classifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Infrastructure;

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
                var imageData = image.ToArray();
                if (!imageData.IsValidImage())
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType);

                tags = await predictionServices.ClassifyImageAsync(imageData);
            }

            return Ok(tags);
        }
    }
}
