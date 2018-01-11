namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Classifier
{
    public class TensorFlowPredictionSettings
    {
        public string InputTensorName { get; set; }
        public string OutputTensorName { get; set; }
        public string ModelFilename { get; set; }
        public string LabelsFilename { get; set; }
        public float Threshold { get; set; }
    }
}

