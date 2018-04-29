using eShopDashboard.EntityModels.Catalog;
using eShopDashboard.Infrastructure.Data.Catalog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace eShopDashboard.Infrastructure.Setup
{
    public class CatalogContextSetup
    {
        private readonly CatalogContext _dbContext;
        private readonly ILogger<CatalogContextSetup> _logger;
        private readonly string _setupPath;

        public CatalogContextSetup(
            CatalogContext dbContext,
            IHostingEnvironment env,
            ILogger<CatalogContextSetup> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _setupPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup");
        }

        public async Task SeedAsync()
        {
            if (await _dbContext.CatalogItems.AnyAsync()) return;

            _logger.LogInformation($@"----- Seeding CatalogContext from ""{_setupPath}""");

            await SeedCatalogItemsAsync();
        }

        private async Task SeedCatalogItemsAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Seeding CatalogItems");

            var sqlFile = Path.Combine(_setupPath, "CatalogItems.sql");

            var sqlLines = await File.ReadAllLinesAsync(sqlFile);

            _logger.LogInformation("----- Inserting CatalogItems");

            var batcher = new SqlBatcher(_dbContext.Database, _logger);

            await batcher.ExecuteInsertCommandsAsync(sqlLines);

            _logger.LogInformation($"----- CatalogItems Inserted ({sw.Elapsed.TotalSeconds:n3}s)");

            await SeedCatalogTagsAsync();
        }

        private async Task SeedCatalogTagsAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Adding CatalogTags");
            var tagsText = await File.ReadAllTextAsync(Path.Combine(_setupPath, "CatalogTags.json"));

            var tags = JsonConvert.DeserializeObject<List<CatalogFullTag>>(tagsText);

            _logger.LogInformation("----- Adding tags to CatalogItems");

            int i = 0;

            foreach (var tag in tags)
            {
                var entity = await _dbContext.CatalogItems.FirstOrDefaultAsync(ci => ci.Id == tag.ProductId);

                if (entity == null) continue;

                entity.TagsJson = JsonConvert.SerializeObject(tag);

                _dbContext.Update(entity);
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"----- {i} CatalogTags added ({sw.Elapsed.TotalSeconds:n3}s)");
        }
    }
}