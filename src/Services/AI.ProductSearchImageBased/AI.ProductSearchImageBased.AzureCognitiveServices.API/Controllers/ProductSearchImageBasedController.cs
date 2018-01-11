using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Controllers
{
    [Route("api/v1/productSearchImage")]
    public class ProductSearchImageBasedController : Controller
    {
        private readonly ICognitiveServicesPrediction cognitiveServicesPrediction;

        public ProductSearchImageBasedController(ICognitiveServicesPrediction predictionServices)
        {
            this.cognitiveServicesPrediction = predictionServices;
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
                tags = await ClassifyImageAsync(image.ToArray());
            }

            return Ok(tags);
        }

        /// <summary>
        /// Classify an image, using a model
        /// </summary>
        /// <param name="image">image (jpeg) file to be analyzed</param>
        /// <param name="model">model used for classification</param>
        /// <returns>image related labels</returns>
        private async Task<IEnumerable<string>> ClassifyImageAsync(byte[] image)
        {
            var classification = await cognitiveServicesPrediction.ClassifyImageAsync(image);

            return classification.DefaultIfEmpty()
                .OrderByDescending(c => c.Probability)
                .Select(c => c.Label);
        }

    }
}
