using Microsoft.eShopOnContainers.Services.Catalog.API;
using Microsoft.eShopOnContainers.Services.Catalog.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalog.API.Extensions
{
    public static class LinqSelectExtensions
    {
        public static IEnumerable<SelectTryResult<TSource, TResult>> SelectTry<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
        {
            foreach (TSource element in enumerable)
            {
                SelectTryResult<TSource, TResult> returnedValue;
                try
                {
                    returnedValue = new SelectTryResult<TSource, TResult>(element, selector(element), null);
                }
                catch (Exception ex)
                {
                    returnedValue = new SelectTryResult<TSource, TResult>(element, default(TResult), ex);
                }
                yield return returnedValue;
            }
        }

        public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<Exception, TResult> exceptionHandler)
        {
            return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.CaughtException));
        }

        public static IEnumerable<TResult> OnCaughtException<TSource, TResult>(this IEnumerable<SelectTryResult<TSource, TResult>> enumerable, Func<TSource, Exception, TResult> exceptionHandler)
        {
            return enumerable.Select(x => x.CaughtException == null ? x.Result : exceptionHandler(x.Source, x.CaughtException));
        }

        public class SelectTryResult<TSource, TResult>
        {
            internal SelectTryResult(TSource source, TResult result, Exception exception)
            {
                Source = source;
                Result = result;
                CaughtException = exception;
            }

            public TSource Source { get; private set; }
            public TResult Result { get; private set; }
            public Exception CaughtException { get; private set; }
        }

        public static IEnumerable<T> FollowsOrder<T, U>(this IEnumerable<T> items, Func<T, U> selector, IEnumerable<U> order)
        {
            var i = 0;
            var ranks = order.Select(c => new { id = c, order = i++ });

            return items.Select(c => new { id = selector(c), item = c })
                .Join(ranks, a => a.id, b => b.id, (a, b) => new { a.item, b.order })
                .OrderBy(a => a.order)
                .Select(a => a.item);
        }

        public static List<CatalogItem> ChangeUriPlaceholder(this List<CatalogItem> items, CatalogSettings settings)
        {
            var baseUri = settings.PicBaseUrl;

            items.ForEach(catalogItem =>
            {
                catalogItem.PictureUri = settings.AzureStorageEnabled
                    ? baseUri + catalogItem.PictureFileName
                    : baseUri.Replace("[0]", catalogItem.Id.ToString());
            });

            return items;
        }

        public static IEnumerable<string> Permutation(this string self)
        {
            const string whiteSpace = " ";
            if (!self.Contains(whiteSpace))
            {
                yield return self;
            }
            else
            {
                var parts = self.Split(whiteSpace);
                string previous = null;
                foreach (var part in parts)
                {
                    yield return part;
                    if (!string.IsNullOrEmpty(previous))
                        yield return $"{previous}-{part}";
                    previous = part;
                }
            }
        }
    }
}
