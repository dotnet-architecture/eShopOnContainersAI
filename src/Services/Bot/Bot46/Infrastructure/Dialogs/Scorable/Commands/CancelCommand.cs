using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot46.API.Infrastructure.Extensions;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Dialogs
{
    public class CancelCommand : CommandScorable
    {
        public CancelCommand(IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {  
        }

        public override string Command
        {
            get{
                return "/cancel";
            }
        }

        protected override async Task PostAsync(IActivity item, bool state, CancellationToken token)
        {
            this.task.Reset();
        }

        private async Task<IMessageActivity> GetLoginAsync(IMessageActivity message)
        {
            var userIdFrom = message.From.Id;
            var reply = BotToUser.MakeMessage();
            reply.Attachments = new List<Attachment>();
            if (!await message.IsAuthenticatedAsync(userIdFrom))
            {
                Attachment login = reply.LoginCard(userIdFrom);
                reply.Attachments.Add(login);
            }
            else {

                Attachment user = await reply.UserCard(userIdFrom);
                reply.Attachments.Add(user);
            }
            return reply;
        }
    }
}