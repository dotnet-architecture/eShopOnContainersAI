using CNTK;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Classifier
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

    static class CNTKExtension
    {
        /// <summary>
        /// Launches a task that performs evaluation on the computation graph defined by 'function', using provided 'input'
        /// and stores the results in the 'outputs' map.
        /// It is implemented as an extension method of the class Function.
        /// </summary>
        /// <param name="function"> The function representing the computation graph on which the evaluation is executed.</param>
        /// <param name="inputs"> The map represents input variables and their values.</param>
        /// <param name="outputs"> The map defines output variables. On return, the results are stored in Values of the map.</param>
        /// <param name="computeDevice">T he device on which the computation is executed.</param>
        /// <returns> The task representing the asynchronous operation for the evaluation.</returns>
        public static Task EvaluateAsync(this Function function, IDictionary<Variable, Value> inputs, IDictionary<Variable, Value> outputs, DeviceDescriptor computeDevice)
        {
            return Task.Run(() => function.Evaluate(inputs, outputs, computeDevice));
        }
    }

    public class CNTKModelPrediction
    {
        public static readonly DeviceDescriptor CPUDeviceDescriptor = DeviceDescriptor.CPUDevice;
        private readonly CNTKModelPredictionResources _CNTKModelPredictionResources;

        public CNTKModelPrediction(CNTKModelPredictionResources _CNTKModelPredictionResources)
        {
            this._CNTKModelPredictionResources = _CNTKModelPredictionResources;
        }

        /// <summary>
        /// Classifiy Image using Deep Neural Networks
        /// </summary>
        /// <param name="image">image (jpeg) file to be analyzed</param>
        /// <returns>labels related to the image</returns>
        public Task<IEnumerable<LabelConfidence>> ClassifyImageAsync(byte[] image)
        {
            return ProcessAsync(image, _CNTKModelPredictionResources.Settings);
        }

        private async Task<IEnumerable<LabelConfidence>> ProcessAsync(byte[] image, CNTKModelPredictionSettings settings)
        {
            var labels = _CNTKModelPredictionResources.Labels;
            var model = _CNTKModelPredictionResources.Model;

            var input = model.Arguments.Single();
            var output = model.Output;

            var imageTensor = LoadImageTensor(image, input);
            var modelEval = await EvalAsync(model, imageTensor, input, output, labels);
            return modelEval.Where(c => c.Probability >= settings.Threshold)
                .OrderByDescending(c => c.Probability);
        }

        private Tensor<float> LoadImageTensor(byte[] image, Variable input)
        {
            using (var imageStream = new MemoryStream(image))
            {
                return ConvertImageToTensorData((Bitmap)Image.FromStream(imageStream), input);
            }
        }

        private Tensor<float> ConvertImageToTensorData(Bitmap image, Variable input)
        {
            int channels = input.Shape.Dimensions[0];
            int width = input.Shape.Dimensions[1];
            int height = input.Shape.Dimensions[2];

            image = ResizeImage(image, new Size(width, height));

            return NormalizeTensorImage(CopyImageToTensor(image, width, height, channels));
        }

        private static Tensor<float> NormalizeTensorImage(Tensor<float> imageData)
        {
            imageData /= 255f;
            imageData -= .5f;
            imageData *= 2f;
            return imageData;
        }

        private static Tensor<float> CopyImageToTensor(Bitmap image, int width, int height, int channels)
        {
            Tensor<float> imageData = new DenseTensor<float>(new[] { width, height, channels }, false); // false: row-major; true: column-major; CNTK uses ColumnMajor layout

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = image.GetPixel(x, y);
                    imageData[x, y, 0] = color.R;
                    imageData[x, y, 1] = color.G;
                    imageData[x, y, 2] = color.B;
                }
            }

            return imageData;
        }

        private Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            Bitmap b = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            }
            return b;
        }

        public static async Task<IEnumerable<LabelConfidence>> EvalAsync(Function modelFunction, Tensor<float> imageData, Variable input, Variable output, string[] labels)
        {
            try
            {
                var inputDataMap = new Dictionary<Variable, Value>();
                var outputDataMap = new Dictionary<Variable, Value>();

                // Create input data map
                Value inputVal = Value.CreateBatch(input.Shape, imageData, CPUDeviceDescriptor);
                inputDataMap.Add(input, inputVal);

                // Create output data map
                outputDataMap.Add(output, null);

                // Start evaluation on the device
                await modelFunction.EvaluateAsync(inputDataMap, outputDataMap, CPUDeviceDescriptor);

                // Get evaluate result as dense output
                Value outputVal = outputDataMap[output];

                // The model has only one single output - a list of 10 floats
                // representing the likelihood of that index being the digit
                var probabilities = outputVal.GetDenseData<float>(output).Single().ToArray();

                var idx = 0;
                return (from label in labels
                       select new LabelConfidence { Label = label, Probability = probabilities[idx++] }).ToArray();
            }
            catch //(Exception ex)
            {
                //Debug.WriteLine(ex.ToString());
                return Enumerable.Empty<LabelConfidence>();
            }
        }

    }
}
