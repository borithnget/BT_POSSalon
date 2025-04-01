using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
	namespace jotun.Models
	{
	public class EmployeeRoleViewModel
	{
		public int EmployeeId { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public bool IsActive { get; set; }
		public string RoleName { get; set; }
		public List<int> SelectedRoles { get; set; } 
		public List<SelectRoles> Roles { get; set; }
		public List<ShiftScheduleViewModel> ShiftSchedules { get; set; }

		public List<AttendanceViewModel> AttendanceRecords { get; set; }

		public SalaryViewModel SalaryDetails { get; set; }
	}
	public class SelectRoles
	{
		public int RoleId { get; set; }
		public string RoleName { get; set; }	
	}
	public class ShiftScheduleViewModel
	{
		public DateTime ShiftDate { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
	}

	public class AttendanceViewModel
	{
		public DateTime Date { get; set; }
		public string Status { get; set; }
	}

	public class SalaryViewModel
	{
		public decimal BaseSalary { get; set; }
		public decimal Bonus { get; set; }
		public decimal Commission{ get; set; }
		public decimal TotalSalary { get; set; }
		public DateTime SalaryDate { get; set; }
	}
}


