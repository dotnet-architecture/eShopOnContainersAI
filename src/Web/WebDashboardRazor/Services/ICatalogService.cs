using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Services
{
    public interface ICatalogService
    {
        Task<IEnumerable<ProductInfo>> GetProductInfoAsync();
        Task<IEnumerable<ProductInfo>> GetSimilarProductsAsync(string description);
        string GetProductPicture(string productId);
    }
}