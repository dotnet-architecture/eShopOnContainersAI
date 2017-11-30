using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArtificialIntelligence.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArtificialIntelligence.API.Controllers
{
    [Produces("application/json")]
    [Route("api/models")]
    public class ModelsController : Controller
    {
        private readonly IPrediction predictionServices;

        public ModelsController(IPrediction predictionServices)
        {
            this.predictionServices = predictionServices;
        }

        [HttpPost]
        [Route("analyzeImage/predict/withTensorFlow")]
        public async Task<IActionResult> AnalyzeImageWithTensorFlow(IFormFile imageFile)
        {
            if (imageFile.Length == 0)
                return NoContent();

            IEnumerable<string> tags = null;
            using (var image = new MemoryStream())
            {
                await imageFile.CopyToAsync(image);
                tags = predictionServices.AnalyzeImageWithTensor(image.ToArray()).DefaultIfEmpty().Distinct();
            }

            return Ok(tags);
        }
    }
}