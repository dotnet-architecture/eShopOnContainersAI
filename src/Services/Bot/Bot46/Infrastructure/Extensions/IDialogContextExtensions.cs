using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Bot46.API.Infrastructure.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Extensions
{
    public static class IDialogContextExtensions {
        public static async Task< bool> IsAuthenticated(this IDialogContext context){
            bool userAuthenticated = false;
            AuthUser authUser = await context.GetAuthUserAsync();
            if (authUser != null)
            {
                if (!authUser.IsExpired)
                {
                    userAuthenticated = true;
                }
            }

            return userAuthenticated;
        }

        public static async Task<BotData> GetUserDataAsync(this IDialogContext context)
        {
            BotData userState = null;
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, context.Activity.AsMessageActivity()))
            {
                var botDataStore = scope.Resolve<IBotDataStore<BotData>>();
                var key = Address.FromActivity(context.Activity);
                userState = await botDataStore.LoadAsync(key, BotStoreType.BotUserData, CancellationToken.None);

            }
            return userState;
        }

        public static async Task<AuthUser> GetAuthUserAsync(this IDialogContext context) {
            AuthUser authUser = null;
            var userState = await context.GetUserDataAsync();
            authUser = userState.GetProperty<AuthUser>("authUser");
            return authUser;
        }

        public static Attachment LoginCard(this IDialogContext context)
        {
            List<CardAction> cardButtons = new List<CardAction>();

            AuthData authData = new AuthData() { 
                BotId = context.Activity.Recipient.Id,
                ChannelId = context.Activity.ChannelId,
                UserId    = context.Activity.From.Id,
                ConversationId = context.Activity.Conversation.Id,
                ServiceUrl = context.Activity.ServiceUrl
                };
            
            string authDataEncoded = authData.Encode();
        

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

        public static Attachment UserCard(this IDialogContext context)
        {
            AuthUser authUser = context.UserData.GetValueOrDefault<AuthUser>("authUser");

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