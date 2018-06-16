using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.WebDashboardRazor.Infrastructure;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;
using System.Net.Http;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Services
{
    public class OrderingService : IOrderingService
    {
        private readonly AppSettings appSettings;
        private readonly HttpClient apiClient;

        public OrderingService(IOptions<AppSettings> settings, HttpClient httpClient)
        {
            this.appSettings = settings.Value;
            this.apiClient = httpClient;
        }

        public async Task<IEnumerable<ProductSales>> GetProductSalesAsync()
        {
            var dataString = await apiClient.GetStringAsync(API.Ordering.ProductStats(appSettings.WebShoppingUrl, "json"));

            return JsonConvert.DeserializeObject<IEnumerable<ProductSales>>(dataString);
        }

        public async Task<IEnumerable<dynamic>> GetProductHistoryDepthAsync(IEnumerable<string> productIds)
        {
            var dataString = await apiClient.GetStringAsync(API.Ordering.ProductsDepths(appSettings.WebShoppingUrl, productIds));

            return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(dataString);
        }
    }
}
