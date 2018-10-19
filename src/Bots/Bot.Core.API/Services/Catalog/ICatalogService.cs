using Microsoft.eShopOnContainers.Bot.API.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Catalog
{
    public interface ICatalogService
    {
        Task<IEnumerable<Brand>> GetBrandsAsync();
        Task<IEnumerable<CatalogType>> GetTypesAsync();
    }
}
