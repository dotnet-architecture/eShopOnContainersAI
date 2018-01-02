namespace Catalog.API.Infrastructure
{
    public static class API
    {
        public static class Recommendation
        {
            public static string Recommend(string baseUri, string productId, string customerId)
            {
                return $"{baseUri}for/product/{productId}/customer/{customerId}";
            }
        }
    }
}
