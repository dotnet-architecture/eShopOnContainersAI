using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API
{
    public class AppSettings
    {
        public string AIModelsPath { get; set; }
        public string TensorFlowPredictionDefaultModel { get; set; }
    }
}
