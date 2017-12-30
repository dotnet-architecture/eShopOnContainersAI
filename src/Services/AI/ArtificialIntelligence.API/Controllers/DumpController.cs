using ArtificialIntelligence.API.Extensions;
using ArtificialIntelligence.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class DumpController : ControllerBase
    {
        private readonly ICatalog catalog;
        private readonly IUsers users;
        private readonly IOrderItems orderItems;

        public DumpController(ICatalog catalog, IUsers users, IOrderItems orderItems)
        {
            this.catalog = catalog;
            this.users = users;
            this.orderItems = orderItems;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Catalog()
        {
            var items = await catalog.GetCatalog();

            var csvFile = File(Encoding.UTF8.GetBytes(items.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "catalog.csv";
            return csvFile;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Users()
        {
            var items = await users.GetUsersAsync();

            var csvFile = File(Encoding.UTF8.GetBytes(items.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "users.csv";
            return csvFile;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> OrderItems()
        {
            var items = await orderItems.GetOrderItemsAsync();

            var csvFile = File(Encoding.UTF8.GetBytes(items.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "orderItems.csv";
            return csvFile;
        }

    }
}
