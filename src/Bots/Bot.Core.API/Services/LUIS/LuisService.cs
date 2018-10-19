using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services.LUIS
{
    public class LuisService : ILuisService
    {
        private readonly LuisApplication luisModel;
        private readonly ILogger<LuisService> logger;

        public LuisService(BotConfiguration botConfiguration, ILogger<LuisService> logger)
        {
            var luisService = botConfiguration.Services.Find(cs => cs.Type == ServiceTypes.Luis) as Microsoft.Bot.Configuration.LuisService;
            if (luisService == null)
                logger.LogCritical("Luis service not defined in Bot configuration");
            else
                this.luisModel = new LuisApplication(luisService.AppId, luisService.SubscriptionKey, luisService.GetEndpoint());
            this.logger = logger;
        }

        public async Task<eShopLuisResult> GetResultAsync(ITurnContext context)
        {
            if (luisModel == null)
            {
                logger.LogDebug("LUIS model not configured");
                return null;
            }

            CancellationToken ct;
            var luisRecognizer = CreateLUISRecognizer();
            var result = await luisRecognizer.RecognizeAsync<eShopLuisResult>(context, ct);
            return result;
        }

        private LuisRecognizer CreateLUISRecognizer()
        {
            return new LuisRecognizer(this.luisModel,
                new LuisPredictionOptions() { Log = true });
        }
    }
}
