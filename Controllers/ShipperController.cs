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
    public class ShipperController : Controller
    {
        // GET: Shipper
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(tblShipper smodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                smodel.CreatedDate = DateTime.Now;
                //status 0 = customer enabled
                smodel.Status = 1;
                db.tblShippers.Add(smodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Shipper");

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
                tblShipper ship = db.tblShippers.Find(id);
                if (ship == null)
                {
                    return HttpNotFound();
                }
                if (ship.UpdatedDate == null)
                {
                    return View(new ShipperViewModels()
                    {
                        Id = ship.Id,
                        ShipperName = ship.ShipperName,
                        ContactName = ship.ContactName,
                        ContactPhone = ship.ContactPhone,
                        Gender = ship.Gender,
                        Address = ship.Address,
                        Noted = ship.Noted,
                        CreatedDate = Convert.ToDateTime(ship.CreatedDate).ToString("dd-MMM-yyyy"),
                    });
                }
                else
                {
                    return View(new ShipperViewModels()
                    {
                        Id = ship.Id,
                        ShipperName = ship.ShipperName,
                        ContactName = ship.ContactName,
                        ContactPhone = ship.ContactPhone,
                        Gender = ship.Gender,
                        Address = ship.Address,
                        Noted = ship.Noted,
                        CreatedDate = Convert.ToDateTime(ship.UpdatedDate).ToString("dd-MMM-yyyy"),
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
                tblShipper ship = db.tblShippers.Find(id);
                if (ship == null)
                {
                    return HttpNotFound();
                }
                return View(new ShipperViewModels()
                {
                    Id = ship.Id,
                    ShipperName = ship.ShipperName,
                    ContactName = ship.ContactName,
                    ContactPhone = ship.ContactPhone,
                    Gender = ship.Gender,
                    Address = ship.Address,
                    Noted = ship.Noted,
                    Status = Convert.ToString(ship.Status),
                    CreatedDate = Convert.ToDateTime(ship.CreatedDate).ToString("dd-MMM-yyyy"),
                });
            }
        }
        // POST: /Account/Register
        [HttpPost]
        public ActionResult Edit(tblShipper smodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                db.Entry(smodel).State = System.Data.Entity.EntityState.Modified;
                smodel.UpdatedDate = DateTime.Now;
                //db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Shipper");
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
        //        tblShipper ship = db.tblShippers.Find(id);
        //        if (ship == null)
        //        {
        //            return HttpNotFound();
        //        }
        //        if (ship.UpdatedDate == null)
        //        {
        //            return View(new ShipperViewModels()
        //            {
        //                Id = ship.Id,
        //                ShipperName = ship.ShipperName,
        //                ContactName = ship.ContactName,
        //                ContactPhone = ship.ContactPhone,
        //                Gender = ship.Gender,
        //                Address = ship.Address,
        //                Noted = ship.Noted,
        //                CreatedDate = Convert.ToDateTime(ship.CreatedDate).ToString("dd-MMM-yyyy"),
        //            });
        //        }
        //        else
        //        {
        //            return View(new ShipperViewModels()
        //            {
        //                Id = ship.Id,
        //                ShipperName = ship.ShipperName,
        //                ContactName = ship.ContactName,
        //                ContactPhone = ship.ContactPhone,
        //                Gender = ship.Gender,
        //                Address = ship.Address,
        //                Noted = ship.Noted,
        //                CreatedDate = Convert.ToDateTime(ship.CreatedDate).ToString("dd-MMM-yyyy"),
        //                UpdatedDate = Convert.ToDateTime(ship.UpdatedDate).ToString("dd-MMM-yyyy"),

        //            });
        //        }
        //    }
        //}
        //// POST: /Account/Register
        //[HttpPost]
        //public ActionResult Delete(tblShipper smodel)
        //{
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        db.Entry(smodel).State = System.Data.Entity.EntityState.Modified;
        //        smodel.Status = 0;
        //        smodel.UpdatedDate = DateTime.Now;
        //        db.SaveChanges();
        //        return RedirectToAction("Index", "Shipper");
        //        //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblShipper cust = await db.tblShippers.FindAsync(id);
                cust.Status = 0;
                //db.tblShippers.Remove(cust);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");

            }
        }
        public JsonResult GetShipperData()
        {
            string xx = "";
            List<ShipperViewModels> models = new List<ShipperViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var shippers = db.tblShippers.OrderBy(x => x.Id).ToList();
                foreach (var s in shippers)
                {
                    if (s.Status == 1)
                    {
                        var aa = s.Noted;
                        var bb = s.Gender;
                        var cc = s.Address;
                        if(aa==null)
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
                            models.Add(new ShipperViewModels()
                            {
                                Id = s.Id.ToString(),
                                ShipperName = s.ShipperName,
                                ContactName = s.ContactName,
                                ContactPhone = s.ContactPhone,
                                Gender = bb,
                                Noted =aa,
                                Address = cc,
                               // Status = s.Status,
                                CreatedDate = Convert.ToDateTime(s.CreatedDate).ToString("dd-MMM-yyyy"),
                            });
                        }
                        else
                        {
                            models.Add(new ShipperViewModels()
                            {
                                Id = s.Id.ToString(),
                                ShipperName = s.ShipperName,
                                ContactName = s.ContactName,
                                ContactPhone = s.ContactPhone,
                                Gender = bb,
                                Noted = aa,
                                Address = cc,
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