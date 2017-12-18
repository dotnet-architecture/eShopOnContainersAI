using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Bot.API.Extensions
{
    public static class IDialogContextExtensions {
        public static bool IsAuthenticated(this IDialogContext context){
            bool userAuthenticated = false;
            DateTime expires = DateTime.MinValue;
            if(context.UserData.TryGetValue("expires_at", out expires))
            {
                userAuthenticated = true;
            }
            return userAuthenticated;
        }

         public static Attachment LoginCard(this IDialogContext context){
            List<CardAction> cardButtons = new List<CardAction>();

            AuthData authData = new AuthData() { 
                ChannelId = context.Activity.ChannelId,
                UserId    = context.Activity.From.Id,
                ConversationId = context.Activity.Conversation.Id,
                ServiceUrl = context.Activity.ServiceUrl
                };
            
            string authDataEncoded = authData.Encode();
        

            CardAction plButton = new CardAction()
            {           
                // TODO change to not Harcoded String to BasePath
               // Value = $"http://localhost:5200/Bot/Auth/?channelId={context.Activity.ChannelId}&userBotId={context.Activity.From.Id}&conversationId={context.Activity.Conversation.Id}&serviceUrl={context.Activity.ServiceUrl}",
                Value = $"http://localhost:5200/Bot/Auth/?authData={authDataEncoded}",
                Type = "signin",
                Title = "Connect"
            };

            cardButtons.Add(plButton);
            SigninCard plCard = new SigninCard("Auth me", cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            return plAttachment;
        }
    }
}