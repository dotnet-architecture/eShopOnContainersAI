using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopOnContainers.Core.AI.ProductSearchImageBased
{
    public interface IImageClassifier
    {
        Task Init();
        Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image);
    }

    public class ImageClassifierException : Exception
    {
        public ImageClassifierException(string message) : base(message) { }
        public ImageClassifierException(string message, Exception innerException) : base(message, innerException) { }
    }
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
}
