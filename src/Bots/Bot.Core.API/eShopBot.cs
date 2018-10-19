using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Dialogs;
using Microsoft.eShopOnContainers.Bot.API.Dialogs.Main;
using Microsoft.eShopOnContainers.Bot.API.Services;
using Microsoft.eShopOnContainers.Bot.API.Services.Catalog;
using Microsoft.eShopOnContainers.Bot.API.Services.LUIS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API
{
    public static class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string self)
        {
            // code from https://stackoverflow.com/a/50607576
            if (!string.IsNullOrEmpty(self) && self.Length > 1)
            {
                return Char.ToLowerInvariant(self[0]) + self.Substring(1);
            }
            return self;
        }

        public static string ToCamelCase(this Type self)
        {
            return self.Name.ToCamelCase();
        }

        public static string ToCamelCase<T>()
        {
            return nameof(T).ToCamelCase();
        }

        public static IStatePropertyAccessor<T> CreatePropertyFromTypeName<T>(this BotState self)
        {
            return self.CreateProperty<T>(nameof(T).ToCamelCase());
        }
    }

    public class eShopBot : IBot
    {
        private readonly DialogSet dialogs;
        private readonly DomainPropertyAccessors eShopBotAccessors;

        public eShopBot(DomainPropertyAccessors eShopBotAccessors, IDialogFactory dialogFactory)
        {
            this.dialogs = new DialogSet(eShopBotAccessors.DialogStateProperty);
            this.dialogs.Add(dialogFactory.MainDialog);
            this.eShopBotAccessors = eShopBotAccessors;
        }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var dc = await dialogs.CreateContextAsync(turnContext);
            var result = await dc.ContinueDialogAsync();            

            if (result.Status == DialogTurnStatus.Empty)
            {
                await dc.BeginDialogAsync(MainDialog.Name);
            }

            await eShopBotAccessors.SaveStatesAsync(turnContext, cancellationToken);
        }
    }
}
