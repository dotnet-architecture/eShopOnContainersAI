using Catalog.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [Route("api/v1/[controller]")]
    public class CatalogAIController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly ILogger<CatalogAIController> _logger;
        private readonly CatalogSettings _catalogSettings;

        public CatalogAIController(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings, ILogger<CatalogAIController> logger)
        {
            _catalogContext = context;
            _logger = logger;
            _catalogSettings = settings.Value;
        }

        [HttpGet]
        [Route("[action]/{productId}")]
        [ProducesResponseType(typeof(CatalogItem[]), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Recommendation(string productId)
        {
            var recommendations = await InvokeAMLRecommendationService(productId, _logger, _catalogSettings);
            var recommendationsInt = recommendations
                .Select(c => Convert.ToInt32(c))
                .ToArray();

            var items = await _catalogContext.CatalogItems
                .Where(c => recommendationsInt.Contains(c.Id))
                .ToArrayAsync();

            return Ok(items);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Dump()
        {
            var catalog = await _catalogContext.CatalogItems
                .Select(c => new { c.CatalogBrandId, c.CatalogTypeId, c.Description, c.Price })
                .ToListAsync();

            var csvFile = File(Encoding.UTF8.GetBytes(catalog.FormatAsCSV()), "text/csv");
            csvFile.FileDownloadName = "catalog.csv";
            return csvFile;
        }

        class StringTable
        {
            public string[] ColumnNames { get; set; }
            public string[,] Values { get; set; }
        }

        class JsonContent : StringContent
        {
            public JsonContent(object obj) :
                base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }

        static async Task<IEnumerable<string>> InvokeAMLRecommendationService(string id, ILogger logger, CatalogSettings catalogSettings)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"CustomerId", "ProductId", "Rating"},
                                Values = new string[,] {  { "0", id, "10" } }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                string apiKey = catalogSettings.AzureMachineLearning.RecommendationAPIKey;
                string uri = catalogSettings.AzureMachineLearning.RecommendationUri;
                if (apiKey == string.Empty || uri == string.Empty)
                {
                    logger.LogError("Please provide apiKey and URI to the Machine Learning WebService");
                    return null;
                }

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(uri);

                HttpResponseMessage response = await client.PostAsync(String.Empty, new JsonContent(scoreRequest));

                string result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError(string.Format("The request failed with status code: {0}", response.StatusCode));
                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    logger.LogDebug(response.Headers.ToString());
                    logger.LogDebug(result);
                    return null;
                }

                var results = JsonConvert.DeserializeObject<dynamic>(result);
                return results.Results.output1.value.Values[0] as string[];
            }
        }
    }
}
