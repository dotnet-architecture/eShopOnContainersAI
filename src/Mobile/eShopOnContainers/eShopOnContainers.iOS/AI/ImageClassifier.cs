using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using CoreML;
using eShopOnContainers.Core.AI.ProductSearchImageBased;
using Foundation;
using UIKit;
using Vision;

[assembly: Xamarin.Forms.Dependency(typeof(eShopOnContainers.iOS.AI.ImageClassifier))]
namespace eShopOnContainers.iOS.AI
{
    public class CoreMlInput
    {
        public static CoreMlInput CreateFrom(byte[] image)
        {
            return new CoreMlInput() { Image = UIImage.LoadFromData(NSData.FromArray(image))};
        }

        private CoreMlInput() { }

        public UIImage Image { get; private set; }
    }

    public class CoreMlModel
    {
        private static readonly CGSize _targetImageSize = new CGSize(227, 227);
        private static readonly string customVisionModelFileName = "customvision";
        private static readonly string customVisionModelExtension = "mlmodel";
        private static readonly string customVisionModelFolderName = "ModelsAI";
        private readonly VNCoreMLModel _model;

        private CoreMlModel(VNCoreMLModel model)
        {
            this._model = model;
        }

        public static CoreMlModel CreateCoreMlModel()
        {
            return new CoreMlModel(LoadModel(customVisionModelFileName));
        }

        private static VNCoreMLModel LoadModel(string modelName)
        {
            var modelPath = CompileModel(modelName);

            if (modelPath == null)
                throw new ImageClassifierException($"Model {modelName} does not exist");

            var mlModel = MLModel.Create(modelPath, out NSError err);

            if (err != null)
                throw new NSErrorException(err);

            var model = VNCoreMLModel.FromMLModel(mlModel, out err);

            if (err != null)
                throw new NSErrorException(err);

            return model;
        }

        private static NSUrl CompileModel(string modelName)
        {
            var uncompiled = NSBundle.MainBundle.GetUrlForResource(modelName, customVisionModelExtension, customVisionModelFolderName);
            var modelPath = MLModel.CompileModel(uncompiled, out NSError err);

            if (err != null)
                throw new NSErrorException(err);

            return modelPath;
        }

        public async Task<IEnumerable<ImageClassification>> Evalute(CoreMlInput source)
        {
            var tcs = new TaskCompletionSource<IEnumerable<ImageClassification>>();

            var request = new VNCoreMLRequest(_model, (response, e) =>
            {
                if (e != null)
                    tcs.SetException(new NSErrorException(e));
                else
                {
                    var results = response.GetResults<VNClassificationObservation>();
                    tcs.SetResult(results.Select(r => new ImageClassification(r.Identifier, r.Confidence)).ToList());
                }
            });

            // Pre-process image (scale down)
            var buffer = source.Image.ToCVPixelBuffer(_targetImageSize);

            var requestHandler = new VNImageRequestHandler(buffer, new NSDictionary());

            requestHandler.Perform(new[] { request }, out NSError error);

            var classifications = await tcs.Task;

            if (error != null)
                throw new NSErrorException(error);

            return classifications;
        }
    }

    public class ImageClassifier : IImageClassifier
    {
        private bool IsInitialized => coreMlModel != null;
        CoreMlModel coreMlModel;

        public async Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image)
        {
            if (!IsInitialized)
                await Init();

            try
            {
                var input = CoreMlInput.CreateFrom(image);
                var results = await coreMlModel.Evalute(input);

                return results
                    .Where(p => p.Probability > 0.85)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to classify image - check the inner exception for more details", ex);
            }
        }

        public Task Init()
        {
            try
            {
                coreMlModel = CoreMlModel.CreateCoreMlModel();
            }
            catch (Exception ex)
            {
                throw new ImageClassifierException("Failed to load the model - check the inner exception for more details", ex);
            }
            return Task.FromResult(1);
        }
    }
}