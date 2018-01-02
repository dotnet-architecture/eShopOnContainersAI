using ArtificialIntelligence.API.Services.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Services.ComputerVision.Client
{
    public interface ICognitiveServicesComputerVisionClient
    {
        Task<IEnumerable<LabelConfidence>> Tags(byte[] image, CognitiveServicesSettings settings);
    }

    public class CognitiveServicesSettings
    {
        public string ComputerVisionAPIKey { get; set; }
        public string ComputerVisionUri { get; set; }
        public float Threshold { get; set; }
        public int MaxLength { get; set; }
    }

    public class CognitiveServicesComputerVisionClient : ICognitiveServicesComputerVisionClient
    {
        private readonly IOptionsSnapshot<ArtificialIntelligenceSettings> settings;
        private readonly ILogger<CognitiveServicesComputerVisionClient> logger;

        public CognitiveServicesComputerVisionClient(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, ILogger<CognitiveServicesComputerVisionClient> logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public async Task<IEnumerable<LabelConfidence>> Tags(byte[] image, CognitiveServicesSettings settings)
        {
            using (var client = new HttpClient())
            {

                var apiKey = settings.ComputerVisionAPIKey;
                var apiUri = settings.ComputerVisionUri;

                if (String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(apiUri))
                {
                    logger.LogError("Please provide apiKey and URI to the Machine Learning WebService");
                    return null;
                }

                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

                string apiUriTag = $"{apiUri}/tag";

                using (ByteArrayContent requestContent = new ByteArrayContent(image))
                {
                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    var response = await client.PostAsync(apiUriTag, requestContent);

                    string contentString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError($"The request failed with status code: {response.StatusCode}");
                        logger.LogDebug(response.Headers.ToString());
                        logger.LogDebug(contentString);
                        return null;
                    }

                    var visionApiResponse = JsonConvert.DeserializeObject<VisionApiResponse>(contentString);

                    var query = visionApiResponse.tags.AsQueryable();

                    if (query.Any(t => t.confidence > settings.Threshold))
                        query = query
                            .Where(t => t.confidence > settings.Threshold)
                            .OrderByDescending(t => t.confidence)
                            .Take(settings.MaxLength);
                    else
                        // In case we don't find any element matching threshold, we match only first one
                        query = query
                            .OrderByDescending(t => t.confidence)
                            .Take(1);

                    return query.Select(t => new LabelConfidence() { Label = t.name, Probability = (float)t.confidence });
                }
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
