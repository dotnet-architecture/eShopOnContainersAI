using Microsoft.eShopOnContainers.WebMVC.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebMVC.Infrastructure;

namespace Microsoft.eShopOnContainers.WebMVC.Services
{
    public class ProductSearchImageBasedService : IProductSearchImageBasedService
    {
        private readonly string _remoteServiceBaseUrl;
        private readonly HttpClient _httpClient;

        public ProductSearchImageBasedService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _remoteServiceBaseUrl = $"{settings.Value.ArtificialIntelligenceUrl}/{settings.Value.ProductSearchImageUrl}/v1/productSearchImage/";
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            var analyzeImageUri = API.ProductImageSearch.ClassifyImage(_remoteServiceBaseUrl);

            var response = await _httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
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
