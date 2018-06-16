using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API
{
    public class AppSettings
    {
        public class ComputerVisionSchema
        {
            public string VisionAPIKey { get; set; }
            public string VisionUri { get; set; }
        }

        public ComputerVisionSchema ComputerVision { get; set; }

        public class CustomVisionSchema
        {
            public string PredictionKey { get; set; }
            public string ProjectId { get; set; }
        }

        public CustomVisionSchema CustomVision { get; set; }

        public string AIModelsPath { get; set; }

        public string CognitiveServicesPredictionDefaultModel { get; set; }
    }
}
