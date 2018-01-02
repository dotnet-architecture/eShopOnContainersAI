using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebMVC.Infrastructure;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public ComputerVisionService(IOptionsSnapshot<AppSettings> settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.ArtificialIntelligenceUrl}/api/v1/computerVision/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            var analyzeImageUri = API.ComputerVision.ClassifyImage(remoteServiceBaseUrl);

            var response = await httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
            var responseString = await response.Content.ReadAsStringAsync();

            var tags = JsonConvert.DeserializeObject<string[]>(responseString);

            return tags;
        }
    }
}
