namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IOrderQueries
    {
        Task<Order> GetOrderAsync(int id);

        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<IEnumerable<dynamic>> GetOrdersAsync(string userId);

        Task<IEnumerable<CardType>> GetCardTypesAsync();

        Task<IEnumerable<dynamic>> GetOrderItems();
    }
}
