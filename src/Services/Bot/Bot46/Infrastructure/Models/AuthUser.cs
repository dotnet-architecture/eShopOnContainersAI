using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot46.API.Infrastructure.Models
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