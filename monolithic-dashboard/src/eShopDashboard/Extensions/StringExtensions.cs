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

        public static bool IsBlank(this string stringObject)
        {
            return string.IsNullOrWhiteSpace(stringObject);
        }

        public static bool IsNotAnInt(this string stringObject)
        {
            return !int.TryParse(stringObject, out int result);
        }
    }
}