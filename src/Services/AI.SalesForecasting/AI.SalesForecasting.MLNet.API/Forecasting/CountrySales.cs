using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.MachineLearning;
using Microsoft.MachineLearning.Api;
using Microsoft.MachineLearning.Data;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Forecasting
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
        // next,country,year,month,sales,avg,count,max,min,p_max,p_med,p_min,std,prev
        public CountrySample(string country, int year, int month, float sales, float avg, int count, float max, float min, float p_max, float p_med, float p_min, float std, float prev)
        {
            this.country = country;
            Features = new Single[] { year, month, sales, avg, count, max, min, p_max, p_med, p_min, std, prev };
        }

        public Single next;

        public string country;

        [VectorType(12)]
        public Single[] Features = new Single[12];
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
        public ScoredCountrySample Predict(string modelPath, string country, int year, int month, float sales, float avg, int count, float max, float min, float p_max, float p_med, float p_min, float std, float prev)
        {
            var env = new TlcEnvironment(conc: 1);

            var predictionEngine = CreatePredictionEngine(env, modelPath);

            var inputExample = new CountrySample(country, year, month, sales, avg, count, max, min, p_max, p_med, p_min, std, prev);

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
