using System;
using System.Collections.Generic;

namespace Catalog.API.Extensions
{
    public static class StringExtensions
    {
        public static string JoinTags(this IEnumerable<string> items)
        {
            return String.Join('-', items ?? new[] { String.Empty });
        }
    }
}
