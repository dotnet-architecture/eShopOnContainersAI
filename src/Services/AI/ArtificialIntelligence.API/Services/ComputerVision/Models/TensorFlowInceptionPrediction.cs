using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using TensorFlow;

namespace ArtificialIntelligence.API.Services.ComputerVision.Models
{
    public class TensorFlowInceptionPrediction : TensorFlowPredictionBase
    {
        private readonly ArtificialIntelligenceSettings settings;
        private readonly IHostingEnvironment environment;

        public TensorFlowInceptionPrediction(IOptionsSnapshot<ArtificialIntelligenceSettings> settings, IHostingEnvironment environment)
        {
            this.settings = settings.Value;
            this.environment = environment;

            //TODO: move these settings to setting file
            modelSettings = new TensorFlowPredictionSettings()
            {
                InputTensorName = "input",
                OutputTensorName = "output",
                ModelFilename = "tensorflow_inception_graph.pb",
                LabelsFilename = "imagenet_comp_graph_label_strings.txt",
                Threshold = 0.3f
            };
        }

        protected override TFTensor LoadImage(byte[] image)
        {
            var tensor = TFTensor.CreateString(image);

            // Construct a graph to normalize the image
            var (graph, input, output) = PreprocessNeuralNetwork();

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
            DownloadIfModelNotExists(modelsFolder, modelFilename, labelsFilename);

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

        private void DownloadIfModelNotExists(string modelsFolder, string modelFile, string labelsFile)
        {
            if (!(File.Exists(Path.Combine(modelsFolder, modelFile)) && File.Exists(Path.Combine(modelsFolder, labelsFile))))
            {
                const string url = "https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip";
                var zipfile = Path.Combine(modelsFolder, "inception5h.zip");

                Directory.CreateDirectory(modelsFolder);
                var wc = new WebClient();
                wc.DownloadFile(url, zipfile);
                ZipFile.ExtractToDirectory(zipfile, modelsFolder);
                File.Delete(zipfile);
            }
        }

        // The inception model takes as input the image described by a Tensor in a very
        // specific normalized format (a particular image size, shape of the input tensor,
        // normalized pixel values etc.).
        //
        // This function constructs a graph of TensorFlow operations which takes as
        // input a JPEG-encoded string and returns a tensor suitable as input to the
        // inception model.
        private (TFGraph graph, TFOutput input, TFOutput output) PreprocessNeuralNetwork()
        {
            // Some constants specific to the pre-trained model at:
            // https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip
            //
            // - The model was trained after with images scaled to 224x224 pixels.
            // - The colors, represented as R, G, B in 1-byte each were converted to
            //   float using (value - Mean)/Scale.

            const int width = 224;
            const int height = 224;
            const float mean = 117;
            const float scale = 1;
            const TFDataType destinationDataType = TFDataType.Float;

            TFGraph graph = new TFGraph();
            TFOutput input = graph.Placeholder(TFDataType.String);

            var decodeJpeg = graph.DecodeJpeg(contents: input, channels: 3);
            var castToFloat = graph.Cast(decodeJpeg, DstT: TFDataType.Float);
            var expandDims = graph.ExpandDims(castToFloat, dim: graph.Const(0, "make_batch")); // shape: [1, height, width, channels]
            var resize = graph.ResizeBilinear(expandDims, size: graph.Const(new int[] { width, height }, "size"));
            var substractMean = graph.Sub(resize, y: graph.Const(mean, "mean"));
            var divByScale = graph.Div(substractMean, y: graph.Const(scale, "scale"));
            var output = graph.Cast(divByScale, destinationDataType); // cast to destination type

            return (graph, input, output);
        }
    }
}

