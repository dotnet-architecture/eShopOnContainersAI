using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API
{
    public class AppSettings
    {
        public class CognitiveServiceSchema
        {
            public string VisionAPIKey { get; set; }
            public string VisionUri { get; set; }
        }

        public CognitiveServiceSchema CognitiveService { get; set; }
    }
}
