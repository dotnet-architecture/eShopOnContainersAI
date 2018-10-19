using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.eShopOnContainers.Bot.API.Models.User;
using OrderModels = Microsoft.eShopOnContainers.Bot.API.Models.Order;
using BasketModels = Microsoft.eShopOnContainers.Bot.API.Services.Basket;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Order
{
    public class OrderingService : IOrderingService
    {
        private readonly AppSettings appSettings;
        private readonly string _remoteServiceBaseUrl;
        private readonly IHttpClientFactory httpClientFactory;

        public OrderingService(IOptions<AppSettings> settings, IHttpClientFactory httpClientFactory)
        {
            this.appSettings = settings.Value;
            _remoteServiceBaseUrl = $"{this.appSettings.PurchaseUrl}/api/v1/o/orders";
            this.httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateOrderClient (string userToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.SetBearerToken(userToken);
            return httpClient;
        }

        private (HttpClient, HttpContent) CreateOrderClient<T> (T item, string userToken, bool setRequestId = true)
        {
            var client = CreateOrderClient(userToken);
            if (setRequestId)
                client.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());

            var content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");

            return (client, content);
        }

        public async Task<OrderModels.Order> GetOrder(string id, string userToken)
        {
            var client = CreateOrderClient(userToken);
            var getOrderUri = Infrastructure.API.Order.GetOrder(_remoteServiceBaseUrl, id);
            var dataString = await client.GetStringAsync(getOrderUri);

            var response = JsonConvert.DeserializeObject<OrderModels.Order>(dataString);

            return response;
        }

        public async Task<List<OrderModels.Order>> GetMyOrders(string userId, string userToken)
        {
            var client = CreateOrderClient(userToken);
            var allMyOrdersUri = Infrastructure.API.Order.GetAllMyOrders(_remoteServiceBaseUrl, userId);

            var dataString = await client.GetStringAsync(allMyOrdersUri);
            var response = JsonConvert.DeserializeObject<List<OrderModels.Order>>(dataString);

            return response;
        }

        public OrderModels.Order MapUserInfoIntoOrder(UserData user, OrderModels.Order order)
        {
            order.City = user.City;
            order.Street = user.Street;
            order.State = user.State;
            order.Country = user.Country;
            order.ZipCode = user.ZipCode;

            order.CardNumber = user.CardNumber;
            order.CardHolderName = user.CardHolderName;
            order.CardExpiration = new DateTime(int.Parse("20" + user.Expiration.Split('/')[1]), int.Parse(user.Expiration.Split('/')[0]), 1);
            order.CardSecurityNumber = user.SecurityNumber;

            return order;
        }

        public async Task CancelOrder(string orderId, string userToken)
        {
            var order = new OrderDTO()
            {
                OrderNumber = orderId
            };
            var (client, content) = CreateOrderClient(order, userToken);

            var cancelOrderUri = Infrastructure.API.Order.CancelOrder(_remoteServiceBaseUrl);

            var response = await client.PutAsync(cancelOrderUri, content);

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new Exception("Error cancelling order, try later.");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task ShipOrder(string orderId, string userToken)
        {
            var order = new OrderDTO()
            {
                OrderNumber = orderId
            };

            var (client, content) = CreateOrderClient(order, userToken);

            var shipOrderUri = Infrastructure.API.Order.ShipOrder(_remoteServiceBaseUrl);

            var response = await client.PutAsync(shipOrderUri, content);

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new Exception("Error in ship order process, try later.");
            }

            response.EnsureSuccessStatusCode();
        }

        public void OverrideUserInfoIntoOrder(OrderModels.Order original, OrderModels.Order destination)
        {
            destination.City = original.City;
            destination.Street = original.Street;
            destination.State = original.State;
            destination.Country = original.Country;
            destination.ZipCode = original.ZipCode;

            destination.CardNumber = original.CardNumber;
            destination.CardHolderName = original.CardHolderName;
            destination.CardExpiration = original.CardExpiration;
            destination.CardSecurityNumber = original.CardSecurityNumber;
        }

        public BasketModels.BasketDTO MapOrderToBasket(OrderModels.Order order)
        {
            order.CardExpirationApiFormat();

            return new BasketModels.BasketDTO()
            {
                City = order.City,
                Street = order.Street,
                State = order.State,
                Country = order.Country,
                ZipCode = order.ZipCode,
                CardNumber = order.CardNumber,
                CardHolderName = order.CardHolderName,
                CardExpiration = order.CardExpiration,
                CardSecurityNumber = order.CardSecurityNumber,
                CardTypeId = 1,
                Buyer = order.Buyer,
                RequestId = order.RequestId
            };
        }
    }
}