using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;
using Ordering.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class RankingsAIController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IOrderQueries _orderQueries;

        public RankingsAIController(IMediator mediator, IOrderQueries orderQueries)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderQueries = orderQueries ?? throw new ArgumentNullException(nameof(orderQueries));
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Dump()
        {
            var rankings = await _orderQueries.GetRankings();
            var rankingsObject = rankings
                .Select(c => new { c.CustomerId, c.ProductId,  c.Ranking })
                .ToList();

            var csvFile = File(Encoding.UTF8.GetBytes(rankingsObject.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "rankings.csv";
            return csvFile;
        }
    }
}
