using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API
{
    public class ArtificialIntelligenceSettings
    {
        public string AIModelsPath { get; set; }
        public string ComputerVisionDefaultModel { get; set; }

        public string CatalogUrl { get; set; }
        public string IdentityUrl { get; set; }
        public string OrderingUrl { get; set; }

        public class AzureMachineLearningSchema
        {
            public string RecommendationAPIKey { get; set; }
            public string RecommendationUri { get; set; }
        }

        public class CognitiveServiceSchema
        {
            public string VisionAPIKey { get; set; }
            public string VisionUri { get; set; }
        }

        public class ModelManagementSchema
        {
            public string ServiceKey { get; set; }
            public string ServiceUri { get; set; }
        }

        public AzureMachineLearningSchema AzureMachineLearning { get; set; }
        public CognitiveServiceSchema CognitiveService { get; set; }
        public ModelManagementSchema ModelManagement { get; set; }
    }
}
