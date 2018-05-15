using System;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using System.Threading.Tasks;

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
    public class CountryData
    {
        // next,country,year,month,max,min,std,count,sales,med,prev
        public CountryData(string country, int year, int month, float max, float min, float std, int count, float sales, float med, float prev)
        {
            this.country = country;

            this.year = year;
            this.month = month;
            this.max = max;
            this.min = min;
            this.std = std;
            this.count = count;
            this.sales = sales;
            this.med = med;
            this.prev = prev;
        }

        [ColumnName("Label")]
        public float next;

        public string country;

        public float year;
        public float month;
        public float max;
        public float min;
        public float std;
        public float count;
        public float sales;
        public float med;
        public float prev;
    }

    /// <summary>
    /// This is the output of the scored model, the prediction.
    /// </summary>
    public class CountrySalesPrediction
    {
        // Below columns are produced by the model's predictor.
        public Single Score;
    }

    public class CountrySales : ICountrySales
    {
        /// <summary>
        /// This method demonstrates how to run prediction on one example at a time.
        /// </summary>
        /// <summary>
        /// This method demonstrates how to run prediction on one example at a time.
        /// </summary>
        public async Task<CountrySalesPrediction> Predict(string modelPath, string country, int year, int month, float max, float min, float std, int count, float sales, float med, float prev)
        {
            // Load model
            var predictionEngine = await CreatePredictionEngineAsync(modelPath);

            // Build country sample
            var countrySample = new CountryData(country, year, month, max, min, std, count, sales, med, prev);

            // Returns prediction
            return predictionEngine.Predict(countrySample);
        }

        /// <summary>
        /// This function creates a prediction engine from the model located in the <paramref name="modelPath"/>.
        /// </summary>
        private async Task<PredictionModel<CountryData, CountrySalesPrediction>> CreatePredictionEngineAsync(string modelPath)
        {
            PredictionModel<CountryData, CountrySalesPrediction> model = await PredictionModel.ReadAsync<CountryData, CountrySalesPrediction>(modelPath);
            return model;
        }
    }
}
