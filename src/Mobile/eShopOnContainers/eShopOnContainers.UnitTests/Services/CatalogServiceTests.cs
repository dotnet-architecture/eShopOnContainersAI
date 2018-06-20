using eShopOnContainers.Core.Services.Catalog;
using eShopOnContainers.Core.Services.Dependency;
using System.Threading.Tasks;
using Xunit;

namespace eShopOnContainers.UnitTests
{
    public class CatalogServiceTests
    {
        static DependencyService _dependencyService = new DependencyService();

        [Fact]
        public async Task GetFakeCatalogTest()
        {
            var catalogMockService = new CatalogMockService(_dependencyService);
            var catalog = await catalogMockService.GetCatalogAsync();

            Assert.NotEmpty(catalog);
        }

        [Fact]
        public async Task GetFakeCatalogBrandTest()
        {
            var catalogMockService = new CatalogMockService(_dependencyService);
            var catalogBrand = await catalogMockService.GetCatalogBrandAsync();

            Assert.NotEmpty(catalogBrand);
        }

        [Fact]
        public async Task GetFakeCatalogTypeTest()
        {
            var catalogMockService = new CatalogMockService(_dependencyService);
            var catalogType = await catalogMockService.GetCatalogTypeAsync();

            Assert.NotEmpty(catalogType);
        }
    }
}