using eShopOnContainers.Core.AI.ProductSearchImageBased;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning.Preview;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;

[assembly: Xamarin.Forms.Dependency(typeof(eShopOnContainers.Windows.AI.ImageClassifier))]
namespace eShopOnContainers.Windows.AI
{
    public sealed class OnnxModelInput
    {
        public static async Task<OnnxModelInput> CreateFrom(byte[] image)
        {
            using (var ms = new MemoryStream(image))
            {
                // Create the decoder from the stream 
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(ms.AsRandomAccessStream());

                // Get the SoftwareBitmap representation of the file in BGRA8 format
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                return new OnnxModelInput() { data = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap) };
            }
        }

        public VideoFrame data { get; private set; }
    }

    public sealed class OnnxModelOutput
    {
        public IList<string> classLabel { get; internal set; }
        public IDictionary<string, float> Loss { get; internal set; }
        public OnnxModelOutput()
        {
            this.classLabel = new List<string>();

            // For dictionary(map) fields onnx needs the variable to be pre-allocatd such that the 
            // length is equal to the number of labels defined in the model. The names are not
            // required to match what is in the model.
            this.Loss = new Dictionary<string, float>();
            for (int x = 0; x < 5; ++x)
                this.Loss.Add("Label_" + x.ToString(), 0.0f);
        }
    }

    public sealed class OnnxModel
    {
        private LearningModelPreview learningModel = null;

        public static async Task<OnnxModel> CreateOnnxModel()
        {
            var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets//ModelsAI//model.onnx"));
            return (await OnnxModel.CreateOnnxModel(modelFile));
        }

        public static async Task<OnnxModel> CreateOnnxModel(StorageFile file)
        {
            LearningModelPreview learningModel = null;

            try
            {
                learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            }
            catch (Exception e)
            {
                var exceptionStr = e.ToString();
                System.Console.WriteLine(exceptionStr);
                throw e;
            }
            OnnxModel model = new OnnxModel();
            learningModel.InferencingOptions.PreferredDeviceKind = LearningModelDeviceKindPreview.LearningDeviceAny;
            learningModel.InferencingOptions.ReclaimMemoryAfterEvaluation = true;

            model.learningModel = learningModel;
            return model;
        }

        public async Task<OnnxModelOutput> EvaluateAsync(OnnxModelInput input)
        {
            OnnxModelOutput output = new OnnxModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.Loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }

    public class ImageClassifier : IImageClassifier
    {
        OnnxModel onnxmodel;

        private bool IsInitialized => onnxmodel != null;

        public async Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image)
        {
            if (!IsInitialized)
                await Init();

            var input = await OnnxModelInput.CreateFrom(image);
            var results = await onnxmodel.EvaluateAsync(input);

            return results.Loss
                .Where(label => label.Value > 0.85)
                .Select(label => new ImageClassification(label.Key, label.Value)).ToList();
        }

        public async Task Init()
        {
            onnxmodel = await OnnxModel.CreateOnnxModel();
        }
    }
}
