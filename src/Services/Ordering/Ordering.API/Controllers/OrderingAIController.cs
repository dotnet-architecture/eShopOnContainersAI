using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;
using Ordering.API.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class OrderingAIController : ControllerBase
    {
        private readonly IOrderQueries _orderQueries;

        public OrderingAIController(IOrderQueries orderQueries)
        {
            _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> All()
        {
            var orderItems = await _orderQueries.GetOrderItems();

            var typedOrderItems = orderItems
                .Select(c => new { c.CustomerId, c.ProductId, c.Units })
                .ToList();

            return new JsonResult(typedOrderItems);
        }
    }
}
