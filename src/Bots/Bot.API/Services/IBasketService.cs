using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Models.Basket;
using Microsoft.Bots.Bot.API.Models.Order;

namespace Microsoft.Bots.Bot.API.Services
{
    public interface IBasketService
    {
        Task<Basket> GetBasket(string userId, string userToken);
        Task AddItemToBasket(string userId, BasketItem product, string userToken);
        Task<Basket> UpdateBasket(Basket basket, string userToken);
        Task Checkout(BasketDTO basket, string userToken);
        Task<Basket> SetQuantities(string userId, Dictionary<string, int> quantities, string userToken);
        Order MapBasketToOrder(Basket basket);
    }
}