using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services
{
    public interface IProductSearchImageService
    {
        Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile);
    }

    public class ProductSearchImageService : IProductSearchImageService
    {
        private readonly string remoteServiceBaseUrl;
        private readonly ILogger<IProductSearchImageService> logger;
        private readonly HttpClient httpClient;

        public ProductSearchImageService(IOptions<AppSettings> settings, ILogger<IProductSearchImageService> logger, HttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.ArtificialIntelligenceUrl}{settings.Value.ProductSearchImagePath}/v1/productSearchImage/";
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            var tags = Enumerable.Empty<string>();
            var analyzeImageUri = Infrastructure.API.ProductSearchImageService.ClassifyImage(remoteServiceBaseUrl);
            logger.LogDebug($"{nameof(analyzeImageUri)}: {analyzeImageUri}");
            try
            {
                var response = await httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    tags = JsonConvert.DeserializeObject<string[]>(responseString);
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Exception classifying image");
            }
            return tags;
        }
    }
}
