using System;

namespace Microsoft.eShopOnContainers.Bot.API
{
    public class AppSettings
    {
        public string PurchaseUrl { get; set; }
        public string ArtificialIntelligenceUrl { get; set; }
        public string IdentityUrl { get; set; }
        public string ImageUrl { get; set; }
        public string AuthenticationConnectionName { get; set; }

        public ProductSearchImageBasedSchema ProductSearchImageBased { get; set; }

        private const string CognitivePath = "/image-cognitive-api";
        private const string TensorflowPath = "/image-tensorflow-api";

        public string ProductSearchImagePath
        {
            get
            {
                if (ProductSearchImageBased == null || string.IsNullOrEmpty(ProductSearchImageBased.Approach))
                    return string.Empty;
                switch (ProductSearchImageBased.ModelApproach)
                {
                    case ProductSearchImageBasedSchema.Approaches.TensorFlowCustom:
                    case ProductSearchImageBasedSchema.Approaches.TensorFlowPreTrained:
                        return TensorflowPath;
                    case ProductSearchImageBasedSchema.Approaches.ComputerVision:
                    case ProductSearchImageBasedSchema.Approaches.CustomVisionOffline:
                    case ProductSearchImageBasedSchema.Approaches.CustomVisionOnline:
                        return CognitivePath;
                    default:
                        return CognitivePath;
                }
            }
        }

    }

    public class ProductSearchImageBasedSchema
    {
        public enum Approaches
        {
            Default,
            ComputerVision,
            CustomVisionOnline,
            CustomVisionOffline,
            TensorFlowPreTrained,
            TensorFlowCustom
        }

        public string Approach { get; set; }

        public Approaches ModelApproach
        {
            get
            {
                var preSelected = Approaches.Default;
                if (string.IsNullOrEmpty(Approach)) return preSelected;
                Enum.TryParse<Approaches>(Approach, true, out preSelected);
                return preSelected;
            }
        }
    }

}
