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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.MachineLearning;
using System.Threading.Tasks;

namespace eShopForecastModelsTrainer
{
    public class ProductModelHelper
    {
        private static TlcEnvironment tlcEnvironment = new TlcEnvironment(seed: 1);
        private static IPredictorModel model;

        public static void SaveModel(string dataPath, string outputModelPath = "product_month_fastTreeTweedle.zip")
        {
            if (File.Exists(outputModelPath))
            {
                File.Delete(outputModelPath);
            }

            using (var saveStream = File.OpenWrite(outputModelPath))
            {
                SaveProductModel(dataPath, saveStream);
            }
        }

        public static void SaveProductModel(string dataPath, Stream stream)
        {
            if (model == null)
            {
                model = CreateProductModelUsingExperiment(dataPath);
            }

            model.Save(tlcEnvironment, stream);
        }

        private static IPredictorModel CreateProductModelUsingExperiment(string dataPath)
        {
            var dataSchema = "col=Label:R4:0 col=productId:TX:1 col=year:R4:2 col=month:R4:3 col=units:R4:4 col=avg:R4:5 " +
                             "col=count:R4:6 col=max:R4:7 col=min:R4:8 col=prev:R4:9 col=price:R4:10 " +
                             "col=color:TX:11 col=size:TX:12 col=shape:TX:13 col=agram:TX:14 col=bgram:TX:15 col=ygram:TX:16 col=zgram:TX:17 " +
                             "header+ sep=,";

            TlcEnvironment tlcEnvironment = new TlcEnvironment(seed: 1);
            Experiment experiment = tlcEnvironment.CreateExperiment();

            var importData = new ImportText { CustomSchema = dataSchema };
            var imported = experiment.Add(importData);

            var numericalConcatenate = new ConcatColumns { Data = imported.Data };
            numericalConcatenate.AddColumn("NumericalFeatures",
                nameof(ProductData.year),
                nameof(ProductData.month),
                nameof(ProductData.units),
                nameof(ProductData.avg),
                nameof(ProductData.count),
                nameof(ProductData.max),
                nameof(ProductData.min),
                nameof(ProductData.prev),
                nameof(ProductData.price));
            var numericalConcatenated = experiment.Add(numericalConcatenate);

            var categoryConcatenate = new ConcatColumns { Data = numericalConcatenated.OutputData };
            categoryConcatenate.AddColumn("CategoryFeatures", 
                nameof(ProductData.productId),
                nameof(ProductData.color),
                nameof(ProductData.size),
                nameof(ProductData.shape),
                nameof(ProductData.agram),
                nameof(ProductData.bgram),
                nameof(ProductData.ygram),
                nameof(ProductData.zgram));
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
            experiment.SetInput(importData.InputFile, new SimpleFileHandle(tlcEnvironment, dataPath, false, false));
            experiment.Run();

            return experiment.GetOutput(combinedModels.PredictorModel);
        }

        public static async Task PredictSamples()
        {
            PredictionModel<ProductData, ProductUnitPrediction> model = await PredictionModel.ReadAsync<ProductData, ProductUnitPrediction>("product_month_fastTreeTweedle.zip");

            // 263/unitdemandestimation?month=10&year=2017&avg=91&max=370&min=1&count=10&prev=1675&units=910&price=1.79&color=&size=jumbo&shape=&agram=bag&bgram=&ygram=&zgram=owls
            ProductData dataSample = new ProductData()
            {
                productId = "263",
                month = 10,
                year = 2017,
                avg = 91,
                max = 370,
                min = 1,
                count = 10,
                prev = 1675,
                units = 910,
                price = 1.79F,
                size = "jumbo",
                agram = "bag",
                zgram = "owls"
            };

            ProductUnitPrediction prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Real value: 551, Forecasting: {prediction.Score}");

            // 263/unitdemandestimation?month=11&year=2017&avg=29&max=221&min=1&count=19&prev=910&units=551&price=1.79&color=&size=jumbo&shape=&agram=bag&bgram=&ygram=&zgram=owls
            dataSample = new ProductData()
            {
                productId = "263",
                month = 11,
                year = 2017,
                avg = 29,
                max = 221,
                min = 1,
                count = 19,
                prev = 910,
                units = 551,
                price = 1.79F,
                size = "jumbo",
                agram = "bag",
                zgram = "owls"
            };

            prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Forecasting: {prediction.Score}");

            // 988/unitdemandestimation?month=10&year=2017&avg=43&max=220&min=1&count=25&prev=1036&units=1094&price=1.79&color=&size=jumbo&shape=&agram=storage&bgram=bag&ygram=&zgram=suki
            dataSample = new ProductData()
            {
                productId = "988",
                month = 10,
                year = 2017,
                avg = 43,
                max = 220,
                min = 1,
                count = 25,
                prev = 1036,
                units = 1094,
                price = 1.79F,
                size = "jumbo",
                agram = "storage",
                bgram = "bag",
                zgram = "suki"
            };

            prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Real Value: 1076, Forecasting: {prediction.Score}");

            // 988/unitdemandestimation?month=11&year=2017&avg=41&max=225&min=4&count=26&prev=1094&units=1076&price=1.79&color=&size=jumbo&shape=&agram=storage&bgram=bag&ygram=&zgram=suki
            dataSample = new ProductData()
            {
                productId = "988",
                month = 11,
                year = 2017,
                avg = 41,
                max = 225,
                min = 4,
                count = 26,
                prev = 1094,
                units = 1076,
                price = 1.79F,
                size = "jumbo",
                agram = "storage",
                bgram = "bag",
                zgram = "suki"
            };

            prediction = model.Predict(dataSample);
            Console.WriteLine($"Product: {dataSample.productId}, month: {dataSample.month+1}, year: {dataSample.year} - Forecasting: {prediction.Score}");
        }
    }
}
