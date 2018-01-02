using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Services
{
    public interface IOrderItems
    {
        Task<IEnumerable<OrderItems.OrderItemsSchema>> GetOrderItemsAsync();
    }

    public class OrderItems : IOrderItems
    {
        private readonly string remoteServiceBaseUrl;
        private readonly IHttpClient httpClient;

        public OrderItems(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IHttpClient httpClient)
        {
            remoteServiceBaseUrl = $"{settings.Value.OrderingUrl}/api/v1/OrderingAI/";
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<OrderItemsSchema>> GetOrderItemsAsync()
        {
            var allOrderItemsUri = Infrastructure.API.OrderItems.GetAll(remoteServiceBaseUrl);

            // TODO: use request with header: Accept-Encoding: gzip
            var dataString = await httpClient.GetStringAsync(allOrderItemsUri);

            var response = JsonConvert.DeserializeObject<IEnumerable<OrderItemsSchema>>(dataString);

            return response;
        }

        public class OrderItemsSchema
        {
            public string CustomerId { get; set; }
            public string ProductId { get; set; }
            public int Units { get; set; }
        }
    }
}
