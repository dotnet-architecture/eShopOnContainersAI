using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier
{
    public class CustomVisionOnlinePrediction : IClassifier
    {
        private readonly ICustomVisionClient customVisionClient;
        private readonly CustomVisionSettings customVisionSettings;

        public CustomVisionOnlinePrediction(IOptionsSnapshot<AppSettings> settings, ICustomVisionClient customVisionClient)
        {
            this.customVisionClient = customVisionClient;

            Guid projectId;
            if (settings.Value.CustomVision == null || string.IsNullOrEmpty(settings.Value.CustomVision.ProjectId) || !Guid.TryParse(settings.Value.CustomVision.ProjectId, out projectId))
                projectId = Guid.Empty;                

            //TODO: move these settings to setting file
            customVisionSettings = new CustomVisionSettings
            {
                CustomVisionPredictionKey = settings.Value.CustomVision?.PredictionKey,
                CustomVisionProjectId = projectId,
                Threshold = 0.85f,
                MaxLength = 5
            };
        }

        public async Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image)
        {
            return await customVisionClient.Tags(image, customVisionSettings);
        }
    }
}
