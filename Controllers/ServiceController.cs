using jotun.Entities;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class ServiceController : Controller
    {
        // GET: Service
        public ActionResult Index()
        {
            using(jotunDBEntities db=new jotunDBEntities())
            {
                var results = db.tblServices.OrderBy(s => s.Name).Where(s => s.IsActive == true).ToList();
                return View(results);
            }
        }

        // GET: Service/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Service/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Service/Create
        [HttpPost]
        public ActionResult Create(tblService service)
        {
            try
            {
                // TODO: Add insert logic here
                //jotunDBEntities db = new jotunDBEntities();
                //service.ServiceId = Guid.NewGuid().ToString();
                //service.IsActive = true;
                //service.CreatedAt = DateTime.Now;
                //service.UpdatedBy = User.Identity.GetUserId();
                //service.UpdatedAt = DateTime.Now;
                //service.CreatedBy= User.Identity.GetUserId();
                //db.tblServices.Add(service);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Service/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Service/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Service/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Service/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
