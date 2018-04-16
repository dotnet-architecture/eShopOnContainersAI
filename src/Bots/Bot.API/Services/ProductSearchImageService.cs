using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Infrastructure;
using Newtonsoft.Json;

namespace Microsoft.Bots.Bot.API.Services
{
    public class ProductSearchImageService : IProductSearchImageService
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public ProductSearchImageService(BotSettings settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.ArtificialIntelligenceUrl}/image-tensorflow-api/v1/productSearchImage/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            string[] tags = null;
            try {

                var analyzeImageUri = Infrastructure.ProductSearchImageService.ClassifyImage(remoteServiceBaseUrl);

                var response = await httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
                var responseString = await response.Content.ReadAsStringAsync();

                tags = JsonConvert.DeserializeObject<string[]>(responseString);

                return tags;
            }
            catch (System.Exception e) {

            }
            return tags;
        }
    }
}
