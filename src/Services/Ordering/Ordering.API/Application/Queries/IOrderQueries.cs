namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IOrderQueries
    {
        Task<Order> GetOrderAsync(int id);

        Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(Guid userId);

        Task<IEnumerable<CardType>> GetCardTypesAsync();

        Task<IEnumerable<dynamic>> GetOrderItemsAsync();
        
        Task<IEnumerable<dynamic>> GetProductHistoryAsync(string productId);

        Task<IEnumerable<dynamic>> GetProductStatsAsync(string productId);

        Task<IEnumerable<dynamic>> GetCountryHistoryAsync(string country);

        Task<IEnumerable<dynamic>> GetCountryStatsAsync(string country);

        Task<IEnumerable<dynamic>> GetProductsHistoryDepthAsync(IEnumerable<int> products);
    }
}
