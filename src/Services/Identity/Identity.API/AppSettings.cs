namespace Microsoft.eShopOnContainers.Services.Identity.API
{
    public class AppSettings
    {
        public string ConnectionString { get; set; }

        public string MvcClient { get; set; }

        public bool UseCustomizationData { get; set; }

        public bool UseCustomizationDataAI { get; set; }
    }
}
