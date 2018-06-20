using eShopOnContainers.Core.Services.Catalog;
using eShopOnContainers.Core.Services.Dependency;
using System.Threading.Tasks;
using Xunit;

namespace eShopOnContainers.UnitTests
{
    public class BasketServiceTests
    {
        static DependencyService _dependencyService = new DependencyService();

        [Fact]
        public async Task GetFakeBasketTest()
        {
            var catalogMockService = new CatalogMockService(_dependencyService);       
            var result  = await catalogMockService.GetCatalogAsync();
            Assert.NotEmpty(result);
        }
    }
}