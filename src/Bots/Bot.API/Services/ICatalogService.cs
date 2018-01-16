using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Models.Catalog;

namespace Microsoft.Bots.Bot.API.Services
{
    public interface ICatalogService
    {
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type);
        Task<IEnumerable<Brand>> GetBrands();
        Task<IEnumerable<CatalogType>> GetTypes();
    }
}
