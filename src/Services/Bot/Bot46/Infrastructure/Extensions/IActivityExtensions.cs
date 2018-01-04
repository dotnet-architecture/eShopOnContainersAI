using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot46.API.Infrastructure.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Extensions
{
    public static class IActivityExtensions
    {
        public static async Task<bool> IsAuthenticatedAsync(this IActivity activity, string userId)
        {
            bool userAuthenticated = false;
            AuthUser authUser = await activity.GetAuthUser(userId);
            if (authUser != null)
            {
                if (!authUser.IsExpired)
                {
                    userAuthenticated = true;
                }
            }

            return userAuthenticated;
        }

        public static async Task<BotData> GetUserData(this IActivity activity, string userId)
        {
            var state = activity.GetStateClient();
            return await state.BotState.GetUserDataAsync(activity.ChannelId, userId);
        }

        public static Attachment LoginCard(this IActivity activity, string userId){
            List<CardAction> cardButtons = new List<CardAction>();

            AuthData authData = new AuthData() { 
                ChannelId = activity.ChannelId,
                UserId    = userId,
                ConversationId = activity.Conversation.Id,
                ServiceUrl = activity.ServiceUrl
                };
            
            string authDataEncoded = authData.Encode();

            // TODO SHOW IMAGE
            CardAction plButton = new CardAction()
            {
                // TODO change to real local URL
                // TODO change to not Harcoded String to BasePath
                Value = $"http://localhost:5200/Auth/Login?authData={authDataEncoded}",
                Type = "signin",
                Title = "Connect"
            };

            cardButtons.Add(plButton);
            SigninCard plCard = new SigninCard("Auth me", cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            return plAttachment;
        }

        public static async Task<AuthUser> GetAuthUser(this IActivity activity, string userId) {
            var botData = await activity.GetUserData(userId);
            AuthUser authUser = botData.GetProperty<AuthUser>("authUser");
            return authUser;
        }

        public static async Task<Attachment> UserCard(this IActivity activity, string userId)
        {
            AuthUser authUser = await activity.GetAuthUser(userId);

            HeroCard userCard = new HeroCard()
            {
                Title = $"User Info: {authUser.UserId}",
                Text = $"**expires (UTC):** {authUser.ExpiresAt}  **token:** {authUser.AccessToken}"
                // TODO SHOW IMAGE
                // Images
            };

            Attachment plAttachment = userCard.ToAttachment();
            return plAttachment;
        }
    }
}