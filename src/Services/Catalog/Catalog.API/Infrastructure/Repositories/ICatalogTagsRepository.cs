using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
    public interface ICatalogTagsRepository
    {
        Task<List<CatalogTag>> FindMatchingCatalogTagAsync(IEnumerable<string> tags);
        Task InsertAsync(IEnumerable<CatalogTag> catalogTags);
        bool Empty { get; }
    }
}