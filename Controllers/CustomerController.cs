using jotun.Entities;
using jotun.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EntityFramework.Extensions;

namespace jotun.Controllers
{
    public class CustomerController : Controller
    {
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

        // GET: Customer
        public ActionResult Index()
        {
            if (isAdminUser())
            {
     
                return View();
            }
            else
            {
                return RedirectToAction("Login","Account");
            }
        }

        // POST: /Account/Register
        [HttpPost]
 
        public ActionResult Index(tblCustomer cmodel)
        {
   

            using(jotunDBEntities db = new jotunDBEntities())
            {
                //db.tblCustomers.Add(cmodel);
                //db.SaveChanges();
                return RedirectToAction("Index", "Customer");

                //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Create()
        {
            if (isAdminUser())
            {
     
                return View();
            }
            else
            {
                return RedirectToAction("Login","Account");
            }
        }

        // POST: /Account/Register
        [HttpPost]
 
        public ActionResult Create(tblCustomer cmodel, string[] locations)
        {
   
            using(jotunDBEntities db = new jotunDBEntities())
            {
                cmodel.CreatedDate = DateTime.Now;
                //status 0 = customer enabled
                cmodel.Status = 1;
                cmodel.is_old_data = false;
                db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                foreach (var l in locations)
                {
                    ///Add Multiple Location
                    if (!String.IsNullOrEmpty(l))
                    {
                        tblCustomer_location location = new tblCustomer_location();
                        location.location = l;
                        location.updated_date = DateTime.Now;
                        location.created_date = DateTime.Now;
                        location.customer_id = cmodel.Id;
                        location.status = true;
                        
                        db.tblCustomer_location.Add(location);
                        db.SaveChanges();
                    }
                    ///End Multiple Location
                }
                return RedirectToAction("Index", "Customer");

                //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult CheckUsernameAvailability(string userdata)
        {
            jotunDBEntities db = new jotunDBEntities();
            System.Threading.Thread.Sleep(200);
            var SeachData = db.tblCustomers.Where(x => x.PhoneNumber == userdata && x.PhoneNumber!="" && x.Status==1).FirstOrDefault();
            
            if (SeachData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }
        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblCustomer customer = db.tblCustomers.Find(id);
                if (customer == null)
                {
                    return HttpNotFound();
                }
                return View(new CustomerViewModels()
                {
                    //Id = customer.Id,
                    //CreatedDate = Convert.ToDateTime(customer.CreatedDate),
                    CreatedDate = Convert.ToDateTime(customer.CreatedDate).ToString("dd-MMM-yyyy"),

                    CustomerName = customer.CustomerName,
                    CustomerNo = customer.CustomerNo,
                    Gender = customer.Gender,
                    PhoneNumber = customer.PhoneNumber,
                    ProjectLocation = customer.ProjectLocation,
                    Noted = customer.Noted,
                    Status= Convert.ToString(customer.Status),
                    locations = db.tblCustomer_location.Where(l=>l.customer_id == customer.Id && l.status == true).ToList()
                });
            }
        }


        [HttpPost]
         public ActionResult Edit(tblCustomer cmodel, string [] locations)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                db.Entry(cmodel).State = System.Data.Entity.EntityState.Modified;
                cmodel.UpdatedDate = DateTime.Now;
                //db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                db.tblCustomer_location.Where(c => c.customer_id == cmodel.Id).Update(c => new tblCustomer_location {  status = false });
                foreach(var l in locations)
                {
                    ///Add Multiple Location
                    if (!String.IsNullOrEmpty(l))
                    {
                        tblCustomer_location location = new tblCustomer_location();
                        location.location = l;
                        location.updated_date = DateTime.Now;
                        location.created_date = DateTime.Now;
                        location.customer_id = cmodel.Id;
                        location.status = true;
                        db.tblCustomer_location.Add(location);
                        db.SaveChanges();
                    }
                    ///End Multiple Location
                }
                return RedirectToAction("Index", "Customer");
                //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblCustomer cust = await db.tblCustomers.FindAsync(id);
                cust.Status = 0;
                //db.tblCustomers.Remove(cust);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");

            }
        }
		public ActionResult Detail(string id)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var customer = db.tblCustomers.Find(id);
				if (customer == null)
					return HttpNotFound();

				// Fetch order history using SaleViewModels
				var orderHistory = SaleViewModels.GetNoDetail(id);
				var viewModel = new CustomerOrderViewModel
				{
					Customer = new CustomerViewModels()
					{
						Id = customer.Id,
						CreatedDate = (customer.UpdatedDate ?? customer.CreatedDate)?.ToString("dd-MMM-yyyy"),
						CustomerName = customer.CustomerName,
						CustomerNo = customer.CustomerNo,
						Gender = customer.Gender,
						PhoneNumber = customer.PhoneNumber,
						ProjectLocation = customer.ProjectLocation,
						Noted = customer.Noted,
						locations = db.tblCustomer_location
							.Where(l => l.customer_id == customer.Id && l.status == true)
							.ToList()
					},
					OrderHistory = (List<SaleViewModels>)orderHistory
				};

				return View(viewModel);
			}
		}
		/* [HttpGet]
		 public ActionResult Detail(string id)
		 {

			 if (id == null)
			 {
				 return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			 }
			 using (jotunDBEntities db = new jotunDBEntities())
			 {
				 tblCustomer customer = db.tblCustomers.Find(id);
				 if (customer == null)
				 {
					 return HttpNotFound();
				 }
				 if (customer.UpdatedDate.Equals(null))
				 {
					 return View(new CustomerViewModels()
					 {
						 Id = customer.Id,
						 CreatedDate = Convert.ToDateTime(customer.CreatedDate).ToString("dd-MMM-yyyy"),
						 CustomerName = customer.CustomerName,
						 CustomerNo = customer.CustomerNo,
						 Gender = customer.Gender,
						 PhoneNumber = customer.PhoneNumber,
						 ProjectLocation = customer.ProjectLocation,
						 Noted = customer.Noted,
						 locations = db.tblCustomer_location.Where(l => l.customer_id == customer.Id && l.status == true).ToList()
					 });
				 }
				 else
				 {
					 return View(new CustomerViewModels()
					 {
						 Id = customer.Id,
						 CreatedDate = Convert.ToDateTime(customer.UpdatedDate).ToString("dd-MMM-yyyy"),
						 CustomerName = customer.CustomerName,
						 CustomerNo = customer.CustomerNo,
						 Gender = customer.Gender,
						 PhoneNumber = customer.PhoneNumber,
						 ProjectLocation = customer.ProjectLocation,
						 Noted = customer.Noted,
						 locations = db.tblCustomer_location.Where(l => l.customer_id == customer.Id && l.status == true).ToList()
					 });
				 }

			 }
		 }*/
		public JsonResult GetCustomerData()
        {
            string xx = "";

            List<CustomerViewModels> models = new List<CustomerViewModels>();
            
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var customers = db.tblCustomers.OrderBy(x => x.CustomerName).ToList();
                foreach(var c in customers)
                {
                    if(c.Status == 1)
                    {
                        var aa = c.Noted;
                        var bb = c.Gender;
                        var cc = c.ProjectLocation;
                        if (aa == null)
                        {
                            aa = xx;
                        }
                        if (bb == null)
                        {
                            bb = xx;
                        }
                        if (cc == null)
                        {
                            cc = xx;
                        }
                        if (c.UpdatedDate == null)
                        {
                            models.Add(new CustomerViewModels()
                            {
                                Id = c.Id.ToString(),
                                CustomerNo = c.CustomerNo,
                                CustomerName = c.CustomerName,
                                PhoneNumber = c.PhoneNumber,
                                ProjectLocation = cc,
                                Gender = bb,
                                Noted = aa,
                                Status = Convert.ToString(c.Status),
                                CreatedDate = Convert.ToDateTime(c.CreatedDate).ToString("dd-MMM-yyyy"),
                                //locations = db.tblCustomer_location.Where(l => l.customer_id == c.Id && l.status == true).ToList()
                            });
                        }
                        else
                        {
                            models.Add(new CustomerViewModels()
                            {
                                Id = c.Id.ToString(),
                                CustomerNo = c.CustomerNo,
                                CustomerName = c.CustomerName,
                                PhoneNumber = c.PhoneNumber,
                                ProjectLocation = cc,
                                Gender = bb,
                                Noted = aa,
                                Status = Convert.ToString(c.Status),
                                CreatedDate = Convert.ToDateTime(c.UpdatedDate).ToString("dd-MMM-yyyy"),
                                //locations = db.tblCustomer_location.Where(l => l.customer_id == c.Id && l.status == true).ToList()
                            });
                        }
                    }
                }
            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        
    }
}