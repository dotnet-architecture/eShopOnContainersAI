using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Bot.API
{
    public class AppSettings
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string PurchaseUrl { get; set; }
        public string ArtificialIntelligenceUrl { get; set; }

        public LuisSchema Luis { get; set; }

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

    public class LuisSchema
    {
        public string ModelId { get; set; }
        public string SubscriptionKey { get; set; }
        public string ServiceUri { get; set; }
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
