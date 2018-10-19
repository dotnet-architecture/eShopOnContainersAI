using System;

namespace Microsoft.eShopOnContainers.Bot.API.Models.User
{
    public class AuthUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => ExpiresAt != null && ExpiresAt != DateTime.MinValue ? ExpiresAt < DateTime.UtcNow : false;
        public bool IsAuthenticated => !String.IsNullOrEmpty(AccessToken) && !IsExpired;

        public void Reset()
        {
            UserId = UserName = AccessToken = RefreshToken = string.Empty;
            ExpiresAt = DateTime.MinValue;
        }
    }
}