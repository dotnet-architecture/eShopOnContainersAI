using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TensorFlow;

namespace ArtificialIntelligence.API.Services
{
    public interface IPrediction
    {
        IEnumerable<string> AnalyzeImageWithTensor(byte[] image);
    }

    public class Prediction : IPrediction
    {
        private readonly ArtificialIntelligenceSettings settings;
        private readonly IHostingEnvironment environment;

        public class TagConfidence
        {
            public int Confidence { get; set; }
            public string Tag { get; set; }
        }

        public Prediction(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IHostingEnvironment environment)
        {
            this.settings = settings.Value;
            this.environment = environment;
        }

        public IEnumerable<string> AnalyzeImageWithTensor(byte[] image)
        {
            return AnalyzeImageWithInceptionModel(image);
        }

        protected IEnumerable<string> AnalyzeImageWithCustomInceptionModel(byte[] image)
        {
            var (modelFile, labelsFile) = TensorFlowSharpHelpers.GetCustomInceptionModel(Path.Combine(environment.ContentRootPath, settings.AIModelsPath));
            const string inputTensorName = "input_1:0", outputTensorName = "dense_2/Softmax:0";

            return DoAnalyzeImageWithTensor(image, modelFile, labelsFile, inputTensorName, outputTensorName);
        }

        protected IEnumerable<string> AnalyzeImageWithInceptionModel(byte[] image)
        {
            var (modelFile, labelsFile) = TensorFlowSharpHelpers.GetOrDownloadInceptionModel(Path.Combine(environment.ContentRootPath, settings.AIModelsPath));
            const string inputTensorName = "input", outputTensorName = "output";

            return DoAnalyzeImageWithTensor(image, modelFile, labelsFile, inputTensorName, outputTensorName);
        }

        private IEnumerable<string> DoAnalyzeImageWithTensor(byte[] image, string modelFile, string labelsFile, string inputTensorName, string outputTensorName)
        {
            var tensor = TensorFlowSharpHelpers.CreateTensorFromImageFile(image);

            // Construct an in-memory graph from the serialized form.
            var graph = new TFGraph();
            // Load the serialized GraphDef from a file.
            var model = File.ReadAllBytes(modelFile);
            var labels = File.ReadAllLines(labelsFile);

            graph.Import(model, "");

            using (var session = new TFSession(graph))
            {
                var runner = session.GetRunner();

                // Create an input layer to feed (tensor) image, 
                // fetch label in output layer
                var input = graph[inputTensorName][0];
                var output = graph[outputTensorName][0];
                runner.AddInput(input, tensor)
                      .Fetch(output);

                var results = runner.Run();

                // output[0].Value() is a vector containing probabilities of
                // labels for each image in the "batch". The batch size was 1.
                // Find the most probably label index.

                var result = results[0];
                var rshape = result.Shape;
                if (result.NumDims != 2 || rshape[0] != 1)
                {
                    var shape = String.Join(' ', rshape.Select(d => d.ToString())).Trim();
                    Console.WriteLine($"Error: expected to produce a [1 N] shaped tensor where N is the number of labels, instead it produced one with shape [{shape}]");
                    //Environment.Exit(1);
                }

                // You can get the data in two ways, as a multi-dimensional array, or arrays of arrays, 
                // code can be nicer to read with one or the other, pick it based on how you want to process it

                var bestIdx = 0;
                float best = 0;
                const double threshold = 0.3;

                var val = (float[,])result.GetValue(jagged: false);

                // Result is [1,N], flatten array
                for (int i = 0; i < val.GetLength(1); i++)
                {
                    if (val[0, i] > best)
                    {
                        bestIdx = i;
                        best = val[0, i];
                    }
                    if (val[0, i] > threshold)
                        yield return labels[i];
                }

                yield return labels[bestIdx];
            }
        }
    }
}

