using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Services
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public ComputerVisionService(BotSettings settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.ArtificialIntelligenceUrl}/api/v1/computerVision/";
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
