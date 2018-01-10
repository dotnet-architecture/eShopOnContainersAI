using System.Configuration;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Classifier
{
    public class CNTKModelPredictionSettings
    {
        public string ModelFilename { get; private set; }
        public string LabelsFilename { get; private set; }
        public float Threshold { get; private set; }

        public static CNTKModelPredictionSettings FromWebSettings()
        {
            var modelFilename = ConfigurationManager.AppSettings["ModelFilename"];
            var labelsFilename = ConfigurationManager.AppSettings["LabelsFilename"];
            var threshold = float.Parse(ConfigurationManager.AppSettings["Threshold"]);
            return new CNTKModelPredictionSettings()
            {
                ModelFilename = modelFilename,
                LabelsFilename = labelsFilename,
                Threshold = threshold
            };
        }
    }
}
