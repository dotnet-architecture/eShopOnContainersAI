using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using eShopDashboard.EntityModels.Catalog;
using eShopDashboard.Infraestructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace eShopDashboard.Infraestructure.Setup
{
    public class CatalogContextSetup
    {
        private readonly CatalogContext _dbContext;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<CatalogContextSetup> _logger;

        public CatalogContextSetup(
            CatalogContext dbContext,
            IHostingEnvironment env,
            ILogger<CatalogContextSetup> logger)
        {
            _dbContext = dbContext;
            _env = env;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _dbContext.CatalogItems.AnyAsync()) return;

            var setupPath = Path.Combine(_env.ContentRootPath, "Infraestructure", "Setup");

            _logger.LogInformation($@"Seeding CatalogContext from ""{setupPath}""");
            _logger.LogInformation("Reading CatalogItems seed data");
            string insert = await File.ReadAllTextAsync(Path.Combine(setupPath, "CatalogItems.sql"));

            _logger.LogInformation("Inserting CatalogItems records");
            await _dbContext.Database.ExecuteSqlCommandAsync(insert);

            _logger.LogInformation("Committing CatalogItems");
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Reading CatalogTags seed data");
            var tagsText = await File.ReadAllTextAsync(Path.Combine(setupPath, "CatalogTags.txt"));

            _logger.LogInformation("Deserializing CatalogTags");
            var tags = JsonConvert.DeserializeObject<List<CatalogFullTag>>(tagsText);

            _logger.LogInformation("Adding tags to CatalogItems");

            int i = 0;

            foreach (var tag in tags)
            {
                var entity = await _dbContext.CatalogItems.FirstOrDefaultAsync(ci => ci.Id == tag.ProductId);

                if (entity == null) continue;

                entity.TagsJson = JsonConvert.SerializeObject(tag);

                _dbContext.Update(entity);

                if (++i % 100 == 0)
                {
                    _logger.LogInformation($"Adding tags to CatalogItems ({i})");
                }
            }

            _logger.LogInformation($"Added tags to {i} CatalogItems");

            _logger.LogInformation("Committing Tags");
            await _dbContext.SaveChangesAsync();
        }
    }
}
