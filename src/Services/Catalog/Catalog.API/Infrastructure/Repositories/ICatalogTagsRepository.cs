using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
    public interface ICatalogTagsRepository
    {
        Task<List<CatalogTag>> FindMatchingTagsAsync(IEnumerable<string> tags);
        Task<List<CatalogTag>> FindMatchingProductsAsync(IEnumerable<int> productIds);
        Task InsertAsync(IEnumerable<CatalogFullTag> catalogTags);
        bool IsEmpty { get; }
        Task<List<CatalogFullTag>> All { get; }
    }
}