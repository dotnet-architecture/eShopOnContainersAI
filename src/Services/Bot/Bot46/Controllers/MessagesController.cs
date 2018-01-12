using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Bot46.API.Infrastructure;
using Bot46.API.Infrastructure.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Bot46.API.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ILifetimeScope scope;
        private BotSettings settings;
        public MessagesController(ILifetimeScope scope, BotSettings settings)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
            SetField.NotNull(out this.settings, nameof(settings), settings);
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken token)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
                {
                    await Conversation.SendAsync(activity, () => scope.Resolve<RootDialog>());
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
                            var replyWelcome = activity.CreateReply();

                            var heroCard = new HeroCard() {
                                Title = $"Welcome {newMember.Name}!",                          
                                Images = new List<CardImage>() { new CardImage() {Alt="eShop Logo", Url = $"{settings.MvcUrl}/images/brand.png" } }
                            };

                            var attachments = new List<Attachment>
                            {
                                heroCard.ToAttachment()
                            };

                            replyWelcome.Attachments = attachments;
                            
                            await client.Conversations.ReplyToActivityAsync(replyWelcome);

                            var replyBotName = activity.CreateReply();
                            replyBotName.Text = " I am Eshop-Bot.";
                            await client.Conversations.ReplyToActivityAsync(replyBotName);

                            var replyActions = activity.CreateReply();
                            replyActions.Text = $"I can show you Eshop Catalog, add items to your cart, place a new order and explorer your orders.";
                            await client.Conversations.ReplyToActivityAsync(replyActions);

                            var replyInitialHelp = activity.CreateReply();
                            replyInitialHelp.Text = $"Just type what ever you want to do, for example: *show me the catalog*";
                            await client.Conversations.ReplyToActivityAsync(replyInitialHelp);
                        }                        
                    }
                }
            }
        }
    }
}