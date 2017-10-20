using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.eShopOnContainers.Services.Identity.API.Extensions
{
    public static class StringExtensions
    {
        public static string FormatAsCSV<T>(this IEnumerable<T> value) where T : class
        {
            StringBuilder stringBuilder = new StringBuilder();
            var properties = typeof(T).GetProperties();
            stringBuilder.AppendLine(String.Join(",", properties.Select(p => p.Name)));
            foreach (var csvLine in value)
            {
                var columnValues = properties
                    .Select(p => p.GetValue(csvLine).ToString())
                    .Select(p => p.FormatAsCSV());

                stringBuilder.AppendLine(String.Join(",", columnValues));
            }
            return stringBuilder.ToString();
        }

        public static bool IsValidCSV(this string value)
        {
            return value.IndexOfAny(new char[] { '"', ',' }) == -1;
        }

        public static string FormatAsCSV(this string value)
        {
            return value.IsValidCSV() ?
                value :
                String.Format("\"{0}\"", value.Replace("\"", "\"\""));
        }
    }
}
