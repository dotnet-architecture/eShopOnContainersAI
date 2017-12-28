using System;
using System.Configuration;

namespace Bot46.API.Infrastructure
{
    public class BotSettings
    {
        public string BasketUrl {get;set;}
        public string CatalogUrl { get; set; }
        public string IdentityUrl { get; set; }

        public string ApiClient { get; set; }
        public string ApiClientSecret { get; set; }
        public string TokenEndPoint { get; set; }
        public string UserInfoEndpoint { get; set; }

        public string BotId { get; set; }
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string WebChatSecret { get; set; }

        public BotSettings() {
            BotId = ConfigurationManager.AppSettings["BotId"];
            MicrosoftAppId = ConfigurationManager.AppSettings["MicrosoftAppId"];
            MicrosoftAppPassword = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
            WebChatSecret = ConfigurationManager.AppSettings["WebChatSecret"];


            BasketUrl = ConfigurationManager.AppSettings["BasketUrl"];
            CatalogUrl = ConfigurationManager.AppSettings["CatalogUrl"];
            IdentityUrl = ConfigurationManager.AppSettings["IdentityUrl"];

            TokenEndPoint = ConfigurationManager.AppSettings["TokenEndPoint"];
            UserInfoEndpoint = ConfigurationManager.AppSettings["UserInfoEndpoint"];

            ApiClient = ConfigurationManager.AppSettings["ApiClient"];
            ApiClientSecret = ConfigurationManager.AppSettings["ApiClientSecret"];
        }
    }
}
