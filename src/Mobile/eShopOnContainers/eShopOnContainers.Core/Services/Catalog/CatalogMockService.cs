using eShopOnContainers.Core.AI.ProductSearchImageBased;
using eShopOnContainers.Core.Extensions;
using eShopOnContainers.Core.Models.Catalog;
using eShopOnContainers.Core.Services.Dependency;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace eShopOnContainers.Core.Services.Catalog
{
    public class CatalogMockService : ICatalogService
    {
        
        public CatalogMockService(IDependencyService dependencyService)
        {
            this.imageClassifier = dependencyService.Get<IImageClassifier>();
        }

        private ObservableCollection<CatalogBrand> MockCatalogBrand = new ObservableCollection<CatalogBrand>
        {
            new CatalogBrand { Id = 0, Brand = "All" },
            new CatalogBrand { Id = 1, Brand = "Azure" },
            new CatalogBrand { Id = 2, Brand = "Visual Studio" },
            new CatalogBrand { Id = 3, Brand = ".NET" }
        };

        private ObservableCollection<CatalogType> MockCatalogType = new ObservableCollection<CatalogType>
        {
            new CatalogType { Id = 0, Type = "All" },
            new CatalogType { Id = 1, Type = "Mug" },
            new CatalogType { Id = 2, Type = "T-Shirt" },
            new CatalogType { Id = 3, Type = "Frisbee"}
        };

        private ObservableCollection<CatalogItem> MockCatalog = new ObservableCollection<CatalogItem>
        {
            new CatalogItem { Id = Common.Common.MockCatalogItemId01, PictureUri = Device.RuntimePlatform != Device.UWP ? "fake_product_01.png" : "Assets/fake_product_01.png", Name = ".NET Bot Blue Sweatshirt (M)", Price = 19.50M, CatalogBrandId = 2, CatalogBrand = "Visual Studio", CatalogTypeId = 2, CatalogType = "T-Shirt" },
            new CatalogItem { Id = Common.Common.MockCatalogItemId02, PictureUri = Device.RuntimePlatform != Device.UWP ? "fake_product_02.png" : "Assets/fake_product_02.png", Name = ".NET Bot Purple Sweatshirt (M)", Price = 19.50M, CatalogBrandId = 2, CatalogBrand = "Visual Studio", CatalogTypeId = 2, CatalogType = "T-Shirt" },
            new CatalogItem { Id = Common.Common.MockCatalogItemId03, PictureUri = Device.RuntimePlatform != Device.UWP ? "fake_product_03.png" : "Assets/fake_product_03.png", Name = ".NET Bot Black Sweatshirt (M)", Price = 19.95M, CatalogBrandId = 2, CatalogBrand = "Visual Studio", CatalogTypeId = 2, CatalogType = "T-Shirt" },
            new CatalogItem { Id = Common.Common.MockCatalogItemId04, PictureUri = Device.RuntimePlatform != Device.UWP ? "fake_product_04.png" : "Assets/fake_product_04.png", Name = ".NET Black Cupt", Price = 17.00M, CatalogBrandId = 2, CatalogBrand = "Visual Studio", CatalogTypeId = 1, CatalogType = "Mug" },
            new CatalogItem { Id = Common.Common.MockCatalogItemId05, PictureUri = Device.RuntimePlatform != Device.UWP ? "fake_product_05.png" : "Assets/fake_product_05.png", Name = "Azure Black Sweatshirt (M)", Price = 19.50M, CatalogBrandId = 1, CatalogBrand = "Azure", CatalogTypeId = 2, CatalogType = "T-Shirt" },
            new CatalogItem { Id = Common.Common.MockCatalogItemId06, PictureUri = Device.RuntimePlatform != Device.UWP ? "fake_product_06.png" : "Assets/fake_product_06.png", Name = ".NET Foundation Frisbee", Price = 19.50M, CatalogBrandId = 3, CatalogBrand = ".NET", CatalogTypeId = 3, CatalogType = "Frisbee" }
        };
        private readonly IImageClassifier imageClassifier;

        public async Task<ObservableCollection<CatalogItem>> GetCatalogAsync()
        {
            await Task.Delay(10);

            return MockCatalog;
        }

        public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
        {
            await Task.Delay(10);

            return MockCatalog
                .Where(c => c.CatalogBrandId == catalogBrandId &&
                c.CatalogTypeId == catalogTypeId)
                .ToObservableCollection();
        }

        public async Task<ObservableCollection<CatalogBrand>> GetCatalogBrandAsync()
        {
            await Task.Delay(10);

            return MockCatalogBrand;
        }

        public async Task<ObservableCollection<CatalogType>> GetCatalogTypeAsync()
        {
            await Task.Delay(10);

            return MockCatalogType;
        }

        public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId, byte[] image)
        {
            //await Task.Delay(5);

            var query = MockCatalog.AsQueryable();
            if (catalogBrandId > 0)
                query = query.Where(c => c.CatalogBrandId == catalogBrandId);
            if (catalogTypeId > 0)
                query = query.Where(c => c.CatalogTypeId == catalogTypeId);
            if (image != null)
            {
                var classification = await imageClassifier.ClassifyImage(image);
                //var classification = new[] { new ImageClassification("frisbee", 1.0f) };
                var tags = classification.Select(c => c.Tag.ToLower()).ToArray();
                query = query.Where(c => tags.Any(t => c.Name.ToLower().Contains(t)));
            }

            return query.ToObservableCollection();
        }
    }
}