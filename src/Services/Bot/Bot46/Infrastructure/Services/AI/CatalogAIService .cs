using Bot46.API.Infrastructure.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Services
{
    public class CatalogAIService : ICatalogAIService
    {
        private readonly IHttpClient _apiClient;

        private readonly string _remoteServiceBaseUrl;

        public CatalogAIService(BotSettings settings, IHttpClient httpClient)
        {
            _apiClient = httpClient;

            _remoteServiceBaseUrl = $"{settings.CatalogUrl}/api/v1/catalogAI/";
        }

        public async Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, IEnumerable<string> productIDs)
        {
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
