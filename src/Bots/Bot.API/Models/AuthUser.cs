using System;

namespace Microsoft.Bots.Bot.API.Models
{
    public class AuthUser
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired { get { return ExpiresAt < DateTime.UtcNow; } }

    }
}