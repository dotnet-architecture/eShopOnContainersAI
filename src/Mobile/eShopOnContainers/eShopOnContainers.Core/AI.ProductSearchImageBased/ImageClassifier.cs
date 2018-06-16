using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    //public abstract class ImageClassifierBase : IImageClassifier
    //{
    //    protected string ModelName { get; private set; }
    //    protected ModelType ModelType { get; private set; }

    //    public virtual Task Init(string modelName, ModelType modelType)
    //    {
    //        if (string.IsNullOrEmpty(modelName))
    //            throw new ArgumentException("modelName must be set", nameof(modelName));

    //        ModelName = modelName;
    //        ModelType = modelType;

    //        return Task.FromResult(0);
    //    }

    //    public abstract Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image);
    //}

    public static class CrossImageClassifier
    {
        static CrossImageClassifier()
        {
            Current = Xamarin.Forms.DependencyService.Get<IImageClassifier>();
        }

        public static IImageClassifier Current { get; private set; }
    }
}
