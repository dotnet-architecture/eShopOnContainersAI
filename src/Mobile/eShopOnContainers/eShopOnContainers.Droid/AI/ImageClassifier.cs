using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using eShopOnContainers.Core.AI.ProductSearchImageBased;

using Org.Tensorflow.Contrib.Android;

[assembly: Xamarin.Forms.Dependency(typeof(eShopOnContainers.Droid.AI.ImageClassifier))]
namespace eShopOnContainers.Droid.AI
{
    public sealed class TensorfloModelInput
    {
        public float[] Data { get; private set; }

        private TensorfloModelInput()
        {
        }

        public static TensorfloModelInput CreateFrom (byte[] image)
        {
            var data = PreProcessImage(BitmapFactory.DecodeByteArray(image, 0, image.Length));
            return new TensorfloModelInput() { Data = data };
        }

        static float[] PreProcessImage(Bitmap bitmap)
        {
            var floatValues = new float[227 * 227 * 3];

            using (var scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, 227, 227, false))
            {
                using (var resizedBitmap = scaledBitmap.Copy(Bitmap.Config.Argb8888, false))
                {
                    var intValues = new int[227 * 227];
                    resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        var val = intValues[i];

                        floatValues[i * 3 + 0] = (val & 0xFF);
                        floatValues[i * 3 + 1] = ((val >> 8) & 0xFF);
                        floatValues[i * 3 + 2] = ((val >> 16) & 0xFF);
                    }

                    resizedBitmap.Recycle();
                }

                scaledBitmap.Recycle();
            }

            return floatValues;
        }
    }

    public sealed class TensorflowModelOutput
    {
        public IDictionary<string, float> Loss { get; private set; }

        public static TensorflowModelOutput CreateTensorflowModelOutput(string[] labels, float[] loss)
        {
            var i = 0;
            var dict = labels.Select(l => new { lbl = l, loss = loss[i++] }).ToDictionary(t => t.lbl, t => t.loss);

            return new TensorflowModelOutput() { Loss = dict };
        }
    }

    public sealed class TensorflowModel
    {
        string[] labels;
        TensorFlowInferenceInterface inferenceInterface;

        static readonly string InputName =  "Placeholder";
        static readonly string OutputName = "loss";
        static readonly string ModelPath =  "ModelsAI/model.pb";
        static readonly string LabelsPath = "ModelsAI/labels.txt";
        static readonly int InputSize = 227;

        private TensorflowModel(string[] labels, TensorFlowInferenceInterface inferenceInterface)
        {
            this.labels = labels;
            this.inferenceInterface = inferenceInterface;
        }

        public static TensorflowModel CreateTensorflowModel ()
        {
            var assets = Application.Context.Assets;
            var inferenceInterface = new TensorFlowInferenceInterface(assets, ModelPath);
            IEnumerable<string> labels;

            using (var sr = new StreamReader(assets.Open(LabelsPath)))
            {
                var content = sr.ReadToEnd();
                labels = content.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

            return new TensorflowModel(labels.ToArray(), inferenceInterface);
        }

        public TensorflowModelOutput Evaluate(TensorfloModelInput input)
        {
            var outputNames = new[] { OutputName };
            var outputs = new float[labels.Count()];

            inferenceInterface.Feed(InputName, input.Data, 1, InputSize, InputSize, 3);
            inferenceInterface.Run(outputNames);
            inferenceInterface.Fetch(OutputName, outputs);

            return TensorflowModelOutput.CreateTensorflowModelOutput(labels, outputs);
        }
    }

    public class ImageClassifier : IImageClassifier
    {
        TensorflowModel tensorflowModel;

        private bool IsInitialized => tensorflowModel != null;

        public async Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image)
        {
            if (!IsInitialized)
                await Init();

            var input = TensorfloModelInput.CreateFrom(image);
            var results = tensorflowModel.Evaluate(input);

            return results.Loss
                .Select(p => new ImageClassification(p.Key, p.Value))
                .Where(p => p.Probability > 0.85)
                .ToList();
        }

        public Task Init()
        {
            tensorflowModel = TensorflowModel.CreateTensorflowModel();
            return Task.FromResult(1);
        }
    }
}