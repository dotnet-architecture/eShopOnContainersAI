using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Infrastructure;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Controllers
{
    public enum ViewApproaches {basic, online, offline, @default };

    [Route("api/v1/productSearchImage")]
    public class ProductSearchImageBasedController : Controller
    {
        private readonly IVisionStrategy visionStrategy;

        public ProductSearchImageBasedController(IVisionStrategy visionStrategy)
        {
            this.visionStrategy = visionStrategy;
        }

        [HttpPost]
        [Route("classifyImage")]
        public Task<IActionResult> ClassifyImage(IFormFile imageFile)
        {
            return ClassifyImage(ViewApproaches.@default, imageFile);
        }

        [HttpPost]
        [Route("classifyImage/{approach}")]
        public async Task<IActionResult> ClassifyImage([FromRoute]ViewApproaches approach, IFormFile imageFile)
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

                Approaches classifyApproach;
                switch (approach)
                {
                    case ViewApproaches.offline:
                        classifyApproach = Approaches.CustomVisionOffline;
                        break;
                    case ViewApproaches.online:
                        classifyApproach = Approaches.CustomVisionOnline;
                        break;
                    case ViewApproaches.basic:
                        classifyApproach = Approaches.ComputerVision;
                        break;
                    default:
                        classifyApproach = Approaches.Default;
                        break;
                }

                tags = await visionStrategy.ClassifyImageAsync(imageData, classifyApproach);
            }

            return Ok(tags);
        }
    }
}
