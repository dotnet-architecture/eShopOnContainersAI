using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Catalog.API.AI
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly CatalogSettings _catalogSettings;

        public ComputerVisionService(IOptionsSnapshot<CatalogSettings> settings)
        {
            _catalogSettings = settings.Value;
        }

        public async Task<IEnumerable<string>> AnalyzeImageAsync(byte[] image)
        {
            // These constants are configurable depending on data features
            const double confidenceThreshold = 0.85;
            const int maxLength = 5;

            var client = new HttpClient();

            var apiKey = _catalogSettings.CognitiveService.VisionAPIKey;
            var apiUri = _catalogSettings.CognitiveService.VisionUri;

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            string uri = $"{apiUri}/tag";

            using (ByteArrayContent content = new ByteArrayContent(image))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();
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
