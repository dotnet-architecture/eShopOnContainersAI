using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier
{
    public class ComputerVisionPrediction : IClassifier
    {
        private readonly IComputerVisionClient computerVisionClient;
        private readonly ComputerVisionSettings computerVisionSettings;

        public ComputerVisionPrediction(IOptionsSnapshot<AppSettings> settings, IComputerVisionClient modelManagementClient)
        {
            this.computerVisionClient = modelManagementClient;

            //TODO: move these settings to setting file
            computerVisionSettings = new ComputerVisionSettings
            {
                ComputerVisionAPIKey = settings.Value.ComputerVision?.VisionAPIKey,
                ComputerVisionUri = settings.Value.ComputerVision?.VisionUri,
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
