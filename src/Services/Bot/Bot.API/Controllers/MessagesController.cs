using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Bot.API.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using System.Threading;
using Bot.API.Services;

namespace Bot.API.Controllers
{
    [Route("api/v1/[controller]")]
    [BotAuthentication]
    public class MessagesController : Controller
    {

        private readonly ILifetimeScope scope;
        private readonly ILogger<MessagesController> logger;
        private readonly BotSettings settings;

        public MessagesController(ILifetimeScope scope, ILogger<MessagesController> logger, IOptionsSnapshot<BotSettings> settings)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
            SetField.NotNull(out this.logger, nameof(logger), logger);
            SetField.NotNull(out this.settings, nameof(settings), settings.Value);  
        }

        // POST api/values
        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody]Activity activity)
        {
            if (activity != null) {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => this.scope.Resolve<CatalogDialog>());
                        break;

                    case ActivityTypes.ConversationUpdate:
                        await ConversationUpdate(activity);
                        break;
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    case ActivityTypes.Ping:
                    default:
                        logger.LogWarning("Unknown activity type ignored: {0}", activity.GetActivityType());
                        break;
                }
            }
            return new StatusCodeResult((int) HttpStatusCode.Accepted);
        }

        private async Task ConversationUpdate(Activity activity){
            IConversationUpdateActivity update = activity;
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                var client = scope.Resolve<IConnectorClient>();
                if (update.MembersAdded.Any())
                {
                    var reply = activity.CreateReply();
                    foreach (var newMember in update.MembersAdded)
                    {
                        if (newMember.Id != activity.Recipient.Id)
                        {
                            reply.Text = $"Welcome {newMember.Name}!";            
                        }
                        else
                        {
                            reply.Text = $"{activity.From.Name} has joined";      
                        }
                        logger.LogInformation("User joinned: {0}",activity.From.Name);
                        await client.Conversations.ReplyToActivityAsync(reply);
                    }
                }
            }
        }
    }
}