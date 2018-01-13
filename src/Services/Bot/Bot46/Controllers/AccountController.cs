using Autofac;
using Bot46.API.Infrastructure;
using Bot46.API.Infrastructure.Models;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static IdentityModel.OidcConstants;

namespace Bot46.API.Controllers
{
    public class AccountController : Controller
    {
        private readonly BotSettings _botSettings;

        public AccountController(BotSettings botSettings)
        {
            _botSettings = botSettings;
        }

        [HttpPost()]
        [Route("account/signin-oidc")]
        public async Task<ActionResult> SigninOIDC()
        {
            var authorizeResponse = GetAuthorizeResponse(Request.Form);

            if (String.IsNullOrEmpty(authorizeResponse.AccessToken))
            {
                var discoveryClient = new DiscoveryClient(_botSettings.IdentityUrl);
                var doc = await discoveryClient.GetAsync();

                var tokenResponse = await GetAccessToken(doc.TokenEndpoint, authorizeResponse.Code);

                var userInfoResponse = await GetUserInfo(doc.UserInfoEndpoint, tokenResponse.AccessToken);

                await SetUserSessionData(tokenResponse, userInfoResponse, AuthData.Decode(authorizeResponse.State));
            }
            return View();
        }

        private IdentityModel.Client.AuthorizeResponse GetAuthorizeResponse (NameValueCollection form)
        {
            var formBody = form.ToString();
            if (String.IsNullOrEmpty(formBody))
                throw new Exception("Error during login");

            return new IdentityModel.Client.AuthorizeResponse(formBody);
        }

        private async Task<IdentityModel.Client.TokenResponse> GetAccessToken(string tokenEndpoint, string code)
        {
            var client = new TokenClient(tokenEndpoint,
                                            _botSettings.ApiClient,
                                            _botSettings.ApiClientSecret);

            var tokenResponse = await client.RequestAuthorizationCodeAsync(code, "http://localhost:5200/account/signin-oidc");

            if (tokenResponse == null || tokenResponse.IsError)
            {
                if (tokenResponse.IsError)
                    throw new Exception(tokenResponse.Error);
                else
                    throw new Exception("Token Response is null");
            }

            return tokenResponse;
        }

        private async Task<UserInfoResponse> GetUserInfo(string userInfoEndpoint, string accessToken)
        {
            var user = new UserInfoClient(userInfoEndpoint);
            var userInfoResponse = await user.GetAsync(accessToken);

            if (userInfoResponse == null || userInfoResponse.IsError)
            {
                if (userInfoResponse.IsError)
                    throw new Exception(userInfoResponse.Error);
                else
                    throw new Exception("User Info is null");
            }

            return userInfoResponse;
        }

        private async Task SetUserSessionData(IdentityModel.Client.TokenResponse token, UserInfoResponse user, AuthData authData, CancellationToken ctoken = default(CancellationToken))
        {
            // Get the resumption cookie
            var address = new Address
                (
                    botId: authData.BotId,
                    channelId: authData.ChannelId,
                    userId: authData.UserId,
                    conversationId: authData.ConversationId,
                    serviceUrl: authData.ServiceUrl
                );

            // resolve on going activity in bot
            var conversationReference = address.ToConversationReference();
            var activity = conversationReference.GetPostToBotMessage();
            var activityAddress = Address.FromActivity(activity);

            // create user data
            var dataUser = new UserData(user.Claims);
            var authUser = GetAuthUser(token, dataUser.UserApplicationId);

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                var botDataStore = scope.Resolve<IBotDataStore<BotData>>();

                var userState = await botDataStore.LoadAsync(activityAddress, BotStoreType.BotUserData, CancellationToken.None);

                userState.SetUserAuthData(authData);
                userState.SetUserData(dataUser);

                await botDataStore.SaveAsync(activityAddress, BotStoreType.BotUserData, userState, CancellationToken.None);
                await botDataStore.FlushAsync(activityAddress, CancellationToken.None);

                // resolve BotToUser
                IBotToUser botToUser = scope.Resolve<IBotToUser>();
                Activity reply = CreateLoginContinueActivity(activity, dataUser.Name);

                await botToUser.PostAsync(reply);
            }
        }

        private static Activity CreateLoginContinueActivity(Activity activity, string userName)
        {
            var reply = activity.CreateReply($"{userName} is now logged in ... you can continue.");
            var cardActions = new List<CardAction>
            {
                new CardAction()
                {
                    Title = "Continue",
                    Type = ActionTypes.ImBack,
                    Value = "Continue"
                }
            };

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = cardActions
            };
            return reply;
        }

        private static AuthUser GetAuthUser(IdentityModel.Client.TokenResponse token, string userApplicationId)
        {
            AuthUser authUser = new AuthUser()
            {
                AccessToken = token.AccessToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
                RefreshToken = token.RefreshToken,
                UserId = userApplicationId
            };
            return authUser;
        }
    }

    public static class BotDataExtensions
    {
        private const string userDataKey = "userData";
        private const string userAuthKey = "authUser";

        public static void SetUserData(this BotData self, UserData value)
        {
            self.SetProperty(userDataKey, value);
        }

        public static AuthData GetUserData(this BotData self)
        {
            return self.GetProperty<AuthData>(userDataKey);
        }

        public static void SetUserAuthData(this BotData self, AuthData value)
        {
            self.SetProperty(userAuthKey, value);
        }

        public static AuthData GetUserAuthData(this BotData self)
        {
            return self.GetProperty<AuthData>(userAuthKey);
        }
    }
}