using System;
using System.Collections.Generic;

namespace Microsoft.eShopOnContainers.Bot.API.Models.Catalog
{
    [Serializable]
    public class CatalogFilter
    {
        public int? Brand { get; set; }
        public int? Type { get; set; }
        public IEnumerable<string> Tags { get; set; }

        public CatalogFilter()
        {
        }

        public static CatalogFilter Map( dynamic value)
        {
            CatalogFilter filter = new CatalogFilter();

            if (value.Brand != "-1")
            {
                filter.Brand = Convert.ToInt32(value.Brand);
            }
            if (value.Type != "-1")
            {
                filter.Type = Convert.ToInt32(value.Type);
            }
            return filter;
        }

        public  CatalogFilter(string brand , string type)
        {
            if (!string.IsNullOrEmpty(brand))
            {
                if(brand != "-1")
                    Brand = Convert.ToInt32(brand);
            }
            if (!string.IsNullOrEmpty(type))
            {
                if(type != "-1")
                    Type = Convert.ToInt32(type);
            }
        }
    }
}