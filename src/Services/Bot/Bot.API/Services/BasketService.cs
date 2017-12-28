using Bot.API.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Bot.API.Infrastructure;

namespace Bot.API.Services
{
     public class BasketService : IBasketService
    {
        private readonly IOptionsSnapshot<BotSettings>  _settings;
        private IHttpClient _apiClient;
        private readonly string _remoteServiceBaseUrl;
        private IHttpContextAccessor _httpContextAccesor;

        public BasketService(IOptionsSnapshot<BotSettings>  settings, IHttpContextAccessor httpContextAccesor, IHttpClient httpClient)
        {
            _settings = settings;
            _remoteServiceBaseUrl = $"{_settings.Value.BasketUrl}/api/v1/basket";
            _httpContextAccesor = httpContextAccesor;
            _apiClient = httpClient;
        }

        public async Task<Basket> GetBasket(string userId, string userToken)
        {
            var getBasketUri = Infrastructure.API.Basket.GetBasket(_remoteServiceBaseUrl, userId);

            var dataString = await _apiClient.GetStringAsync(getBasketUri, userToken);

            // Use the ?? Null conditional operator to simplify the initialization of response
            var response = JsonConvert.DeserializeObject<Basket>(dataString) ??
                new Basket()
                {
                    BuyerId = userId
                };

            return response;
        }

        public async Task<Basket> UpdateBasket(Basket basket,string userToken)
        {
            var updateBasketUri = Infrastructure.API.Basket.UpdateBasket(_remoteServiceBaseUrl);

            var response = await _apiClient.PostAsync(updateBasketUri, basket, userToken);

            response.EnsureSuccessStatusCode();

            return basket;
        }

        public async Task Checkout(BasketDTO basket, string userToken)
        {
            var updateBasketUri = Infrastructure.API.Basket.CheckoutBasket(_remoteServiceBaseUrl);

            var response = await _apiClient.PostAsync(updateBasketUri, basket, userToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task<Basket> SetQuantities(string userId, Dictionary<string, int> quantities, string userToken)
        {
            var basket = await GetBasket(userId, userToken);

            basket.Items.ForEach(x =>
            {
                // Simplify this logic by using the
                // new out variable initializer.
                if (quantities.TryGetValue(x.Id, out var quantity))
                {
                    x.Quantity = quantity;
                }
            });

            return basket;
        }

        public Order MapBasketToOrder(Basket basket)
        {
            var order = new Order();
            order.Total = 0;

            basket.Items.ForEach(x =>
            {
                order.OrderItems.Add(new OrderItem()
                {
                    ProductId = int.Parse(x.ProductId),

                    PictureUrl = x.PictureUrl,
                    ProductName = x.ProductName,
                    Units = x.Quantity,
                    UnitPrice = x.UnitPrice
                });
                order.Total += (x.Quantity * x.UnitPrice);
            });

            return order;
        }

        public async Task AddItemToBasket(string userId, BasketItem product,string userToken)
        {
            var basket = await GetBasket(userId,userToken);

            if (basket == null)
            {
                basket = new Basket()
                {
                    BuyerId = userId,
                    Items = new List<BasketItem>()
                };
            }

            basket.Items.Add(product);

            await UpdateBasket(basket, userToken);
        }        

    }
}
