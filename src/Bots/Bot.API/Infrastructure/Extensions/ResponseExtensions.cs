using System;
using IdentityModel.Client;

namespace Microsoft.Bots.Bot.API.Infrastructure.Extensions
{
    public static class ResponseExtensions
    {
        public static void ShouldBeOK(this Response response, string errorMessage)
        {
            if (response == null)
                throw new Exception(errorMessage);
            if (response.IsError)
                throw new Exception(response.Error);
        }
    }
}