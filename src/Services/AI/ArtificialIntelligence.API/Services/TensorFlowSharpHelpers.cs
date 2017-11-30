using System.IO;
using System.IO.Compression;
using System.Net;
using TensorFlow;

namespace ArtificialIntelligence.API.Services
{
    public static class TensorFlowSharpHelpers
    {
        public static TFTensor CreateTensorFromImageFile(string file, TFDataType destinationDataType = TFDataType.Float)
        {
            return CreateTensorFromImageFile(File.ReadAllBytes(file), destinationDataType);
        }

        public static TFTensor CreateTensorFromImageFile(byte[] contents, TFDataType destinationDataType = TFDataType.Float)
        {
            // DecodeJpeg uses a scalar String-valued tensor as input.
            var tensor = TFTensor.CreateString(contents);

            // Construct a graph to normalize the image
            var (graph, input, output) = NormalizeImageForInceptionModel(destinationDataType);

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

        // The inception model takes as input the image described by a Tensor in a very
        // specific normalized format (a particular image size, shape of the input tensor,
        // normalized pixel values etc.).
        //
        // This function constructs a graph of TensorFlow operations which takes as
        // input a JPEG-encoded string and returns a tensor suitable as input to the
        // inception model.
        private static (TFGraph graph, TFOutput input, TFOutput output) NormalizeImageForInceptionModel(TFDataType destinationDataType = TFDataType.Float)
        {
            // Some constants specific to the pre-trained model at:
            // https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip
            //
            // - The model was trained after with images scaled to 224x224 pixels.
            // - The colors, represented as R, G, B in 1-byte each were converted to
            //   float using (value - Mean)/Scale.

            const int W = 224;
            const int H = 224;
            const float Mean = 117;
            const float Scale = 1;

            TFGraph graph = new TFGraph();
            TFOutput input = graph.Placeholder(TFDataType.String);

            TFOutput output = graph.Cast(graph.Div(
                x: graph.Sub(
                    x: graph.ResizeBilinear(
                        images: graph.ExpandDims(
                            input: graph.Cast(
                                graph.DecodeJpeg(contents: input, channels: 3), DstT: TFDataType.Float),
                            dim: graph.Const(0, "make_batch")),
                        size: graph.Const(new int[] { W, H }, "size")),
                    y: graph.Const(Mean, "mean")),
                y: graph.Const(Scale, "scale")), destinationDataType);

            return (graph, input, output);
        }

        //
        // Downloads the inception graph and labels
        //
        public static (string modelFile, string labelsFile) GetOrDownloadInceptionModel(string dir)
        {
            var modelFile = Path.Combine(dir, "tensorflow_inception_graph.pb");
            var labelsFile = Path.Combine(dir, "imagenet_comp_graph_label_strings.txt");

            if (!(File.Exists(modelFile) && File.Exists(labelsFile)))
            {
                const string url = "https://storage.googleapis.com/download.tensorflow.org/models/inception5h.zip";
                var zipfile = Path.Combine(dir, "inception5h.zip");

                Directory.CreateDirectory(dir);
                var wc = new WebClient();
                wc.DownloadFile(url, zipfile);
                ZipFile.ExtractToDirectory(zipfile, dir);
                File.Delete(zipfile);
            }

            return (modelFile, labelsFile);
        }

        public static (string modelFile, string labelsFile) GetCustomInceptionModel(string dir)
        {
            var modelFile = Path.Combine(dir, "model.pb");
            var labelsFile = Path.Combine(dir, "labels.txt");

            return File.Exists(modelFile) && File.Exists(labelsFile) ? 
                (modelFile, labelsFile) : 
                ((string)null,(string)null);
        }
    }

}

