using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductRecommender.AzureML.API
{
    public class AppSettings
    {
        public class AzureMachineLearningSchema
        {
            public string RecommendationAPIKey { get; set; }
            public string RecommendationUri { get; set; }
        }

        public AzureMachineLearningSchema AzureMachineLearning { get; set; }
    }
}
