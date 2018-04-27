using Microsoft.MachineLearning.Runtime;
using Microsoft.MachineLearning.Runtime.Api.Experiment;
using Microsoft.MachineLearning.Runtime.Api.Experiment.Categorical;
using Microsoft.MachineLearning.Runtime.Api.Experiment.TweedieFastTree;
using Microsoft.MachineLearning.Runtime.Api.Experiment.ImportTextData;
using Microsoft.MachineLearning.Runtime.Api.Experiment.ModelOperations;
using Microsoft.MachineLearning.Runtime.Api.Experiment.SchemaManipulation;
using Microsoft.MachineLearning.Runtime.Data;
using Microsoft.MachineLearning.Runtime.EntryPoints;
using System;
using System.IO;
using Microsoft.MachineLearning;
using System.Threading.Tasks;

namespace eShopForecastModelsTrainer
{
    public class CountryModelHelper
    {
        private static TlcEnvironment tlcEnvironment = new TlcEnvironment(seed: 1);
        private static IPredictorModel model;

        public static void SaveModel(string dataPath, string outputModelPath = "country_month_fastTreeTweedle.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            using (var saveStream = File.OpenWrite(outputModelPath))
            {
                SaveCountryModel(dataPath, saveStream);
            }
        }

        public static void SaveCountryModel(string dataPath, Stream stream)
        {
            if (model == null)
            {
                model = CreateCountryModelUsingExperiment(dataPath);
            }

            model.Save(tlcEnvironment, stream);
        }

        private static IPredictorModel CreateCountryModelUsingExperiment(string dataPath)
        {
            var dataSchema = "col=Label:R4:0 col=country:TX:1 col=year:R4:2 col=month:R4:3 col=sales:R4:4 col=avg:R4:5 " +
                             "col=count:R4:6 col=max:R4:7 col=min:R4:8 col=p_max:R4:9 col=p_med:R4:10 col=p_min:R4:11 " +
                             "col=std:R4:12 col=prev:R4:13 " +
                             "header+ sep=,";

            TlcEnvironment tlcEnvironment = new TlcEnvironment(seed: 1);
            Experiment experiment = tlcEnvironment.CreateExperiment();

            var importData = new ImportText { CustomSchema = dataSchema };
            var imported = experiment.Add(importData);

            var numericalConcatenate = new ConcatColumns { Data = imported.Data };
            numericalConcatenate.AddColumn("NumericalFeatures",
                nameof(CountryData.year),
                nameof(CountryData.month),
                nameof(CountryData.sales),
                //nameof(CountryData.avg),
                nameof(CountryData.count),
                //nameof(CountryData.max),
                //nameof(CountryData.min),
                nameof(CountryData.p_max),
                nameof(CountryData.p_med),
                nameof(CountryData.p_min),
                nameof(CountryData.std),
                nameof(CountryData.prev));
            var numericalConcatenated = experiment.Add(numericalConcatenate);

            var categoryConcatenate = new ConcatColumns { Data = numericalConcatenated.OutputData };
            categoryConcatenate.AddColumn("CategoryFeatures", nameof(CountryData.country));
            var categoryConcatenated = experiment.Add(categoryConcatenate);

            var categorize = new CatTransformDict { Data = categoryConcatenated.OutputData };
            categorize.AddColumn("CategoryFeatures");
            var categorized = experiment.Add(categorize);

            var featuresConcatenate = new ConcatColumns { Data = categorized.OutputData };
            featuresConcatenate.AddColumn("Features", "NumericalFeatures", "CategoryFeatures");
            var featuresConcatenated = experiment.Add(featuresConcatenate);

            var learner = new TrainRegression { TrainingData = featuresConcatenated.OutputData, NumThreads = 1 };
            var learnerOutput = experiment.Add(learner);

            var combineModels = new CombineModels
            {
                TransformModels = new ArrayVar<ITransformModel>(numericalConcatenated.Model, categoryConcatenated.Model, categorized.Model, featuresConcatenated.Model),
                PredictorModel = learnerOutput.PredictorModel
            };
            var combinedModels = experiment.Add(combineModels);

            experiment.Compile();
            experiment.SetInput(importData.InputFile, new SimpleFileHandle(tlcEnvironment, "data/countries.stats.csv", false, false));
            experiment.Run();

            return experiment.GetOutput(combinedModels.PredictorModel);
        }

