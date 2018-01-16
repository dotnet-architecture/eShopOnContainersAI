using System.Web.Mvc;
using System.Web.Routing;

namespace Microsoft.Bots.Bot.API
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

