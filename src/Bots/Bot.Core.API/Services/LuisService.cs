using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.eShopOnContainers.Bot.API.LUIS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API.Services
{
    public interface ILuisService
    {
        Task<eShopLuisResult> GetResultAsync(string utterance);
    }

    public class LuisService : ILuisService
    {
        private readonly LuisModel luisModel;
        private readonly ILogger<LuisService> logger;

        public LuisService(IOptions<AppSettings> appSettings, ILogger<LuisService> logger)
        {
            if (appSettings.Value.Luis == null || String.IsNullOrEmpty(appSettings.Value.Luis.ModelId))
                return;

            this.luisModel = new LuisModel(
                appSettings.Value.Luis.ModelId,
                appSettings.Value.Luis.SubscriptionKey,
                new Uri(appSettings.Value.Luis.ServiceUri),
                Cognitive.LUIS.LuisApiVersion.V2);
            this.logger = logger;
        }

        public async Task<eShopLuisResult> GetResultAsync(string utterance)
        {
            if (luisModel == null)
            {
                logger.LogDebug("LUIS model not configured");
                return null;
            }

            System.Threading.CancellationToken ct;
            var luisRecognizer = CreateLUISRecognizer();
            var result = await luisRecognizer.Recognize<eShopLuisResult>(utterance, ct);
            return result;
        }

        private LuisRecognizer CreateLUISRecognizer()
        {
            return new LuisRecognizer(this.luisModel,
                new LuisRecognizerOptions() { Verbose = true });
        }
    }
}
