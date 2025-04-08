using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class LanguageController : Controller
    {
        public ActionResult SetLanguage(string lang)
        {
            if (!string.IsNullOrEmpty(lang))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

                HttpCookie cookie = new HttpCookie("Language", lang)
                {
                    Expires = DateTime.Now.AddYears(1)
                };
                Response.Cookies.Add(cookie);
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/");
        }
    }
}