using System.Collections.Generic;
using System.Threading.Tasks;
using ArtificialIntelligence.API.Services.ComputerVision.Client;
using Microsoft.Extensions.Options;

namespace ArtificialIntelligence.API.Services.ComputerVision.Models
{
    public interface ICognitiveServicesComputerVision { }

    public class CognitiveServicesComputerVision : ICognitiveServicesComputerVision, IClassifier
    {
        private readonly ICognitiveServicesComputerVisionClient computerVisionClient;
        private readonly CognitiveServicesSettings computerVisionSettings;

        public CognitiveServicesComputerVision(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, ICognitiveServicesComputerVisionClient modelManagementClient)
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
