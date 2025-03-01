using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace jotun
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Coffee", action = "Index", id = UrlParameter.Optional }
            );
          /*  routes.MapRoute(
    name: "CustomerHistory",
    url: "Sale/CustomerHistory/{id}",
    defaults: new { controller = "Sale", action = "CustomerHistory", id = UrlParameter.Optional }
);*/

        }
    }
}
