namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries
{
    using Dapper;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    public class OrderQueries
        : IOrderQueries
    {
        private string _connectionString = string.Empty;

        public OrderQueries(string constr)
        {
            _connectionString = !string.IsNullOrWhiteSpace(constr) ? constr : throw new ArgumentNullException(nameof(constr));
        }


        public async Task<Order> GetOrderAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var result = await connection.QueryAsync<dynamic>(
                   @"select o.[Id] as ordernumber,o.OrderDate as date, o.Description as description,
                        o.Address_City as city, o.Address_Country as country, o.Address_State as state, o.Address_Street as street, o.Address_ZipCode as zipcode,
                        os.Name as status, 
                        oi.ProductName as productname, oi.Units as units, oi.UnitPrice as unitprice, oi.PictureUrl as pictureurl
                        FROM ordering.Orders o
                        LEFT JOIN ordering.Orderitems oi ON o.Id = oi.orderid 
                        LEFT JOIN ordering.orderstatus os on o.OrderStatusId = os.Id
                        WHERE o.Id=@id"
                        , new { id }
                    );

                if (result.AsList().Count == 0)
                    throw new KeyNotFoundException();

                return MapOrderItems(result);
            }
        }

        public async Task<IEnumerable<dynamic>> GetOrdersAsync(string userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(
                    @"SELECT o.[Id] as ordernumber,o.[OrderDate] as [date],os.[Name] as [status], COALESCE(SUM(oi.units*oi.unitprice),0) as total
                     FROM [ordering].[Orders] o
                     INNER JOIN [ordering].[buyers] ob ON o.BuyerId = ob.Id AND ob.IdentityGuid = @userId
                     LEFT JOIN[ordering].[orderitems] oi ON  o.Id = oi.orderid 
                     LEFT JOIN[ordering].[orderstatus] os on o.OrderStatusId = os.Id                     
                     GROUP BY o.[Id], o.[OrderDate], os.[Name] 
                     ORDER BY o.[Id]"
                    , new { userId });
            }
        }

        public async Task<IEnumerable<dynamic>> GetOrderItemsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(
                    @"  select oi.ProductId, 
	                        ob.IdentityGuid as CustomerId, 
	                        oi.Units
                        from ordering.orderItems oi
                        inner join ordering.orders oo on oi.OrderId = oo.Id
                        inner join ordering.buyers ob on oo.BuyerId = ob.Id
                        ");
            }
        }


        public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(Guid userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<OrderSummary>(@"SELECT o.[Id] as ordernumber,o.[OrderDate] as [date],os.[Name] as [status], SUM(oi.units*oi.unitprice) as total
                     FROM [ordering].[Orders] o
                     INNER JOIN [ordering].[buyers] ob ON o.BuyerId = ob.Id AND ob.IdentityGuid = @userId
                     LEFT JOIN[ordering].[orderitems] oi ON  o.Id = oi.orderid 
                     LEFT JOIN[ordering].[orderstatus] os on o.OrderStatusId = os.Id                     
                     LEFT JOIN[ordering].[buyers] ob on o.BuyerId = ob.Id
                     WHERE ob.IdentityGuid = @userId
                     GROUP BY o.[Id], o.[OrderDate], os.[Name] 
                     ORDER BY o.[Id]", new { userId });
            }
        }

        public async Task<IEnumerable<CardType>> GetCardTypesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<CardType>("SELECT * FROM ordering.cardtypes");
            }
        }

        public async Task<IEnumerable<dynamic>> GetProductHistoryAsync(string productId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sqlCommandText = $@"
select p.productId, p.[year], p.[month], p.units, p.[avg], p.[count], p.[max], p.[min],
    LAG (units, 1) OVER (PARTITION BY p.productId ORDER BY p.productId, p.date) as prev,
    LEAD (units, 1) OVER (PARTITION BY p.productId ORDER BY p.productId, p.date) as [next]
from (
    select oi.ProductId as productId, 
        YEAR(CAST(oi.OrderDate as datetime)) as [year], 
        MONTH(CAST(oi.OrderDate as datetime)) as [month], 
        MIN(CAST(oi.OrderDate as datetime)) as date,
        sum(oi.Units) as units,
        avg(oi.Units) as [avg],
        count(oi.Units) as [count],
        max(oi.Units) as [max],
        min(oi.Units) as [min]
    from (
        select CONVERT(date, oo.OrderDate) as OrderDate, oi.ProductId, sum(oi.Units) as units
        from [ordering].[orderItems] oi
        inner join [ordering].[orders] oo on oi.OrderId = oo.Id
        {(string.IsNullOrEmpty(productId) ? string.Empty : "where oi.ProductId = @productId")} 
        group by CONVERT(date, oo.OrderDate), oi.ProductId) as oi 
        group by oi.ProductId, YEAR(CAST(oi.OrderDate as datetime)), MONTH(CAST(oi.OrderDate as datetime))
    ) as p";

                connection.Open();

                return await connection.QueryAsync<dynamic>(sqlCommandText, new { productId });
            }
        }

        public async Task<IEnumerable<dynamic>> GetProductStatsAsync(string productId)
        {
            var productHistory = await GetProductHistoryAsync(productId);
            return productHistory.Where(p => p.next != null && p.prev != null);
        }

        public async Task<IEnumerable<dynamic>> GetCountryHistoryAsync(string country)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sqlCommandText = $@"
