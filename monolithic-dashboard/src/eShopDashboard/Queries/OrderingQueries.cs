using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Queries
{
    public class OrderingQueries : IOrderingQueries
    {
        private readonly string _connectionString;

        public OrderingQueries(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<dynamic>> GetCountryHistoryAsync(string country)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(OrderingQueriesText.CountryHistory(country), new { country });
            }
        }

        public async Task<IEnumerable<dynamic>> GetProductHistoryAsync(string productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(OrderingQueriesText.ProductHistory(productId), new { productId });
            }
        }

        public async Task<IEnumerable<dynamic>> GetProductStatsAsync(string productId)
        {
            var productHistory = await GetProductHistoryAsync(productId);

            return productHistory.Where(p => p.next != null && p.prev != null);
        }
    }
}