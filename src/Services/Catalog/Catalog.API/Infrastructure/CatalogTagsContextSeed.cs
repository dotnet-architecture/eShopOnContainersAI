using Microsoft.AspNetCore.Hosting;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure
{
    public class CatalogTagsContextSeed : ICatalogTagsContextSeed
    {
        private readonly ICatalogTagsRepository catalogTagsRepository;
        private readonly IHostingEnvironment env;
        private readonly ILogger<CatalogTagsContextSeed> logger;

        public CatalogTagsContextSeed(ICatalogTagsRepository catalogTagsRepository, IHostingEnvironment env, ILogger<CatalogTagsContextSeed> logger)
        {
            this.catalogTagsRepository = catalogTagsRepository;
            this.env = env;
            this.logger = logger;
        }

        public async Task SeedAsync()
        {
            logger.LogInformation("Starting CatalogTags seed");

            var contentRootPath = env.ContentRootPath;

            if (catalogTagsRepository.Empty)
            {
                var catalogTagsSeed = await GetCatalogTagsFromFile(contentRootPath, logger);
                if (catalogTagsSeed.Any())
                {
                    await catalogTagsRepository.Insert(catalogTagsSeed);
                    logger.LogInformation("CatalogTags inserted into database.");
                }
            }
        }

        private async Task<IEnumerable<CatalogTag>> GetCatalogTagsFromFile(string contentRootPath, ILogger<CatalogTagsContextSeed> logger)
        {
            string jsonCatalogTagsFile = Path.Combine(contentRootPath, "Setup", "CatalogTags.txt");

            if (!File.Exists(jsonCatalogTagsFile))
            {
                logger.LogInformation($"File {jsonCatalogTagsFile} not found. No tags added to database.");
                return Enumerable.Empty<CatalogTag>();
            }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(jsonCatalogTagsFile);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<CatalogTag>>(jsonContent);
            } catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return Enumerable.Empty<CatalogTag>();
            }
        }
    }
}
