using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopOnContainers.Core.AI.ProductSearchImageBased
{
    public struct ImageClassification
    {
        public ImageClassification(string tag, float probability)
        {
            Tag = tag;
            Probability = probability;
        }

        public string Tag { get; }
        public float Probability { get; }

        public override string ToString() => $"Tag={Tag}, Probability={Probability:N2}";
    }

    public class ImageClassifierException : Exception
    {
        public ImageClassifierException(string message) : base(message) { }
        public ImageClassifierException(string message, Exception innerException) : base(message, innerException) { }
    }

    public enum ModelType
    {
        General,
        Landscape,
        Retail
    }

    public interface IImageClassifier
    {
        Task Init();
        Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image);
    }

    public static class CrossImageClassifier
    {
        static CrossImageClassifier()
        {
            Current = Xamarin.Forms.DependencyService.Get<IImageClassifier>();
        }

        public static IImageClassifier Current { get; private set; }
    }
}
