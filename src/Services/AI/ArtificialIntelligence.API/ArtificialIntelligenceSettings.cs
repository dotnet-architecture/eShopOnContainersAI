using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API
{
    public class ArtificialIntelligenceSettings
    {
        public string AIModelsPath { get; set; }
        public string DefaultModel { get; set; }

        public string ModelManagementServiceUri { get; set; }
        public string ModelManagementServiceKey { get; set; }
    }
}
