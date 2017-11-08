namespace Microsoft.eShopOnContainers.Services.Catalog.API
{
    public class CatalogSettings
    {
        public string PicBaseUrl { get;set;}

        public string EventBusConnection { get; set; }

        public bool UseCustomizationData { get; set; }
        public bool UseCustomizationDataAI { get; set; }
        public bool AzureStorageEnabled { get; set; }
        public string ConnectionString { get; set; }

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

        public AzureMachineLearningSchema AzureMachineLearning { get; set; }
        public CognitiveServiceSchema CognitiveService { get; set; }

        public string MongoConnectionString { get; set; }
        public string MongoDatabase { get; set; }
    }
}
