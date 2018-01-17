using System.Linq;

namespace Microsoft.eShopOnContainers.Services.Catalog.API.ViewModel
{
    using System.Collections.Generic;


    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {
        public static PaginatedItemsViewModel<TEntity> Empty => new PaginatedItemsViewModel<TEntity>(0, 0, 0, Enumerable.Empty<TEntity>());

        public int PageIndex { get; private set; }

        public int PageSize { get; private set; }

        public long Count { get; private set; }

        public IEnumerable<TEntity> Data { get; private set; }

        public PaginatedItemsViewModel(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
        {
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.Count = count;
            this.Data = data;
        }
    }
}
