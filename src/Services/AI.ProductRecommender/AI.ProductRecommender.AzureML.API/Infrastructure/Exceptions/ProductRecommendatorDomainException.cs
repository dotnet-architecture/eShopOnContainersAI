using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductRecommender.AzureML.API.Infrastructure.Exceptions
{
    /// <summary>
    /// Exception type for app exceptions
    /// </summary>
    public class ProductRecommendatorDomainException : Exception
    {
        public ProductRecommendatorDomainException()
        { }

        public ProductRecommendatorDomainException(string message)
            : base(message)
        { }

        public ProductRecommendatorDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

}
