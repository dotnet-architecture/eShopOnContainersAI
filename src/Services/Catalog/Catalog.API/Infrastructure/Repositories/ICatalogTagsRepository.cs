using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
    public interface ICatalogTagsRepository
    {
        IEnumerable<CatalogTag> FindMatchingCatalogTag(IEnumerable<string> tags);
        Task Insert(IEnumerable<CatalogTag> catalogTags);
        bool Empty { get; }
    }
}