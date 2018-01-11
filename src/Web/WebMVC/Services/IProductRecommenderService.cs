using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public interface IProductRecommenderService
    {
        Task<IEnumerable<string>> GetRecommendProductsAsync(string productId, string customerId);
    }
}