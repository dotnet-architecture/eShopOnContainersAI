using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.WebDashboardRazor.Infrastructure;
using Microsoft.eShopOnContainers.WebDashboardRazor.Models;
using System.Net.Http;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly AppSettings appSettings;
        private readonly HttpClient apiClient;

        public CatalogService(IOptions<AppSettings> settings, HttpClient httpClient)
        {
            this.appSettings = settings.Value;
            this.apiClient = httpClient;
        }

        public async Task<IEnumerable<ProductInfo>> GetProductInfoAsync()
        {
            var dataString = await apiClient.GetStringAsync(API.Catalog.ProductInfo(appSettings.WebShoppingUrl, "json"));

            return JsonConvert.DeserializeObject<IEnumerable<ProductInfo>>(dataString);
        }

        public async Task<IEnumerable<ProductInfo>> GetSimilarProductsAsync(string description)
        {
            var dataString = await apiClient.GetStringAsync(API.Catalog.SimilarProducts(appSettings.WebShoppingUrl, description));

            return JsonConvert.DeserializeObject<IEnumerable<ProductInfo>>(dataString);
        }

        public string GetProductPicture(string productId)
        {
            return API.Catalog.ProductPicture(appSettings.WebShoppingUrlExternal, productId);
        }

    }
}
