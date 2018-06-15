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

        public string MongoConnectionString { get; set; }
        public string MongoDatabase { get; set; }
    }
}
