using jotun.Entities;
using jotun.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var userid = User.Identity.GetUserId();
            List<LoginViewModel> models = new List<LoginViewModel>();
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var users = db.AspNetUsers.OrderBy(x => x.UserName).ToList();
                foreach (var u in users)
                {
                    var userRoles =
                                    (from user in context.Users
                                     where user.Id == userid
                                     select user
                                     ).ToList();

                                //(from user in context.Users
                                // select new
                                // {
                                //     UserId = user.Id,

                                //     LockoutEnabled = user.LockoutEnabled,

                                // }).FirstOrDefault();

                    foreach(var k in userRoles)
                    {
                        if (k.LockoutEnabled == true)
                        {
                            return View();

                        }
                        else
                        {

                            TempData["message"] = "Your Account has been disabled !!!";

                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                           // return RedirectToAction("Login", "Account");

                            return RedirectToAction("Login", "Account");

                            //        return Json(new {Status = "success", Message="Succesfully updated"});


                        }
                    }
                   
                }
            }
            return View();


        }
        private Microsoft.Owin.Security.IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


    }


}