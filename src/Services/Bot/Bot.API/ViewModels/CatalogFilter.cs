using System;

namespace Bot.API.ViewModels
{
    [Serializable]
    public class CatalogFilter
    {
        public string Brand { get; set; }
        public string Type { get; set; }

        public static CatalogFilter Map( dynamic value)
        {
            return new CatalogFilter
            {
                Brand = value.Brand,
                Type = value.Type
            };
        }
    }
}