using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Models;

namespace Microsoft.Bots.Bot.API.Services
{
    public interface IOIDCClient
    {
        AuthorizeResponse GetAuthorizeResponse(NameValueCollection form);
        Task<TokenResponse> GetAccessToken(string tokenEndpoint, string code);
        Task<UserInfoResponse> GetUserInfo(string userInfoEndpoint, string accessToken);
        Task<CompactDiscoveryResponse> GetDiscoveryClient();
        Task<string> CreateAuthorizeUrlAsync(IActivity activity);
    }

    public class CompactDiscoveryResponse
    {
        public CompactDiscoveryResponse(string issuer)
        {
            Issuer = issuer;
        }

        public string AuthorizeEndpoint { get { return $"{Issuer}/connect/authorize"; } }
        public string Issuer { get; set; }
        public string TokenEndpoint { get { return $"{Issuer}/connect/token"; } }
        public string UserInfoEndpoint { get { return $"{Issuer}/connect/userinfo"; } }
    }

    public class OIDCClient : IOIDCClient
    {
        private readonly BotSettings botSettings;
        private readonly string signinUrl;
        private readonly CompactDiscoveryResponse discoveryResponse;

        public OIDCClient(BotSettings botSettings)
        {
            this.botSettings = botSettings;
            signinUrl = ConfigurationManager.AppSettings["SignInUrl"];
            discoveryResponse = new CompactDiscoveryResponse(this.botSettings.IdentityUrl);
        }

        public async Task<string> CreateAuthorizeUrlAsync(IActivity activity)
        {
            var doc = await GetDiscoveryClient();

            var request = new RequestUrl(doc.AuthorizeEndpoint);
            var extra = new Dictionary<string, string>
            {
                { OidcConstants.TokenRequest.ClientSecret, "secret" }
            };
            var authorizeUrl = request.CreateAuthorizeUrl(
                    clientId: "Bot",
                    responseType: OidcConstants.ResponseTypes.CodeIdToken,
                    responseMode: OidcConstants.ResponseModes.FormPost,
                    redirectUri: signinUrl,
                    scope: "openid profile offline_access orders basket marketing locations",
                    state: AuthData.Encode(activity.Recipient.Id, activity.ChannelId, activity.From.Id, activity.Conversation.Id, activity.ServiceUrl),
                    nonce: CryptoRandom.CreateUniqueId(),
                    extra: extra);
            return authorizeUrl;
        }

        public Task<CompactDiscoveryResponse> GetDiscoveryClient()
        {
            //var discoveryClient = new DiscoveryClient(botSettings.IdentityUrl);
            //var doc = await discoveryClient.GetAsync();
            //return doc;
            return Task.FromResult(discoveryResponse);
        }

        public AuthorizeResponse GetAuthorizeResponse(NameValueCollection form)
        {
            var formBody = form.ToString();
            if (String.IsNullOrEmpty(formBody))
                throw new Exception("Error during login");

            return new AuthorizeResponse(formBody);
        }

        public async Task<TokenResponse> GetAccessToken(string tokenEndpoint, string code)
        {
            var client = new TokenClient(tokenEndpoint,
                botSettings.ApiClient,
                botSettings.ApiClientSecret);

            var tokenResponse = await client.RequestAuthorizationCodeAsync(code, signinUrl);

            tokenResponse.ShouldBeOK("Token Response is null");

            return tokenResponse;
        }

        public async Task<UserInfoResponse> GetUserInfo(string userInfoEndpoint, string accessToken)
        {
            var user = new UserInfoClient(userInfoEndpoint);
            var userInfoResponse = await user.GetAsync(accessToken);

            userInfoResponse.ShouldBeOK("User Info is null");

            return userInfoResponse;
        }
    }
}