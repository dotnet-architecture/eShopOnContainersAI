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
            var client = new HttpClient();

            var apiKey = _catalogSettings.CognitiveService.VisionAPIKey;
            var apiUri = _catalogSettings.CognitiveService.VisionUri;

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "visualFeatures=Tags,Description&language=en";

            // Assemble the URI for the REST API Call.
            string uri = apiUri + "/analyze" + "?" + requestParameters;

            using (ByteArrayContent content = new ByteArrayContent(image))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                var response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
                var customVisionResponse = JsonConvert.DeserializeObject<CustomVisionResponse>(contentString);

                return customVisionResponse.tags.Select(t => t.name)
                    .Concat(customVisionResponse.description.tags)
                    .Distinct();
            }
        }

        private class CustomVisionResponse
        {
            public class Tag
            {
                public string name { get; set; }
                public double confidence { get; set; }
            }

            public class Description
            {
                public class Caption
                {
                    public string text { get; set; }
                    public double confidence { get; set; }
                }

                public string[] tags { get; set; }
                public Caption[] captions { get; set; }
            }

            public Tag[] tags { get; set; }
            public Description description { get; set; }
        }
    }
}
