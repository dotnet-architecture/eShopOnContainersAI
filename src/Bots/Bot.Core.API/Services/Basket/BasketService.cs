using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.Bot.API.Models.Basket;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using BasketModels = Microsoft.eShopOnContainers.Bot.API.Models.Basket;
using OrderModels = Microsoft.eShopOnContainers.Bot.API.Models.Order;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Basket
{
    public class BasketService : IBasketService
    {
        private readonly string _basketByPassUrl;
        private readonly string _purchaseUrl;
        private readonly AppSettings settings;
        private readonly IHttpClientFactory httpClientFactory;

        public BasketService(IOptions<AppSettings> settings, IHttpClientFactory httpClientFactory)
        {
            this.settings = settings.Value;
            this.httpClientFactory = httpClientFactory;

            _basketByPassUrl = $"{this.settings.PurchaseUrl}/api/v1/b/basket";
            _purchaseUrl = $"{this.settings.PurchaseUrl}/api/v1";
        }

        private HttpClient CreateBasketClient(string userToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.SetBearerToken(userToken);
            return httpClient;
        }

        private (HttpClient, HttpContent) CreateBasketClient<T>(T item, string userToken, bool setRequestId = true)
        {
            var client = CreateBasketClient(userToken);
            if (setRequestId)
                client.DefaultRequestHeaders.Add("x-requestid", Guid.NewGuid().ToString());

            var content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8, "application/json");

            return (client, content);
        }

        public async Task<BasketModels.Basket> GetBasket(string userId, string userToken)
        {
            var getBasketUri = Infrastructure.API.Basket.GetBasket(_basketByPassUrl, userId);
            var client = CreateBasketClient(userToken);

            var dataString = await client.GetStringAsync(getBasketUri);

            var response = JsonConvert.DeserializeObject<BasketModels.Basket>(dataString) ??
                new BasketModels.Basket() { BuyerId = userId };

            return response;
        }

        public async Task<BasketModels.Basket> UpdateBasket(BasketModels.Basket basket, string userToken)
        {
            var updateBasketUri = Infrastructure.API.Basket.UpdateBasket(_basketByPassUrl);
            var (client, content) = CreateBasketClient(basket, userToken);

            var response = await client.PostAsync(updateBasketUri, content);

            response.EnsureSuccessStatusCode();

            return basket;
        }

        public async Task Checkout(BasketDTO basket, string userToken)
        {
            var updateBasketUri = Infrastructure.API.Basket.CheckoutBasket(_basketByPassUrl);
            var (client, content) = CreateBasketClient(basket, userToken);

            var response = await client.PostAsync(updateBasketUri, content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<BasketModels.Basket> SetQuantities(string userId, Dictionary<string, int> quantities, string userToken)
        {
            var updateBasketUri = Infrastructure.API.Basket.UpdateBasket(_purchaseUrl);
            var updatedBasket = new
            {
                BasketId = userId,
                Updates = quantities.Select(kvp => new
                {
                    BasketItemId = kvp.Key,
                    NewQty = kvp.Value
                }).ToArray()
            };
            var (client, content) = CreateBasketClient(updatedBasket, userToken);

            var response = await client.PutAsync(updateBasketUri, content);

            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BasketModels.Basket>(jsonResponse);
        }

        public OrderModels.Order MapBasketToOrder(BasketModels.Basket basket)
        {
            var order = new OrderModels.Order();
            order.Total = 0;

            basket.Items.ForEach(x =>
            {
                order.OrderItems.Add(new Models.Order.OrderItem()
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
                basket = new BasketModels.Basket()
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