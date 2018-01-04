using Bot46.API.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot46.API.Infrastructure.Services
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