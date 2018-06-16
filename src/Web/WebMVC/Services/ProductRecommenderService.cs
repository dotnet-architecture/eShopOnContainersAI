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
    public class ProductRecommenderService : IProductRecommenderService
    {
        private readonly HttpClient _apiClient;

        private readonly string _remoteServiceBaseUrl;

        public ProductRecommenderService(IOptions<AppSettings> settings, HttpClient httpClient)
        {
            _apiClient = httpClient;

            _remoteServiceBaseUrl = $"{settings.Value.ArtificialIntelligenceUrl}/recommender-azure-api/v1/productRecommender/";
        }

        public async Task<IEnumerable<string>> GetRecommendProductsAsync(string productId, string customerId)
        {
            var recommendationsUri = API.ProductRecommender.GetRecommendProducts(_remoteServiceBaseUrl, productId, customerId);

            var dataString = await _apiClient.GetStringAsync(recommendationsUri);

            var response = String.IsNullOrEmpty(dataString) ?
                Enumerable.Empty<string>() :
                JsonConvert.DeserializeObject<List<string>>(dataString);

            return response;
        }
    }
}
