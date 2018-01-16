using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bots.Bot.API.Models
{
    public class AuthData
    {
        public string BotId { get; set; }
        public string ChannelId { get; set; }
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public string ServiceUrl { get; set; }


        public string Encode()
        {
            string json = JsonConvert.SerializeObject(this);
            string base64EncodedAuthData = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            return base64EncodedAuthData;
        }

        public static AuthData Decode(string base64)
        {
            byte[] byteArray = Convert.FromBase64String(base64);
            string jsonBack = Encoding.UTF8.GetString(byteArray);
            var authDataDecoded = JsonConvert.DeserializeObject<AuthData>(jsonBack);
            return authDataDecoded;
        }

        private AuthData(string botId, string channelId, string userId, string conversationId, string serviceUrl)
        {
            BotId = botId;
            ChannelId = channelId;
            UserId = userId;
            ConversationId = conversationId;
            ServiceUrl = serviceUrl;
        }

        public AuthData()
        { }

        public static string Encode (string botId, string channelId, string userId, string conversationId, string serviceUrl)
        {
            string json = JsonConvert.SerializeObject(new AuthData(botId, channelId, userId, conversationId, serviceUrl));
            string base64EncodedAuthData = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            return base64EncodedAuthData;
        }
    }
}