using Microsoft.AspNetCore.Hosting;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Controllers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.TensorFlow.API.Classifier
{
    public interface ITensorFlowPredictionStrategy
    {
        System.Threading.Tasks.Task<IEnumerable<string>> ClassifyImageAsync(byte[] image);
    }

    public class TensorFlowPredictionStrategy : ITensorFlowPredictionStrategy
    {
        private readonly Dictionary<Approaches, IClassifier> models;
        private readonly Approaches defaultModel;

        public TensorFlowPredictionStrategy(IOptionsSnapshot<AppSettings> settings, IHostingEnvironment environment)
        {
            object parseDefaultModel;
            defaultModel =
                (Enum.TryParse(typeof(Approaches), settings.Value.TensorFlowPredictionDefaultModel, ignoreCase: true, result: out parseDefaultModel)) ?
                (Approaches) parseDefaultModel :
                 Approaches.Default;

            if (defaultModel == Approaches.Default)
                defaultModel = Approaches.TensorFlowPreTrained;

            models = new Dictionary<Approaches, IClassifier>
            {
                { Approaches.TensorFlowPreTrained, new TensorFlowInceptionPrediction(settings, environment) },
                { Approaches.TensorFlowCustom, new TensorFlowModelPrediction(settings, environment) }
            };
        }

        /// <summary>
        /// Classify an image, using a model
        /// </summary>
        /// <param name="image">image (jpeg) bytes to be analyzed</param>
        /// <returns>image related labels</returns>
        public async System.Threading.Tasks.Task<IEnumerable<string>> ClassifyImageAsync(byte[] image)
        {
            var classification = await models[defaultModel].ClassifyImageAsync(image);

            return classification.OrderByDescending(c => c.Probability)
                .Select(c => c.Label);
        }
    }
}

