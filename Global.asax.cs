using jotun.Entities;
using jotun.Models;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Unity;
using Unity.Lifetime;

namespace jotun
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			var container = new UnityContainer();
			container.RegisterType<jotunDBEntities, jotunDBEntities>(new HierarchicalLifetimeManager());
			container.RegisterType<ApplicationDbContext, ApplicationDbContext>(new HierarchicalLifetimeManager());
			container.RegisterType<PermissionService>(new HierarchicalLifetimeManager());
			/*container.RegisterType<ModuleController>(new HierarchicalLifetimeManager());*/
			DependencyResolver.SetResolver(new UnityDependencyResolver(container));
		}

    }
}
