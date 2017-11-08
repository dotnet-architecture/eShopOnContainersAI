using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMVC.Infrastructure;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public class CatalogAIService : ICatalogAIService
    {
        private readonly IHttpClient _apiClient;

        private readonly string _remoteServiceBaseUrl;

        public CatalogAIService(IOptionsSnapshot<AppSettings> settings, IHttpClient httpClient)
        {
            _apiClient = httpClient;

            _remoteServiceBaseUrl = $"{settings.Value.CatalogUrl}/api/v1/catalogAI/";
        }

        public async Task<string[]> AnalyzeImage(byte[] imageFile)
        {
            var analyzeImageUri = API.CatalogAI.AnalyzeImage(_remoteServiceBaseUrl);

            var response = await _apiClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
            var responseString = await response.Content.ReadAsStringAsync();

            var tags = JsonConvert.DeserializeObject<string[]>(responseString);

            return tags;
        }

        public async Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, string customerId)
        {
            var recommendationsUri = API.CatalogAI.GetRecommendations(_remoteServiceBaseUrl, productId, customerId);

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
