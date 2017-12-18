
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bot.Helpers
{

    public static class Bot 
    {

        internal static async Task<string> GetChatToken(string webChatSecret)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://webchat.botframework.com/api/tokens");
            request.Headers.Add("Authorization", "BOTCONNECTOR " + webChatSecret);
            HttpResponseMessage response = await new HttpClient().SendAsync(request);
            if(response.StatusCode.Equals(HttpStatusCode.OK))
            {
                string token = await response.Content.ReadAsStringAsync();
                token = token.Replace("\"", "");
                return token;
            }
            else{
                return "";
            }
        }
    }
}