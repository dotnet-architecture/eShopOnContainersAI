using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using IdentityModel.Client;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Models;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOIDCClient oidcClient;
        private readonly IBotDataRepository botDataRepository;

        public AccountController(IOIDCClient oidcClient, IBotDataRepository botDataRepository)
        {
            this.oidcClient = oidcClient;
            this.botDataRepository = botDataRepository;
        }

        [HttpPost()]
        [Route("account/signin-oidc", Name = "signin")]
        public async Task<ActionResult> SigninOIDC()
        {
            var authorizeResponse = oidcClient.GetAuthorizeResponse(Request.Form);

            if (String.IsNullOrEmpty(authorizeResponse.AccessToken))
            {
                var doc = await oidcClient.GetDiscoveryClient();

                var tokenResponse = await oidcClient.GetAccessToken(doc.TokenEndpoint, authorizeResponse.Code);

                var userInfoResponse = await oidcClient.GetUserInfo(doc.UserInfoEndpoint, tokenResponse.AccessToken);

                await SetUserSessionData(tokenResponse, userInfoResponse, AuthData.Decode(authorizeResponse.State));
            }
            return View();
        }

        private async Task SetUserSessionData(TokenResponse token, UserInfoResponse user, AuthData authData, CancellationToken cancellationToken = default(CancellationToken))
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

            // create user data
            var dataUser = new UserData(user.Claims);
            var authUser = GetAuthUser(token, dataUser.UserApplicationId);
            var activityAddress = Address.FromActivity(activity);

            var userState = await botDataRepository.LoadUserDataAsync(activityAddress, cancellationToken);

                userState.SetUserAuthData(authUser);
                userState.SetUserData(dataUser);

            await botDataRepository.SaveUserDataAsync(activityAddress, userState, cancellationToken);

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                // resolve BotToUser
                var botToUser = scope.Resolve<IBotToUser>();
                var reply = CreateLoginContinueActivity(activity, dataUser.Name);

                await botToUser.PostAsync(reply, cancellationToken);
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

        private static AuthUser GetAuthUser(TokenResponse token, string userApplicationId)
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
}