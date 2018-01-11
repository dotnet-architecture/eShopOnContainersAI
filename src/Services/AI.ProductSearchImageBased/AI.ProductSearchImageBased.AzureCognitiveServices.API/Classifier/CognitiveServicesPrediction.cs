using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier
{
    public class LabelConfidence
    {
        public float Probability { get; set; }
        public string Label { get; set; }
    }

    public interface ICognitiveServicesPrediction {
        Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image);
    }

    public class CognitiveServicesPrediction : ICognitiveServicesPrediction
    {
        private readonly ICognitiveServicesPredictionClient computerVisionClient;
        private readonly CognitiveServicesSettings computerVisionSettings;

        public CognitiveServicesPrediction(IOptionsSnapshot<AppSettings> settings, ICognitiveServicesPredictionClient modelManagementClient)
        {
            this.computerVisionClient = modelManagementClient;

            //TODO: move these settings to setting file
            computerVisionSettings = new CognitiveServicesSettings
            {
                ComputerVisionAPIKey = settings.Value.CognitiveService.VisionAPIKey,
                ComputerVisionUri = settings.Value.CognitiveService.VisionUri,
                Threshold = 0.85f,
                MaxLength = 5
            };
        }

        public async Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image)
        {
            return await computerVisionClient.Tags(image, computerVisionSettings);
        }
    }

}
