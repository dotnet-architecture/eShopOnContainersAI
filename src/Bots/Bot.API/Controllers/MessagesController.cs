using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure;

namespace Microsoft.Bots.Bot.API.Controllers
{
    public class MessagesController : ApiController
    {
        private readonly ILifetimeScope scope;
        private BotSettings settings;
        public MessagesController(ILifetimeScope scope, BotSettings settings)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
            SetField.NotNull(out this.settings, nameof(settings), settings);
        }

        public HttpResponseMessage Options([FromBody]Activity activity, CancellationToken token)
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [BotAuthentication]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken token)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
                {
                    await Conversation.SendAsync(activity, () => scope.Resolve<IDialog<object>>());
                }
            }
            else
            {
               await HandleSystemMessageAsync(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                await ConversationUpdate(message);
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async Task ConversationUpdate(Activity activity)
        {
            IConversationUpdateActivity update = activity;
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                var client = scope.Resolve<IConnectorClient>();
                if (update.MembersAdded.Any())
                {
                    foreach (var newMember in update.MembersAdded)
                    {
                        if (newMember.Id != activity.Recipient.Id)
                        {
                            var welcomeReply = activity.CreateReply();

                            var welcomeCard = new HeroCard() {
                                Title = $"Welcome {newMember.Name}!",                          
                                Images = new List<CardImage>() { new CardImage() {Alt="eShop Logo", Url = $"{settings.MvcUrl}/images/brand.png" } }
                            };

                            welcomeReply.Attachments = new List<Attachment> { welcomeCard.ToAttachment() };
                            
                            await client.Conversations.ReplyToActivityAsync(welcomeReply);

                            var replyBotName = activity.CreateReply();
                            replyBotName.Text = " Howdy! - I am eShopAI-Bot.";
                            await client.Conversations.ReplyToActivityAsync(replyBotName);

                            var replyActions = activity.CreateReply();
                            replyActions.Text = $"I can show you the eShopAI Catalog, add items to your shopping cart, place a new order and explore your order's status.";
                            await client.Conversations.ReplyToActivityAsync(replyActions);

                            var replyInitialHelp = activity.CreateReply();
                            replyInitialHelp.Text = $"Just type whatever you want to do, for example: *show me the product catalog*";
                            await client.Conversations.ReplyToActivityAsync(replyInitialHelp);
                        }                        
                    }
                }
            }
        }
    }
}