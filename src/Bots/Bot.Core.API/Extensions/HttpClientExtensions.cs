using Microsoft.AspNetCore.Http;

namespace Microsoft.eShopOnContainers.Bot.API.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string AbsoluteHost(this HttpRequest httpRequest)
        {
            return $"{httpRequest.Scheme}://{httpRequest.Host.ToUriComponent()}";
        }
    }

}
