using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Services
{
    public interface ICatalog
    {
        Task<IEnumerable<Catalog.CatalogSchema>> GetCatalog();
    }

    public class Catalog : ICatalog
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public Catalog(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.CatalogUrl}/api/v1/catalogAI/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<CatalogSchema>> GetCatalog()
        {
            var allcatalogItemsUri = Infrastructure.API.Catalog.GetAll(remoteServiceBaseUrl);

            // TODO: use request with header: Accept-Encoding: gzip
            var dataString = await httpClient.GetStringAsync(allcatalogItemsUri);

            var response = JsonConvert.DeserializeObject<IEnumerable<CatalogSchema>>(dataString);

            return response;
        }

        public class CatalogSchema
        {
            public string Id { get; set; }
            public string CatalogBrandId { get; set; }
            public string Description { get; set; }
            public double Price { get; set; }
            public string color { get; set; }
            public string size { get; set; }
            public string shape { get; set; }
            public string quantity { get; set; }
            public string agram { get; set; }
            public string bgram { get; set; }
            public string abgram { get; set; }
            public string zgram { get; set; }
            public string ygram { get; set; }
            public string yzgram { get; set; }
        }
    }
}
