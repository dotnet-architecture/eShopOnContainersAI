using Autofac;
using Bot46.API.Infrastructure;
using Bot46.API.Infrastructure.Models;
using IdentityModel.Client;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Bot46.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly BotSettings _botSettings;

        public AuthController(BotSettings botSettings) {
            _botSettings = botSettings;
        }
        // GET: Auth
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login(string authData)
        {
            if (string.IsNullOrEmpty(authData))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var authDataDecoded = AuthData.Decode(authData);

            // basic scenarios perform well using in-memory session
            // advanced scenarios, please check session store
            Session.Add("userBotId", authDataDecoded.UserId);
            Session.Add("channelId", authDataDecoded.ChannelId);
            Session.Add("conversationId", authDataDecoded.ConversationId);
            Session.Add("serviceUrl", authDataDecoded.ServiceUrl);

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string userName, string UserPassword)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(UserPassword))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            // token endpoint
            var client = new TokenClient($"{_botSettings.IdentityUrl}{_botSettings.TokenEndPoint}",
                                            _botSettings.ApiClient,
                                            _botSettings.ApiClientSecret);
            // log in user
            TokenResponse token = await client.RequestResourceOwnerPasswordAsync(userName, UserPassword);
            if (token == null || token.IsError)
            {
                if (token.IsError)
                    throw new Exception(token.Error);
                else
                    throw new Exception("Token Response is null");
            }

            // get user Info
            var user = new UserInfoClient($"{_botSettings.IdentityUrl}{_botSettings.UserInfoEndpoint}");
            var response = await user.GetAsync(token.AccessToken);

            if (response == null || response.IsError)
            {
                if(response.IsError)
                    throw new Exception(response.Error);
                else
                    throw new Exception("User Info is null");
            }

            await SetUserSessionData(token, response);

            return View();
        }

        private async Task SetUserSessionData(TokenResponse token, UserInfoResponse user, CancellationToken ctoken = default(CancellationToken))
        {
            // Get the resumption cookie
            var address = new Address
                (
                    // purposefully using named arguments because these all have the same type
                    botId: _botSettings.BotId,
                    channelId: Session["channelId"].ToString(),
                    userId: Session["userBotId"].ToString(),
                    conversationId: Session["conversationId"].ToString(),
                    serviceUrl: Session["serviceUrl"].ToString()
                );


            var conversationReference = address.ToConversationReference();
            var activity = conversationReference.GetPostToBotMessage();

            var userAppId = user.Claims
                                .Where(claim => claim.Type.Equals("sub"))
                                .Select(claimData => claimData.Value)
                                .FirstOrDefault();

            var userName = user.Claims
                                .Where(claim => claim.Type.Equals("name"))
                                .Select(claimData => claimData.Value)
                                .FirstOrDefault();

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {


                var botCred = new MicrosoftAppCredentials(_botSettings.MicrosoftAppId, _botSettings.MicrosoftAppPassword);
                StateClient stateClient;
                if (Session["channelId"].ToString().Equals("emulator"))
                {
                    // for emulator we should use serviceUri of the emulator for storage
                    var serviceUri = new Uri(Session["serviceUrl"].ToString());
                    stateClient = new StateClient(serviceUri, botCred);
                }
                else {
                    stateClient = new StateClient(botCred);
                }

                BotState botState = new BotState(stateClient);

                var userState = botState.GetUserData(Session["channelId"].ToString(), userId: Session["userBotId"].ToString());

                AuthUser authUser = new AuthUser() {
                    AccessToken = token.AccessToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
                    RefreshToken = token.RefreshToken,
                    UserId = userAppId
                };

                userState.SetProperty("authUser", authUser);

                await stateClient.BotState.SetUserDataAsync(
                                    Session["channelId"].ToString(),
                                    Session["userBotId"].ToString(),
                                    userState);

                // resolve BotToUser
                IBotToUser boToUser = scope.Resolve<IBotToUser>();
                var reply = activity.CreateReply($"{userName} you are now logeed, you can continue.");
                await boToUser.PostAsync(reply);
              
            }


            // Clear Session Data
            Session.Remove("userBotId");
            Session.Remove("channelId");
            Session.Remove("conversationId");
            Session.Remove("serviceUrl");
        }
    }
}