using Catalog.API.Controllers;
using Microsoft.eShopOnContainers.Services.Catalog.API;
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

namespace Catalog.API.ServicesAI
{
    public class AzureMachineLearningService : IAzureMachineLearningService
    {
        private readonly ILogger<CatalogAIController> _logger;
        private readonly CatalogSettings _catalogSettings;

        public AzureMachineLearningService(IOptionsSnapshot<CatalogSettings> settings, ILogger<CatalogAIController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _catalogSettings = settings.Value;
        }

        public Task<IEnumerable<string>> Recommendations(string productId) => Recommendations(productId, string.Empty);

        public async Task<IEnumerable<string>> Recommendations(string productId, string customerId)
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
                string uri = _catalogSettings.AzureMachineLearning.RecommendationUri;
                if (String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(uri))
                {
                    _logger.LogError("Please provide apiKey and URI to the Machine Learning WebService");
                    return null;
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(uri);

                HttpResponseMessage response = await client.PostAsync(String.Empty, new JsonContent(scoreRequest));

                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"The request failed with status code: {response.StatusCode}");
                    _logger.LogDebug(response.Headers.ToString());
                    _logger.LogDebug(jsonResponse);
                    return null;
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
