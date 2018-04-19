using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Infrastructure.Exceptions
{
    /// <summary>
    /// Exception type for app exceptions
    /// </summary>
    public class ArtificialIntelligenceDomainException : Exception
    {
        public ArtificialIntelligenceDomainException()
        { }

        public ArtificialIntelligenceDomainException(string message)
            : base(message)
        { }

        public ArtificialIntelligenceDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

}
