using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bot46.API.Infrastructure.Extensions;
using Bot46.API.Infrastructure.Models;
using Bot46.API.Infrastructure.Modules;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;

namespace Bot46.API.Infrastructure.Dialogs
{
    public class AddBasketAction: ActionScorable
    {

        private readonly IBasketService serviceBasket = ServiceResolver.Get<IBasketService>();

        public AddBasketAction(IBotToUser botToUser, IDialogTask task) : base(botToUser, task)
        {  
        }

        public override string Action
        {
            get{
                return BotActionTypes.AddBasket;
            }
        }

        protected override async Task PostAsync(IActivity item, bool state, CancellationToken token)
        {
            var message = item.AsMessageActivity();
            var  response = await AddBasket(message);
            await BotToUser.PostAsync(response);

            var dialog = new BasketDialog();
            var interruption = dialog.Void(task);
           
            task.Call(interruption, null);
            await task.PollAsync(token);
        }

        private async Task<IMessageActivity> AddBasket(IMessageActivity message)
        {

            var reply = BotToUser.MakeMessage();
            try {
                var userData = await message.UserData(message.From.Id);
                var authUser = userData.GetProperty<AuthUser>("authUser");
                if (authUser == null)
                {
                    reply.Text = $"Please log in.";
                    reply.Attachments = new List<Attachment>();
                    reply.Attachments.Add(((IActivity)message).LoginCard(message.From.Id));
                    return reply;
                }
                var json = JObject.Parse(message.Text);
                var producName = json.GetValue("ProductName").ToString();
                var product = new BasketItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 1,
                    ProductName = producName,
                    PictureUrl = json.GetValue("PictureUrl").ToString(),
                    UnitPrice = json.GetValue("UnitPrice").ToObject<decimal>(),
                    ProductId = json.GetValue("ProductId").ToString(),
                };
                await serviceBasket.AddItemToBasket(authUser.UserId, product, authUser.AccessToken);
                reply.Text = $"You have added {producName} to your basket.";
            }
            catch (AggregateException ae)
            {
                ae.Handle( (ex) => {
                    if (ex is HttpException e) // This we know how to handle.
                    {
                        reply.Text = $"Please log in.";
                        reply.Attachments = new List<Attachment>();
                        reply.Attachments.Add(((IActivity)message).LoginCard(message.From.Id));
                        return true;
                    }
                    return false;
                });
            }
            return reply;
        }
    }
}