using Microsoft.eShopOnContainers.Bot.API.Models.Catalog;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Microsoft.eShopOnContainers.Bot.API.Infrastructure.API;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Catalog
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
            var recommendationsUri = CatalogAI.GetProducSetDetailsByIDs(_remoteServiceBaseUrl, productId, productIDs);

            var dataString = await _apiClient.GetStringAsync(recommendationsUri);

            var response = String.IsNullOrEmpty(dataString) ?
                Enumerable.Empty<CatalogItem>() :
                JsonConvert.DeserializeObject<List<CatalogItem>>(dataString);

            return response;
        }

        public async Task<Models.Catalog.Catalog> GetCatalogItems(int page, int take, int? brand, int? type, IEnumerable<string> tags)
        {
            var allcatalogItemsUri = CatalogAI.GetAllCatalogItems(_remoteServiceBaseUrl, page, take, brand, type, tags);

            var dataString = await _apiClient.GetStringAsync(allcatalogItemsUri);

            var response = string.IsNullOrEmpty(dataString) ? Models.Catalog.Catalog.Empty : JsonConvert.DeserializeObject<Models.Catalog.Catalog>(dataString);

            return response;
        }

    }
}
