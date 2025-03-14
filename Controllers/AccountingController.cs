using iTextSharp.text;
using jotun.Entities;
using jotun.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class AccountingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
		public ActionResult ExpenseList()
		{
			using (var db = new jotunDBEntities()) 
			{				
				var accountingData = db.tb_Acc_Account
					.Where(a => (bool)a.IsActive) 
					.Join(db.tb_Acc_AccountType,
						  a => a.AccountTypeId,
						  at => at.Id,
						  (a, at) => new { Account = a, AccountType = at }) 
					.Select(joined => new AccountingViewModel	
					{
						Id = joined.Account.Id,
						AccountCode = joined.Account.AccountCode,
						AccountName = joined.Account.AccountName,
						AccountTypeName = joined.AccountType.AccountTypeName,
						Description = joined.Account.Description,
						IsActive = (bool)joined.Account.IsActive,
						
						Expenses = (from ed in db.tb_Acc_Expense_Detail
									join e in db.tb_Acc_Expense on ed.ExpenseId equals e.Id
									where ed.AccountId == joined.Account.Id 
									select new ExpenseViewModel
									{
										Id = ed.Id,
										Date = (DateTime)ed.ExpenseDate, 
										Account = joined.Account.AccountName,
										AccountCode = joined.Account.AccountCode,
										Description = ed.Description,
										SupplierName = ed.SuppplierName, 
										SupplierContact = ed.SupplierContact, 
										Cost = ed.Cost ?? 0
									}).ToList()
					}).ToList();				
				return View(accountingData);
			}
		}
		public ActionResult EditExpense(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var expense = db.tb_Acc_Expense_Detail
					.Where(ed => ed.Id == id)
					.Select(ed => new ExpenseViewModel
					{
						Id = ed.Id,
						Date = (DateTime)ed.ExpenseDate,
						AccountId = (int)ed.AccountId,
						Description = ed.Description,
						SupplierName = ed.SuppplierName,
						SupplierContact = ed.SupplierContact,
						Cost = ed.Cost ?? 0
					})
					.FirstOrDefault();

				if (expense == null)
				{
					return HttpNotFound();
				}
				var accountingViewModel = new AccountingViewModel
				{
					Expenses = new List<ExpenseViewModel> { expense } 
				};
				return PartialView("_EditExpenseForm", accountingViewModel); 
			}
		}
		[HttpPost]
		public JsonResult EditExpense(AccountingViewModel model)
		{
			if (model.Expenses != null && model.Expenses.Any())
			{
				using (var db = new jotunDBEntities())
				{
					foreach (var expense in model.Expenses)
					{
						var existingExpense = db.tb_Acc_Expense_Detail.Find(expense.Id);
						if (existingExpense != null)
						{
							existingExpense.ExpenseDate = expense.Date;
							existingExpense.Cost = expense.Cost;
							existingExpense.SuppplierName = expense.SupplierName;
							existingExpense.SupplierContact = expense.SupplierContact;
							existingExpense.Description = expense.Description;
							existingExpense.UpdatedAt = DateTime.Now;
							existingExpense.UpdatedBy = User.Identity.Name;

							db.SaveChanges();
						}
					}
					return Json(new { success = true, message = "Expense updated successfully." });
				}
			}
			return Json(new { success = false, message = "No expenses found." });
		}
		[HttpPost]
		public JsonResult Delete(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var expense = db.tb_Acc_Expense_Detail.Find(id);
				if (expense != null)
				{
					db.tb_Acc_Expense_Detail.Remove(expense);
					db.SaveChanges();
					return Json(new { success = true });
				}
				return Json(new { success = false });
			}
		}
		public ActionResult CreateExpense()
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var model = new AccountingViewModel
				{
					Accounts = db.tb_Acc_Account.Select(a => new SelectListItem
					{
						Value = a.AccountCode,
						Text = a.AccountCode + " : " + a.AccountName
					}).ToList(),
					Expenses = new List<ExpenseViewModel>(),

					// Initialize expense_Details as an empty string
					expense_Details = string.Empty
				};
				return View(model);
			}
		}
		[HttpPost]
		public ActionResult CreateExpense(AccountingViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					var existingExpense = db.tb_Acc_Expense
											.Where(e => e.Status == "Active")
											.OrderByDescending(e => e.CreatedAt)
											.FirstOrDefault();
					if (existingExpense != null)
					{						
						List<Expense_Detail> expenseDetails = null;
						if (!string.IsNullOrEmpty(model.expense_Details))
						{
							expenseDetails = JsonConvert.DeserializeObject<List<Expense_Detail>>(model.expense_Details);
						}
						foreach (var detail in expenseDetails)
						{			
							string accountCode = detail.Account.Split(':')[0];
							var account = db.tb_Acc_Account.FirstOrDefault(a => a.AccountCode == accountCode);
							int? accountId = account?.Id;

							if (accountId.HasValue)
							{
								var expenseDetailEntity = new tb_Acc_Expense_Detail
								{
									ExpenseId = existingExpense.Id,
									AccountId = accountId.Value, 
									Cost = detail.Cost,
									Description = detail.Description,
									SuppplierName = detail.SupplierName,
									SupplierContact = detail.SupplierContact,
									CreatedAt = DateTime.Now,
									ExpenseDate = DateTime.Now,
									CreatedBy = User.Identity.Name,
									ShopId = 2
								};
								db.tb_Acc_Expense_Detail.Add(expenseDetailEntity);
							}							
						}

						db.SaveChanges();
					}
				}			
			}
			return View(model);
		}
		public ActionResult TopupList()	
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var TopupList = db.tb_Expense_Topup.ToList();
				return View(TopupList);
			}
		}
		[HttpPost]
		public ActionResult CreateTopup(tb_Expense_Topup model)
		{
			if (ModelState.IsValid)
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					tb_Expense_Topup newTopup = new tb_Expense_Topup
					{
						TopupAmount = model.TopupAmount,
						TopupDate = model.TopupDate,
						Remark = model.Remark,
						CreatedAt = DateTime.Now,
						CreatedBy = User.Identity.Name,
						IsActive = true,
					};
					db.tb_Expense_Topup.Add(newTopup);
					db.SaveChanges();
				}
				return RedirectToAction("TopupList");
			}
			return View(model);
		}
		public ActionResult EditTopup(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var topup = db.tb_Expense_Topup
					  .Where(t => t.Id == id)
					  .Select(t => new EditTopupViewModel
					  {
						  Id = t.Id,
						  Remark = t.Remark,
						  TopupAmount = (decimal)t.TopupAmount,
						  TopupDate = (DateTime)t.TopupDate,
					  })
					  .FirstOrDefault();
				if (topup == null)
				{
					return HttpNotFound();
				}
				var accountingViewModel = new AccountingViewModel
				{
					editTop = new List<EditTopupViewModel> { topup }
				};
				return PartialView("_EditTopupForm", accountingViewModel);
			}
		}
		[HttpPost]
		public JsonResult EditTopup(AccountingViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					foreach (var topupModel in model.editTop)
					{
						var topup = db.tb_Expense_Topup.Find(topupModel.Id);

						if (topup == null)
						{
							return Json(new { success = false, message = "Topup record not found." });
						}
						topup.TopupAmount = topupModel.TopupAmount;
						topup.TopupDate = topupModel.TopupDate;
						topup.Remark = topupModel.Remark;
						topup.IsActive = true;
						topup.UpdatedAt = DateTime.Now;
						topup.UpdatedBy = User.Identity.Name;
					}

					db.SaveChanges();
				}
				return Json(new { success = true });
			}
			return Json(new { success = false, message = "Validation failed." });
		}
		public ActionResult ExpenseDiture()
		{
			using (var db = new jotunDBEntities())
			{
				var accountingData = db.tb_Acc_Account
					.Where(a => (bool)a.IsActive)
					.Join(db.tb_Acc_AccountType,
						  a => a.AccountTypeId,
						  at => at.Id,
						  (a, at) => new { Account = a, AccountType = at })
					.Select(joined => new AccountingViewModel
					{
						Id = joined.Account.Id,
						AccountCode = joined.Account.AccountCode,
						AccountName = joined.Account.AccountName,
						AccountTypeName = joined.AccountType.AccountTypeName,
						Description = joined.Account.Description,
						IsActive = (bool)joined.Account.IsActive,

						Expenses = (from ed in db.tb_Acc_Expense_Detail
									join e in db.tb_Acc_Expense on ed.ExpenseId equals e.Id
									where ed.AccountId == joined.Account.Id
									select new ExpenseViewModel
									{
										Id = ed.Id,
										Date = (DateTime)ed.ExpenseDate,
										Account = joined.Account.AccountName,
										AccountCode = joined.Account.AccountCode,
										Description = ed.Description,
										SupplierName = ed.SuppplierName,
										SupplierContact = ed.SupplierContact,
										Cost = ed.Cost ?? 0
									}).ToList()
					}).ToList();
				return View(accountingData);
			}
		}

		public ActionResult AccountType()
        {
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var accountType = db.tb_Acc_AccountType.ToList();
				return View(accountType);
			}
		}
		public ActionResult EditAccountType(int id)
		{
			using (var db = new jotunDBEntities())
			{
				var accountType = db.tb_Acc_AccountType
					.Where(a => a.Id == id)
					.Select(a => new EditAccountTypeViewModel
					{
						Id = a.Id,
						Name = a.AccountTypeName,
						Code = a.AccountTypeCode,
						Description = a.Description,
					})
					.FirstOrDefault();

				if (accountType == null)
				{
					return HttpNotFound();
				}

				var accountingViewModel = new AccountingViewModel
				{
					EditAccountTypes = new List<EditAccountTypeViewModel> { accountType }
				};

				return PartialView("_EditAccountTypeForm", accountingViewModel);
			}
		}
		[HttpPost]
		public JsonResult EditAccountType(AccountingViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{
					foreach (var accountTypeModel in model.EditAccountTypes)
					{
						var accountType = db.tb_Acc_AccountType.Find(accountTypeModel.Id);

						if (accountType == null)
						{
							return Json(new { success = false, message = "Account Type record not found." });
						}
						accountType.AccountTypeCode = accountTypeModel.Code;
						accountType.AccountTypeName = accountTypeModel.Name;
						accountType.Description = accountTypeModel.Description;
						accountType.UpdatedAt = DateTime.Now;
						accountType.UpdatedBy = User.Identity.Name;
					}

					db.SaveChanges();
				}

				return Json(new { success = true });
			}

			return Json(new { success = false, message = "Validation failed." });
		}
		public ActionResult Create(tb_Acc_AccountType newAccountType)
		{
			if (ModelState.IsValid)
			{
				using (var db = new jotunDBEntities())
				{				
					var existingAccountType = db.tb_Acc_AccountType
												.FirstOrDefault(a => a.AccountTypeCode == newAccountType.AccountTypeCode);

					if (existingAccountType != null)
					{	
						newAccountType.AccountTypeCode = existingAccountType.AccountTypeCode;
						newAccountType.AccountTypeName = existingAccountType.AccountTypeName;
						newAccountType.Description = existingAccountType.Description;
						ModelState.AddModelError("AccountTypeCode", "Account Type with this code already exists.");
						return View(newAccountType);
					}
					else
					{
						newAccountType.IsActive = true; 
						newAccountType.CreatedBy = User.Identity.Name; 
						newAccountType.CreatedAt = DateTime.Now;
						newAccountType.ShopId = 2; 
					}				
					db.tb_Acc_AccountType.Add(newAccountType);				
					db.SaveChanges();
				}
				return RedirectToAction("AccountType");
			}		
			return View(newAccountType);
		}
		public ActionResult AccountList()
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var accounts = (from acc in db.tb_Acc_Account
								join accType in db.tb_Acc_AccountType
								on acc.AccountTypeId equals accType.Id into accountGroup
								from accType in accountGroup.DefaultIfEmpty() 
								select new AccountingViewModel
								{
									Id = acc.Id,
									AccountCode = acc.AccountCode,
									AccountName = acc.AccountName,
									AccountTypeName = accType != null ? accType.AccountTypeName : "N/A",
									Description = acc.Description,
								}).ToList();
				return View(accounts);
			}
		}
		public ActionResult EditAccount(int id)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var account = (from acc in db.tb_Acc_Account
							   join accType in db.tb_Acc_AccountType
							   on acc.AccountTypeId equals accType.Id into accountGroup
							   from accType in accountGroup.DefaultIfEmpty()
							   where acc.Id == id
							   select new EditAccountViewModel
							   {
								   Id = acc.Id,
								   AccountCode = acc.AccountCode,
								   AccountName = acc.AccountName,
								   AccountTypeId = (int)acc.AccountTypeId,
								   Description = acc.Description,
							   }).FirstOrDefault();

				if (account == null)
				{
					return HttpNotFound();
				}
				var accountTypes = db.tb_Acc_AccountType
									 .Select(at => new SelectListItem
									 {
										 Value = at.Id.ToString(),
										 Text = at.AccountTypeName
									 })
									 .ToList();
				accountTypes.Insert(0, new SelectListItem
				{
					Value = "0",
					Text = "N/A"
				});
				var accountingViewModel = new AccountingViewModel
				{
					EditAccounts = new List<EditAccountViewModel> { account },
					AccountTypes = accountTypes
				};

				return PartialView("_EditAccountListForm", accountingViewModel);
			}
		}
		[HttpPost]
		public ActionResult EditAccount(AccountingViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					foreach (var editAccount in model.EditAccounts)
					{
						var account = db.tb_Acc_Account.Find(editAccount.Id);

						if (account == null)
						{
							return HttpNotFound();
						}
						account.AccountCode = editAccount.AccountCode;
						account.AccountName = editAccount.AccountName;
						account.Description = editAccount.Description;

						var existingAccountType = db.tb_Acc_AccountType
													.FirstOrDefault(at => at.Id == editAccount.AccountTypeId);

						if (existingAccountType == null)
						{
							var newAccountType = new tb_Acc_AccountType
							{
								AccountTypeName = editAccount.AccountTypeName
							};
							db.tb_Acc_AccountType.Add(newAccountType);
							db.SaveChanges();
							account.AccountTypeId = newAccountType.Id;
						}
						else
						{
							account.AccountTypeId = existingAccountType.Id;
						}
						account.UpdatedAt = DateTime.Now;
						account.UpdatedBy = User.Identity.Name;
						db.Entry(account).State = EntityState.Modified;
					}
					db.SaveChanges();
				}
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false, message = "Validation failed." });
			}
		}
	}
}