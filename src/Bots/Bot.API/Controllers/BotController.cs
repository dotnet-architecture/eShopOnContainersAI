using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Helpers;

namespace Microsoft.Bots.Bot.API.Controllers
{
    public class BotController : Controller
    {
        private readonly BotSettings _settings;

        public BotController(BotSettings settings)
        {
            _settings = settings;
        }

        // GET: Bot
        public async Task<ActionResult> Index()
        {
            string cookievalue = GetBotUserId();
            string token = await BotHelper.GetChatToken(_settings.WebChatSecret);

            ViewData["chatToken"] = token;
            ViewData["userId"] = cookievalue;
            ViewData["botId"] = _settings.BotId;
            ViewData["userName"] = "Customer";

            return View();
        }

        private string GetBotUserId()
        {
            string cookievalue;
            if (Request.Cookies["cookie"] != null)
            {
                cookievalue = Request.Cookies["cookie"].Value;

            }
            else
            {
                var userId = Guid.NewGuid();
                cookievalue = userId.ToString();
                Response.Cookies["cookie"].Value = cookievalue;
                Response.Cookies["cookie"].Expires = DateTime.MaxValue;
            }

            return cookievalue;
        }

    }
}