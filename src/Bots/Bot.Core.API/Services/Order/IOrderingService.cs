using System.Collections.Generic;
using System.Threading.Tasks;
using OrderModels = Microsoft.eShopOnContainers.Bot.API.Models.Order;
using BasketModels = Microsoft.eShopOnContainers.Bot.API.Services.Basket;
using Microsoft.eShopOnContainers.Bot.API.Models.User;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Order
{
    public interface IOrderingService
    {
        Task<List<OrderModels.Order>> GetMyOrders(string userId, string userToken);
        Task<OrderModels.Order> GetOrder(string orderId, string userToken);
        Task CancelOrder(string orderId, string userToken);
        Task ShipOrder(string orderId, string userToken);
        OrderModels.Order MapUserInfoIntoOrder(UserData user, OrderModels.Order order);
        BasketModels.BasketDTO MapOrderToBasket(OrderModels.Order order);
        void OverrideUserInfoIntoOrder(OrderModels.Order original, OrderModels.Order destination);
    }
}