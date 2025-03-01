using jotun.Entities;
using jotun.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class SupplierController : Controller
    {
        // GET: Supplier
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(tblSupplier smodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                smodel.CreatedDate = DateTime.Now;
                //status 0 = customer enabled
                smodel.Status = 1;
                db.tblSuppliers.Add(smodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Supplier");

            }
        }

        [HttpGet]
        public ActionResult Detail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblSupplier supp = db.tblSuppliers.Find(id);
                if (supp == null)
                {
                    return HttpNotFound();
                }
                if (supp.UpdatedDate == null)
                {
                    return View(new SupplierViewModels()
                    {
                        Id = supp.Id,
                        SupplierName = supp.SupplierName,
                        ContactName = supp.ContactName,
                        ContactPhone = supp.ContactPhone,
                        Gender = supp.Gender,
                        Address = supp.Address,
                        Description = supp.Description,
                        CreatedDate = Convert.ToDateTime(supp.CreatedDate).ToString("dd-MMM-yyyy"),
                    });
                }
                else
                {
                    return View(new SupplierViewModels()
                    {
                        Id = supp.Id,
                        SupplierName = supp.SupplierName,
                        ContactName = supp.ContactName,
                        ContactPhone = supp.ContactPhone,
                        Gender = supp.Gender,
                        Address = supp.Address,
                        Description = supp.Description,
                        CreatedDate = Convert.ToDateTime(supp.UpdatedDate).ToString("dd-MMM-yyyy"),
                    });
                }
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
                tblSupplier supp = db.tblSuppliers.Find(id);
                if (supp == null)
                {
                    return HttpNotFound();
                }
                return View(new SupplierViewModels()
                {
                    Id = supp.Id,
                    SupplierName = supp.SupplierName,
                    ContactName = supp.ContactName,
                    ContactPhone = supp.ContactPhone,
                    Gender = supp.Gender,
                    Address = supp.Address,
                    Description = supp.Description,
                    Status = Convert.ToString(supp.Status),
                    CreatedDate = Convert.ToDateTime(supp.CreatedDate).ToString("dd-MMM-yyyy"),
                });
            }
        }
        // POST: /Account/Register
        [HttpPost]
        public ActionResult Edit(tblSupplier smodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                db.Entry(smodel).State = System.Data.Entity.EntityState.Modified;
                smodel.UpdatedDate = DateTime.Now;
                //db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Supplier");
                //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpGet]
        //public ActionResult Delete(string id)
        //{

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        tblSupplier supp = db.tblSuppliers.Find(id);
        //        if (supp == null)
        //        {
        //            return HttpNotFound();
        //        }
        //        if (supp.UpdatedDate == null)
        //        {
        //            return View(new SupplierViewModels()
        //            {
        //                Id = supp.Id,
        //                SupplierName = supp.SupplierName,
        //                ContactName = supp.ContactName,
        //                ContactPhone = supp.ContactPhone,
        //                Gender = supp.Gender,
        //                Address = supp.Address,
        //                Description = supp.Description,
        //                CreatedDate = Convert.ToDateTime(supp.CreatedDate).ToString("dd-MMM-yyyy"),
        //            });
        //        }
        //        else
        //        {
        //            return View(new SupplierViewModels()
        //            {
        //                Id = supp.Id,
        //                SupplierName = supp.SupplierName,
        //                ContactName = supp.ContactName,
        //                ContactPhone = supp.ContactPhone,
        //                Gender = supp.Gender,
        //                Address = supp.Address,
        //                Description = supp.Description,
        //                CreatedDate = Convert.ToDateTime(supp.CreatedDate).ToString("dd-MMM-yyyy"),
        //                UpdatedDate = Convert.ToDateTime(supp.UpdatedDate).ToString("dd-MMM-yyyy"),

        //            });
        //        }
        //    }
        //}
        //// POST: /Account/Register
        //[HttpPost]
        //public ActionResult Delete(tblSupplier smodel)
        //{
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        db.Entry(smodel).State = System.Data.Entity.EntityState.Modified;
        //        smodel.Status = 0;
        //        smodel.UpdatedDate = DateTime.Now;
        //        db.SaveChanges();
        //        return RedirectToAction("Index", "Supplier");
        //        //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblSupplier cust = await db.tblSuppliers.FindAsync(id);
                cust.Status = 0;
                //db.tblShippers.Remove(cust);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");

            }
        }


        public JsonResult GetSupplierData()
        {
            string xx = "";
            List<SupplierViewModels> models = new List<SupplierViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var suppliers = db.tblSuppliers.OrderBy(x => x.Id).ToList();
                foreach (var s in suppliers)
                {
                    if (s.Status == 1)
                    {
                        var bb = s.Address;
                        var aa = s.Description;
                        var cc = s.Gender;
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
                        if (s.UpdatedDate == null)
                        {
                            models.Add(new SupplierViewModels()
                            {
                                Id = s.Id.ToString(),
                                SupplierName = s.SupplierName,
                                ContactName = s.ContactName,
                                ContactPhone = s.ContactPhone,
                                Gender = cc,
                                Description = aa,
                                Address = bb,
                                // Status = s.Status,
                                CreatedDate = Convert.ToDateTime(s.CreatedDate).ToString("dd-MMM-yyyy"),
                            });
                        }
                        else
                        {
                            models.Add(new SupplierViewModels()
                            {
                                Id = s.Id.ToString(),
                                SupplierName = s.SupplierName,
                                ContactName = s.ContactName,
                                ContactPhone = s.ContactPhone,
                                Gender = cc,
                                Description = aa,
                                Address = bb,
                                CreatedDate = Convert.ToDateTime(s.UpdatedDate).ToString("dd-MMM-yyyy"),
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