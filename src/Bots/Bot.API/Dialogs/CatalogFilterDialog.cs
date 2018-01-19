using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Models.Catalog;
using Microsoft.Bots.Bot.API.Properties;
using Microsoft.Bots.Bot.API.Services;

namespace Microsoft.Bots.Bot.API.Dialogs
{
    [Serializable]
    public class CatalogFilterDialog : IDialog<CatalogFilter>
    {
        private readonly ICatalogService catalogService;
        private readonly IProductSearchImageService productSearchImageService;
        private Brand brandSelected;

        public CatalogFilterDialog(ICatalogService catalogService, IProductSearchImageService productSearchImageService) {
            this.catalogService = catalogService;
            this.productSearchImageService = productSearchImageService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await ShowSelectBrand(context);
            context.Wait(BrandMessageReceivedAsync);
        }

        private async Task ShowSelectBrand(IDialogContext context)
        {
            await context.PostAsync("Please select a brand you'd like to search for");

            var reply = context.MakeMessage();
            reply.Text = "You can also upload an image/photo to search for similar products";
            var brands = await catalogService.GetBrands();
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
            await context.PostAsync("Please select a type you'd like to search for");

            var reply = context.MakeMessage();
            reply.Text = "You can also upload an image/photo to search for similar products";
            var types = await catalogService.GetTypes();

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
            if (message.IsValidTextMessage())
            {
                var brands = await catalogService.GetBrands();
                var brandSelected = brands.FirstOrDefault(b => b.Text.Equals(message.Text));
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
                    reply.Text = TextResources.I_did_not_understand_you__please_select_one_brand;
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
                    var tags = await productSearchImageService.ClassifyImageAsync(content);
                    CatalogFilter filter = new CatalogFilter() { Tags = tags };
                    context.Done(filter);
                    return;
                }
            }
        }

        public virtual async Task TypeMessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (message.IsValidTextMessage())
            {
                var types = await catalogService.GetTypes();
                var typeSelected = types.FirstOrDefault(b => b.Text.Equals(message.Text));
                if (brandSelected != null)
                {
                    CatalogFilter filter = new CatalogFilter(brandSelected.Id, typeSelected.Id);
                    context.Done(filter);
                    return;
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Text = TextResources.I_did_not_understand_you_please_select_one_type;
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
                    var tags = await productSearchImageService.ClassifyImageAsync(content);
                    CatalogFilter filter = new CatalogFilter() { Tags = tags };
                    context.Done(filter);
                    return;
                }
            }
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