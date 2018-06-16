using Microsoft.AspNetCore.Hosting;
using Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Classifier
{
    public class TensorFlowNeuralNetworkSettings
    {
        public string InputTensorName { get; internal set; }
        public string OutputTensorName { get; internal set; }
        public string ModelFilename { get; internal set; }
        public string LabelsFilename { get; internal set; }
        public float Threshold { get; internal set; }
        public int InputTensorWidth { get; internal set; }
        public int InputTensorHeight { get; internal set; }
        public int InputTensorChannels { get; internal set; }
    }

    public class CustomVisionOfflinePrediction : IClassifier
    {
        protected TensorFlowNeuralNetworkSettings modelSettings;

        public static readonly TensorFlowNeuralNetworkSettings Settings = new TensorFlowNeuralNetworkSettings()
        {
            InputTensorName = "Placeholder",
            OutputTensorName = "loss",
            InputTensorWidth = 227,
            InputTensorHeight = 227,
            InputTensorChannels = 3,
            ModelFilename = "model.pb",
            LabelsFilename = "labels.txt",
            Threshold = 0.85f
        };

        private readonly AppSettings settings;
        private readonly IHostingEnvironment environment;
        private readonly ILogger<CustomVisionOfflinePrediction> logger;

        public CustomVisionOfflinePrediction(IOptionsSnapshot<AppSettings> settings, IHostingEnvironment environment, ILogger<CustomVisionOfflinePrediction> logger)
        {
            this.settings = settings.Value;
            this.environment = environment;
            this.logger = logger;
            modelSettings = Settings;
        }

        /// <summary>
        /// Classifiy Image using Deep Neural Networks
        /// </summary>
        /// <param name="image">image (jpeg) file to be analyzed</param>
        /// <returns>labels related to the image</returns>
        public Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image)
        {
            // TODO: new Task
            //return Task.FromResult(Process(image, modelSettings));
            return Task.FromResult(Process(image, modelSettings));
        }

        private IEnumerable<LabelConfidence> Process(byte[] image, TensorFlowNeuralNetworkSettings settings)
        {
            var (model, labels) = LoadModelAndLabels(settings.ModelFilename, settings.LabelsFilename);
            var imageTensor = ImageUtil.CreateTensorFromImage(image);

            var result = Eval(model, imageTensor, settings.InputTensorName, settings.OutputTensorName, labels).ToArray();

            IEnumerable<LabelConfidence> labelsToReturn = result                            
                                    .Where(c => c.Probability >= settings.Threshold)
                                    .OrderByDescending(c => c.Probability);
            return labelsToReturn;
        }

        private (TFGraph, string[]) LoadModelAndLabels(string modelFilename, string labelsFilename)
        {
            const string EmptyGraphModelPrefix = "";

            var modelsFolder = Path.Combine(environment.ContentRootPath, settings.AIModelsPath);

            modelFilename = Path.Combine(modelsFolder, modelFilename);
            if (!File.Exists(modelFilename))
                throw new ArgumentException("Model file not exists", nameof(modelFilename));

            var model = new TFGraph();
            model.Import(File.ReadAllBytes(modelFilename), EmptyGraphModelPrefix);

            labelsFilename = Path.Combine(modelsFolder, labelsFilename);
            if (!File.Exists(labelsFilename))
                throw new ArgumentException("Labels file not exists", nameof(labelsFilename));

            var labels = File.ReadAllLines(labelsFilename);

            return (model, labels);
        }

        private IEnumerable<LabelConfidence> Eval(TFGraph graph, TFTensor imageTensor, string inputTensorName, string outputTensorName, string[] labels)
        {
            using (var session = new TFSession(graph))
            {
                var runner = session.GetRunner();

                // Create an input layer to feed (tensor) image, 
                // fetch label in output layer
                var input = graph[inputTensorName][0];

                var output = graph[outputTensorName][0];

                runner.AddInput(input, imageTensor)
                      .Fetch(output);

                var results = runner.Run();

                // convert output tensor in float array
                var probabilities = (float[,])results[0].GetValue(jagged: false);

                var idx = 0;
                return from label in labels
                       select new LabelConfidence { Label = label, Probability = probabilities[0,idx++] };
            }
        }
    }
}
