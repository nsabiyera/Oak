using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Oak.Controllers;

namespace DynamicBlog
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            var unityContainer = new UnityContainer();
            var controllerFactory = new UnityControllerFactory(unityContainer);

            Type[] controllers = typeof(SeedController).Assembly.GetTypes();
            foreach (Type item in controllers)
            {
                if (item.Name.EndsWith("Controller"))
                {
                    controllerFactory.RegisterController(item.Name, item.UnderlyingSystemType);
                }
            }

            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        }
    }
}