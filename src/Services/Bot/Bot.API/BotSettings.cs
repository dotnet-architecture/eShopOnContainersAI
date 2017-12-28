using System;

namespace Bot.API
{
    public class BotSettings
    {
        public string BotId { get; set; }
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string WebChatSecret { get; set; }


        public string CatalogUrl { get; set; }
        public string BasketUrl { get; set; }
    }
}
