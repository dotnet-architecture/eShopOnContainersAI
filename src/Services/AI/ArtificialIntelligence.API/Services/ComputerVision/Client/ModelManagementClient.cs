using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ArtificialIntelligence.API.Services.ComputerVision.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ArtificialIntelligence.API.Services.ComputerVision.Client
{
    public interface IModelManagementClient
    {
        Task<IEnumerable<LabelConfidence>> ProcessImage(byte[] image, ModelManagementPredictionSettings settings);
    }

    public class ModelManagementPredictionSettings
    {
        public string ServiceUri { get; set; }
        public string ServiceKey { get; set; }
        public float Threshold { get; set; }
    }

    public class ModelManagementClient : IModelManagementClient
    {
        private readonly ILogger<ModelManagementClient> logger;

        public ModelManagementClient(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, ILogger<ModelManagementClient> logger)
        {
            this.logger = logger;
        }

        public async Task<IEnumerable<LabelConfidence>> ProcessImage(byte[] image, ModelManagementPredictionSettings settings)
        {
            var base64Image = Convert.ToBase64String(image);
            var input_df = new { input_df = new { base64image = base64Image } };

            var response = await DoPostAsync(
                settings.ServiceUri,
                input_df,
                authorizationToken: String.IsNullOrWhiteSpace(settings.ServiceKey) ? null : settings.ServiceKey);

            var responseString = await response.Content.ReadAsStringAsync();

            var output = JsonConvert.DeserializeObject<dynamic[]>(
                            JsonConvert.DeserializeObject<string>(responseString)
                         );

            var result = output
                .Select(c => new LabelConfidence { Label = c.label, Probability = (float)c.probability })
                .Where (c => c.Probability >= settings.Threshold)
                .OrderByDescending(c => c.Probability)
                .ToArray();

            return result;
        }

        private async Task<HttpResponseMessage> DoPostAsync<T>(string uri, T item, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            using (var client = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(item))
                };

                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                var response = await client.SendAsync(requestMessage);

                // raise exception if HttpResponseCode 500
                // needed for circuit breaker to track fails
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            }
        }
    }
}
