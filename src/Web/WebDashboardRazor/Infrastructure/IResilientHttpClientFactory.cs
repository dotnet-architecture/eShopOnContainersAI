using Microsoft.eShopOnContainers.BuildingBlocks.Resilience.Http;
using System;

namespace Microsoft.eShopOnContainers.WebDashboardRazor.Infrastructure
{
    public interface IResilientHttpClientFactory
    {
        ResilientHttpClient CreateResilientHttpClient();
    }
}