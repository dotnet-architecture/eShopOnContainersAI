using eShopDashboard.Infrastructure.Data.Ordering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace eShopDashboard.Infrastructure.Setup
{
    public class OrderingContextSetup
    {
        private readonly OrderingContext _dbContext;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<OrderingContextSetup> _logger;

        public OrderingContextSetup(
            OrderingContext dbContext,
            IHostingEnvironment env,
            ILogger<OrderingContextSetup> logger)
        {
            _dbContext = dbContext;
            _env = env;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _dbContext.Orders.AnyAsync()) return;

            var setupPath = Path.Combine(_env.ContentRootPath, "Infrastructure", "Setup");
            _logger.LogInformation($@"----- Seeding OrderingContext from ""{setupPath}""");

            await SeedOrdersAsync(setupPath);
            await SeedOrderItemsAsync(setupPath);
        }

        private async Task SeedOrderItemsAsync(string setupPath)
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Seeding OrderItems");

            var sqlFile = Path.Combine(setupPath, "OrderItems.sql");

            var sqlLines = await File.ReadAllLinesAsync(sqlFile);

            _logger.LogInformation("----- Inserting OrderItems");

            var batcher = new SqlBatcher(_dbContext.Database, _logger);

            await batcher.ExecuteInsertCommandsAsync(sqlLines);

            _logger.LogInformation($"----- OrderItems Inserted ({sw.Elapsed.TotalSeconds:n3}s)");
        }

        private async Task SeedOrdersAsync(string setupPath)
        {
            var sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("----- Seeding Orders");

            var sqlFile = Path.Combine(setupPath, "Orders.sql");

            var sqlLines = await File.ReadAllLinesAsync(sqlFile);

            _logger.LogInformation("----- Inserting Orders");

            var batcher = new SqlBatcher(_dbContext.Database, _logger);

            await batcher.ExecuteInsertCommandsAsync(sqlLines);

            _logger.LogInformation($"----- Orders Inserted ({sw.Elapsed.TotalSeconds:n3}s)");
        }
    }
}