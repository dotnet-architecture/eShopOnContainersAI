using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Infrastructure
{
    public class SignInMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            //BotAssert.ContextNotNull(context);

            //if (context.Activity.Type == ActivityTypes.Message)
            //{
            //    var byteArray = Encoding.UTF8.GetBytes(context.Activity.Text);
            //    var textStream = new MemoryStream(byteArray);

            //    var client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(this.subscriptionKey));
            //    client.BaseUrl = $"{this.region}.api.cognitive.microsoft.com";

            //    var screenResult = client.TextModeration.ScreenText(
            //        language: "eng",
            //        textContentType: "text/plain",
            //        textContent: textStream,
            //        autocorrect: true,
            //        pII: true,
            //        listId: null,
            //        classify: true);

            //    context.TurnState.Add(TextModeratorResultKey, screenResult);
            //}

            await next(cancellationToken);

        }
    }
}
