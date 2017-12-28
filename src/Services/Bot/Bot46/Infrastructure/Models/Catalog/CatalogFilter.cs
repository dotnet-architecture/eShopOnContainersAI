using System;

namespace Bot46.API.Infrastructure.Models
{
    [Serializable]
    public class CatalogFilter
    {
        public int? Brand { get; set; }
        public int? Type { get; set; }

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
    }
}