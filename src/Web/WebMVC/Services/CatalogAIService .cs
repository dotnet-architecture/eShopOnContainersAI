using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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

        public async Task<List<CatalogItem>> GetRecommendationsAsync(string productId)
        {
            var recommendationsUri = API.CatalogAI.GetRecommendations(_remoteServiceBaseUrl, productId);

            var dataString = await _apiClient.GetStringAsync(recommendationsUri);

            var response = JsonConvert.DeserializeObject<List<CatalogItem>>(dataString);

            return response;
        }
    }
}
