using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.WebMVC.ViewModels
{
    public class Catalog
    {
        public static Catalog Empty { get; } = new Catalog()
        {
            PageIndex = 0,
            PageSize = 0,
            Count = 0,
            Data = Enumerable.Empty<CatalogItem>().ToList()
        };

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Count { get; set; }
        public List<CatalogItem> Data { get; set; }
    }
}
