using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Models;
using Microsoft.Bots.Bot.API.Models.Basket;
using Microsoft.Bots.Bot.API.Models.Order;
using Newtonsoft.Json;
using Order = Microsoft.Bots.Bot.API.Models.Order.Order;

namespace Microsoft.Bots.Bot.API.Services
{
    public class OrderingService : IOrderingService
    {
        private IHttpClient _apiClient;
        private readonly string _remoteServiceBaseUrl;
        private readonly BotSettings _settings;

        public OrderingService(BotSettings settings, IHttpClient httpClient)
        {
            _remoteServiceBaseUrl = $"{settings.OrderingUrl}/api/v1/orders";
            _settings = settings;
            _apiClient = httpClient;
        }

        public async Task<Order> GetOrder(string id, string userToken)
        {
            var token = userToken;
            var getOrderUri = Infrastructure.Order.GetOrder(_remoteServiceBaseUrl, id);

            var dataString = await _apiClient.GetStringAsync(getOrderUri, token);

            var response = JsonConvert.DeserializeObject<Order>(dataString);

            return response;
        }

        public async Task<List<Order>> GetMyOrders(string userId, string userToken)
        {
            var token = userToken;
            var allMyOrdersUri = Infrastructure.Order.GetAllMyOrders(_remoteServiceBaseUrl, userId);

            var dataString = await _apiClient.GetStringAsync(allMyOrdersUri, token);
            var response = JsonConvert.DeserializeObject<List<Order>>(dataString);

            return response;
        }

        public Order MapUserInfoIntoOrder(UserData user, Order order)
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
            var token = userToken;
            var order = new OrderDTO()
            {
                OrderNumber = orderId
            };

            var cancelOrderUri = Infrastructure.Order.CancelOrder(_remoteServiceBaseUrl);

            var response = await _apiClient.PutAsync(cancelOrderUri, order, token, Guid.NewGuid().ToString());

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new Exception("Error cancelling order, try later.");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task ShipOrder(string orderId, string userToken)
        {
            var token = userToken;
            var order = new OrderDTO()
            {
                OrderNumber = orderId
            };

            var shipOrderUri = Infrastructure.Order.ShipOrder(_remoteServiceBaseUrl);

            var response = await _apiClient.PutAsync(shipOrderUri, order, token, Guid.NewGuid().ToString());

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new Exception("Error in ship order process, try later.");
            }

            response.EnsureSuccessStatusCode();
        }

        public void OverrideUserInfoIntoOrder(Order original, Order destination)
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

        public BasketDTO MapOrderToBasket(Order order)
        {
            order.CardExpirationApiFormat();

            return new BasketDTO()
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