using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bots.Bot.API.Infrastructure.Extensions;
using Microsoft.Bots.Bot.API.Models;

namespace Microsoft.Bots.Bot.API.Infrastructure
{
    public interface IIdentityService
    {
        Task<bool> IsAuthenticatedAsync(IActivity activity);
        Task<bool> IsAuthenticatedAsync(IDialogContext dialogContext);
        Task<BotData> GetBotDataAsync(IActivity activity);
        Task<BotData> GetBotDataAsync(IDialogContext dialogContext);
        Task<AuthUser> GetAuthUserAsync(IActivity activity);
        Task<AuthUser> GetAuthUserAsync(IDialogContext dialogContext);
    }

    public class IdentityService : IIdentityService
    {
        private readonly IBotDataRepository botDataRepository;

        public IdentityService(IBotDataRepository botDataRepository)
        {
            this.botDataRepository = botDataRepository;
        }

        public Task<bool> IsAuthenticatedAsync(IDialogContext dialogContext)
        {
            return IsAuthenticatedAsync(dialogContext.Activity);
        }

        public async Task<bool> IsAuthenticatedAsync(IActivity activity)
        {
            var authUser = await GetAuthUserAsync(activity);
            return authUser != null && !authUser.IsExpired;
        }

        public Task<BotData> GetBotDataAsync(IDialogContext dialogContext)
        {
            return GetBotDataAsync(dialogContext.Activity);
        }

        public Task<BotData> GetBotDataAsync(IActivity activity)
        {
            return botDataRepository.LoadUserDataAsync(activity);
        }

        public Task<AuthUser> GetAuthUserAsync(IDialogContext dialogContext)
        {
            return GetAuthUserAsync(dialogContext.Activity);
        }

        public async Task<AuthUser> GetAuthUserAsync(IActivity activity)
        {
            var userState = await GetBotDataAsync(activity);
            return userState.GetUserAuthData();
        }
    }
}