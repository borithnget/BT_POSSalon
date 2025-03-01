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
    public class UnitController : Controller
    {
        // GET: Unit
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(tblUnit umodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                umodel.CreatedDate = DateTime.Now;
                //status 0 = customer enabled
                umodel.Status = 1;
                db.tblUnits.Add(umodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Unit");

            }
        }
        public JsonResult CheckUsernameAvailability(string userdata)
        {
            jotunDBEntities db = new jotunDBEntities();
            System.Threading.Thread.Sleep(200);
            var SeachData = db.tblUnits.Where(x => x.UnitNameEng == userdata && x.Status==1).FirstOrDefault();
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
        public ActionResult Detail(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblUnit unit = db.tblUnits.Find(id);
                if (unit == null)
                {
                    return HttpNotFound();
                }
                if (unit.UpdatedDate == null)
                {
                    return View(new UnitViewModels()
                    {
                        Id = unit.Id,
                        UnitNameEng = unit.UnitNameEng,
                        UnitNameKh = unit.UnitNameKh,
                        Quantity = unit.Quantity,
                        Description = unit.Description,
                        CreatedDate = Convert.ToDateTime(unit.CreatedDate).ToString("dd-MMM-yyyy"),
                    });
                }
                else
                {
                    return View(new UnitViewModels()
                    {
                        Id = unit.Id,
                        UnitNameEng = unit.UnitNameEng,
                        UnitNameKh = unit.UnitNameKh,
                        Quantity = unit.Quantity,

                        Description = unit.Description,
                        CreatedDate = Convert.ToDateTime(unit.UpdatedDate).ToString("dd-MMM-yyyy"),
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
                tblUnit unit = db.tblUnits.Find(id);
                if (unit == null)
                {
                    return HttpNotFound();
                }

                return View(new UnitViewModels()
                {
                    Id = unit.Id,
                    UnitNameEng = unit.UnitNameEng,
                    UnitNameKh = unit.UnitNameKh,
                    Quantity = unit.Quantity,
                    Description = unit.Description,
                    CreatedDate = Convert.ToDateTime(unit.CreatedDate).ToString("dd-MMM-yyyy"),
                    Status = Convert.ToString(unit.Status),

                });
            }
        }
        // POST: /Account/Register
        [HttpPost]
        public ActionResult Edit(tblUnit cmodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                db.Entry(cmodel).State = System.Data.Entity.EntityState.Modified;
                cmodel.UpdatedDate = DateTime.Now;
                //db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Unit");
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
        //        tblUnit unit = db.tblUnits.Find(id);
        //        if (unit == null)
        //        {
        //            return HttpNotFound();
        //        }
        //        if (unit.UpdatedDate == null)
        //        {
        //            return View(new UnitViewModels()
        //            {
        //                Id = unit.Id,
        //                UnitNameEng = unit.UnitNameEng,
        //                UnitNameKh = unit.UnitNameKh,
        //                Quantity = unit.Quantity,
        //                Description = unit.Description,
        //                CreatedDate = Convert.ToDateTime(unit.CreatedDate).ToString("dd-MMM-yyyy"),
        //                // UpdatedDate = Convert.ToDateTime(category.UpdatedDate).ToString("dd-MMM-yyyy"),
        //                Status = Convert.ToString(unit.Status),

        //            });
        //        }
        //        else
        //        {
        //            return View(new UnitViewModels()
        //            {
        //                Id = unit.Id,
        //                UnitNameEng = unit.UnitNameEng,
        //                UnitNameKh = unit.UnitNameKh,
        //                Quantity = unit.Quantity,
        //                Description = unit.Description,
        //                CreatedDate = Convert.ToDateTime(unit.CreatedDate).ToString("dd-MMM-yyyy"),
        //                UpdatedDate = Convert.ToDateTime(unit.UpdatedDate).ToString("dd-MMM-yyyy"),
        //                Status = Convert.ToString(unit.Status),

        //            });
        //        }

        //    }
        //}
        //// POST: /Account/Register
        //[HttpPost]
        //public ActionResult Delete(tblUnit cmodel)
        //{
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        db.Entry(cmodel).State = System.Data.Entity.EntityState.Modified;
        //        cmodel.Status = 0;
        //        db.SaveChanges();
        //        return RedirectToAction("Index", "Unit");
        //        //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblUnit cust = await db.tblUnits.FindAsync(id);
                cust.Status = 0;
                //db.tblShippers.Remove(cust);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");

            }
        }


        public JsonResult GetUnitData()
        {
            string xx = "";
            List<UnitViewModels> models = new List<UnitViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var units = db.tblUnits.OrderBy(x => x.Id).ToList();
                foreach (var u in units)
                {
                    if (u.Status == 1)
                    {
                        var aa = u.Description;
                        var bb = u.UnitNameKh;
                        if (aa == null)
                        {
                            aa = xx;
                        }
                        if (bb == null)
                        {
                            bb = xx;
                        }
                        if (u.UpdatedDate == null)
                        {
                            models.Add(new UnitViewModels()
                            {
                                Id = u.Id.ToString(),
                                UnitNameEng = u.UnitNameEng,
                                UnitNameKh = bb,
                                Quantity = Convert.ToInt64(u.Quantity),
                                Description = aa,
                                CreatedDate = Convert.ToDateTime(u.CreatedDate).ToString("dd-MMM-yyyy"),
                            });
                        }
                        else
                        {
                            models.Add(new UnitViewModels()
                            {
                                Id = u.Id.ToString(),
                                UnitNameEng = u.UnitNameEng,
                                UnitNameKh = bb,
                                Quantity = Convert.ToInt64(u.Quantity),

                                Description = aa,
                                CreatedDate = Convert.ToDateTime(u.UpdatedDate).ToString("dd-MMM-yyyy"),
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