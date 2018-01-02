using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.ServicesAI
{
    public interface IAzureMachineLearningService
    {
        Task<IEnumerable<string>> RecommendationsAsync(string productId, string customerId);
        Task<IEnumerable<string>> RecommendationsAsync(string productId);
    }
}