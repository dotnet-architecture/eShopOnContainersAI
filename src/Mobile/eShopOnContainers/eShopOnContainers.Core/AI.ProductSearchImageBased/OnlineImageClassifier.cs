using eShopOnContainers.Core.Helpers;
using eShopOnContainers.Core.Services.RequestProvider;
using System.Threading.Tasks;

namespace eShopOnContainers.Core.AI.ProductSearchImageBased
{
    public interface IOnlineImageClassifier
    {
        Task<string[]> ClassifyImage(byte[] image);
    }
    public class OnlineImageClassifier : IOnlineImageClassifier
    {
        enum ClassifierService { CognitiveServices , Tensorflow };
        private readonly ClassifierService CurrentClassifierService = ClassifierService.CognitiveServices;
        private readonly IRequestProvider requestProvider;
        private readonly string classifierServicePath;
        public OnlineImageClassifier(IRequestProvider requestProvider)
        {
            this.requestProvider = requestProvider;
            classifierServicePath = CurrentClassifierService == ClassifierService.CognitiveServices ? "/image-cognitive-api" : "/image-tensorflow-api";
        }
        public async Task<string[]> ClassifyImage(byte[] image)
        {
            var analyzeImageUri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{classifierServicePath}/v1/productSearchImage/classifyImage");

            var response = await requestProvider.PostFileAsync<string[]>(analyzeImageUri, image, "imageFile");
            return response;
        }
    }
}
