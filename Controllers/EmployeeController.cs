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
					.Include("tblEmployeeRoles.tblRole")
					.Include("tblShiftSchedules")
					.Include("tblAttendances")
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
					RoleName = string.Join(", ", employee.tblEmployeeRoles
						  .Select(er => er.tblRole.RoleName)
						  .DefaultIfEmpty("No Role Assigned")),
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
					// Create a new employee
					var employee = new tblEmployee
					{
						Name = model.Name,
						Email = model.Email,
						Phone = model.Phone,
						IsActive = true
					};
					db.tblEmployees.Add(employee);
					db.SaveChanges(); 
					var shiftSchedule = new tblShiftSchedule
					{
						EmployeeId = employee.EmployeeId,
						ShiftDate = model.ShiftDate,
						StartTime = model.ShiftStartTime,
						EndTime = model.ShiftEndTime
					};
					db.tblShiftSchedules.Add(shiftSchedule);
					var salary = new tblSalary
					{
						EmployeeId = employee.EmployeeId,
						BaseSalary = model.BaseSalary,
						Commission = model.Commission,
						Bonus = model.Bonus,
						SalaryDate = model.SalaryDate
					};
					db.tblSalaries.Add(salary);
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
		public ActionResult Edit(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var employee = db.tblEmployees
					.Include("tblEmployeeRoles.tblRole")
					.Include("tblShiftSchedules")
					.Include("tblAttendances")
					.Include("tblSalaries")
					.FirstOrDefault(e => e.EmployeeId == id);

				if (employee == null)
					return HttpNotFound();

				// Get available roles for the dropdown list
				var roles = db.tblRoles.ToList();

				var viewModel = new EmployeeRoleViewModel
				{
					EmployeeId = employee.EmployeeId,
					Name = employee.Name,
					Email = employee.Email,
					Phone = employee.Phone,
					IsActive = employee.IsActive ?? false,
					SelectedRoles = employee.tblEmployeeRoles
						.Where(er => er.RoleId.HasValue)
						.Select(er => er.RoleId.Value) 
						.ToList(),
					AvailableRoles = roles.Select(r => new SelectRoles
					{
						RoleId = r.RoleId,
						RoleName = r.RoleName
					}).ToList(),
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
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(EmployeeRoleViewModel model)
		{
			if (!ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{
					model.AvailableRoles = db.tblRoles
						.Select(r => new SelectRoles { RoleId = r.RoleId, RoleName = r.RoleName })
						.ToList();
				}
				return View(model);
			}
			using (var db = new jotunDBEntities())
			{
				var employee = db.tblEmployees
					.Include("tblEmployeeRoles")
					.Include("tblShiftSchedules")
					.Include("tblAttendances")
					.Include("tblSalaries")
					.FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

				if (employee == null)
					return HttpNotFound();
				employee.Name = model.Name;
				employee.Email = model.Email;
				employee.Phone = model.Phone;
				employee.IsActive = model.IsActive;			
				employee.tblEmployeeRoles.Clear();
				if (model.SelectedRoles != null)
				{
					foreach (var roleId in model.SelectedRoles)
					{
						employee.tblEmployeeRoles.Add(new tblEmployeeRole { RoleId = roleId });
					}
				}
				employee.tblShiftSchedules.Clear();
				if (model.ShiftSchedules != null)
				{
					foreach (var shift in model.ShiftSchedules)
					{
						employee.tblShiftSchedules.Add(new tblShiftSchedule
						{
							ShiftDate = shift.ShiftDate,
							StartTime = shift.StartTime,
							EndTime = shift.EndTime
						});
					}
				}		
				employee.tblAttendances.Clear();
				if (model.AttendanceRecords != null)
				{
					foreach (var attendance in model.AttendanceRecords)
					{
						employee.tblAttendances.Add(new tblAttendance
						{
							Date = attendance.Date,
							Status = attendance.Status
						});
					}
				}			
				var existingSalary = db.tblSalaries.FirstOrDefault(s => s.EmployeeId == employee.EmployeeId);
				if (existingSalary != null)
				{
					existingSalary.BaseSalary = model.SalaryDetails.BaseSalary;
					existingSalary.Bonus = model.SalaryDetails.Bonus;
					existingSalary.Commission = model.SalaryDetails.Commission;
					existingSalary.SalaryDate = model.SalaryDetails.SalaryDate;
				}
				else if (model.SalaryDetails != null)
				{
					db.tblSalaries.Add(new tblSalary
					{
						EmployeeId = employee.EmployeeId,
						BaseSalary = model.SalaryDetails.BaseSalary,
						Bonus = model.SalaryDetails.Bonus,
						Commission = model.SalaryDetails.Commission,
						SalaryDate = model.SalaryDetails.SalaryDate
					});
				}

				db.SaveChanges();
			}

			return RedirectToAction("Index");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Delete(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var employee = db.tblEmployees
					.Include("tblEmployeeRoles")
					.Include("tblShiftSchedules")
					.Include("tblAttendances")
					.Include("tblSalaries")
					.FirstOrDefault(e => e.EmployeeId == id);

				if (employee == null)
				{
					return HttpNotFound();
				}
				db.tblEmployeeRoles.RemoveRange(employee.tblEmployeeRoles);
				db.tblShiftSchedules.RemoveRange(employee.tblShiftSchedules);
				db.tblAttendances.RemoveRange(employee.tblAttendances);
				db.tblSalaries.RemoveRange(employee.tblSalaries);			
				db.tblEmployees.Remove(employee);
				db.SaveChanges();
				return Json(new { success = true });
			}
		}
	}
}