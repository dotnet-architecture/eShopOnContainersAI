using Microsoft.eShopOnContainers.Bot.API.Models.Catalog;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Catalog
{

    //public class MockCatalogService : ICatalogService
    //{
    //    public Task<IEnumerable<Brand>> GetBrandsAsync()
    //    {
    //        IEnumerable<Brand> result = new[] { new Brand() { Id = "-1", Text = "All" } };
    //        return Task.FromResult(result);
    //    }

    //    public Task<IEnumerable<CatalogType>> GetTypesAsync()
    //    {
    //        IEnumerable<CatalogType> result = new[] { new CatalogType() { Id="-1", Text="All" } };
    //        return Task.FromResult(result);
    //    }
    //}

    //public class MockCatalogAIService : ICatalogAIService
    //{
    //    public Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type, IEnumerable<string> tags)
    //    {
    //        var catalog = new Catalog();
    //        catalog.Count = 300;
    //        catalog.PageIndex = page;
    //        catalog.PageSize = take;
    //        catalog.Data = new List<CatalogItem> { new CatalogItem() { Name = "product_name", Description="description", Price = 99, Id = "000" } };

    //        return Task.FromResult(catalog);
    //    }

    //    public Task<IEnumerable<CatalogItem>> GetRecommendationsAsync(string productId, IEnumerable<string> productIDs)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class CatalogService : ICatalogService
    {
        //private readonly AppSettings _settings;
        private readonly HttpClient _apiClient;

        private readonly string _remoteServiceBaseUrl;

        public CatalogService(IOptions<AppSettings> settings, HttpClient httpClient)
        {
            //_settings = settings.Value;
            _apiClient = httpClient;

            _remoteServiceBaseUrl = $"{settings.Value.PurchaseUrl}/api/v1/c/catalog/";
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync()
        {
            var getBrandsUri = Infrastructure.API.Catalog.GetAllBrands(_remoteServiceBaseUrl);

            var dataString = await _apiClient.GetStringAsync(getBrandsUri);

            var items = new List<Brand>();
            items.Add(new Brand() { Id = "-1", Text = "All", IsSelected = true });

            var brands = JArray.Parse(dataString);

            foreach (var brand in brands.Children<JObject>())
            {
                items.Add(new Brand()
                {
                    Id = brand.Value<string>("id"),
                    Text = brand.Value<string>("brand")
                });
            }

            return items;
        }

        public async Task<IEnumerable<CatalogType>> GetTypesAsync()
        {
            var getTypesUri = Infrastructure.API.Catalog.GetAllTypes(_remoteServiceBaseUrl);

            var dataString = await _apiClient.GetStringAsync(getTypesUri);

            var items = new List<CatalogType>();
            items.Add(new CatalogType() { Id = "-1", Text = "All", IsSelected = true });

            var brands = JArray.Parse(dataString);
            foreach (var brand in brands.Children<JObject>())
            {
                items.Add(new CatalogType()
                {
                    Id = brand.Value<string>("id"),
                    Text = brand.Value<string>("type")
                });
            }
            return items;
        }
    }
}
