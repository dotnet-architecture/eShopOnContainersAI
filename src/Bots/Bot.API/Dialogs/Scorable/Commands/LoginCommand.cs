using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Dialogs.Scorable.Commands
{
    public class LoginCommand: CommandScorable
    {
        private readonly IOIDCClient oidcClient;
        private readonly IIdentityService identityService;

        public LoginCommand(IOIDCClient oidcClient, IIdentityService identityService, IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {
            this.oidcClient = oidcClient;
            this.identityService = identityService;
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
            await BotToUser.PostAsync(response, token);
        }

        private async Task<IMessageActivity> GetLoginAsync(IMessageActivity message)
        {
            var reply = BotToUser.MakeMessage();
            reply.Attachments = new List<Attachment>();
            if (!await identityService.IsAuthenticatedAsync(message))
            {
                var loginCard = await message.CreateLoginCardAsync(oidcClient);
                reply.Attachments.Add(loginCard);
            }
            else {

                var userCard = await reply.CreateUserCardAsync(identityService);
                reply.Attachments.Add(userCard);
            }
            return reply;
        }
    }
}