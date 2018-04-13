using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMVC.Infrastructure;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public class ProductSearchImageBasedService : IProductSearchImageBasedService
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public ProductSearchImageBasedService(IOptionsSnapshot<AppSettings> settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.ProductSearchImageUrl}/v1/productSearchImage/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            var analyzeImageUri = API.ProductImageSearch.ClassifyImage(remoteServiceBaseUrl);

            var response = await httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var tags = JsonConvert.DeserializeObject<string[]>(responseString);
                return tags;
            }

            return Enumerable.Empty<string>();
        }
    }
}
