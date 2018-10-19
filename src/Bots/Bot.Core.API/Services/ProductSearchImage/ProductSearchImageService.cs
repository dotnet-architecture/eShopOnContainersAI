using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.ProductSearchImage
{
    public class ProductSearchImageService : IProductSearchImageService
    {
        private readonly string remoteServiceBaseUrl;
        private readonly ILogger<IProductSearchImageService> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public ProductSearchImageService(IOptions<AppSettings> settings, ILogger<IProductSearchImageService> logger, IHttpClientFactory httpClientFactory)
        {
            remoteServiceBaseUrl = $"{settings.Value.ArtificialIntelligenceUrl}{settings.Value.ProductSearchImagePath}/v1/productSearchImage/";
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            var tags = Enumerable.Empty<string>();
            var analyzeImageUri = Infrastructure.API.ProductSearchImageService.ClassifyImage(remoteServiceBaseUrl);
            logger.LogDebug($"{nameof(analyzeImageUri)}: {analyzeImageUri}");
            try
            {
                var httpClient = httpClientFactory.CreateClient();
                //var response = await httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
                HttpContent content = new MultipartFormDataContent()
                {
                    { new ByteArrayContent(imageFile), "\"imageFile\"", "\"imageFile\"" }
                };
                var response = await httpClient.PostAsync(analyzeImageUri, content);
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
