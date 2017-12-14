using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bot.API.Controllers
{



    namespace Bot.API.Controllers
    {

        public class BotController : Controller
        {
            private readonly ILogger<BotController> _logger;
            private readonly BotSettings _botSettings;

            public BotController(ILoggerFactory loggerFactory, IOptionsSnapshot<BotSettings> settings)
            {
                _logger = loggerFactory.CreateLogger<BotController>();
                _botSettings = settings.Value;
            }

            public async Task<ActionResult> Index(string id)
            {
                string cookievalue = string.Empty;
                if (!string.IsNullOrWhiteSpace(id) || !string.IsNullOrEmpty(id))
                {
                    cookievalue = id;
                }
                else {
                    cookievalue = GetBotUserId();
                }
                
                ViewData["ChatToken"]   = await GetChatToken(_botSettings.WebChatSecret);
                ViewData["Userid"]      = cookievalue;
                ViewData["BotID"]       = _botSettings.BotId;
                ViewData["UserName"]    = "Customer";

                return View();
            }



            private static async Task<string> GetChatToken(string webChatSecret)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://webchat.botframework.com/api/tokens");
                request.Headers.Add("Authorization", "BOTCONNECTOR " + webChatSecret);
                HttpResponseMessage response = await new HttpClient().SendAsync(request);
                string token = await response.Content.ReadAsStringAsync();
                token = token.Replace("\"", "");
                return token;
            }

            private string GetBotUserId()
            {
                string cookievalue;
                if (Request.Cookies["cookie"] != null)
                {
                    cookievalue = Request.Cookies["cookie"];

                }
                else
                {
                    // get or genertate unique user id
                    var userId = Guid.NewGuid();
                    cookievalue = userId.ToString();
                    CookieOptions options = new CookieOptions();
                    options.Expires = DateTime.MaxValue;
                    Response.Cookies.Append("cookie", cookievalue, options);
                }

                return cookievalue;
            }
        }
    }
}