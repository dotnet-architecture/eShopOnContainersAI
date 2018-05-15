using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
    public class CatalogTagsRepository : ICatalogTagsRepository
    {
        private readonly CatalogTagsContext dbContext;

        public CatalogTagsRepository(IOptionsSnapshot<CatalogSettings> settings)
        {
            dbContext = new CatalogTagsContext(settings);
        }

        public bool IsEmpty => dbContext.CatalogFullTags.Count(Builders<CatalogFullTag>.Filter.Empty) == 0;

        public Task<List<CatalogTag>> FindMatchingTagsAsync(IEnumerable<string> tags)
        {
            var filter = Builders<CatalogFullTag>.Filter.AnyIn(x => x.Tagrams, tags);
            var projection = Builders<CatalogFullTag>.Projection.Expression(c => c as CatalogTag);

            return dbContext.CatalogFullTags
                .Find(filter)
                .Project(projection)
                .ToListAsync();
        }

        public Task<List<CatalogFullTag>> FindMatchingProductsAsync(IEnumerable<int> productIds)
        {
            var filter = Builders<CatalogFullTag>.Filter.In(x => x.ProductId, productIds);

            return dbContext.CatalogFullTags
                .Find(filter)
                .ToListAsync();
        }

        public async Task InsertAsync(IEnumerable<CatalogFullTag> catalogTags)
        {
            // InsertManyAsync uses internally BulkWriteAsync
            // https://stackoverflow.com/questions/32921533/mongodb-c-sharp-driver-2-0-insertmanyasync-vs-bulkwriteasync
            await dbContext.CatalogFullTags.InsertManyAsync(catalogTags);
        }

        public Task<List<CatalogFullTag>> All {
            get
            {
                return dbContext.CatalogFullTags.AsQueryable().ToListAsync();
            }
        }

    }
}
