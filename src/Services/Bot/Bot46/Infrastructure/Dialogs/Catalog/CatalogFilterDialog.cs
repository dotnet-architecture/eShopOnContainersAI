using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using Bot46.API.Infrastructure.Models;
using Bot46.API.Infrastructure.Modules;
using Bot46.API.Infrastructure.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Bot46.API.Infrastructure.Dialogs
{
    [Serializable]
    public class CatalogFilterDialog : IDialog<CatalogFilter>
    {
        //protected int count = 1;
        private static readonly ICatalogService serviceCatalog = ServiceResolver.Get<ICatalogService>();
        private static readonly IProductSearchImageService serviceComputerVision = ServiceResolver.Get<IProductSearchImageService>();
        private Brand brandSelected;

        public CatalogFilterDialog() {
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowSelectBrand(context);
            context.Wait(BrandMessageReceivedAsync);
        }

        private async Task ShowCardFilter(IDialogContext context)
        {
            var replyFilter = context.MakeMessage();
            var attachement = await AdaptiveCatalogFilter();
            replyFilter.Attachments = new List<Attachment>() { attachement };
            await context.PostAsync(replyFilter);
        }

        private async Task ShowSelectBrand(IDialogContext context) {
            var reply = context.MakeMessage();
            reply.Text = "Please select a brand do you want to search or upload a image to search by image.";
            var brands = await serviceCatalog.GetBrands();
            var options = new List<CardAction>();
 
            foreach (var brand in brands)
            {
                options.Add(new CardAction()
                {
                    Title = brand.Text,
                    Value = brand.Text,
                    Type = ActionTypes.ImBack
                });
            }
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = options

            };
            await context.PostAsync(reply);
        }


        private async Task ShowSelectType(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Text = "Please select a item category to seach or upload a image.";
            var types = await serviceCatalog.GetTypes();

            var options = new List<CardAction>();
            foreach (var type in types)
            {
                options.Add(new CardAction()
                {
                    Title = type.Text,
                    Value = type.Text,
                    Type = ActionTypes.ImBack
                });
            }
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = options

            };
            await context.PostAsync(reply);

        }

        public virtual async Task BrandMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message != null && message.Type == ActivityTypes.Message && !string.IsNullOrEmpty(message.Text))
            {
                var brands = await serviceCatalog.GetBrands();
                var brandSelected = brands.Where(b => b.Text.Equals(message.Text)).FirstOrDefault();
                if(brandSelected != null)
                {
                    this.brandSelected = brandSelected;
                    await ShowSelectType(context);
                    context.Wait(TypeMessageReceivedAsync);
                    return;
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Text = "I did not understand you, please select one brand.";
                    await context.PostAsync(reply);
                    await ShowSelectBrand(context);
                    context.Wait(BrandMessageReceivedAsync);
                    return;
                }
            }
            else
            {
                var content = await HandleAttachments(context, message);
                if (content != null)
                {
                    var tags = await serviceComputerVision.ClassifyImageAsync(content);
                    CatalogFilter filter = new CatalogFilter() { Tags = tags };
                    context.Done(filter);
                    return;
                }
            }
        }

        public virtual async Task TypeMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message != null && message.Type == ActivityTypes.Message && !string.IsNullOrEmpty(message.Text))
            {
                var types = await serviceCatalog.GetTypes();
                var typeSelected = types.Where(b => b.Text.Equals(message.Text)).FirstOrDefault();
                if (brandSelected != null)
                {
                    CatalogFilter filter = new CatalogFilter(brandSelected.Id, typeSelected.Id);
                    context.Done(filter);
                    return;
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Text = "I did not understand you, please select one type.";
                    await context.PostAsync(reply);
                    await ShowSelectType(context);
                    context.Wait(BrandMessageReceivedAsync);
                    return;
                }
            }
            else
            {
                var content = await HandleAttachments(context, message);
                if (content != null)
                {
                    var tags = await serviceComputerVision.ClassifyImageAsync(content);
                    CatalogFilter filter = new CatalogFilter() { Tags = tags };
                    context.Done(filter);
                    return;
                }
            }
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
         
            var brands = await serviceCatalog.GetBrands();
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
            var types = await serviceCatalog.GetTypes();
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
                DataJson = $"{{ 'ActionType': '{BotActionTypes.CatalogFilter}' }}"
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
                    case BotActionTypes.CatalogFilter:
                        CatalogFilter filter = CatalogFilter.Map(value);
                        context.Done(filter);
                        break;
                }
                return;
            }
            else
            {
                var content = await HandleAttachments(context, message);
                if(content != null)
                {
                    var tags = await serviceComputerVision.ClassifyImageAsync(content);
                    CatalogFilter filter = new CatalogFilter() { Tags = tags };
                    context.Done(filter);
                    return;
                }
            }
            await context.PostAsync("Please choose your selection or upload a image.");
            await ShowCardFilter(context);
            context.Wait(AdaptiveMessageReceivedAsync);
        }

        public async Task<byte[]> HandleAttachments(IDialogContext context, IMessageActivity message){
            byte[] content = null;
            if (message.Attachments != null && message.Attachments.Count > 0)
            {
                var attachment = message.Attachments[0];
                var client = new ConnectorClient(new Uri(context.Activity.ServiceUrl), new MicrosoftAppCredentials());
                content = await client.HttpClient.GetByteArrayAsync(attachment.ContentUrl);
            }
            return content;
        }
    }
}