using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Bot46.API.Infrastructure.Extensions;
using Bot46.API.Infrastructure.Models;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using static IdentityModel.OidcConstants;

namespace Bot46.API.Infrastructure.Dialogs
{
    public class LoginCommand: CommandScorable
    {
        public LoginCommand(IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {  
        }

        public override string Command
        {
            get{
                return "/login";
            }
        }

        protected override async Task PostAsync(IActivity item, bool state, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            var  response = await GetLoginAsync(message);
            await BotToUser.PostAsync(response);
        }

        private async Task<IMessageActivity> GetLoginAsync(IMessageActivity message)
        {
            var userIdFrom = message.From.Id;
            var reply = BotToUser.MakeMessage();
            reply.Attachments = new List<Attachment>();
            if (!await message.IsAuthenticatedAsync(userIdFrom))
            {
                Attachment login = await LoginCard(reply, userIdFrom);
                reply.Attachments.Add(login);
            }
            else {

                Attachment user = await reply.UserCard(userIdFrom);
                reply.Attachments.Add(user);
            }
            return reply;
        }

        protected async Task<Attachment> LoginCard(IActivity activity, string userId)
        {
            List<CardAction> cardButtons = new List<CardAction>();

            //AuthData authData = new AuthData() { 
            //    ChannelId = activity.ChannelId,
            //    UserId    = userId,
            //    ConversationId = activity.Conversation.Id,
            //    ServiceUrl = activity.ServiceUrl
            //    };

            //string authDataEncoded = authData.Encode();

            var identityUrl = ConfigurationManager.AppSettings["IdentityUrl"];
            var discoveryClient = new DiscoveryClient(identityUrl);
            var doc = await discoveryClient.GetAsync();

            var request = new RequestUrl(doc.AuthorizeEndpoint);
            var extra = new Dictionary<string, string>
            {
                { TokenRequest.ClientSecret, "secret" }
            };
            var authorizeUrl = request.CreateAuthorizeUrl(
                clientId: "Bot",                
                responseType: OidcConstants.ResponseTypes.CodeIdToken,
                responseMode: OidcConstants.ResponseModes.FormPost,
                redirectUri: "http://localhost:5200/account/signin-oidc",
                scope: "openid profile offline_access orders basket marketing locations",
                state: AuthData.Encode(ConfigurationManager.AppSettings["BotId"], activity.ChannelId, userId, activity.Conversation.Id, activity.ServiceUrl),
                nonce: CryptoRandom.CreateUniqueId(),
                extra: extra);

            // TODO SHOW IMAGE
            CardAction plButton = new CardAction()
            {
                // TODO change to real local URL
                // TODO change to not Harcoded String to BasePath
                //Value = $"http://localhost:5200/Auth/Login?authData={authDataEncoded}",
                Value = authorizeUrl,
                Type = "signin",
                Title = "Connect"
            };

            cardButtons.Add(plButton);
            SigninCard plCard = new SigninCard("Auth me", cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            return plAttachment;
        }

    }
}