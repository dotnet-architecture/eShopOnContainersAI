using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using Bot.API.Services;
using Bot.API.ViewModels;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Bot.API.Dialogs
{
    [Serializable]
    public class CatalogFilterDialog : IDialog<CatalogFilter>
    {
        protected int count = 1;
        private static readonly ICatalogService service = ServiceResolver.Get<ICatalogService>();

        public CatalogFilterDialog() {
        }

        public async Task StartAsync(IDialogContext context)
        {   
            await context.PostAsync("CatalogDialog");
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            await HandleAttachemts(context,message);
          
            var reply = context.MakeMessage();
            var attachement = await AdaptiveCatalogFilter();
            reply.Attachments = new List<Attachment>() {attachement};
            await context.PostAsync(reply);

            context.Wait(AdaptiveMessageReceivedAsync);
        }

        public async Task<Attachment> AdaptiveCatalogFilter()
        {
            AdaptiveCard card = new AdaptiveCard();
            // Add text to the card.
            card.Body.Add(new AdaptiveTextBlock(){
                Text = "Catalog Filter",
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder
            });
           
            // Add list of brands to the card. 
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Brand",
                Weight = AdaptiveTextWeight.Bolder
            });
         
            var brands = await service.GetBrands();
            var brandChoices = new List<AdaptiveChoice>();
            foreach ( var brand in brands){
                brandChoices.Add( new AdaptiveChoice() { Title = brand.Text, Value = brand.Id });
            }

            card.Body.Add(new AdaptiveChoiceSetInput(){
                Id = "Brand",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices  = brandChoices,
                Value = "-1"
            });
          

            // Add list of types to the card. 
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Type",
                Weight = AdaptiveTextWeight.Bolder
            });         
            var types = await service.GetTypes();
            var typeChoices = new List<AdaptiveChoice>();
            foreach (var type in types){
                typeChoices.Add( new AdaptiveChoice() { Title = type.Text, Value = type.Id });
            }

            card.Body.Add(new AdaptiveChoiceSetInput(){
                Id = "Type",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices  = typeChoices,
                Value = "-1"
            });

            card.Actions.Add(new AdaptiveSubmitAction(){
                Title = "Search",
                DataJson = "{ \"ActionType\": \"CatalogFilter\" }"
            });
          
            // Create the attachment.
            Attachment attachment = new Attachment(){
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;
        }


        public virtual async Task AdaptiveMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Value != null)
            {
                // Got an Action Submit
                dynamic value = message.Value;
                switch (value.ActionType.ToString())
                {
                    case "CatalogFilter":
                        var filter = CatalogFilter.Map(value);
                        context.Done(filter);
                        break;
                }
            }
            await context.PostAsync("Please make a selection");
            context.Wait(MessageReceivedAsync);
        }




        public async Task HandleAttachemts(IDialogContext context, IMessageActivity message){
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];
                var client = new ConnectorClient(new Uri(context.Activity.ServiceUrl), new MicrosoftAppCredentials());
                var content = await client.HttpClient.GetByteArrayAsync(attachment.ContentUrl);
            }
        }
    }
}