select 
    LEAD (log10(sum(R.sale)), 1) OVER (PARTITION BY R.country ORDER BY R.[year], R.[month]) as [next],
	R.country, R.year, R.month, sum(R.sale) as sales, count(R.sale) as count, 
    max(R.p_max) as [max], min(R.p_med) as [med], min(R.p_min) as [min], stdevp(R.sale) as std,
    LAG (sum(R.sale), 1) OVER (PARTITION BY R.country ORDER BY R.[year], R.[month]) as prev
from (
    select S.country, S.[month], S.[year], S.sale,
        PERCENTILE_CONT(0.20) WITHIN GROUP (ORDER BY S.sale) OVER (PARTITION BY S.country, S.[year], S.[month]) as p_min,
        PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY S.sale) OVER (PARTITION BY S.country, S.[year], S.[month]) as p_med,
        PERCENTILE_CONT(0.80) WITHIN GROUP (ORDER BY S.sale) OVER (PARTITION BY S.country, S.[year], S.[month]) as p_max
        from 
        (select min(T.country) as country, min(T.year) as [year], min(T.month) as [month], sum(T.sale) as sale
            from (
            select oo.Address_Country as country, oo.Id as id, YEAR(oo.OrderDate) as [year], MONTH(oo.OrderDate) as [month], oi.UnitPrice * oi.Units as sale
            from [ordering].[orderItems] oi
            inner join [ordering].[orders] oo on oi.OrderId = oo.Id {(string.IsNullOrEmpty(country) ? string.Empty : "and oo.Address_Country = (@country)")}
        ) as T
            group by T.id
        ) as S
    ) as R
group by R.country, R.year, R.month
order by R.country, R.year, R.month";

                connection.Open();

                return await connection.QueryAsync<dynamic>(sqlCommandText, new { country });
            }
        }

        public async Task<IEnumerable<dynamic>> GetCountryStatsAsync(string country)
        {
            var countryHistory = await GetCountryHistoryAsync(country);
            return countryHistory.Where(p => p.next != null && p.prev != null);
        }

        public async Task<IEnumerable<dynamic>> GetProductsHistoryDepthAsync(IEnumerable<int> products)
        {
            var sqlCommandText = $@"
select productId, count(*) as [count]
from (
    select distinct oi.ProductId as productId, YEAR(oo.OrderDate) as [year], MONTH(oo.OrderDate) as [month]
    from [ordering].[orderItems] oi
    inner join [ordering].[orders] oo on oi.OrderId = oo.Id and oi.ProductId in ({String.Join(',',products.Select(p => p.ToString()))})
) as R
group by R.productId
";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return await connection.QueryAsync<dynamic>(sqlCommandText);
            }
        }

        private Order MapOrderItems(dynamic result)
        {
            var order = new Order
            {
                ordernumber = result[0].ordernumber,
                date = result[0].date,
                status = result[0].status,
                description = result[0].description,
                street = result[0].street,
                city = result[0].city,
                zipcode = result[0].zipcode,
                country = result[0].country,
                orderitems = new List<Orderitem>(),
                total = 0
            };

            foreach (dynamic item in result)
            {
                var orderitem = new Orderitem
                {
                    productname = item.productname,
                    units = item.units,
                    unitprice = (double)item.unitprice,
                    pictureurl = item.pictureurl
                };

                order.total += item.units * item.unitprice;
                order.orderitems.Add(orderitem);
            }

            return order;
        }
    }
}
