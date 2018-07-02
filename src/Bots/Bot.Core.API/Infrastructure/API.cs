using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Infrastructure
{
    public class API
    {
        public static class Catalog
        {
            public static string GetAllBrands(string baseUri)
            {
                return $"{baseUri}catalogBrands";
            }

            public static string GetAllTypes(string baseUri)
            {
                return $"{baseUri}catalogTypes";
            }
        }

        public static class CatalogAI
        {
            public static string GetProducSetDetailsByIDs(string baseUri, string productId, IEnumerable<string> productIDs)
            {
                return $"{baseUri}productSetDetailsByIDs?productId={productId}&productIDs={String.Join(",", productIDs)}";
            }

            public static string GetAllCatalogItems(string baseUri, int page, int take, int? brand, int? type, IEnumerable<string> tags)
            {
                var brandQs = (brand.HasValue) ? $"&catalogBrandId={brand.Value.ToString()}" : String.Empty;
                var typeQs = (type.HasValue) ? $"&catalogTypeId={type.Value.ToString()}" : String.Empty;
                var tagsQs = (tags != null && tags.Any()) ? $"&tags={String.Join(",", tags)}" : String.Empty;

                return $"{baseUri}items?pageIndex={page}&pageSize={take}{brandQs}{typeQs}{tagsQs}";
            }
        }

        public static class ProductSearchImageService
        {
            public static string ClassifyImage(string baseUri)
            {
                return $"{baseUri}classifyImage";
            }
        }
    }
}
