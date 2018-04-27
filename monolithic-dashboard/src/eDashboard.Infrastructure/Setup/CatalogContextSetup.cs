using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using eDashboard.Infrastructure.Data;
using eShopDashboard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace eDashboard.Infrastructure.Setup
{
    public class CatalogContextSetup
    {
        public static async Task SeedAsync(CatalogContext dbContext)
        {
            if (await dbContext.CatalogItems.AnyAsync()) return;

            var setupPath = Path.Combine(Path.GetDirectoryName(new Uri(typeof(CatalogContextSetup).Assembly.CodeBase).AbsolutePath), "Setup");

            string insert = await File.ReadAllTextAsync(Path.Combine(setupPath, "CatalogItems.sql"));

            await dbContext.Database.ExecuteSqlCommandAsync(insert);

            var tagsText = await File.ReadAllTextAsync(Path.Combine(setupPath, "CatalogTags.txt"));

            var tags = JsonConvert.DeserializeObject<List<CatalogFullTag>>(tagsText);

            foreach (var tag in tags)
            {
                var entity = await dbContext.FindAsync<CatalogItem>(tag.ProductId);

                if (entity == null) continue;

                entity.Tags = JsonConvert.SerializeObject(tag);

                dbContext.Update(entity);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
