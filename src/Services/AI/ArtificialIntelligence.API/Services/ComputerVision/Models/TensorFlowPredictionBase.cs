using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace ArtificialIntelligence.API.Services.ComputerVision.Models
{
    public interface IClassifier
    {
        Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image);
    }

    public class LabelConfidence
    {
        public float Probability { get; set; }
        public string Label { get; set; }
    }

    public class TensorFlowPredictionSettings
    {
        public string InputTensorName { get; set; }
        public string OutputTensorName { get; set; }
        public string ModelFilename { get; set; }
        public string LabelsFilename { get; set; }
        public float Threshold { get; set; }
    }

    public abstract class TensorFlowPredictionBase : IClassifier
    {
        protected TensorFlowPredictionSettings modelSettings;

        /// <summary>
        /// Classifiy Image using Deep Neural Networks
        /// </summary>
        /// <param name="image">image (jpeg) file to be analyzed</param>
        /// <returns>labels related to the image</returns>
        public Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image)
        {
            // TODO: new Task
            return Task.FromResult(Process(image, modelSettings));
        }

        protected IEnumerable<LabelConfidence> Process(byte[] image, TensorFlowPredictionSettings settings)
        {
            var (model, labels) = LoadModelAndLabels(settings.ModelFilename, settings.LabelsFilename);
            var imageTensor = LoadImage(image);
            return Eval(model, imageTensor, settings.InputTensorName, settings.OutputTensorName, labels)
                    .Where(c => c.Probability >= settings.Threshold)
                    .OrderByDescending(c => c.Probability);
        }

        protected abstract (TFGraph, string[]) LoadModelAndLabels(string modelFilename, string labelsFilename);
        protected abstract TFTensor LoadImage(byte[] image);
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
                       select new LabelConfidence { Label = label, Probability = probabilities[0, idx++] };
            }
        }
    }
}

