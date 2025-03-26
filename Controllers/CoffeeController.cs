using iTextSharp.text.pdf;
using jotun.Entities;
using jotun.Functions;
using jotun.Models;
using jotun.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Windows.Input;
using static jotun.Models.ProductViewModels;

namespace jotun.Controllers
{
	[Authorize]
	public class CoffeeController : Controller
	{
		private readonly PermissionService _permissionService;

		public CoffeeController(PermissionService permissionService)
		{
			_permissionService = permissionService;
		}
		public ActionResult SomeAction()
		{
			// Check if the user is authenticated
			if (User.Identity.IsAuthenticated)
			{
				var user = User.Identity;
				ApplicationDbContext context = new ApplicationDbContext();
				var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
				var roles = userManager.GetRoles(user.GetUserId());

				if (roles.Contains("SuperAdmin") || roles.Contains("Administrator") || roles.Contains("Admin"))
				{

					ViewBag.CanCreateCustomerType = true;
				}
				else
				{

					ViewBag.CanCreateCustomerType = false;
				}
			}
			else
			{

				ViewBag.CanCreateCustomerType = false;
			}

			return View();
		}
		// GET: Coffee
		public ActionResult Index(string appointmentId="")
		{
			SaleViewModels model = new SaleViewModels();
			ViewData["HideHeader"] = true;
			ViewBag.AppointmentId = appointmentId;
			//if(!string.IsNullOrEmpty(appointmentId))
			//{
			//	model.appointment = AppointmentModel.GetAppointmentModel(appointmentId);
			//}
			return View();
		}
		public ActionResult GetData()
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				List<tblProduct> employeeList = db.tblProducts.ToList<tblProduct>();
				return Json(new { data = employeeList }, JsonRequestBehavior.AllowGet);
			}
		}
		public JsonResult GetProductData(ProductViewModels model, string categoryFilter = null, int pageNumber = 1, int pageSize = 100, int? selectedCustomerTypeId = null, bool? isService = null)
		{
			string descrip = "";
			List<ProductViewModels> models = new List<ProductViewModels>();

			using (jotunDBEntities db = new jotunDBEntities())
			{
				int skip = (pageNumber - 1) * pageSize;
				if (selectedCustomerTypeId.HasValue)
				{
					Session["SelectedCustomerTypeId"] = selectedCustomerTypeId;
				}
				else
				{
					selectedCustomerTypeId = Session["SelectedCustomerTypeId"] as int?;
				}
				if (!selectedCustomerTypeId.HasValue)
				{
					selectedCustomerTypeId = 1;
				}
				var query = db.tblProducts.Where(p => p.Status == 1);

				if (isService.HasValue)
				{
					query = query.Where(p => p.Service == isService.Value);
				}
				/*var query = db.tblProducts
							  .Where(p => p.Status == 1 && p.Service==isService );*/

				if (!string.IsNullOrEmpty(categoryFilter))
				{
					var categoryIds = categoryFilter.Split(',').ToList();

					if (selectedCustomerTypeId.HasValue && selectedCustomerTypeId.Value != 1)
					{
						query = query.Where(p =>
							categoryIds.Contains(p.CategoryId.ToString()) &&
							db.tblProductByUnits.Any(pu =>
								pu.ProductID == p.Id &&
								pu.CustomerType == selectedCustomerTypeId)
						);
					}
					else
					{
						query = query.Where(p => categoryIds.Contains(p.CategoryId.ToString()));
					}
				}
				var productData = query
					.GroupJoin(
						db.tblProductByUnits.Where(pu => selectedCustomerTypeId.HasValue && pu.CustomerType == selectedCustomerTypeId),
						p => p.Id,
						pu => pu.ProductID,
						(p, productUnits) => new
						{
							Product = p,
							ProductUnit = productUnits.FirstOrDefault()
						}
					)
					.OrderByDescending(p => p.Product.CreatedDate)
					.Skip(skip)
					.Take(pageSize)
					.ToList();

				foreach (var p in productData)
				{
					var product = p.Product;
					var productUnit = p.ProductUnit;
					var category = db.tblCategories.FirstOrDefault(c => c.Id == product.CategoryId);
					var categoryss = category?.CategoryNameEng ?? category?.CategoryNameKh;
					var date = product.UpdatedDate ?? product.CreatedDate;
					var dd = product.Description ?? descrip;

					double? priceInStock = product.PriceInStock;

					if (selectedCustomerTypeId.HasValue && selectedCustomerTypeId.Value != 1 && productUnit != null)
					{
						priceInStock = (double?)(productUnit.Price) ?? product.PriceInStock;
					}
					models.Add(new ProductViewModels()
					{
						Id = product.Id.ToString(),
						ProductNo = product.ProductNo,
						ProductCode = product.ProductCode,
						CategoryName = categoryss,
						ProductName = product.ProductName,
						Description = product.Description,
						ProductImage = product.ProductImage,
						QuantityInStock = Convert.ToString(product.QuantityInStock),
						PriceInStock = (priceInStock.HasValue && priceInStock.Value % 1 == 0)
							? priceInStock.Value.ToString("F0")
							: Math.Round(priceInStock.Value, 2, MidpointRounding.AwayFromZero).ToString("F2"),
						CreatedDate = Convert.ToDateTime(date).ToString("dd-MMM-yyyy"),
						quantity_alert = Convert.ToString(product.quantity_alert),
						isService = product.Service == true ? 1 : 0,
					}) ;
				}
			}
			return Json(models, JsonRequestBehavior.AllowGet);
		}
		public JsonResult GetProductAttributes(string productId)
		{
			using (var db = new jotunDBEntities())
			{
				var attributes = db.tblProductAttributes
					.Where(a => a.ProductId == productId)
					.Select(a => new
					{
						a.AttributeName,
						a.AttributeValue
					})
					.ToList();
				return Json(attributes, JsonRequestBehavior.AllowGet);
			}
		}
		public JsonResult GetCategories(bool? isService)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var categories = db.tblCategories
								   .Where(c => c.Status == 1 &&
										  (!isService.HasValue || c.Service == isService))
								   .Select(c => new { c.Id, c.CategoryNameEng, c.CategoryNameKh })
								   .ToList();
				return Json(categories, JsonRequestBehavior.AllowGet);
			}
		}
		public JsonResult GetCustomerTypes()
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				var customerTypes = db.tblCustomerTypes
									  .Select(ct => new { ct.Id, ct.customer_type })
									  .ToList();
				return Json(customerTypes, JsonRequestBehavior.AllowGet);
			}
		}
		public JsonResult GetDiscount(int? selectedCustomerTypeId = null)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				if (selectedCustomerTypeId.HasValue)
				{
					var customerType = db.tblCustomerTypes
										 .Where(ct => ct.Id == selectedCustomerTypeId)
										 .Select(ct => new { ct.discount, ct.StoreDiscount })
										 .FirstOrDefault();

					if (customerType != null)
					{
						return Json(new
						{
							success = true,
							discount = customerType.discount,
							storeDiscount = customerType.StoreDiscount
						}, JsonRequestBehavior.AllowGet);
					}
				}
				return Json(new { success = false, message = "Customer type not found" }, JsonRequestBehavior.AllowGet);
			}
		}
		[HttpGet]
		public ActionResult CreateCustomerType()
		{
			return View();
		}
		[HttpPost]
		public JsonResult CreateCustomerType(string CustomerType, decimal? DiscountPercentage, decimal? StoreDiscount)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				using (var transaction = db.Database.BeginTransaction())
				{
					try
					{
						var newCustomerType = new tblCustomerType
						{
							customer_type = CustomerType,
							discount = (DiscountPercentage ?? 0) / 100,
							StoreDiscount = StoreDiscount ?? 0
						};
						db.tblCustomerTypes.Add(newCustomerType);
						db.SaveChanges();
						var products = db.tblProducts
										 .Where(p => p.CreatedDate.HasValue && p.CreatedDate.Value.Year == 2025)
										 .ToList();
						foreach (var product in products)
						{
							decimal adjustedPrice = (decimal)product.PriceInStock;

							if (DiscountPercentage.HasValue && DiscountPercentage.Value > 0)
							{
								adjustedPrice -= adjustedPrice * (DiscountPercentage.Value / 100m);
							}
							if (StoreDiscount.HasValue && StoreDiscount.Value > 0)
							{
								adjustedPrice -= StoreDiscount.Value;
							}
							if (adjustedPrice < 0)
							{
								adjustedPrice = 0;
							}
							var newProductByUnit = new tblProductByUnit
							{
								Id = Guid.NewGuid().ToString(),
								ProductID = product.Id,
								CustomerType = newCustomerType.Id,
								Price = adjustedPrice,
								CreatedAt = DateTime.Now,
								IsActive = true
							};

							db.tblProductByUnits.Add(newProductByUnit);
						}
						db.SaveChanges();
						transaction.Commit();

						return Json(new { success = true, message = "Customer Type Added Successfully, Products Updated." });
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						return Json(new { success = false, message = "Error: " + ex.Message });
					}
				}
			}
		}
		public JsonResult GetAllTables()
		{
			using (var db = new jotunDBEntities())
			{
				var tables = db.tblTables.ToList();
				return Json(new { success = true, data = tables }, JsonRequestBehavior.AllowGet);
			}
		}
		[HttpPost]
		public ActionResult CustomerLeaves(string tableNumber)
		{
			try
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					var table = db.tblTables.FirstOrDefault(t => t.TableNumber == tableNumber);

					if (table == null)
						return Json(new { success = false, message = "Table not found." });

					table.IsAvailable = true;
					db.SaveChanges();

					return Json(new
					{
						success = true,
						message = "Table released successfully.",
						table = new
						{
							TableId = table.TableId,
							TableNumber = table.TableNumber,
							Decription = table.Decription ?? "No description",
							People = table.People
						}
					});
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
		[HttpPost]
		public ActionResult SelectTable(SaveSaleRequest request)
		{
			try
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					var selectedTable = db.tblTables.FirstOrDefault(t => t.TableNumber == request.TableNumber);

					if (selectedTable == null || selectedTable.IsAvailable == false)
					{
						return Json(new { success = false, message = "Table is not available." });
					}

					selectedTable.IsAvailable = false;
					db.SaveChanges();

					return Json(new { success = true });
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}
		public JsonResult GetAllService()
		{
			using (var db = new jotunDBEntities())
			{
				var serviceTypes = db.tblServiceTypes
									 .Select(s => new { s.ServiceTypeId, s.Name, s.Description, s.IsActive })
									 .ToList();
				return Json(new { success = true, data = serviceTypes }, JsonRequestBehavior.AllowGet);
			}
		}		
		public JsonResult GetServicesByServiceType(Guid? serviceTypeId)
		{
			using (var db = new jotunDBEntities())
			{
				var services = db.tblServices
								 .Where(s => s.ServiceTypeId == serviceTypeId)
								 .Select(s => new
								 {
									 s.ServiceId,
									 s.Name,
									 s.Description,
									 s.Price,
									 s.IsActive
								 }).ToList();
				return Json(new { success = true, data = services }, JsonRequestBehavior.AllowGet);
			}
		}
		public JsonResult GetProductsForSelectedServices(List<Guid> serviceIds)
		{
			using (var db = new jotunDBEntities())
			{
				var serviceProducts = db.tblServiceProducts
					.Where(sp => serviceIds.Contains(sp.ServiceId))
					.Join(db.tblProducts,
						  sp => sp.ProductId,
						  p => p.Id,
						  (sp, p) => new
						  {
							  p.Id,
							  p.ProductName,
							  p.Description,
							  p.PriceInStock,
							  p.ProductImage,
							  sp.Quantity, 
							  sp.Quality,
							  ServiceName = db.tblServices
									 .Where(s => s.ServiceId == sp.ServiceId)
									 .Select(s => s.Name)
									 .FirstOrDefault()
						  })
					.ToList();

				if (!serviceProducts.Any())
					return Json(new { success = false, message = "No products found." }, JsonRequestBehavior.AllowGet);

				return Json(new { success = true, data = serviceProducts }, JsonRequestBehavior.AllowGet);
			}
		}
		[HttpPost]
	public ActionResult CreateSale([FromBody] SaveSaleRequest request)
	{
		try
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				tblSale client = request.client;
				SaleViewModels model = request.model;
				ProductViewModels pmodel = request.pmodel;
				SaleDetailViewModel cliented = request.cliented;
				CommonFunctions functions = new CommonFunctions();
				client.Amount = Convert.ToDouble(model.Amount);
				client.CustomerId = model.CustomerId;
				client.Discount = Convert.ToDouble(model.Discount);
				client.RevicedFromCustomer = Convert.ToDouble(model.RevicedFromCustomer);
				client.ReceivedByABA = model.ReceivedByABA;
				var discount = client.Discount;
				client.Owe = (client.Amount) - (client.RevicedFromCustomer + Convert.ToDouble(client.ReceivedByABA));
				if (client.Owe < 0)
				{
					client.Owe = 0;
				}
				var ids = Guid.NewGuid().ToString();
				client.Id = ids;
				client.CreatedDate = CommonFunctions.ToLocalTime(DateTime.Now);
				client.UpdatedDate = CommonFunctions.ToLocalTime(DateTime.Now);
				client.SaleAmount = Convert.ToDecimal(client.Amount);
				client.SaleDeposit = Convert.ToDecimal(client.ReceivedByABA) + Convert.ToDecimal(client.RevicedFromCustomer);
				client.SaleImage = string.IsNullOrEmpty(client.SaleImage)
					? "/Images/defaultimage.jpg"
					: "/Images/" + model.SaleImage;
				var getIsLocation = (from l in db.tblCustomer_location
									 where l.customer_id == client.CustomerId && l.location == model.LocationText
									 select l).FirstOrDefault();
				if (getIsLocation == null)
				{
					tblCustomer_location location = new tblCustomer_location();
					location.location = model.LocationText;
					location.updated_date = CommonFunctions.ToLocalTime(DateTime.Now);
					location.created_date = CommonFunctions.ToLocalTime(DateTime.Now);
					location.customer_id = model.CustomerId;
					location.status = true;
					db.tblCustomer_location.Add(location);
					db.SaveChanges();
					client.customer_location = location.id;
				}
				else
				{
					if (client.customer_location != null)
					{
						client.customer_location = getIsLocation.id;
					}
					else
					{
						client.customer_location = Convert.ToInt32(model.CustomerLocation);
					}
				}
				if (Convert.ToDouble(model.RevicedFromCustomer) > 0 || Convert.ToDouble(model.ReceivedByABA) > 0)
				{
					client.UpdatedDate = CommonFunctions.ToLocalTime(DateTime.Now);
				}
				client.Status = 1;
				if (client.Owe <= 0)
				{
					client.InvoiceStatus = "Paid";
				}
				else
				{
					client.InvoiceStatus = "Not Paid";
				}
				db.tblSales.Add(client);
				db.SaveChanges();
				decimal totalCostGoodSold = 0;
				decimal totalDepositCoGs = 0;
				decimal totalReceivedCostSale = 0;
				decimal percentageCashRecived = client.SaleDeposit > 0 ? Convert.ToDecimal((client.SaleDeposit / client.SaleAmount) * 100) : 0;

				if (model.GetDetail != null)
				{
					foreach (SaleDetailViewModel item in model.GetDetail)
					{
						decimal productCostGoodSold = 0;
						var i = (from i1 in db.tblUnits
								 where i1.Id == item.UNit
								 select i1).FirstOrDefault();
						tblSalesDetail clientd = new tblSalesDetail();
						clientd.Id = Guid.NewGuid().ToString();
						clientd.SaleId = client.Id;
						clientd.UnitTypeId = item.UnitTypeId;
						clientd.ProductId = item.ProductId;
						clientd.color_code = item.color_code;
						clientd.Quantity = Convert.ToDecimal(item.Quantity);
						clientd.Price = Convert.ToDecimal(item.Price);
						clientd.actual_price = Convert.ToDecimal(item.actual_price);
						clientd.Sugar = item.Sugar;
						clientd.Size = item.Size;
						var costGoodSold = db.tblProductByUnits.Where(s => string.Compare(s.ProductID, item.ProductId) == 0 && string.Compare(s.UnitTypeID, clientd.UnitTypeId) == 0).FirstOrDefault();
						if (costGoodSold != null)
						{
							clientd.cost = costGoodSold.Cost;
							totalCostGoodSold = totalCostGoodSold + Convert.ToDecimal(costGoodSold.Cost * clientd.Quantity);
							if (client.SaleDeposit > 0)
							{
								totalDepositCoGs = totalDepositCoGs + totalCostGoodSold;
							}
							productCostGoodSold = Convert.ToDecimal(costGoodSold.Cost);
						}
						var receivedAmountByItem = ((clientd.actual_price * clientd.Quantity) * percentageCashRecived) / 100;
						var receivedSoldQtyByItem = clientd.actual_price > 0 ? receivedAmountByItem / clientd.actual_price : 0;
						var receivedCostPerItem = receivedSoldQtyByItem * productCostGoodSold;
						totalReceivedCostSale = totalReceivedCostSale + Convert.ToDecimal(receivedCostPerItem);

						db.tblSalesDetails.Add(clientd);
						db.SaveChanges();
						/*functions.stock_adjustment(item.ProductId, Convert.ToDecimal(item.Quantity), i.Id);*/
					}
				}
				decimal profit = Convert.ToDecimal(client.SaleDeposit) - Convert.ToDecimal(totalReceivedCostSale);
				updateSaleProfit(client.Id, totalReceivedCostSale, profit);
				SaveSalePayment(client);
				createInvoiceNo(ids);
				var redirectUrl = Url.Action("InvoiceReport", "Coffee", new { id = ids });
				return Json(new
				{
					success = true,
					redirectUrl = redirectUrl
				}, JsonRequestBehavior.AllowGet);
			}
		}
		catch (Exception ex)
		{
			return Json(new { success = false, message = ex.Message });
		}
	}
	public void SaveSalePayment(tblSale sale)
	{
		using (jotunDBEntities db = new jotunDBEntities())
		{
			tbSalePayment salePayment = new tbSalePayment();
			salePayment.sale_payment_id = Guid.NewGuid().ToString();
			salePayment.sale_id = sale.Id;
			salePayment.payment_amount = Convert.ToDecimal(sale.Amount);
			salePayment.owe_amount = Convert.ToDecimal(sale.Owe);
			salePayment.created_at = CommonFunctions.ToLocalTime(DateTime.Now);
			salePayment.created_by = User.Identity.GetUserId();
			salePayment.payment_amount_aba = sale.ReceivedByABA;
			db.tbSalePayments.Add(salePayment);
			db.SaveChanges();
		}
	}
	public void createInvoiceNo(string id)
	{
		jotunDBEntities db = new jotunDBEntities();
		tblSale s = db.tblSales.Find(id);
		var ss = db.tblSalesDetails.Where(ID => ID.SaleId == id).ToList();
		var firstId = (from p in db.tblSales where p.InvoiceNo != null && p.Status == 1 select p).ToList();
		var maxid = (from x in db.tblSales where x.InvoiceNo != null && x.Status == 1 select x.InvoiceId).Max();
		string today = DateTime.Now.ToString("yyMMdd");
		if (maxid == null)
		{
			var i = 1;
			string d = i.ToString().PadLeft(3, '0');
			s.InvoiceNo = today + '-' + d;
			s.InvoiceId = +1;
		}
		if (s.InvoiceNo != null)
		{
			s.InvoiceNo = s.InvoiceNo;
		}
		else
		{
			foreach (var item in firstId)
			{
				if (item.InvoiceId == maxid)
				{
					string firstId1 = item.InvoiceNo;
					string splits = firstId1;
					string[] arrayString = splits.Split('-');
					var j = arrayString[0];

					if (arrayString.Count() > 1)
					{
						var j1 = arrayString[1];

						var sum = Convert.ToInt32(j1) + 1;
						string c = sum.ToString().PadLeft(3, '0');
						if (today == j)
						{
							s.InvoiceNo = today + '-' + c;
							s.InvoiceId = item.InvoiceId + 1;
						}
						else
						{
							var i = 1;
							string d = i.ToString().PadLeft(3, '0');
							s.InvoiceNo = today + '-' + d;
							s.InvoiceId = item.InvoiceId + 1;
						}
					}
					else
					{
						var j1 = "0";
						var sum = Convert.ToInt32(j1) + 1;
						string c = sum.ToString().PadLeft(3, '0');
						if (today == j)
						{
							s.InvoiceNo = today + '-' + c;
							s.InvoiceId = item.InvoiceId + 1;
						}
						else
						{
							var i = 1;
							string d = i.ToString().PadLeft(3, '0');
							s.InvoiceNo = today + '-' + d;
							s.InvoiceId = item.InvoiceId + 1;
						}	
					}

				}
			}
		}

		db.SaveChanges();
	}
	public void updateSaleProfit(string saleId, decimal totalReceivedCoGS, decimal profit)
	{
		try
		{
			jotunDBEntities db = new jotunDBEntities();
			tblSale sale = db.tblSales.Find(saleId);
			if (sale != null)
			{
				sale.SaleCost = totalReceivedCoGS;
				sale.Profit = profit;
				db.SaveChanges();
			}
		}
		catch (Exception ex)
		{

		}
	}
		public ActionResult InvoiceReport(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return HttpNotFound("SaleId is missing in the request.");
			}
			using (jotunDBEntities db = new jotunDBEntities())
			{
				tblSale s = db.tblSales.Find(id);
				if (s == null)
				{
					return HttpNotFound($"Sale with ID {id} not found.");
				}				
				var salesList = db.tblSales
					.Where(sale => DbFunctions.TruncateTime(sale.UpdatedDate) == DbFunctions.TruncateTime(s.UpdatedDate) &&
								   sale.TableNumber == s.TableNumber)
					.OrderBy(sale => sale.UpdatedDate)
					.ToList();
				int waitingNumber = salesList.FindIndex(sale => sale.Id.Equals(s.Id)) + 1;
				DataTable dt = new DataTable();
				dt.Columns.Add(new DataColumn("Id"));
				dt.Columns.Add(new DataColumn("ProductId"));
				dt.Columns.Add(new DataColumn("Quantity"));
				dt.Columns.Add(new DataColumn("Price"));
				dt.Columns.Add(new DataColumn("CustomerId"));
				dt.Columns.Add(new DataColumn("Date"));
				dt.Columns.Add(new DataColumn("Total"));
				dt.Columns.Add(new DataColumn("Totals"));
				dt.Columns.Add(new DataColumn("VAT"));
				dt.Columns.Add(new DataColumn("Phone"));
				dt.Columns.Add(new DataColumn("Address"));
				dt.Columns.Add(new DataColumn("Discount"));
				dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
				dt.Columns.Add(new DataColumn("Owe"));
				dt.Columns.Add(new DataColumn("InvoiceNo"));
				dt.Columns.Add(new DataColumn("ColorCode"));
				dt.Columns.Add(new DataColumn("UnitId"));
				dt.Columns.Add(new DataColumn("SaleImage"));
				dt.Columns.Add(new DataColumn("SaleCode"));
				dt.Columns.Add(new DataColumn("Size"));
				dt.Columns.Add(new DataColumn("Sugar"));
				dt.Columns.Add(new DataColumn("TableNumber"));
				dt.Columns.Add(new DataColumn("WaitingNumber"));
				var salesDetails = db.tblSalesDetails.Where(d => d.SaleId == id).ToList();
				var customer = db.tblCustomers.FirstOrDefault(c => c.Id == s.CustomerId);
				if (salesDetails.Any())
				{
					foreach (var detail in salesDetails)
					{
						var product = db.tblProducts.FirstOrDefault(p => p.Id == detail.ProductId);
						var unit = db.tblUnits.FirstOrDefault(u => u.Id == detail.UnitTypeId);						
						DataRow dr = dt.NewRow();
						dr["CustomerId"] = customer?.CustomerName ?? "N/A";
						dr["Date"] = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
						dr["Id"] = detail.Id;
						dr["ProductId"] = product?.ProductName ?? "Unknown Product";					
						dr["Quantity"] = Convert.ToInt32(detail.Quantity);
						dr["Price"] = detail.Price.HasValue ? detail.Price.Value.ToString("N") : "0.00";
						dr["Total"] = (detail.Price.HasValue ? detail.Price.Value : 0) * detail.Quantity;
						dr["Totals"] = s.Amount.HasValue ? s.Amount.Value.ToString("N") : "0.00";
						dr["VAT"] = (s.RevicedFromCustomer ?? 0) - (s.Amount.HasValue ? s.Amount.Value : 0);
						dr["Phone"] = customer?.PhoneNumber ?? "N/A";
						dr["Address"] = customer?.ProjectLocation ?? "N/A";
						dr["Discount"] = s.Discount;
						dr["RevicedFromCustomer"] = s.RevicedFromCustomer;
						dr["Owe"] = s.Amount - ((s.Amount * s.Discount) / 100 + s.RevicedFromCustomer);
						dr["InvoiceNo"] = s.InvoiceNo;
						dr["ColorCode"] = detail.color_code;
						dr["UnitId"] = detail.UnitTypeId ?? "Unknown Unit";
						dr["SaleImage"] = s.SaleCode;
						dr["SaleCode"] = s.SaleCode;
						dr["Size"] = detail.Size;
						dr["Sugar"] = detail.Sugar;	
						dr["TableNumber"] = s.TableNumber;
						dr["WaitingNumber"] = waitingNumber;
						dt.Rows.Add(dr);
					}
				}			
				decimal totalAmount = (decimal)salesDetails.Sum(d => (d.Price ?? 0) * d.Quantity) - (decimal)s.Discount;
				decimal receivedAmount = (decimal)(s.RevicedFromCustomer ?? 0);
				decimal Deposit = receivedAmount - totalAmount;
				ViewBag.InvoiceData = dt;
				ViewBag.TotalAmount = totalAmount;
				ViewBag.ReceivedAmount = receivedAmount;
				ViewBag.Deposit = Deposit;
				ViewBag.CustomerName = customer?.CustomerName ?? "N/A";
				ViewBag.PhoneNumber = customer?.PhoneNumber ?? "N/A";
				ViewBag.CustomerAddress = customer?.ProjectLocation ?? "N/A";
				ViewBag.InvoiceNo = s.InvoiceNo;
				ViewBag.InvoiceDate = DateTime.Now.ToString("dd-MMM-yyyy h:mm tt");
				ViewBag.Discount = s.Discount ?? 0;
				ViewBag.Tax = s.Tax ?? 0;
				/*ViewBag.CashierName = cashier?.FullName ?? "Unknown Cashier";*/	
				ViewBag.WaitingNo = waitingNumber; 
				return View();
			}
		}
		public ActionResult ExportToPDF(string id)
	{
		try
		{
			jotunDBEntities db = new jotunDBEntities();
			var aa = 0;
			tblSale s = db.tblSales.Find(id);
			if (s.Amount == null)
			{
				s.Amount = aa;
			}
			if (s.Discount == null)
			{
				s.Discount = aa;
			}
			if (s.RevicedFromCustomer == null)
			{
				s.RevicedFromCustomer = aa;
			}

			decimal k = (decimal)(s.Amount - (((s.Amount * s.Discount) / 100) + s.RevicedFromCustomer));
			if (k <= 0)
			{
				s.InvoiceStatus = "Paid";
			}
			else
			{
				s.InvoiceStatus = "Not Paid";
			}
			var salesList = db.tblSales
				.Where(sale => DbFunctions.TruncateTime(sale.UpdatedDate) == DbFunctions.TruncateTime(s.UpdatedDate) &&
			   sale.TableNumber == s.TableNumber)
				.OrderBy(sale => sale.UpdatedDate)
				.ToList();
			int waitingNumber = salesList.FindIndex(sale => sale.Id.Equals(s.Id)) + 1;
			db.SaveChanges();
			ReportViewer rv = new ReportViewer();
			rv.ProcessingMode = ProcessingMode.Local;
			rv.SizeToReportContent = true;
			rv.Width = Unit.Percentage(100);
			rv.Height = Unit.Percentage(100);
			DataTable dt = new DataTable();

			dt.Columns.Add(new DataColumn("Id"));
			dt.Columns.Add(new DataColumn("ProductId"));
			dt.Columns.Add(new DataColumn("Quantity"));
			dt.Columns.Add(new DataColumn("Price"));
			dt.Columns.Add(new DataColumn("CustomerId"));
			dt.Columns.Add(new DataColumn("Date"));
			dt.Columns.Add(new DataColumn("Total"));
			dt.Columns.Add(new DataColumn("Totals"));
			dt.Columns.Add(new DataColumn("VAT"));
			dt.Columns.Add(new DataColumn("Phone"));
			dt.Columns.Add(new DataColumn("Address"));
			dt.Columns.Add(new DataColumn("Discount"));
			dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
			dt.Columns.Add(new DataColumn("Owe"));
			dt.Columns.Add(new DataColumn("InvoiceNo"));
			dt.Columns.Add(new DataColumn("ColorCode"));
			dt.Columns.Add(new DataColumn("UnitId"));
			dt.Columns.Add(new DataColumn("SaleImage"));
			dt.Columns.Add(new DataColumn("Size"));
			dt.Columns.Add(new DataColumn("Sugar"));
			dt.Columns.Add(new DataColumn("TableNumber"));
			dt.Columns.Add(new DataColumn("WaitingNumber"));
			var saless = db.tblSalesDetails.Where(ID => ID.SaleId == id);
			var customer = db.tblSales.Where(ID => ID.Id == id);
			if (saless.Any())
			{
				foreach (var cuss in customer)
				{
					foreach (var ii in saless)
					{
						var proname = (from p in db.tblProducts
									   where p.Id == ii.ProductId
									   select p).FirstOrDefault();

						var cus = (from c in db.tblCustomers
								   where c.Id == cuss.CustomerId
								   select c).FirstOrDefault();

						var unit = (from u in db.tblUnits
									where u.Id == ii.UnitTypeId
									select u).FirstOrDefault();

						DataRow dr = dt.NewRow();
						/*dr["CustomerId"] = cus.CustomerName ?? null ;*/
						dr["Date"] = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
						dr["Id"] = ii.Id;
						dr["ProductId"] = proname.ProductName;
						dr["Quantity"] = float.Parse((ii.Quantity).ToString());
						dr["Price"] = float.Parse((ii.Price).ToString()).ToString("N");
						dr["Total"] = float.Parse((ii.Price * ii.Quantity).ToString()).ToString("N");
						dr["Totals"] = float.Parse((cuss.Amount).ToString()).ToString("N");
						dr["VAT"] = (cuss.RevicedFromCustomer ?? 0) - (cuss.Amount.HasValue ? cuss.Amount.Value : 0);
						dr["Discount"] = cuss.Discount;
						dr["RevicedFromCustomer"] = float.Parse((cuss.RevicedFromCustomer).ToString()).ToString("N");
						dr["InvoiceNo"] = cuss.InvoiceNo;
						/*dr["ColorCode"] = ii.color_code;*/
						dr["UnitId"] = ii.UnitTypeId;
						dr["SaleImage"] = cuss.SaleCode;
						dr["Size"] = ii.Size;
						dr["Sugar"] = ii.Sugar;
						dr["TableNumber"] = cuss.TableNumber;
						dr["WaitingNumber"] = waitingNumber;
						dt.Rows.Add(dr);

					}

				}

			}
			rv.ProcessingMode = ProcessingMode.Local;
			rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\CoffeeInvoice.rdlc";
			rv.LocalReport.DataSources.Clear();
			ReportDataSource rds = new ReportDataSource("DSSaleInvoice", dt);
			rv.LocalReport.DataSources.Add(rds);
			rv.ShowPrintButton = true;
			ViewBag.ReportViewer = rv;

			Warning[] warnings;
			string[] streamids;
			string mimeType, encoding, filenameExtension;

			byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
			//File  
			string FileName = "Invoice_" + DateTime.Now.Ticks.ToString() + ".pdf";
			string FilePath = HttpContext.Server.MapPath(@"~\pdf\") + FileName;

			//create and set PdfReader  
			PdfReader reader = new PdfReader(bytes);
			FileStream output = new FileStream(FilePath, FileMode.Create);


			string Agent = HttpContext.Request.Headers["User-Agent"].ToString();

			//create and set PdfStamper  
			PdfStamper pdfStamper = new PdfStamper(reader, output, '0', true);

			if (Agent.Contains("Firefox"))
				pdfStamper.JavaScript = "var res = app.loaded('var pp = this.getPrintParams();pp.interactive = pp.constants.interactionLevel.full;this.print(pp);');";
			else
				pdfStamper.JavaScript = "var res = app.setTimeOut('var pp = this.getPrintParams();pp.interactive = pp.constants.interactionLevel.full;this.print(pp);', 200);";

			pdfStamper.FormFlattening = false;
			pdfStamper.Close();
			reader.Close();
			//return file path  
			string FilePathReturn = @"pdf/" + FileName;
			return Content(FilePathReturn);
		}
		catch (Exception Ex)
		{
			return View("Error", new HandleErrorInfo(Ex, "InvoiceReport", "Sale"));

		}
	}
	private class FromBodyAttribute : Attribute
	{
	}
	/*[HttpPost]
		public ActionResult GetAvailableUnits(string productId)
		{
			using (var db = new jotunDBEntities())
			{
				var product = db.tblProducts
					.Where(p => p.Id.ToString() == productId)
					.FirstOrDefault();
				if (product != null)
				{
					var unit = db.tblUnits
						.Where(u => u.Id == product.Unitid) 
						.Select(u => new SelectListItem
						{
							Value = u.Id.ToString(),
							Text = u.UnitNameKh         
						})
						.ToList();
					return Json(unit, JsonRequestBehavior.AllowGet);
				}
				return Json(new List<SelectListItem>(), JsonRequestBehavior.AllowGet);
			}
		}*/
		[PermissionAuthorize(moduleId: 1, permissionTypes: "view")]
		public ActionResult CreatePackage()
		{
			var userId = User.Identity.GetUserId();
			var hasViewPermission = _permissionService.HasPermission(userId, 1, "view");
			ViewBag.HasViewPermission = hasViewPermission;
			using (var db = new jotunDBEntities())
			{
				var model = new CreatePackageViewModel
				{			
					AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name
						})
						.ToList(),
					AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList(),				
					SelectedServices = new List<ServiceSelectionViewModel> { new ServiceSelectionViewModel() },
					SelectedProducts = new List<ProductSelectionViewModel> { new ProductSelectionViewModel() }
				};
				return View(model);
			}
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreatePackage(CreatePackageViewModel model)
		{
			if (!ModelState.IsValid)
			{
				var selectedServices = model.SelectedServices;
				var selectedProducts = model.SelectedProducts;
				using (var db = new jotunDBEntities())
				{
					model.AvailableServices = db.tblServices
						.Where(s => s.IsActive)
						.Select(s => new SelectListItem
						{
							Value = s.ServiceId.ToString(),
							Text = s.Name
						})
						.ToList();

					model.AvailableProducts = db.tblProducts
						.Where(p => p.CreatedDate.Value.Year == 2025)
						.Select(p => new SelectListItem
						{
							Value = p.Id.ToString(),
							Text = p.ProductName
						})
						.ToList();
				}
				return View(model);
			}

			using (var db = new jotunDBEntities())
			{
				var package = new Package
				{
					PackageName = model.Name,
					Price = model.Price,
					Description = model.Description,
					CreatedAt = DateTime.Now,
					/*CreatedBy = */
					Status = 1,
				};
				db.Packages.Add(package);
				db.SaveChanges();			
				if (model.SelectedServices != null && model.SelectedServices.Any())
				{
					foreach (var service in model.SelectedServices)
					{
						var packageService = new PackageService
						{
							PackageId = package.Id,
							ServiceId = service.Id,
							Quantity = service.Quantity,	
						};
						db.PackageServices.Add(packageService);
					}
					db.SaveChanges();
				}
				// Save selected products
				if (model.SelectedProducts != null && model.SelectedProducts.Any())
				{
					foreach (var product in model.SelectedProducts)
					{
						var packageProduct = new PackageProduct
						{
							PackageId = package.Id,
							ProductId = product.Id,
							Quantity = product.Quantity,
							Unit = product.Unit,
							Qty = product.Qty,
						};
						db.PackageProducts.Add(packageProduct);
					}
					db.SaveChanges();
				}
			}
			return RedirectToAction("Index");
		}


	}

}
