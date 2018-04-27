using System;
using System.Collections.Generic;

namespace eShopDashboard.Extensions
{
    public static class StringExtensions
    {
        public static string JoinTags(this IEnumerable<string> items)
        {
            return String.Join('-', items ?? new[] {String.Empty});
        }
    }
}