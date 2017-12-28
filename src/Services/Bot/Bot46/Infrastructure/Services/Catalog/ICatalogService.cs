using Bot46.API.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Services
{
    public interface ICatalogService
    {
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type);
        Task<IEnumerable<Brand>> GetBrands();
        Task<IEnumerable<CatalogType>> GetTypes();
    }
}
