using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier
{
    public class CustomVisionSettings
    {
        public string CustomVisionPredictionKey { get; set; }
        public Guid CustomVisionProjectId { get; set; }
        public float Threshold { get; set; }
        public int MaxLength { get; set; }
    }

    public interface ICustomVisionClient
    {
        Task<IEnumerable<LabelConfidence>> Tags(byte[] image, CustomVisionSettings settings);
    }

    public class CustomVisionClient : ICustomVisionClient
    {
        private readonly ILogger<CustomVisionClient> logger;        

        public CustomVisionClient(IOptionsSnapshot<AppSettings> settings, ILogger<CustomVisionClient> logger)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<LabelConfidence>> Tags(byte[] image, CustomVisionSettings settings)
        {
        // Create a prediction endpoint, passing in obtained prediction key
        var endpoint = new PredictionEndpoint() { ApiKey = settings.CustomVisionPredictionKey };

            Cognitive.CustomVision.Prediction.Models.ImagePredictionResultModel result;

            using (var testImage = new MemoryStream(image))
            {
                // Make a prediction against the prediction project
                // IMPORTANT: the prediction project should have an iteration marked as default. 
                // Otherwise, you need to specify an iteration ID
                result = await endpoint.PredictImageWithNoStoreAsync(settings.CustomVisionProjectId, testImage);
            }

            return result.Predictions.Select(t => new LabelConfidence() { Label = t.Tag, Probability = (float)t.Probability })
                            .Where(c => c.Probability >= settings.Threshold)
                            .OrderByDescending(c => c.Probability);
        }
    }
}   
