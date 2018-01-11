using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductRecommender.AzureML.API.Recommender
{
    public interface IAzureMLClient
    {
        Task<IEnumerable<string>> RecommendationsAsync(string productId, string customerId);
        Task<IEnumerable<string>> RecommendationsAsync(string productId);
    }
}