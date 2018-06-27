using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductRecommender.AzureML.API.Recommender
{
    public class AzureMLClient : IAzureMLClient
    {
        private readonly ILogger<AzureMLClient> _logger;
        private readonly AppSettings _catalogSettings;

        public AzureMLClient(IOptionsSnapshot<AppSettings> settings, ILogger<AzureMLClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _catalogSettings = settings.Value;
        }

        public Task<IEnumerable<string>> RecommendationsAsync(string productId) => RecommendationsAsync(productId, string.Empty);

        public async Task<IEnumerable<string>> RecommendationsAsync(string productId, string customerId)
        {
            if (String.IsNullOrEmpty(customerId))
                customerId = "0";

            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {"CustomerId", customerId},
                                            {"ProductId", productId},
                                            {"Units", "10"},
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                string apiKey = _catalogSettings.AzureMachineLearning.RecommendationAPIKey;
                string apiUri = _catalogSettings.AzureMachineLearning.RecommendationUri;
                if (String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(apiUri))
                {
                    _logger.LogError("Please provide apiKey and URI to the Machine Learning WebService");
                    return Enumerable.Empty<string>();
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                if (!Uri.TryCreate(apiUri, UriKind.Absolute, out Uri api))
                {
                    _logger.LogError("API URI not valid URI format");
                    return Enumerable.Empty<string>();
                }
                client.BaseAddress = api;

                HttpResponseMessage response = await client.PostAsync(String.Empty, new JsonContent(scoreRequest));

                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"The request failed with status code: {response.StatusCode}");
                    _logger.LogDebug(response.Headers.ToString());
                    _logger.LogDebug(jsonResponse);
                    return Enumerable.Empty<string>();
                }

                var result = (JsonConvert
                                    .DeserializeObject<dynamic>(jsonResponse)
                                    .Results
                                    .output1[0] as JObject)
                                .Properties()
                                .Values()
                                .Skip(1) // first value is the productId used as parameter
                                .Select(c => c.ToString())
                                .ToArray();

                return result;
            }
        }

        class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }
    }
}
