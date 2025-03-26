using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;

namespace jotun.Services
{
	public class PermissionAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly List<string> _permissionTypes;
		private readonly int _moduleId;
		public PermissionAuthorizeAttribute(int moduleId, params string[] permissionTypes)
		{
			_moduleId = moduleId;
			_permissionTypes = new List<string>(permissionTypes);
		}

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			var userId = httpContext.User.Identity.GetUserId();
			var permissionService = DependencyResolver.Current.GetService<PermissionService>();
			foreach (var permissionType in _permissionTypes)
			{
				if (!permissionService.HasPermission(userId, _moduleId, permissionType))
				{
					return false; 
				}
			}
			return true;
		}
		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			base.OnAuthorization(filterContext);
			if (!AuthorizeCore(filterContext.HttpContext))
			{
				filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Index" }));
			}
		}
	}
}
