using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Infrastructure.Extensions
{
    public static class IActivityExtensions
    {
        public static async Task<Attachment> CreateLoginCardAsync(this IActivity self, IOIDCClient oidcClient)
        {
            string authorizeUrl = await oidcClient.CreateAuthorizeUrlAsync(self);

            // TODO SHOW IMAGE
            var cardButtons = new List<CardAction> {
                new CardAction
                {
                    Value = authorizeUrl,
                    Type = ActionTypes.Signin,
                    Title = "Login"
                }
            };

            return new SigninCard("LOGIN", cardButtons).ToAttachment();
        }

        public static async Task<Attachment> CreateUserCardAsync(this IActivity activity, IIdentityService identityService)
        {
            var authUser = await identityService.GetAuthUserAsync(activity);

            var userCard = new HeroCard()
            {
                Title = $"User Info: {authUser.UserId}",
                Text = $"**expires (UTC):** {authUser.ExpiresAt}  **token:** {authUser.AccessToken}"
                // TODO SHOW IMAGE
                // Images
            };

            var plAttachment = userCard.ToAttachment();
            return plAttachment;
        }

        public static bool IsValidTextMessage(this IMessageActivity self)
        {
            return self != null && self.Type == ActivityTypes.Message && !string.IsNullOrEmpty(self.Text);
        }
    }
}