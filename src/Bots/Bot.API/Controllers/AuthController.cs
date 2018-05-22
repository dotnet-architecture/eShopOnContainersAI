using IdentityModel.Client;
using Microsoft.Bots.Bot.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bots.Bot.API.Infrastructure;

namespace Microsoft.Bots.Bot.API.Controllers
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
            Session.Add("BotId", authDataDecoded.BotId);
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
                    botId: Session["BotId"].ToString(),//_botSettings.BotId,
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
                var botDataStore = scope.Resolve<IBotDataStore<Microsoft.Bot.Connector.BotData>>();
                var key = Address.FromActivity(activity);
                var userState = await botDataStore.LoadAsync(key, BotStoreType.BotUserData, CancellationToken.None);

                AuthUser authUser = new AuthUser() {
                    AccessToken = token.AccessToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
                    RefreshToken = token.RefreshToken,
                    UserId = userAppId
                };

                UserData userData = new UserData(user.Claims);

                userState.SetProperty("authUser", authUser);
                userState.SetProperty("userData", userData);

                await botDataStore.SaveAsync(key, BotStoreType.BotUserData, userState, CancellationToken.None);
                await botDataStore.FlushAsync(key, CancellationToken.None);

                // resolve BotToUser
                IBotToUser boToUser = scope.Resolve<IBotToUser>();
                var reply = activity.CreateReply($"{userName} you are now logeed, you can continue.");
                var cardActions = new List<Microsoft.Bot.Connector.CardAction>();

                cardActions.Add(new Microsoft.Bot.Connector.CardAction()
                {
                    Title = "Continue",
                    Type = Microsoft.Bot.Connector.ActionTypes.ImBack,
                    Value = "Continue"
                });

                reply.SuggestedActions = new Microsoft.Bot.Connector.SuggestedActions()
                {
                    Actions = cardActions
                };

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