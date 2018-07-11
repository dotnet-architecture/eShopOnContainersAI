using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using eShopOnContainers.Core.Models.Catalog;
using eShopOnContainers.Core.Services.RequestProvider;
using eShopOnContainers.Core.Extensions;
using System.Collections.Generic;
using eShopOnContainers.Core.Services.FixUri;
using System.Linq;
using eShopOnContainers.Core.AI.ProductSearchImageBased;
using eShopOnContainers.Core.Helpers;

namespace eShopOnContainers.Core.Services.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IRequestProvider _requestProvider;
        private readonly IFixUriService _fixUriService;
        private readonly IOnlineImageClassifier imageClassifier;
        private const string ApiAIUrlBase = "api/v1/c/catalogai";
        private const string ApiUrlBase = "api/v1/c/catalog";

        public CatalogService(IRequestProvider requestProvider, IFixUriService fixUriService, IOnlineImageClassifier imageClassifier)
        {
            _requestProvider = requestProvider;
            _fixUriService = fixUriService;
            this.imageClassifier = imageClassifier;
        }

        public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/items/type/{catalogTypeId}/brand/{catalogBrandId}");

            CatalogRoot catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
                return catalog?.Data.ToObservableCollection();
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogItem>> GetCatalogAsync()
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/items");

            CatalogRoot catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
            {
                _fixUriService.FixCatalogItemPictureUri(catalog?.Data);
                return catalog?.Data.ToObservableCollection();
            }
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId, byte[] image)
        {
            UriBuilder builder = new UriBuilder(GlobalSetting.Instance.GatewayShoppingEndpoint);
            builder.Path = $"{ApiAIUrlBase}/items";

            const int page = 0;
            const int take = 12;

            var tags = Enumerable.Empty<string>();
            if (image != null)
            {
                tags = await imageClassifier.ClassifyImage(image);
                if (tags == null || !tags.Any())
                    return new ObservableCollection<CatalogItem>();
            }


            var brandQs = catalogBrandId == 0 ? String.Empty : $"&catalogBrandId={catalogBrandId}";
            var typeQs  = catalogTypeId == 0 ? String.Empty : $"&catalogTypeId={catalogTypeId}";
            var tagsQs = (tags != null && tags.Any()) ? $"&tags={String.Join(",", tags)}" : String.Empty;

            builder.Query = $"?pageIndex={page}&pageSize={take}{brandQs}{typeQs}{tagsQs}";

            string uri = builder.ToString();

            CatalogRoot catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
                return catalog?.Data.ToObservableCollection();
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogBrand>> GetCatalogBrandAsync()
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/catalogbrands");

            IEnumerable<CatalogBrand> brands = await _requestProvider.GetAsync<IEnumerable<CatalogBrand>>(uri);

            if (brands != null)
            {
                var temp = brands.ToList();
                temp.Insert(0, new CatalogBrand { Id = 0, Brand = "All" });
                return temp?.ToObservableCollection();
            }
            else
                return new ObservableCollection<CatalogBrand>();
        }

        public async Task<ObservableCollection<CatalogType>> GetCatalogTypeAsync()
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/catalogtypes");

            IEnumerable<CatalogType> types = await _requestProvider.GetAsync<IEnumerable<CatalogType>>(uri);

            if (types != null)
            {
                var temp = types.ToList();
                temp.Insert(0, new CatalogType { Id = 0, Type = "All" });
                return temp?.ToObservableCollection();
            }
            else
                return new ObservableCollection<CatalogType>();
        }
    }
}
