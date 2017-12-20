using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using TensorFlow;

namespace ArtificialIntelligence.API.Services
{
    public class TensorFlowModelPrediction : TensorFlowPredictionBase
    {
        private readonly ArtificialIntelligenceSettings settings;
        private readonly IHostingEnvironment environment;

        public TensorFlowModelPrediction(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IHostingEnvironment environment)
        {
            this.settings = settings.Value;
            this.environment = environment;

            //TODO: move these settings to setting file
            modelSettings = new TensorFlowPredictionSettings()
            {
                InputTensorName = "input_1",
                OutputTensorName = "dense_2/Softmax",
                ModelFilename = "model.pb",
                LabelsFilename = "labels.txt",
                Threshold = 0.9f
            };
        }

        protected override TFTensor LoadImage(byte[] image)
        {
            var tensor = TFTensor.CreateString(image);

            // Construct a graph to normalize the image
            // TODO: move width & height to settings
            var (graph, input, output) = PreprocessNeuralNetwork(224, 224);

            // Execute that graph to normalize this one image
            using (var session = new TFSession(graph))
            {
                var normalized = session.Run(
                         inputs: new[] { input },
                         inputValues: new[] { tensor },
                         outputs: new[] { output });

                return normalized[0];
            }
        }

        protected override (TFGraph, string[]) LoadModelAndLabels(string modelFilename, string labelsFilename)
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

        // The inception model takes as input the image described by a Tensor in a very
        // specific normalized format (a particular image size, shape of the input tensor,
        // normalized pixel values etc.).
        //
        // This function constructs a graph of TensorFlow operations which takes as
        // input a JPEG-encoded string and returns a tensor suitable as input to the
        // inception model.
        private (TFGraph graph, TFOutput input, TFOutput output) PreprocessNeuralNetwork(int width, int height)
        {
            // Custom model has been processed using Keras
            // which uses its own algorithm for normalizing images
            // RGB values are normalized between -1 and 1
            // image shape can be customized in keras, 
            // so width and height are converted to method arguments
            const float maxValue = 255f;
            const float mean = 2 / maxValue;
            const TFDataType destinationDataType = TFDataType.Float;

            TFGraph graph = new TFGraph();
            TFOutput input = graph.Placeholder(TFDataType.String);

            var decodeJpeg = graph.DecodeJpeg(contents: input, channels: 3);
            var castToFloat = graph.Cast(decodeJpeg, DstT: TFDataType.Float);
            var expandDims = graph.ExpandDims(castToFloat, dim: graph.Const(0, "make_batch")); // shape: [1, height, width, channels]
            var resize = graph.ResizeBilinear(expandDims, size: graph.Const(new int[] { width, height }, "size"));
            var substract = graph.Sub(resize, y: graph.Const(maxValue, "sub"));
            var multiply = graph.Mul(substract, y: graph.Const(mean, TFDataType.Float, "multiply"));
            var add = graph.Add(multiply, y: graph.Const(1f, "add"));
            var castToDestinationDataType = graph.Cast(add, destinationDataType);

            return (graph, input, castToDestinationDataType);
        }
    }
}

