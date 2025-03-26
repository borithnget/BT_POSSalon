using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using jotun.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using jotun.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Web.Security;

namespace jotun.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext context;

        public AccountController()
        {
            context = new ApplicationDbContext();

        }



        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {


            if (!ModelState.IsValid)
            {

                return View(model);
            }
            // This doesn't count login failures towards account lockout   
            // To enable password failures to trigger account lockout, change to shouldLockout: true   

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);


            switch (result)
            {
                case SignInStatus.Success:

                    return RedirectToLocal(returnUrl);
                //if (model.LockoutEnabled == false)
                //{
                //    return LogOff();
                //}
                //else
                //{
                //    return RedirectToLocal(returnUrl);

                //}
                //return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }


        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login   
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }


        public JsonResult GetUserData()
        {
            List<RegisterViewModel> models = new List<RegisterViewModel>();
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var users = db.AspNetUsers.OrderBy(x => x.UserName).ToList();
                foreach (var u in users)
                {
                    var userRoles = (from user in context.Users
                                     select new
                                     {
                                         UserId = user.Id,
                                         UserName = user.UserName,
                                         LockoutEnabled = user.LockoutEnabled,
                                         Email = user.Email,
                                         RoleName = (from userrole in user.Roles
                                                     join role in context.Roles on userrole.RoleId equals role.Id
                                                     select role.Name).ToList(),
                                     }).ToList()
                                   .Select(s => new
                                   {
                                       id = s.UserId,
                                       Role = string.Join(" ,", s.RoleName)
                                   }).Where(w => w.id == u.Id).FirstOrDefault();
                    models.Add(new RegisterViewModel()
                    {
                        Id = u.Id,
                        Email = u.Email,
                        UserName = u.UserName,
                        LockoutEnabled = u.LockoutEnabled,
                        UserRoles = userRoles.Role,
                    });
                }
            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (isAdminUser())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Name = new SelectList(context.Roles.Where(u => !u.Name.Contains("SuperAdmin"))
                                              .ToList(), "Name", "Name");
            if (isAdminUser())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }


        public Boolean isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "SuperAdmin" || s[0].ToString() == "Administrator")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public JsonResult CheckUsernameAvailability(string userdata)
        {
            jotunDBEntities db = new jotunDBEntities();
            System.Threading.Thread.Sleep(200);
            var SeachData = db.AspNetUsers.Where(x => x.UserName == userdata).SingleOrDefault();
            if (SeachData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }
        public JsonResult CheckUsernameAvailability1(string userdata)
        {
            jotunDBEntities db = new jotunDBEntities();
            System.Threading.Thread.Sleep(200);
            var SeachData = db.AspNetUsers.Where(x => x.Email == userdata).SingleOrDefault();
            if (SeachData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };


                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await this.UserManager.AddToRoleAsync(user.Id, model.UserRoles);
                    //Ends Here    
                    return RedirectToAction("Index", "Account");
                }
                ViewBag.Name = new SelectList(context.Roles.Where(u => !u.Name.Contains("SuperAdmin"))
                                          .ToList(), "Name", "Name");
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form   
            return View(model);
        }




        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {


            ViewBag.Name = new SelectList(context.Roles.Where(u => !u.Name.Contains("SuperAdmin"))
                                            .ToList(), "Name", "Name");
            return View();

        }


        [HttpGet]
        public async Task<ActionResult> Detail(string id)
        {

            ViewBag.Name = new SelectList(context.Roles.Where(u => !u.Name.Contains("SuperAdmin"))
                                            .ToList(), "Name", "Name");


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var userRoles = await UserManager.GetRolesAsync(user.Id);


            var users = User.Identity;
            var UserManagers = await UserManager.GetRolesAsync(user.Id);
            var userroles = UserManagers.ToList();
            var userrole = userroles[0].ToString();
            
            if (isAdminUser())
            {
                return View(new RegisterViewModel()
                {
                    // Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    LockoutEnabled = user.LockoutEnabled,
                    UserRoles = userrole,

                });
            }
            else
            {
                return RedirectToAction("Login");
            }



        }


        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {

            ViewBag.Name = new SelectList(context.Roles.Where(u => !u.Name.Contains("SuperAdmin"))
                                            .ToList(), "Name", "Name");


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var userRoles = await UserManager.GetRolesAsync(user.Id);


            var users = User.Identity;
            var UserManagers = await UserManager.GetRolesAsync(user.Id);
            var userroles = UserManagers.ToList();
            var userrole = userroles[0].ToString();
            if (isAdminUser())
            {
                return View(new RegisterViewModel()
                {
                    // Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    LockoutEnabled = user.LockoutEnabled,
                    UserRoles = userrole,

                });
            }
            else
            {
                return RedirectToAction("Login");
            }

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RegisterViewModel roleModel, string rolesuser)
        {

            var role = await UserManager.FindByIdAsync(roleModel.Id);
            role.Id = roleModel.Id;
            role.UserName = roleModel.UserName;
            role.Email = roleModel.Email;
            role.LockoutEnabled = roleModel.LockoutEnabled;

            using (jotunDBEntities db = new jotunDBEntities())
            {
                var olduser = db.AspNetUsers.SingleOrDefault(u => u.Id == roleModel.Id);
                var oldRoleId = olduser.AspNetRoles.SingleOrDefault().Id;
                var oldRoleName = db.AspNetRoles.SingleOrDefault(r => r.Id == oldRoleId).Name;
                if (oldRoleName != rolesuser)
                {
                    UserManager.RemoveFromRole(roleModel.Id, oldRoleName);
                    await this.UserManager.AddToRoleAsync(roleModel.Id, roleModel.UserRoles);                    //await this.UserManager.UpdateAsync(rolesuser);
                    // UserManager.AddToRole(roleModel.Id, rolesuser);

                }
                //db.Entry(oldRoleName).State = System.Data.Entity.EntityState.Modified;
                //await this.UserManager.UpdateAsync(role);

                return RedirectToAction("Index");
            }

        }


        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                AspNetUser cust = await db.AspNetUsers.FindAsync(id);
                // cust.Status = 0;
                if (isAdminUser())
                {
                    db.AspNetUsers.Remove(cust);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Login");
                }
                

            }
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(RegisterViewModel roleModel, string rolesuser)
        //{

        //    var role = await UserManager.FindByIdAsync(roleModel.Id);
        //    role.Id = roleModel.Id;
        //    role.UserName = roleModel.UserName;
        //    role.Email = roleModel.Email;
        //    role.LockoutEnabled = roleModel.LockoutEnabled;

        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        var olduser = db.AspNetUsers.SingleOrDefault(u => u.Id == roleModel.Id);
        //        var oldRoleId = olduser.AspNetRoles.SingleOrDefault().Id;
        //        var oldRoleName = db.AspNetRoles.SingleOrDefault(r => r.Id == oldRoleId).Name;
        //        if (oldRoleName != rolesuser)
        //        {
        //            UserManager.RemoveFromRole(roleModel.Id, oldRoleName);
        //            await this.UserManager.AddToRoleAsync(roleModel.Id, roleModel.UserRoles);                    //await this.UserManager.UpdateAsync(rolesuser);
        //            // UserManager.AddToRole(roleModel.Id, rolesuser);

        //        }
        //        //db.Entry(oldRoleName).State = System.Data.Entity.EntityState.Modified;
        //        //await this.UserManager.UpdateAsync(role);

        //        return RedirectToAction("Index");
        //    }

        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(RegisterViewModel roleModel)
        //{

        //    var role = await UserManager.FindByIdAsync(roleModel.Id);
        //    role.Id = roleModel.Id;
        //    role.UserName = roleModel.UserName;
        //    role.Email = roleModel.Email;
        //    //await this.UserManager.AddToRoleAsync(roleModel.Id, roleModel.UserRoles);


        //    //var k = await this.UserManager.FindByIdAsync(roleModel.Id);
        //    // await this.UserManager.UpdateAsync(role);


        //    await this.UserManager.UpdateAsync(role);
        //    //  usermanagersrolename = roleModel.UserRoles;

        //    return RedirectToAction("Index");

        //}


        // [HttpGet]
        //public async Task<ActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var userRoles = await UserManager.GetRolesAsync(user.Id);

        //    return View(new RegisterViewModel()
        //    {
        //        Id = user.Id,
        //        Email = user.Email,
        //        // Include the Addresss info:
        //        UserName = user.UserName,

        //    });
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Delete(RegisterViewModel roleModel)
        //{

        //        var role = await UserManager.FindByIdAsync(roleModel.Id);
        //        role.Id = roleModel.Id;
        //        role.UserName = roleModel.UserName;
        //        role.Email = roleModel.Email;
        //        await UserManager.UpdateAsync(role);
        //        return RedirectToAction("Index");

        //}




        //// GET: /Movies/Delete/5
        //public async Task<ActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var userRoles = await UserManager.GetRolesAsync(user.Id);
        //    var users = User.Identity;
        //    var UserManagers = await UserManager.GetRolesAsync(user.Id);
        //    var userroles = UserManagers.ToList();
        //    var userrole = userroles[0].ToString();
        //    if (userrole == "SuperAdmin")
        //    {
        //        return RedirectToAction("Error", "Error");
        //    }
        //    {
        //        return View();
        //    }

        //}

        //// POST: /Movies/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(FormCollection fcNotUsed, string id)
        //{
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        AspNetUser usersacc = db.AspNetUsers.Find(id);
        //        if (usersacc == null)
        //        {
        //            return HttpNotFound();

        //        }

        //        db.AspNetUsers.Remove(usersacc);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //}




        //[HttpGet]
        //public async Task<ActionResult> AddOrEdit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var userRoles = await UserManager.GetRolesAsync(user.Id);

        //    return View(new RegisterViewModel()
        //    {
        //        Id = user.Id,
        //        Email = user.Email,
        //        // Include the Addresss info:
        //        UserName = user.UserName,

        //    });
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> AddOrEdit(RegisterViewModel roleModel)
        //{

        //    var role = await UserManager.FindByIdAsync(roleModel.Id);
        //    role.Id = roleModel.Id;
        //    role.UserName = roleModel.UserName;
        //    role.Email = roleModel.Email;
        //    await UserManager.UpdateAsync(role);
        //    return RedirectToAction("Index");

        //}


        // [HttpGet]
        //public async Task<ActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var user = await UserManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    var userRoles = await UserManager.GetRolesAsync(user.Id);

        //    return View(new RegisterViewModel()
        //    {
        //        Id = user.Id,
        //        Email = user.Email,
        //        // Include the Addresss info:
        //        UserName = user.UserName,

        //    });
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Delete(RegisterViewModel roleModel)
        //{

        //        var role = await UserManager.FindByIdAsync(roleModel.Id);
        //        role.Id = roleModel.Id;
        //        role.UserName = roleModel.UserName;
        //        role.Email = roleModel.Email;
        //        await UserManager.UpdateAsync(role);
        //        return RedirectToAction("Index");

        //}




        //// GET: /Movies/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    return View();


        //}

        //// POST: /Movies/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(FormCollection fcNotUsed, string id)
        //{
        //    using (DBEntityModel db = new DBEntityModel())
        //    {
        //        AspNetUser usersacc = db.AspNetUsers.Find(id);
        //        if (usersacc == null)
        //        {
        //            return HttpNotFound();

        //        }

        //        db.AspNetUsers.Remove(usersacc);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //}






        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //      
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Coffee");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {

                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }



        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}