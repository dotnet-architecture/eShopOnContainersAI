using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.ServicesAI
{
    public class Recommendation : IRecommendation
    {
        private readonly string remoteServiceUrl;
        private readonly IHttpClient httpClient;

        public Recommendation(IOptionsSnapshot<CatalogSettings> settings, IHttpClient httpClient)
        {
            remoteServiceUrl = $"{settings.Value.ArtificialIntelligenceUrl}/api/v1/recommendation/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> RecommendAsync(string productId, string customerId)
        {
            var recommendUri = Infrastructure.API.Recommendation.Recommend(remoteServiceUrl, productId, customerId);

            var dataString = await httpClient.GetStringAsync(recommendUri);

            var response = JsonConvert.DeserializeObject<IEnumerable<string>>(dataString);

            return response;
        }
    }

    public interface IRecommendation
    {
        Task<IEnumerable<string>> RecommendAsync(string productId, string customerId);
    }
}
