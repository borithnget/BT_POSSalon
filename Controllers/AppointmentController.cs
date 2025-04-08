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
    public class AppointmentController : BaseController
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
                        AppointmentStatus = s.AppointmentStatus,
                        PriceFinal = s.PriceFinal,
						EmployeeId = (int)s.EmployeeId, 
						Name = db.tblEmployees
					.Where(e => e.EmployeeId == s.EmployeeId)
					.Select(e => e.Name)
					.FirstOrDefault()
					}).ToList();
                return View(models);
            }

        }
		// GET: Appointment/Details/5
		public ActionResult Detail(Guid appointmentId)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				string appointmentIdString = appointmentId.ToString();
				var appointment = db.tblAppointments
						   .FirstOrDefault(a => a.AppointmentId == appointmentIdString);
				if (appointment == null)
				{
					return HttpNotFound();
				}
				var model = new AppointmentModel
				{
					AppointmentId = appointment.AppointmentId,
					AppointmentDate = appointment.AppointmentDate,
					CustomerId = appointment.CustomerId,
					CustomerName = appointment.CustomerName,
					CustomerContact = appointment.CustomerContact,
					StatTime = appointment.StatTime.HasValue ? appointment.StatTime.Value.ToString(@"hh\:mm") : "",
					AppointmentStatus = appointment.AppointmentStatus,
					PriceFull = appointment.PriceFull,
					PriceFinal = appointment.PriceFinal,
					DiscountAmount = appointment.DiscountAmount,
					IsActive = appointment.IsActive,
					EmployeeId = (int)appointment.EmployeeId
				};

				ViewBag.Employees = (from e in db.tblEmployees
									 join s in db.tblShiftSchedules on e.EmployeeId equals s.EmployeeId
									 where s.IsAssigned == false || s.IsAssigned == null || e.EmployeeId == appointment.EmployeeId
									 select new SelectListItem
									 {
										 Value = e.EmployeeId.ToString(),
										 Text = e.Name,
										 Selected = e.EmployeeId == appointment.EmployeeId
									 }).ToList();

				return View(model);
			}
		}
		// GET: Appointment/Create
		public ActionResult Create()
        {
			using (jotunDBEntities db = new jotunDBEntities())
			{
				ViewBag.Employees = (from e in db.tblEmployees
									 join s in db.tblShiftSchedules on e.EmployeeId equals s.EmployeeId
									 where s.IsAssigned == false || s.IsAssigned == null
									 select new SelectListItem
									 {
										 Value = e.EmployeeId.ToString(),
										 Text = e.Name
									 })
					 .ToList();
			}
			return View();
        }
		public JsonResult GetShiftTime(int employeeId)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var shift = db.tblShiftSchedules
							  .Where(s => s.EmployeeId == employeeId)
							  .Select(s => new
							  {
								  s.StartTime,
								  s.EndTime
							  })
							  .FirstOrDefault();

				if (shift != null)
				{
					return Json(new
					{
						StartTime = shift.StartTime.HasValue ? FormatTime(shift.StartTime.Value) : "N/A",
						EndTime = shift.EndTime.HasValue ? FormatTime(shift.EndTime.Value) : "N/A"
					}, JsonRequestBehavior.AllowGet);
				}

				return Json(null, JsonRequestBehavior.AllowGet);
			}
		}
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
                appointment.EmployeeId = model.EmployeeId;
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
				var shiftSchedule = db.tblShiftSchedules.FirstOrDefault(s => s.EmployeeId == model.EmployeeId);
				if (shiftSchedule != null)
				{
					shiftSchedule.IsAssigned = true;
					db.SaveChanges();
				}
				return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }
		// GET: Appointment/Edit/5
		public ActionResult Edit(Guid appointmentId)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				string appointmentIdString = appointmentId.ToString();
				var appointment = db.tblAppointments
						   .FirstOrDefault(a => a.AppointmentId == appointmentIdString);
				if (appointment == null)
				{
					return HttpNotFound();
				}
				var model = new AppointmentModel
				{
					AppointmentId = appointment.AppointmentId,
					AppointmentDate = appointment.AppointmentDate,
					CustomerId = appointment.CustomerId,
					CustomerName = appointment.CustomerName,
					CustomerContact = appointment.CustomerContact,
					StatTime = appointment.StatTime.HasValue ? appointment.StatTime.Value.ToString(@"hh\:mm") : "",
					AppointmentStatus = appointment.AppointmentStatus,
					PriceFull = appointment.PriceFull,
					PriceFinal = appointment.PriceFinal,
					DiscountAmount = appointment.DiscountAmount,
					IsActive = appointment.IsActive,
					EmployeeId = (int)appointment.EmployeeId
				};

				ViewBag.Employees = (from e in db.tblEmployees
									 join s in db.tblShiftSchedules on e.EmployeeId equals s.EmployeeId
									 where s.IsAssigned == false || s.IsAssigned == null || e.EmployeeId == appointment.EmployeeId
									 select new SelectListItem
									 {
										 Value = e.EmployeeId.ToString(),
										 Text = e.Name,
										 Selected = e.EmployeeId == appointment.EmployeeId
									 }).ToList();

				return View(model);
			}
		}
		// POST: Appointment/Edit/5
		[HttpPost]
		public ActionResult Edit(AppointmentModel model)
		{
			try
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					var appointment = db.tblAppointments.Find(model.AppointmentId);
					if (appointment == null)
					{
						return HttpNotFound();
					}
					var employeeExists = db.tblEmployees.Any(e => e.EmployeeId == model.EmployeeId);
					if (!employeeExists)
					{				
						ModelState.AddModelError("EmployeeId", "The selected employee does not exist.");
						return View(model);
					}
					if (model.CustomerId == null)
					{						
						model.CustomerId = Guid.NewGuid().ToString(); 
					}
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
					appointment.EmployeeId = model.EmployeeId;	
					appointment.UpdatedAt = DateTime.UtcNow;
					appointment.UpdatedBy = User.Identity.Name;	
					var previousShift = db.tblShiftSchedules.FirstOrDefault(s => s.EmployeeId == appointment.EmployeeId);
					if (previousShift != null)
					{
						previousShift.IsAssigned = false; 
					}

					var newShift = db.tblShiftSchedules.FirstOrDefault(s => s.EmployeeId == model.EmployeeId);
					if (newShift != null)
					{
						newShift.IsAssigned = true; 
					}

					db.SaveChanges();
				}
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				return View(model);
			}
		}
		[HttpPost]
		public ActionResult CancelAppointment(Guid appointmentId, string cancelReason)
		{
			using (var db = new jotunDBEntities())
			{
				string appointmentIdString = appointmentId.ToString();
				var appointment = db.tblAppointments
						   .FirstOrDefault(a => a.AppointmentId == appointmentIdString);

				if (appointment == null)
				{
					return HttpNotFound();
				}			
				appointment.AppointmentStatus = "Cancelled"; 
				appointment.CanceledReason = cancelReason;			
				appointment.IsActive = false;
				var shiftSchedule = db.tblShiftSchedules.FirstOrDefault(s => s.EmployeeId == appointment.EmployeeId);
				if (shiftSchedule != null)
				{
					shiftSchedule.IsAssigned = false;
					db.SaveChanges();
				}
				db.SaveChanges();
			}		
			return RedirectToAction("Index");
		}		
		public ActionResult ReleaseAndRedirectToInvoice(Guid appointmentId)
		{
			using (var db = new jotunDBEntities())
			{
				string appointmentIdString = appointmentId.ToString();			
				var appointment = db.tblAppointments
							   .FirstOrDefault(a => a.AppointmentId == appointmentIdString);
				if (appointment != null)
				{			
					appointment.AppointmentStatus = "Completed";
					appointment.IsActive = false;
					var shiftSchedule = db.tblShiftSchedules.FirstOrDefault(s => s.EmployeeId == appointment.EmployeeId);
					if (shiftSchedule != null)
					{
						shiftSchedule.IsAssigned = false;  
					}

					db.SaveChanges(); 
				}
			}
			return RedirectToAction("Index", "Coffee", new { appointmentId = appointmentId });
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
		private string FormatTime(TimeSpan time)
		{
			return DateTime.Today.Add(time).ToString("hh:mm tt");
		}

	}
}