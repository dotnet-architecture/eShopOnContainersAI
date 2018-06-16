using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier
{
    public enum Approaches
    {
        Default,
        ComputerVision,
        CustomVisionOffline,
        CustomVisionOnline
    }

    public class LabelConfidence
    {
        public float Probability { get; set; }
        public string Label { get; set; }
    }

    public interface IClassifier
    {
        Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image);
    }

    public interface IVisionStrategy
    {
        Task<IEnumerable<string>> ClassifyImageAsync(byte[] image, Approaches approach = Approaches.Default);
    }

    public class VisionStrategy : IVisionStrategy
    {
        private readonly Dictionary<Approaches, IClassifier> models;
        private readonly Approaches defaultModel;

        public VisionStrategy(IOptionsSnapshot<AppSettings> settings, IHostingEnvironment environment, IComputerVisionClient computerVisionClient, ICustomVisionClient customVisionClient, ILoggerFactory loggerFactory)
        {
            object parseDefaultModel;
            defaultModel =
                (Enum.TryParse(typeof(Approaches), settings.Value.CognitiveServicesPredictionDefaultModel, ignoreCase: true, result: out parseDefaultModel)) ?
                (Approaches)parseDefaultModel :
                 Approaches.Default;

            if (defaultModel == Approaches.Default)
                defaultModel = Approaches.ComputerVision;            

            models = new Dictionary<Approaches, IClassifier>
            {
                { Approaches.ComputerVision, new ComputerVisionPrediction(settings, computerVisionClient) },
                { Approaches.CustomVisionOffline, new CustomVisionOfflinePrediction(settings, environment, loggerFactory.CreateLogger<CustomVisionOfflinePrediction>()) },
                { Approaches.CustomVisionOnline, new CustomVisionOnlinePrediction(settings, customVisionClient) }
            };
        }

        /// <summary>
        /// Classify an image, using a model
        /// </summary>
        /// <param name="image">image (jpeg) file to be analyzed</param>
        /// <param name="model">model used for classification</param>
        /// <returns>image related labels</returns>
        public async Task<IEnumerable<string>> ClassifyImageAsync(byte[] image, Approaches approach = Approaches.Default)
        {
            var classification = await models[approach == Approaches.Default ? defaultModel : approach].ClassifyImageAsync(image);

            return classification.OrderByDescending(c => c.Probability)
                .Select(c => c.Label);
        }
    }

}
