using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.MachineLearning;
using Microsoft.MachineLearning.Api;
using Microsoft.MachineLearning.Data;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Forecasting
{
    /// <summary>
    /// This is the input to the trained model.
    ///
    /// In most pipelines, not all columns that are used in training are also used in scoring. Namely, the label 
    /// and weight columns are almost never required at scoring time. Since TLC doesn't know which columns 
    /// are 'optional' in this sense, all the columns are listed below.
    ///
    /// You are free to remove any fields from the below class. If the fields are not required for scoring, the model 
    /// will continue to work. Otherwise, the exception will be thrown when a prediction engine is created.
    /// </summary>
    public class CountrySample
    {
        public CountrySample(string country, int year, int month, float avg, int max, int min, int prev, int count, int units)
        {
            this.country = country;
            Features = new Single[] { year, month, units, avg, count, max, min, prev };
        }

        public Single next;

        public string country;

        [VectorType(8)]
        public Single[] Features = new Single[8];
    }

    /// <summary>
    /// This is the output of the scored model, the prediction.
    /// </summary>
    public class ScoredCountrySample
    {
        // Below columns are produced by the model's predictor.
        public Single Score;
    }

    public class CountrySales : ICountrySales
    {
        /// <summary>
        /// This method demonstrates how to run prediction on one example at a time.
        /// </summary>
        public ScoredCountrySample Predict(string modelPath, string country, int year, int month, float avg, int max, int min, int prev, int count, int units)
        {
            var env = new TlcEnvironment(conc: 1);

            var predictionEngine = CreatePredictionEngine(env, modelPath);

            var inputExample = new CountrySample(country, year, month, avg, max, min, prev, count, units);

            return predictionEngine.Predict(inputExample);
        }

        /// <summary>
        /// This function creates a prediction engine from the model located in the <paramref name="modelPath"/>.
        /// </summary>
        private PredictionEngine<CountrySample, ScoredCountrySample> CreatePredictionEngine(IHostEnvironment env, string modelPath)
        {
            using (var fs = File.OpenRead(modelPath))
                return env.CreatePredictionEngine<CountrySample, ScoredCountrySample>(fs);
        }
    }
}
