using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Models;

namespace Microsoft.Bots.Bot.API.Infrastructure.Extensions
{
    public static class BotDataExtensions
    {
        private const string userDataKey = "userData";
        private const string userAuthKey = "authUser";

        public static void SetUserData(this BotData self, UserData value)
        {
            self.SetProperty(userDataKey, value);
        }

        public static UserData GetUserData(this BotData self)
        {
            return self.GetProperty<UserData>(userDataKey);
        }

        public static void SetUserAuthData(this BotData self, AuthUser value)
        {
            self.SetProperty(userAuthKey, value);
        }

        public static AuthUser GetUserAuthData(this BotData self)
        {
            return self.GetProperty<AuthUser>(userAuthKey);
        }
    }
}