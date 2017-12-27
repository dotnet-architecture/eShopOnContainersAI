using System.Collections.Generic;
using System.Threading.Tasks;
using ArtificialIntelligence.API.Services.ComputerVision.Client;
using Microsoft.Extensions.Options;

namespace ArtificialIntelligence.API.Services.ComputerVision.Models
{
    public interface IModelManagementService { }

    public class ModelManagementService : IModelManagementService, IClassifier
    {
        private readonly IModelManagementClient modelManagementClient;
        private readonly ModelManagementPredictionSettings modelManagementPredictionSettings;

        public ModelManagementService(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IModelManagementClient modelManagementClient)
        {
            this.modelManagementClient = modelManagementClient;

            //TODO: move these settings to setting file
            modelManagementPredictionSettings = new ModelManagementPredictionSettings
            {
                ServiceKey = settings.Value.ModelManagementServiceKey,
                ServiceUri = settings.Value.ModelManagementServiceUri,
                Threshold = 0.9f
            };
        }

        public async Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image)
        {
            return await modelManagementClient.ProcessImage(image, modelManagementPredictionSettings);
        }
    }
}
