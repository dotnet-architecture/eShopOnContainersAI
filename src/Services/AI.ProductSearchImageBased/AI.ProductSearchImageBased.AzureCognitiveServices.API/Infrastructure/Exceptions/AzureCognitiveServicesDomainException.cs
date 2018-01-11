using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.AzureCognitiveServices.API.Infrastructure.Exceptions
{
    /// <summary>
    /// Exception type for app exceptions
    /// </summary>
    public class AzureCognitiveServicesDomainException : Exception
    {
        public AzureCognitiveServicesDomainException()
        { }

        public AzureCognitiveServicesDomainException(string message)
            : base(message)
        { }

        public AzureCognitiveServicesDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

}
