using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services
{
    public interface IAttachmentService
    {
        Task<byte[]> DownloadAttachmentFromActivityAsync(Activity self);
    }

    public class AttachmentService : IAttachmentService
    {
        private readonly ILogger<AttachmentService> logger;
        private readonly AppSettings appSettings;
        private readonly HttpClient httpClient;

        public AttachmentService(IOptions<AppSettings> appSettings, ILogger<AttachmentService> logger, HttpClient httpClient)
        {
            this.appSettings = appSettings.Value;
            this.logger = logger;
            this.httpClient = httpClient;
        }

        public async Task<byte[]> DownloadAttachmentFromActivityAsync(Activity self)
        {
            byte[] content = null;
            if (self.Attachments?.Count > 0)
            {
                var attachment = self.Attachments[0];
                try
                {
                    if (self.IsMsTeamsChannel() || self.IsSkypeChannel())
                    {
                        if (String.IsNullOrEmpty(appSettings.MicrosoftAppPassword))
                        {
                            logger.LogDebug("MicrosoftAppPassword is not provided");
                            return content;
                        }
                        var token = await new Microsoft.Bot.Connector.Authentication.MicrosoftAppCredentials(appSettings.MicrosoftAppId, appSettings.MicrosoftAppPassword).GetTokenAsync();
                        logger.LogDebug($"{nameof(attachment.ContentUrl)}: {attachment.ContentUrl}");
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                    content = await httpClient.GetByteArrayAsync(attachment.ContentUrl);
                }
                catch (Exception ex) {
                    logger.LogError(ex, "Exception downloading attachment");
                }
            }
            return content;
        }
    }
}
