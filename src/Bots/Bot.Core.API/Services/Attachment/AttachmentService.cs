using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.eShopOnContainers.Bot.API.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.Attachment
{
    public class AttachmentService : IAttachmentService
    {
        private readonly string appPassword;
        private readonly string appId;
        private readonly ILogger<AttachmentService> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public AttachmentService(BotConfiguration botConfiguration, ILogger<AttachmentService> logger, IHttpClientFactory httpClientFactory)
        {
            var botService = botConfiguration.Services.Find(cs => cs.Type == ServiceTypes.Endpoint) as EndpointService;
            if (botService != null)
            {
                this.appPassword = botService.AppPassword;
                this.appId = botService.AppId;
            }
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<byte[]> DownloadAttachmentFromActivityAsync(Activity self)
        {
            byte[] content = null;
            if (self.Attachments?.Count > 0)
            {
                var httpClient = httpClientFactory.CreateClient();
                var attachment = self.Attachments[0];
                try
                {
                    if (self.IsMsTeamsChannel() || self.IsSkypeChannel())
                    {
                        if (String.IsNullOrEmpty(appPassword))
                        {
                            logger.LogDebug("MicrosoftAppPassword is not provided");
                            return content;
                        }
                        var token = await new Microsoft.Bot.Connector.Authentication.MicrosoftAppCredentials(appId, appPassword).GetTokenAsync();
                        logger.LogDebug($"{nameof(attachment.ContentUrl)}: {attachment.ContentUrl}");
                        //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        httpClient.SetBearerToken(token);
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
