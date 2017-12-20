using Bot.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveCards;

namespace Bot.API.Services
{
    public interface ICatalogService
    {
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type);
        Task<IEnumerable<Brand>> GetBrands();
        Task<IEnumerable<CatalogType>> GetTypes();
    }
}
