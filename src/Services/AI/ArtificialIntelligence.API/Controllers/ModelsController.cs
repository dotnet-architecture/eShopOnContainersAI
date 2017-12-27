using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ArtificialIntelligence.API.Services.ComputerVision;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ArtificialIntelligence.API.Controllers
{

    public enum AnalyzeImageModel
    {
        Default,
        TensorFlowInception,
        TensorFlowModel,
        CNTKModel,
        ModelManagement
    } 

    [Produces("application/json")]
    [Route("api/models")]
    public class ModelsController : Controller
    {
        private readonly IComputerVisionServices predictionServices;

        public ModelsController(IComputerVisionServices predictionServices)
        {
            this.predictionServices = predictionServices;
        }

        [HttpPost]
        [Route("analyzeImage/predict/{model}")]
        public async Task<IActionResult> AnalyzeImage(IFormFile imageFile, AnalyzeImageModel model)
        {
            if (imageFile.Length == 0)
                return NoContent();

            IEnumerable<string> tags = null;
            using (var image = new MemoryStream())
            {
                await imageFile.CopyToAsync(image);
                tags = await predictionServices.ClassifyImageAsync(image.ToArray(), model);
            }

            return Ok(tags);
        }
    }
}