using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Models.Catalog;

namespace Microsoft.Bots.Bot.API.Services
{
    public interface ICatalogAIService
    {
        Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, IEnumerable<string> productIDs);
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type, IEnumerable<string> tags);
    }
}
