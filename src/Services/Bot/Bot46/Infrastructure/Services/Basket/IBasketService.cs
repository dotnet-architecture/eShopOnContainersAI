using Bot46.API.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot46.API.Infrastructure.Services
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