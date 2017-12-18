using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
            _logger.LogInformation($"User Connected to Bot");
            string userIdValue = string.Empty;
            if (!string.IsNullOrWhiteSpace(id) || !string.IsNullOrEmpty(id))
            {
                userIdValue = id;
            }
            else {
                userIdValue = GetBotUserId();
            }

            var chatToken = await  Bot.Helpers.Bot.GetChatToken(_botSettings.WebChatSecret);
                
            if(!chatToken.Equals(string.Empty)){              
                ViewData["chatToken"]   = chatToken;
                ViewData["userid"]      = userIdValue;
                ViewData["botID"]       = _botSettings.BotId;
                ViewData["userName"]    = "Customer";
                return View();
            }
            else{
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        public async Task<ActionResult> Auth(string authData) {
            if (string.IsNullOrEmpty(authData) )
            {
                return BadRequest();
            }

            var authDataDecoded = AuthData.Decode(authData);
           
            // basic scenarios perform well using in-memory session
            // advanced scenarios, please check session store
            // https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed
            HttpContext.Session.SetString("userBotId",authDataDecoded.UserId);
            HttpContext.Session.SetString("channelId",authDataDecoded.ChannelId);
            HttpContext.Session.SetString("conversationId",authDataDecoded.ConversationId);
            HttpContext.Session.SetString("serviceUrl",authDataDecoded.ServiceUrl);
            
            // "Bot" because UrlHelper doesn't support nameof() for controllers
            // https://github.com/aspnet/Mvc/issues/5853
            return RedirectToAction(nameof(BotController.AuthCallBack), "Bot");
        }

        [Authorize]
        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            
            // "Bot" because UrlHelper doesn't support nameof() for controllers
            // https://github.com/aspnet/Mvc/issues/5853
            var homeUrl = Url.Action(nameof(BotController.SignoutCallBack), "Bot");
            return new SignOutResult(OpenIdConnectDefaults.AuthenticationScheme, 
                new AuthenticationProperties { RedirectUri = homeUrl });
        }

        [Authorize]
        public async Task<ActionResult> AuthCallBack(CancellationToken ctoken = default(CancellationToken))
        {
            var userAppId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var idToken = await HttpContext.GetTokenAsync("id_token");
            var token = await HttpContext.GetTokenAsync("access_token");
            var expires = await HttpContext.GetTokenAsync("expires_at");

            // Get the resumption cookie
            var address = new Address
                (
                    // purposefully using named arguments because these all have the same type
                    botId: _botSettings.BotId,
                    channelId:  HttpContext.Session.GetString("channelId"),
                    userId: HttpContext.Session.GetString("userBotId"),
                    conversationId: HttpContext.Session.GetString("conversationId"),
                    serviceUrl: HttpContext.Session.GetString("serviceUrl")
                );

            // Clear Session Data
            HttpContext.Session.Remove("userBotId");
            HttpContext.Session.Remove("channelId");
            HttpContext.Session.Remove("conversationId");
            HttpContext.Session.Remove("serviceUrl");

            var conversationReference = address.ToConversationReference();
            var activity = conversationReference.GetPostToBotMessage();

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                var dataBag = scope.Resolve<IBotData>();
                await dataBag.LoadAsync(ctoken);
                
            //    dataBag.UserData.SetValue("userLogged",true);
                dataBag.UserData.SetValue("userAppId",userAppId);
                dataBag.UserData.SetValue("access_token",token);
                dataBag.UserData.SetValue("expires_at",expires);
                await dataBag.FlushAsync(ctoken);
            }

            return View();
        }

         public async Task<ActionResult> SignoutCallBack(){
            return View(); 
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