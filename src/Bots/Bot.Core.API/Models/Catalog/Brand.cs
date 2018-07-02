using System;

namespace Microsoft.eShopOnContainers.Bot.API.Models.Catalog
{
    [Serializable]
    public class Brand
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsSelected {get;set;}
    }
}