using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialIntelligence.API.Infrastructure
{
    public static class API
    {
        public static class Catalog
        {
            public static string GetAll(string baseUri)
            {
                return $"{baseUri}all";
            }
        }

        public static class Identity
        {
            public static string GetAll(string baseUri)
            {
                return $"{baseUri}all";
            }
        }

        public static class OrderItems
        {
            public static string GetAll(string baseUri)
            {
                return $"{baseUri}all";
            }
        }
    }
}
