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
        private readonly Dictionary<AnalyzeImageModel, IClassifier> models;
        private readonly AnalyzeImageModel defaultModel;

        public TensorFlowPredictionStrategy(IOptionsSnapshot<AppSettings> settings, IHostingEnvironment environment)
        {
            object parseDefaultModel;
            defaultModel =
                (Enum.TryParse(typeof(AnalyzeImageModel), settings.Value.TensorFlowPredictionDefaultModel, ignoreCase: true, result: out parseDefaultModel)) ?
                (AnalyzeImageModel) parseDefaultModel :
                 AnalyzeImageModel.Default;

            if (defaultModel == AnalyzeImageModel.Default)
                defaultModel = AnalyzeImageModel.TensorFlowInception;

            models = new Dictionary<AnalyzeImageModel, IClassifier>
            {
                { AnalyzeImageModel.TensorFlowInception, new TensorFlowInceptionPrediction(settings, environment) },
                { AnalyzeImageModel.TensorFlowModel, new TensorFlowModelPrediction(settings, environment) }
            };
        }

        /// <summary>
        /// Classify an image, using a model
        /// </summary>
        /// <param name="image">image (jpeg) file to be analyzed</param>
        /// <param name="model">model used for classification</param>
        /// <returns>image related labels</returns>
        public async System.Threading.Tasks.Task<IEnumerable<string>> ClassifyImageAsync(byte[] image)
        {
            var classification = await models[defaultModel].ClassifyImageAsync(image);

            return classification.OrderByDescending(c => c.Probability)
                .Select(c => c.Label);
        }
    }
}

