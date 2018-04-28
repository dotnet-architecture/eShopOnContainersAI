using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using eShopDashboard.Infrastructure.Data.Ordering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            if (await _dbContext.OrderItems.AnyAsync()) return;

            var setupPath = Path.Combine(_env.ContentRootPath, "Infrastructure", "Setup");

            _logger.LogInformation($@"Seeding OrderingContext from ""{setupPath}""");
            _logger.LogInformation("Reading Orders seed data");

            var bulkInsertOptions = "format = 'CSV', firstrow = 2";

            var ordersDataFile = Path.Combine(setupPath, "Infrastructure", "Setup", "Orders.csv");
            var ordersBulkInsert = $@"bulk insert [Ordering].[Orders] from ""{ordersDataFile}"" with ({bulkInsertOptions})";
            await _dbContext.Database.ExecuteSqlCommandAsync(ordersBulkInsert);

            var orderItemsDataFile = Path.Combine(setupPath, "Infrastructure", "Setup", "OrderItems.csv");
            var orderItemsBulkInsert = $@"bulk insert [Ordering].[OrderItems] from ""{orderItemsDataFile}"" with ({bulkInsertOptions})";
            await _dbContext.Database.ExecuteSqlCommandAsync(orderItemsBulkInsert);

        }
    }
}
