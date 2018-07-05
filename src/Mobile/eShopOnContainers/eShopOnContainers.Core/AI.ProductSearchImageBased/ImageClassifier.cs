using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopOnContainers.Core.AI.ProductSearchImageBased
{
    public interface IImageClassifier
    {
        Task Init();
        Task<IReadOnlyList<ImageClassification>> ClassifyImage(byte[] image);
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
