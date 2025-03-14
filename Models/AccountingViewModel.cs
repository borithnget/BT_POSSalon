using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace jotun.Models
{
	public class AccountingViewModel
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public string SupplierName { get; set; }
		public string SupplierContact { get; set; }
		public string AccountCode { get; set; }
		public string AccountName { get; set; }
		public string AccountTypeName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
		public List<ExpenseViewModel> Expenses { get; set; }
		public string expense_Details { get; set; }
		public List<SelectListItem> Accounts { get; set; }
		public List<SelectListItem> AccountTypes { get; set; }
		public List<EditTopupViewModel> editTop {  get; set; }
		public List<EditAccountTypeViewModel> EditAccountTypes { get; set; }
		public List<EditAccountViewModel> EditAccounts { get; set; }
	}
	public class ExpenseViewModel
	{
		public int Id { get; set; }
		public int AccountId { get; set; }
		public DateTime Date { get; set; }
		public string Account { get; set; } 
		public string Description { get; set; } 
		public string SupplierName { get; set; } 
		public string SupplierContact { get; set; } 
		public decimal Cost { get; set; }
		public string AccountCode { get; set; }
	}
	public class Expense_Detail
	{
		public int ExpenseDetailId { get; set; }
		public int ExpenseId { get; set; } 
		public decimal Cost { get; set; }
		public string Description { get; set; }
		public string SupplierName { get; set; }
		public string SupplierContact { get; set; }
		public string Account { get; set; }
	}
	public class EditTopupViewModel
	{
		public int Id { get; set; }
		public string Remark { get; set; }
		public decimal TopupAmount { get; set; }
		public DateTime TopupDate { get; set; }
	}
	public class EditAccountTypeViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
	}
	public class EditAccountViewModel
	{
		public int Id { get; set; }
		public string AccountCode { get; set; }
		public int AccountTypeId { get; set; }
		public string AccountName { get; set; }
		public string AccountTypeName { get; set; }
		public string Description { get; set; }
	}
}