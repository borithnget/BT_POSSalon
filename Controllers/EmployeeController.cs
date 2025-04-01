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
    public class EmployeeController : Controller
    {
		// GET: Employee
		public ActionResult Index()
		{
			using (var db = new jotunDBEntities())
			{
				var employeeRoles = db.tblEmployees
									  .Join(db.tblEmployeeRoles,
											employee => employee.EmployeeId,
											employeeRole => employeeRole.EmployeeId,
											(employee, employeeRole) => new { employee, employeeRole })
									  .Join(db.tblRoles,
											employeeRole => employeeRole.employeeRole.RoleId,
											role => role.RoleId,
											(employeeRole, role) => new
											{
												employeeRole.employee.EmployeeId,
												employeeRole.employee.Name,
												employeeRole.employee.Email,
												employeeRole.employee.Phone,
												employeeRole.employee.IsActive,
												role.RoleName
											})
									  .ToList();

				if (employeeRoles.Count == 0)
					return HttpNotFound();			
				var groupedEmployeeRoles = employeeRoles
					.GroupBy(er => er.EmployeeId)
					.Select(g => new EmployeeRoleViewModel
					{
						EmployeeId = g.Key,
						Name = g.First().Name,
						Email = g.First().Email,
						Phone = g.First().Phone,
						IsActive = g.First().IsActive ?? false,					
						RoleName = string.Join(", ", g.Select(er => er.RoleName))
					})
					.ToList();

				return View(groupedEmployeeRoles);
			}
		}
		public ActionResult Detail(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var employee = db.tblEmployees
					.Include("tblEmployeeRoles.tblRoles")
					.Include("tblShiftSchedule")
					.Include("tblAttendance")
					.Include("tblSalaries")
					.FirstOrDefault(e => e.EmployeeId == id);

				if (employee == null)
					return HttpNotFound();
				var viewModel = new EmployeeRoleViewModel
				{
					EmployeeId = employee.EmployeeId,
					Name = employee.Name,
					Email = employee.Email,
					Phone = employee.Phone,
					IsActive = employee.IsActive ?? false,
					RoleName = employee.tblEmployeeRoles.FirstOrDefault()?.tblRole.RoleName ?? "No Role Assigned",
					ShiftSchedules = employee.tblShiftSchedules
						.Select(s => new ShiftScheduleViewModel
						{
							ShiftDate = (DateTime)s.ShiftDate,
							StartTime = (TimeSpan)s.StartTime,
							EndTime = (TimeSpan)s.EndTime
						}).ToList(),
					AttendanceRecords = employee.tblAttendances
						.Select(a => new AttendanceViewModel
						{
							Date = (DateTime)a.Date,
							Status = a.Status
						}).ToList(),
					SalaryDetails = employee.tblSalaries
						.Select(s => new SalaryViewModel
						{
							BaseSalary = (decimal)s.BaseSalary,
							Bonus = (decimal)s.Bonus,
							Commission = (decimal)s.Commission,						
							SalaryDate = (DateTime)s.SalaryDate
						}).FirstOrDefault() ?? new SalaryViewModel()
				};

				return View(viewModel);
			}
		}
		public ActionResult Create()
		{
			using (var db = new jotunDBEntities())
			{
				var roles = db.tblRoles.Select(r => new SelectRoles
				{
					RoleId = r.RoleId,
					RoleName = r.RoleName
				}).ToList();

				var viewModel = new EmployeeRoleViewModel
				{
					Roles = roles,
					SelectedRoles = new List<int>() 
				};
				return View(viewModel);
			}
		}
		// POST: Employee/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(EmployeeRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{				
					var employee = new tblEmployee
					{
						Name = model.Name,
						Email = model.Email,
						Phone = model.Phone,
						IsActive = model.IsActive
					};					
					db.tblEmployees.Add(employee);
					db.SaveChanges();
					foreach (var roleId in model.SelectedRoles)
					{
						db.tblEmployeeRoles.Add(new tblEmployeeRole
						{
							EmployeeId = employee.EmployeeId,
							RoleId = roleId
						});
					}
					db.SaveChanges();
				}
				return RedirectToAction("Index"); 
			}
			using (var db = new jotunDBEntities())
			{
				var roles = db.tblRoles.Select(r => new SelectRoles
				{
					RoleId = r.RoleId,
					RoleName = r.RoleName
				}).ToList();

				ViewBag.Roles = new SelectList(roles, "RoleId", "RoleName");
			}	
			return View(model);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(tblEmployee employee)
		{
			if (ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{
					db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
					db.SaveChanges();
				}
				return RedirectToAction("Index");
			}
			return View(employee);
		}
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var employee = db.tblEmployees.Find(id);
				if (employee != null)
				{
					db.tblEmployees.Remove(employee);
					db.SaveChanges();
				}
				return RedirectToAction("Index");
			}
		}
	}
}