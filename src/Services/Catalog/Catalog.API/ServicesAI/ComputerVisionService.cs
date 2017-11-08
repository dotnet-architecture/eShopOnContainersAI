using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Catalog.API.ServicesAI
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ILogger<ComputerVisionService> _logger;

        public ComputerVisionService(IOptionsSnapshot<CatalogSettings> settings, ILogger<ComputerVisionService> logger)
        {
            _catalogSettings = settings.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> AnalyzeImageAsync(byte[] image)
        {
            // These constants are configurable depending on data features
            const double confidenceThreshold = 0.85;
            const int maxLength = 5;

            var client = new HttpClient();

            var apiKey = _catalogSettings.CognitiveService.VisionAPIKey;
            var apiUri = _catalogSettings.CognitiveService.VisionUri;

            if (String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(apiUri))
            {
                _logger.LogError("Please provide apiKey and URI to the Machine Learning WebService");
                return Enumerable.Empty<string>();
            }


            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            string uri = $"{apiUri}/tag";

            using (ByteArrayContent content = new ByteArrayContent(image))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync(uri, content);
                string contentString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"The request failed with status code: {response.StatusCode}");
                    _logger.LogDebug(response.Headers.ToString());
                    _logger.LogDebug(contentString);
                    return Enumerable.Empty<string>();
                }

                var visionApiResponse = JsonConvert.DeserializeObject<VisionApiResponse>(contentString);

                var query = visionApiResponse.tags.AsQueryable();

                if (query.Any(t => t.confidence > confidenceThreshold))
                    query = query
                        .Where(t => t.confidence > confidenceThreshold)
                        .OrderByDescending(t => t.confidence)
                        .Take(maxLength);
                else
                    // In case we don't find any element matching threshold, we match only first one
                    query = query
                        .OrderByDescending(t => t.confidence)
                        .Take(1);

                return  query.Select(t => t.name);
            }
        }

        private class VisionApiResponse
        {
            public class Tag
            {
                public string name { get; set; }
                public double confidence { get; set; }
            }

            public Tag[] tags { get; set; }
        }
    }
}
