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
    public class CategoryController : Controller
    {
        // GET: Category
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
        // POST: /Account/Register
        [HttpPost]
        public ActionResult Create(tblCategory cmodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                cmodel.CreatedDate = DateTime.Now;
                //status 0 = customer enabled
                cmodel.Status = 1;
                db.tblCategories.Add(cmodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Category");

            }

        }
        public JsonResult CheckUsernameAvailability(string userdata)
        {
            jotunDBEntities db = new jotunDBEntities();
            System.Threading.Thread.Sleep(200);
            var SeachData = db.tblCategories.Where(x => x.CategoryNameEng == userdata && x.Status== 1 ).SingleOrDefault();
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
            var SeachData = db.tblCategories.Where(x => x.CategoryNameKh == userdata && x.Status == 1).SingleOrDefault();
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
                tblCategory category = db.tblCategories.Find(id);
                if (category == null)
                {
                    return HttpNotFound();
                }
                if(category.UpdatedDate == null)
                {
                    return View(new CategoryViewModels()
                    {
                        Id = category.Id,
                        CategoryNameEng = category.CategoryNameEng,
                        CategoryNameKh = category.CategoryNameKh,
                        Description = category.Description,
                        CreatedDate = Convert.ToDateTime(category.CreatedDate).ToString("dd-MMM-yyyy"),
                    });
                }
                else
                {
                    return View(new CategoryViewModels()
                    {
                        Id = category.Id,
                        CategoryNameEng = category.CategoryNameEng,
                        CategoryNameKh = category.CategoryNameKh,
                        Description = category.Description,
                        CreatedDate = Convert.ToDateTime(category.UpdatedDate).ToString("dd-MMM-yyyy"),
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
                tblCategory category = db.tblCategories.Find(id);
                if (category == null)
                {
                    return HttpNotFound();
                }

                return View(new CategoryViewModels()
                {
                    Id = category.Id,
                    CategoryNameEng = category.CategoryNameEng,
                    CategoryNameKh = category.CategoryNameKh,

                    Description = category.Description,
                    CreatedDate = Convert.ToDateTime(category.CreatedDate).ToString("dd-MMM-yyyy"),
                    Status = Convert.ToString(category.Status),

                });
            }
        }
        // POST: /Account/Register
        [HttpPost]
        public ActionResult Edit(tblCategory cmodel)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                db.Entry(cmodel).State = System.Data.Entity.EntityState.Modified;
                cmodel.UpdatedDate = DateTime.Now;
                //db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                return RedirectToAction("Index", "Category");
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
        //        tblCategory category = db.tblCategories.Find(id);
        //        if (category == null)
        //        {
        //            return HttpNotFound();
        //        }
        //        if (category.UpdatedDate == null)
        //        {
        //            return View(new CategoryViewModels()
        //            {
        //                Id = category.Id,
        //                CategoryNameEng = category.CategoryNameEng,
        //                CategoryNameKh = category.CategoryNameKh,
        //                Description = category.Description,
        //                CreatedDate = Convert.ToDateTime(category.CreatedDate).ToString("dd-MMM-yyyy"),
        //               // UpdatedDate = Convert.ToDateTime(category.UpdatedDate).ToString("dd-MMM-yyyy"),
        //                Status = Convert.ToString(category.Status),

        //            });
        //        }
        //        else
        //        {
        //            return View(new CategoryViewModels()
        //            {
        //                Id = category.Id,
        //                CategoryNameEng = category.CategoryNameEng,
        //                CategoryNameKh = category.CategoryNameKh,
        //                Description = category.Description,
        //                CreatedDate = Convert.ToDateTime(category.CreatedDate).ToString("dd-MMM-yyyy"),
        //                UpdatedDate = Convert.ToDateTime(category.UpdatedDate).ToString("dd-MMM-yyyy"),
        //                Status = Convert.ToString(category.Status),

        //            });
        //        }

        //    }
        //}
        // POST: /Account/Register
        //[HttpPost]
        //public ActionResult Delete(tblCategory cmodel)
        //{
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        db.Entry(cmodel).State = System.Data.Entity.EntityState.Modified;
        //        cmodel.Status = 0;
        //        //db.tblCustomers.Add(cmodel);
        //        db.SaveChanges();
        //        return RedirectToAction("Index", "Category");
        //        //return Json(new { success = true, message = "saved Successfully" }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblCategory cust = await db.tblCategories.FindAsync(id);
                cust.Status = 0;
                //db.tblShippers.Remove(cust);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");

            }
        }
        public JsonResult GetCategoryData()
        {
            List<CategoryViewModels> models = new List<CategoryViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                string xx = "";
                string yy = "";
                ApplicationDbContext context = new ApplicationDbContext();
                var categories = db.tblCategories.OrderBy(x => x.Id).ToList();
                foreach (var c in categories)
                {
                    if (c.Status == 1)
                    {
                        var aa = c.Description;
                        var bb = c.CategoryNameKh;
                        if(bb == null)
                        {
                            bb = yy;
                        }
                        if (aa == null)
                        {
                            aa = xx;
                        }
                        if(c.UpdatedDate == null)
                        {
                            models.Add(new CategoryViewModels()
                            {
                                Id = c.Id.ToString(),
                                CategoryNameEng = c.CategoryNameEng,
                                CategoryNameKh = bb,
                                Description = aa,
                                CreatedDate = Convert.ToDateTime(c.CreatedDate).ToString("dd-MMM-yyyy"),
                            });
                        }
                        else
                        {
                            models.Add(new CategoryViewModels()
                            {
                                Id = c.Id.ToString(),
                                CategoryNameEng = c.CategoryNameEng,
                                CategoryNameKh = bb,
                                Description = aa,
                                CreatedDate = Convert.ToDateTime(c.UpdatedDate).ToString("dd-MMM-yyyy"),
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