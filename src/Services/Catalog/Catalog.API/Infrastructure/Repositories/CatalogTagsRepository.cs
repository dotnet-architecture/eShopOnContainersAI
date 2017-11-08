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

        public bool Empty => dbContext.CatalogTags.Count(Builders<CatalogTag>.Filter.Empty) == 0;

        public Task<List<CatalogTag>> FindMatchingCatalogTagAsync(IEnumerable<string> tags)
        {
            var filter = Builders<CatalogTag>.Filter.AnyIn(x => x.Tagrams, tags);

            return dbContext.CatalogTags
                .Find(filter)
                .ToListAsync();
        }

        public async Task InsertAsync(IEnumerable<CatalogTag> catalogTags)
        {
            // InsertManyAsync uses internally BulkWriteAsync
            // https://stackoverflow.com/questions/32921533/mongodb-c-sharp-driver-2-0-insertmanyasync-vs-bulkwriteasync
            await dbContext.CatalogTags.InsertManyAsync(catalogTags);
        }

    }
}
