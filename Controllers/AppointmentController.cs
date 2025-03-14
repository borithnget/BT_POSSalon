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
    public class AppointmentController : Controller
    {
        // GET: Appointment
        public ActionResult Index()
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var models = db.tblAppointments.OrderByDescending(s => s.CreatedAt).Where(s => s.IsActive == true)
                    .Select(s => new AppointmentModel()
                    {
                        AppointmentId = s.AppointmentId,
                        CustomerId = s.CustomerId,
                        CustomerName = s.CustomerName,
                        CustomerContact = s.CustomerContact,
                        AppointmentDate = s.AppointmentDate,
                        StatTime = s.StatTime.ToString(),
                        AppointmentStatus = s.AppointmentStatus
                    }).ToList();
                return View(models);
            }

        }
        // GET: Appointment/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Appointment/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        public ActionResult Create(AppointmentModel model)
        {
            try
            {
                // TODO: Add insert logic here
                jotunDBEntities db = new jotunDBEntities();
                tblAppointment appointment = new tblAppointment();
                appointment.AppointmentId = Guid.NewGuid().ToString();
                appointment.AppointmentDate = Convert.ToDateTime(model.AppointmentDate);
                appointment.CustomerId = model.CustomerId;
                appointment.CustomerName = model.CustomerName;
                appointment.CustomerContact = model.CustomerContact;
                appointment.StatTime = TimeSpan.Parse(model.StatTime);
                appointment.AppointmentStatus = "Pending";
                appointment.PriceFull = model.PriceFull;
                appointment.PriceFinal = model.PriceFinal;
                appointment.DiscountAmount = model.DiscountAmount;
                appointment.IsActive = true;
                appointment.CreatedAt = DateTime.Now;
                appointment.CreatedBy = User.Identity.GetUserId();
                db.tblAppointments.Add(appointment);
                db.SaveChanges();
                if (model.service_id != null)
                {
                    for (int i = 0; i < model.service_id.Count(); i++)
                    {
                        tblServiceBooked booked = new tblServiceBooked();
                        booked.Id = Guid.NewGuid().ToString();
                        booked.AppointmentId = appointment.AppointmentId;
                        booked.ServiceId = model.service_id[i];
                        booked.Price = Convert.ToDecimal(model.service_price[i]);
                        db.tblServiceBookeds.Add(booked);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Appointment/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Appointment/Edit/5
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

        // GET: Appointment/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Appointment/Delete/5
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
        [HttpPost]
        public ActionResult GetAppointmentAJAX(string id)
        {
            return Json(new { data = AppointmentModel.GetAppointmentModel(id) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetEventCalendar()
        {
            var events = new List<EventViewModel>
            {
        new EventViewModel { Name = "Product Launch", Date = new DateTime(2025, 5, 1, 10, 0, 0), Description = "New product launch event" },
        new EventViewModel { Name = "Team Meeting", Date = new DateTime(2025, 5, 2, 14, 0, 0), Description = "Discuss project updates" },
        new EventViewModel { Name = "Client Visit", Date = new DateTime(2025, 5, 5, 9, 0, 0), Description = "Visit client for project discussion" }
            };
            var model = new EventCalendarViewModel
            {
                Events = events
            };

            return View(model); 
        }

    }
}