using Microsoft.ML.Data;
using Microsoft.ML;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.Training.MLNet.API
{
    public class CountryModelHelper
    {
        /// <summary>
        /// Train and save model for predicting next month country unit sales
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <param name="outputModelPath">Trained model path</param>
        public static async Task SaveModel(string dataPath, string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            var model = CreateCountryModelUsingPipeline(dataPath);

            await model.WriteAsync(outputModelPath);
        }

        /// <summary>
        /// Build model for predicting next month country unit sales using Learning Pipelines API
        /// </summary>
        /// <param name="dataPath">Input training file path</param>
        /// <returns></returns>
        private static PredictionModel<CountryData, CountrySalesPrediction> CreateCountryModelUsingPipeline(string dataPath)
        {
            Console.WriteLine("*************************************************");
            Console.WriteLine("Training country forecasting model using Pipeline");

            var learningPipeline = new LearningPipeline();
            // First node in the workflow will be reading the source csv file, following the schema defined by dataSchema
            learningPipeline.Add(new TextLoader(dataPath).CreateFrom<CountryData>(useHeader: true, separator: ','));

            // The model needs the columns to be arranged into a single column of numeric type
            // First, we group all numeric columns into a single array named NumericalFeatures
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(
                outputColumn: "NumericalFeatures",
                nameof(CountryData.year),
                nameof(CountryData.month),
                nameof(CountryData.max),
                nameof(CountryData.min),
                nameof(CountryData.std),
                nameof(CountryData.count),
                nameof(CountryData.sales),
                nameof(CountryData.med),
                nameof(CountryData.prev)
            ));

            // Second group is for categorical features (just one in this case), we name this column CategoryFeatures
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(outputColumn: "CategoryFeatures", nameof(CountryData.country)));

            // Then we need to transform the category column using one-hot encoding. This will return a numeric array
            learningPipeline.Add(new ML.Transforms.CategoricalOneHotVectorizer("CategoryFeatures"));

            // Once all columns are numeric types, all columns will be combined
            // into a single column, named Features 
            learningPipeline.Add(new ML.Transforms.ColumnConcatenator(outputColumn: "Features", "NumericalFeatures", "CategoryFeatures"));

            // Add the Learner to the pipeline. The Learner is the machine learning algorithm used to train a model
            // In this case, TweedieFastTree.TrainRegression was one of the best performing algorithms, but you can 
            // choose any other regression algorithm (StochasticDualCoordinateAscentRegressor,PoissonRegressor,...)
            learningPipeline.Add(new ML.Trainers.FastTreeTweedieRegressor { NumThreads = 1, FeatureColumn = "Features" });

            // Finally, we train the pipeline using the training dataset set at the first stage
            var model = learningPipeline.Train<CountryData, CountrySalesPrediction>();

            return model;
        }

        /// <summary>
        /// Predict samples using saved model
        /// </summary>
        /// <param name="outputModelPath">Model file path</param>
        /// <returns></returns>
        public static async Task TestPrediction(string outputModelPath = "country_month_fastTreeTweedie.zip")
        {
            Console.WriteLine("*********************************");
            Console.WriteLine("Testing country forecasting model");

            // Read the model that has been previously saved by the method SaveModel
            var model = await PredictionModel.ReadAsync<CountryData, CountrySalesPrediction>(outputModelPath);

            // Build sample data
            var dataSample = new CountryData()
            {
                country = "United Kingdom",
                month = 10,
                year = 2017,
                med = 323.4F,
                max = 616.96F,
                min = 145.9800F,
                std = 3041.14133F,
                prev = 930338.28F,
                count = 1705,
                sales = 1075435.72F,
            };
            // Predict sample data
            var prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (US$): {Math.Pow(6.207962F, 10)}, Forecasting (US$): {Math.Pow(prediction.Score,10)}");

            dataSample = new CountryData()
            {
                country = "United Kingdom", 
                month = 11,
                year = 2017,
                med = 301.04F,
                max = 515.272F,
                min = 139.42800F,
                std = 6580.22797F,
                prev = 1075435.72F,
                count = 2387,
                sales = 1614217.83F,
            };
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecasting (US$):  {Math.Pow(prediction.Score,10)}");

            dataSample = new CountryData()
            {
                country = "Germany",
                month = 10,
                year = 2017,
                med = 410.0349F,
                max = 912.8460F,
                min = 148.226F,
                std = 527.95676F,
                prev = 19553.63F,
                count = 58,
                sales = 33507.58F
            };
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month + 1}, year: {dataSample.year} - Real value (US$): {Math.Pow(4.483736F, 10)}, Forecasting (US$): {Math.Pow(prediction.Score,10)}");

            dataSample = new CountryData()
            {
                country = "Germany",
                month = 11,
                year = 2017,
                med = 353.755F,
                max = 668.076F,
                min = 123.2400F,
                std = 403.04297F,
                prev = 33507.58F,
                count = 68,
                sales = 30460.45F,
            };
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month + 1}, year: {dataSample.year} - Forecasting (US$):  {Math.Pow(prediction.Score,10)}");
        }
    }
}
