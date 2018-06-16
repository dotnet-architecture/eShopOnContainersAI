using System;

namespace Microsoft.eShopOnContainers.WebMVC
{
    public class AppSettings
    {
        //public Connectionstrings ConnectionStrings { get; set; }
        public string MarketingUrl { get; set; }
        public string ProductSearchImageUrl
        {
            get
            {
                if (ProductSearchImageBased == null || string.IsNullOrEmpty(ProductSearchImageBased.Approach))
                    return string.Empty;
                switch (ProductSearchImageBased.ModelApproach)
                {
                    case ProductSearchImageBasedSchema.Approaches.ComputerVision:
                    case ProductSearchImageBasedSchema.Approaches.CustomVisionOffline:
                    case ProductSearchImageBasedSchema.Approaches.CustomVisionOnline:
                        return ProductSearchImageBased.CognitiveUrl;
                    case ProductSearchImageBasedSchema.Approaches.TensorFlowCustom:
                    case ProductSearchImageBasedSchema.Approaches.TensorFlowPreTrained:
                        return ProductSearchImageBased.TensorFlowUrl;
                    default:
                        return ProductSearchImageBased.TensorFlowUrl;
                }
            }
        }

        public string ArtificialIntelligenceUrl { get; set; }
        public string PurchaseUrl { get; set; }
        public string SignalrHubUrl { get; set; }
        public bool ActivateCampaignDetailFunction { get; set; }
        public Logging Logging { get; set; }
        public bool UseCustomizationData { get; set; }

        public ProductSearchImageBasedSchema ProductSearchImageBased { get; set; }
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

        public string CognitiveUrl { get; set; }
        public string TensorFlowUrl { get; set; }
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

    public class Connectionstrings
    {
        public string DefaultConnection { get; set; }
    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public Loglevel LogLevel { get; set; }
    }

    public class Loglevel
    {
        public string Default { get; set; }
        public string System { get; set; }
        public string Microsoft { get; set; }
    }
}
