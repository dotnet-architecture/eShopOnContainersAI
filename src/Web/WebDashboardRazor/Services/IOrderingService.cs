using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Services
{
    public interface IOrderingService
    {
        Task<IEnumerable<ProductSales>> GetProductSalesAsync();
    }
}