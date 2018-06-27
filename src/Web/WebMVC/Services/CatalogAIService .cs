using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebMVC.Infrastructure;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public class CatalogAIService : ICatalogAIService
    {
        private readonly HttpClient _apiClient;

        private readonly string _remoteServiceBaseUrl;

        public CatalogAIService(IOptions<AppSettings> settings, HttpClient httpClient)
        {
            _apiClient = httpClient;

            _remoteServiceBaseUrl = $"{settings.Value.PurchaseUrl}/catalog-ai-api/v1/CatalogAI/";
        }

        public async Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, IEnumerable<string> productIDs)
        {
            if (String.IsNullOrEmpty(productId) || productIDs == null || !productIDs.Any())
                return Enumerable.Empty<CatalogItem>();

            var recommendationsUri = API.CatalogAI.GetProducSetDetailsByIDs(_remoteServiceBaseUrl, productId, productIDs);

            var dataString = await _apiClient.GetStringAsync(recommendationsUri);

            var response = String.IsNullOrEmpty(dataString) ? 
                Enumerable.Empty<CatalogItem>() : 
                JsonConvert.DeserializeObject<List<CatalogItem>>(dataString);

            return response;
        }

        public async Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type, IEnumerable<string> tags)
        {
            var allcatalogItemsUri = API.CatalogAI.GetAllCatalogItems(_remoteServiceBaseUrl, page, take, brand, type, tags);

            var dataString = await _apiClient.GetStringAsync(allcatalogItemsUri);

            var response = JsonConvert.DeserializeObject<Catalog>(dataString);

            return response;
        }

    }
}