        public static async Task PredictSamples()
        {
            PredictionModel<CountryData, CountrySalesPrediction> model = await PredictionModel.ReadAsync<CountryData, CountrySalesPrediction>("country_month_fastTreeTweedle.zip");

            //month=10&year=2017&avg=506.73602&p_max=587.902&p_med=309.945&p_min=135.64000000000001&max=25035&min=0.38&prev=856548.78&count=1724&std=1063.9320923325279&sales=873612.9
            CountryData dataSample = new CountryData()
            {
                country = "United Kingdom",
                month = 10,
                year = 2017,
                avg = 506.73602F,
                p_max = 587.902F,
                p_med = 309.945F,
                p_min = 135.64000000000001F,
                max = 25035,
                min = 0.38F,
                prev = 856548.78F,
                count = 1724F,
                std = 1063.9320923325279F,
                sales = 873612.9F
            };
            CountrySalesPrediction prediction = model.Predict(dataSample);

            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month+1}, year: {dataSample.year} - Real value: 6.0084501, Forecasting: {prediction.Score}");

            //country/United%20Kingdom/salesforecast?month=11,year=2017&avg=427.167017&p_max=501.48800000000017&p_med=288.72&p_min=134.53600000000003&max=11351.51&min=0.42&prev=873612.9&count=2387&std=707.5642048503361&sales=1019647.67
            dataSample = new CountryData()
            {
                country = "United Kingdom",
                month = 11,
                year = 2017,
                avg = 427.167017F,
                p_max = 501.48800000000017F,
                p_med = 288.72F,
                p_min = 134.53600000000003F,
                max = 11351.51F,
                min = 0.42F,
                prev = 873612.9F,
                count = 2387,
                std = 707.5642048503361F,
                sales = 1019647.67F
            };
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month+1}, year: {dataSample.year} - Forecasting: {prediction.Score}");

            //country/United%20States/salesforecast?month=10&year=2017&avg=532.256&p_max=573.6299999999998&p_med=400.17&p_min=340.39599999999996&max=1463.87&min=281.66&prev=4264.94&count=10&std=338.2866742039953&sales=5322.56
            dataSample = new CountryData()
            {
                country = "United States",
                month = 10,
                year = 2017,
                avg = 532.256F,
                p_max = 573.6299999999998F,
                p_med = 400.17F,
                p_min = 340.39599999999996F,
                max = 1463.87F,
                min = 281.66F,
                prev = 4264.94F,
                count = 10,
                std = 338.2866742039953F,
                sales = 5322.56F
            };
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month+1}, year: {dataSample.year} - Real value: 3.8057699,  Forecasting: {prediction.Score}");

            //country/United%20States/salesforecast?month=11&year=2017&avg=581.26909&p_max=1135.99&p_med=317.9&p_min=249.44&max=1252.57&min=171.6&prev=5322.56&count=11&std=409.75528400729723&sales=6393.96
            dataSample = new CountryData()
            {
                country = "United States",
                month = 11,
                year = 2017,
                avg = 581.26909F,
                p_max = 1135.99F,
                p_med = 317.9F,
                p_min = 249.44F,
                max = 1252.57F,
                min = 171.6F,
                prev = 5322.56F,
                count = 11,
                std = 409.75528400729723F,
                sales = 6393.96F
            };
            prediction = model.Predict(dataSample);
            Console.WriteLine($"Country: {dataSample.country}, month: {dataSample.month+1}, year: {dataSample.year} - Forecasting: {prediction.Score}");
        }
    }
}
