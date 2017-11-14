using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Catalog.API.Infrastructure
{
    public class CatalogTagsContext
    {
        private readonly IMongoDatabase _database = null;

        public CatalogTagsContext(IOptions<CatalogSettings> settings)
        {
            var client = new MongoClient(settings.Value.MongoConnectionString);

            if (client != null)
            {
                _database = client.GetDatabase(settings.Value.MongoDatabase);
            }
        }

        public IMongoCollection<CatalogFullTag> CatalogFullTags
        {
            get
            {
                return _database.GetCollection<CatalogFullTag>("CatalogTags");
            }
        }
    }
}
