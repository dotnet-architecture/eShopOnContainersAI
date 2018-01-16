using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Microsoft.Bots.Bot.API.Infrastructure
{
    public interface IBotDataRepository
    {
        Task<BotData> LoadUserDataAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken));
        Task<BotData> LoadUserDataAsync(Address addressKey, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveUserDataAsync(Address addressKey, BotData botData, CancellationToken cancellationToken = default(CancellationToken));
    }

    public class BotDataRepository : IBotDataRepository
    {
        private readonly IBotDataStore<BotData> botDataStore;

        public BotDataRepository(IBotDataStore<BotData> botDataStore)
        {
            this.botDataStore = botDataStore;
        }

        public async Task<BotData> LoadUserDataAsync(IActivity activity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = Address.FromActivity(activity);
            var value = await botDataStore.LoadAsync(key, BotStoreType.BotUserData, cancellationToken);
            return value;
        }

        public async Task<BotData> LoadUserDataAsync(Address addressKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var value = await botDataStore.LoadAsync(addressKey, BotStoreType.BotUserData, cancellationToken);
            return value;
        }

        public async Task SaveUserDataAsync(Address addressKey, BotData botData, CancellationToken cancellationToken = default(CancellationToken))
        {
            await botDataStore.SaveAsync(addressKey, BotStoreType.BotUserData, botData, cancellationToken);
            await botDataStore.FlushAsync(addressKey, cancellationToken);
        }
    }

    public class CardFactory
    {
        private readonly IBotDataRepository botDataRepository;

        public CardFactory(IBotDataRepository botDataRepository)
        {
            this.botDataRepository = botDataRepository;
        }


    }
}