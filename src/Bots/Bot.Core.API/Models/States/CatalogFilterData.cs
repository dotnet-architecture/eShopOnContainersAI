namespace Microsoft.eShopOnContainers.Bot.API
{
    public class CatalogFilterData
    {
        public const string Key = nameof(CatalogFilterData);
        public string Brand { get; set; }
        public string Type { get; set; }
        public string[] Tags { get; set; }
        public int PageIndex { get; set; }
        public const int PageSize = 6;
    }

}
