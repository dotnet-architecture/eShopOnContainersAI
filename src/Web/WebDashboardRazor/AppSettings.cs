namespace Microsoft.eShopOnContainers.WebDashboardRazor
{
    public class AppSettings
    {
        public string ArtificialIntelligenceUrl { get; set; }
        public string WebShoppingUrl { get; set; }

        public Logging Logging { get; set; }
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
