using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
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
        private readonly HttpClient httpClient;

        public ProductSearchImageService(IOptions<AppSettings> settings, HttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.ArtificialIntelligenceUrl}{settings.Value.ProductSearchImagePath}/v1/productSearchImage/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] imageFile)
        {
            var tags = Enumerable.Empty<string>();
            try
            {

                var analyzeImageUri = Infrastructure.API.ProductSearchImageService.ClassifyImage(remoteServiceBaseUrl);

                var response = await httpClient.PostFileAsync(analyzeImageUri, imageFile, "imageFile");
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    tags = JsonConvert.DeserializeObject<string[]>(responseString);
                }
            }
            catch (System.Exception e)
            {

            }
            return tags;
        }
    }
}
