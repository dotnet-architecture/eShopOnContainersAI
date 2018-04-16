using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Models.Basket;
using Microsoft.Bots.Bot.API.Models.Order;
using Newtonsoft.Json;
using Basket = Microsoft.Bots.Bot.API.Models.Basket.Basket;
using Order = Microsoft.Bots.Bot.API.Models.Order.Order;

namespace Microsoft.Bots.Bot.API.Services
{
    public class BasketService : IBasketService
    {
        private readonly BotSettings _settings;
        private readonly string _basketByPassUrl;
        private readonly string _purchaseUrl;
        private IHttpClient _apiClient;

        public BasketService(BotSettings settings, IHttpClient httpClient)
        {
            _settings = settings;
            _basketByPassUrl = $"{_settings.PurchasingUrl}/api/v1/b/basket";
            _purchaseUrl = $"{_settings.PurchasingUrl}/api/v1";
            _apiClient = httpClient;
        }

        public async Task<Basket> GetBasket(string userId, string userToken)
        {
            var getBasketUri = Infrastructure.Basket.GetBasket(_basketByPassUrl, userId);

            var dataString = await _apiClient.GetStringAsync(getBasketUri, userToken);

            var response = JsonConvert.DeserializeObject<Basket>(dataString) ??
                new Basket() { BuyerId = userId };

            return response;
        }

        public async Task<Basket> UpdateBasket(Basket basket, string userToken)
        {
            var updateBasketUri = Infrastructure.Basket.UpdateBasket(_basketByPassUrl);

            var response = await _apiClient.PostAsync(updateBasketUri, basket, userToken);

            response.EnsureSuccessStatusCode();

            return basket;
        }

        public async Task Checkout(BasketDTO basket, string userToken)
        {
            var updateBasketUri = Infrastructure.Basket.CheckoutBasket(_basketByPassUrl);

            var response = await _apiClient.PostAsync(updateBasketUri, basket, userToken);

            response.EnsureSuccessStatusCode();
        }

        public async Task<Basket> SetQuantities(string userId, Dictionary<string, int> quantities, string userToken)
        {
            var updateBasketUri = Infrastructure.Basket.UpdateBasket(_purchaseUrl);

            var response = await _apiClient.PutAsync(updateBasketUri, new
            {
                BasketId = userId,
                Updates = quantities.Select(kvp => new
                {
                    BasketItemId = kvp.Key,
                    NewQty = kvp.Value
                }).ToArray()
            });

            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Basket>(jsonResponse);
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

        public async Task AddItemToBasket(string userId, BasketItem product, string userToken)
        {
            var basket = await GetBasket(userId, userToken);

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