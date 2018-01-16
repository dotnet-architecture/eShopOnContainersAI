using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bots.Bot.API.Models;
using Microsoft.Bots.Bot.API.Models.Basket;
using Microsoft.Bots.Bot.API.Models.Order;

namespace Microsoft.Bots.Bot.API.Services
{
    public interface IOrderingService
    {
        Task<List<Order>> GetMyOrders(string userId, string userToken);
        Task<Order> GetOrder(string orderId, string userToken);
        Task CancelOrder(string orderId, string userToken);
        Task ShipOrder(string orderId, string userToken);
        Order MapUserInfoIntoOrder(UserData user, Order order);
        BasketDTO MapOrderToBasket(Order order);
        void OverrideUserInfoIntoOrder(Order original, Order destination);
    }
}