using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class BaseController : Controller
    {
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            var langCookie = requestContext.HttpContext.Request.Cookies["Language"];
            var lang = langCookie?.Value ?? "en"; 

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

            base.Initialize(requestContext);
        }
    }
}