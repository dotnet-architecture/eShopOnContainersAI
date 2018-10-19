using System.Collections.Generic;
using System.Threading.Tasks;
using BasketModels = Microsoft.eShopOnContainers.Bot.API.Models.Basket;
using OrderModels = Microsoft.eShopOnContainers.Bot.API.Models.Order;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Basket
{
    public interface IBasketService
    {
        Task<BasketModels.Basket> GetBasket(string userId, string userToken);
        Task AddItemToBasket(string userId, BasketModels.BasketItem product, string userToken);
        Task<BasketModels.Basket> UpdateBasket(BasketModels.Basket basket, string userToken);
        Task Checkout(BasketDTO basket, string userToken);
        Task<BasketModels.Basket> SetQuantities(string userId, Dictionary<string, int> quantities, string userToken);
        OrderModels.Order MapBasketToOrder(BasketModels.Basket basket);
    }
}