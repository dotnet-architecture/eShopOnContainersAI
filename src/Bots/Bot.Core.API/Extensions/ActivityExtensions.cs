using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            return self.ChannelId == "skype";
        }

        public static bool AttachmentContainsImageFile(this Activity self)
        {
            var attachment = self.Attachments?.FirstOrDefault(a =>
                    a.ContentType.ToLower() == "image/jpg" ||
                    a.ContentType.ToLower() == "image/jpeg" ||
                    a.ContentType.ToLower() == "image/pjpeg" ||
                    a.ContentType.ToLower() == "image/gif" ||
                    a.ContentType.ToLower() == "image/x-png" ||
                    a.ContentType.ToLower() == "image/png");

            return attachment != default(Attachment);
        }

        public static async Task<byte[]> GetFileAsync(this Activity self)
        {
            byte[] content = null;
            if (self.Attachments != null && self.Attachments.Count > 0)
            {
                var attachment = self.Attachments[0];
                byte[] imageFile = null;
                try
                {
                    var client = new ConnectorClient(new Uri(self.ServiceUrl) /*, new Microsoft.Bot.Connector.Authentication.MicrosoftAppCredentials()*/);
                    var stream = await client.HttpClient.GetStreamAsync(attachment.ContentUrl);
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        imageFile = ms.ToArray();
                    }

                }
                catch (Exception ex) { }
                return imageFile;
            }
            return content;
        }

        public static async Task UpdateCatalogFilterTagsAsync(this ITurnContext context, IProductSearchImageService productSearchImageService)
        {
            var userState = context.GetUserState<UserInfo>();
            var imageFile = await context.Activity.GetFileAsync();
            var tags = await productSearchImageService.ClassifyImageAsync(imageFile);
            if (userState.CatalogFilter == null)
                userState.CatalogFilter = new CatalogFilterData();
            userState.CatalogFilter.Tags = tags.ToArray();
        }
    }
}
