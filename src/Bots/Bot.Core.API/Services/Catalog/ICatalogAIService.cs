using Microsoft.eShopOnContainers.Bot.API.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Catalog
{
    public interface ICatalogAIService
    {
        Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, IEnumerable<string> productIDs);
        Task<Models.Catalog.Catalog> GetCatalogItems(int page, int take, int? brand, int? type, IEnumerable<string> tags);
    }
}
