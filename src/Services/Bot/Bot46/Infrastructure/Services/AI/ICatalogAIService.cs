using Bot46.API.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Services
{
    public interface ICatalogAIService
    {
        Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, IEnumerable<string> productIDs);
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type, IEnumerable<string> tags);
    }
}
