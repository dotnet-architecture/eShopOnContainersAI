using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using eShopOnContainers.Core.Models.Catalog;
using eShopOnContainers.Core.Services.RequestProvider;
using eShopOnContainers.Core.Extensions;
using System.Collections.Generic;
using eShopOnContainers.Core.Services.FixUri;
using System.Linq;
using eShopOnContainers.Core.Services.Dependency;
using eShopOnContainers.Core.AI.ProductSearchImageBased;

namespace eShopOnContainers.Core.Services.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IRequestProvider _requestProvider;
        private readonly IFixUriService _fixUriService;
        private readonly IImageClassifier classifyImage;
        private const string ApiUrlBase = "mobileshoppingapigw/api/v1/c/catalog";
        private const string ApiAIUrlBase = "mobileshoppingapigw/api/v1/c/catalogai";

        public CatalogService(IRequestProvider requestProvider, IFixUriService fixUriService, IDependencyService dependencyService)
        {
            _requestProvider = requestProvider;
            _fixUriService = fixUriService;
            this.classifyImage = dependencyService.Get<IImageClassifier>();
        }

        public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
        {
            UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BaseEndpoint);
            builder.Path = $"{ApiUrlBase}/items/type/{catalogTypeId}/brand/{catalogBrandId}";
            string uri = builder.ToString();

            CatalogRoot catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
                return catalog?.Data.ToObservableCollection();
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogItem>> GetCatalogAsync()
        {
            UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BaseEndpoint);
            builder.Path = $"{ApiUrlBase}/items";
            string uri = builder.ToString();

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
            UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BaseEndpoint);
            //builder.Path = $"{ApiAIUrlBase}/items";

            const int page = 0;
            const int take = 12;

            var tags = Enumerable.Empty<ImageClassification>();
            if (image != null)
                tags = await classifyImage.ClassifyImage(image);

            var brandQs = catalogBrandId == 0 ? String.Empty : $"&catalogBrandId={catalogBrandId}";
            var typeQs  = catalogTypeId == 0 ? String.Empty : $"&catalogTypeId={catalogTypeId}";
            var tagsQs = (tags != null && tags.Any()) ? $"&tags={String.Join(",", tags.Select(t => t.Tag))}" : String.Empty;

            builder.Path = $"{ApiAIUrlBase}/items?pageIndex={page}&pageSize={take}{brandQs}{typeQs}{tagsQs}";

            string uri = builder.ToString();

            CatalogRoot catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
                return catalog?.Data.ToObservableCollection();
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogBrand>> GetCatalogBrandAsync()
        {
            UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BaseEndpoint);
            builder.Path = $"{ApiUrlBase}/catalogbrands";
            string uri = builder.ToString();

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
            UriBuilder builder = new UriBuilder(GlobalSetting.Instance.BaseEndpoint);
            builder.Path = $"{ApiUrlBase}/catalogtypes";
            string uri = builder.ToString();

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
