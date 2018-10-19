using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Linq;

namespace Microsoft.eShopOnContainers.Bot.API.Extensions
{
    public static class ActivityExtensions
    {
        public static bool IsFirstTime(this Activity self)
        {
            return self.Type == ActivityTypes.ConversationUpdate && self.MembersAdded.FirstOrDefault()?.Id == self.Recipient.Id;
        }

        public static bool IsSkypeChannel(this IActivity self)
        {
            return self.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsMsTeamsChannel(this IActivity self)
        {
            return self.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool AttachmentContainsImageFile(this IMessageActivity self)
        {
            var attachment = self.Attachments?.FirstOrDefault(a =>
                    a.ContentType.ToLower() == "image" || // SKYPE
                    a.ContentType.ToLower() == "image/jpg" ||
                    a.ContentType.ToLower() == "image/jpeg" ||
                    a.ContentType.ToLower() == "image/pjpeg" ||
                    a.ContentType.ToLower() == "image/gif" ||
                    a.ContentType.ToLower() == "image/x-png" ||
                    a.ContentType.ToLower() == "image/png");

            return attachment != default(Attachment);
        }

        public static bool AttachmentContainsImageFile(this Activity self)
        {
            var attachment = self.Attachments?.FirstOrDefault(a =>
                    a.ContentType.ToLower() == "image" || // SKYPE
                    a.ContentType.ToLower() == "image/jpg" ||
                    a.ContentType.ToLower() == "image/jpeg" ||
                    a.ContentType.ToLower() == "image/pjpeg" ||
                    a.ContentType.ToLower() == "image/gif" ||
                    a.ContentType.ToLower() == "image/x-png" ||
                    a.ContentType.ToLower() == "image/png");

            return attachment != default(Attachment);
        }

        //public static bool? IsAuthenticated (this ITurnContext turnContext)
        //{
        //    return turnContext.TurnState.Values.OfType<System.Security.Claims.ClaimsIdentity>().FirstOrDefault()?.IsAuthenticated;
        //}
    }
}
