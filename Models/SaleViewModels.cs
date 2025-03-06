using jotun.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jotun.Models
{
	public class SaleViewModels
	{
		/* internal DateTime Date;*/

		public string Id { get; set; }
		public string SaleCode { get; set; }
		public string InvoiceNo { get; set; }
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }
		public string Owe { get; set; }
		public string Amount { get; set; }
		public string Discount { get; set; }
		public string RevicedFromCustomer { get; set; }
		public string RevicedFromCustomer1 { get; set; }
		public string InvoiceStatus { get; set; }
		public string CustomerNo { get; set; }
		public string CustomerId { get; set; }
		public string filter_project_location_id { get; set; }
		public string filter_project_location_name { get; set; }
		public string filter_product_id { get; set; }
		public string filter_product_name { get; set; }
		public string Description { get; set; }
		public string CreatedDate { get; set; }
		public string UpdatedDate { get; set; }
		public Nullable<int> Status { get; set; }
		[Required]
		[Display(Name = "CustomerName")]
		public string CustomerNames { get; set; }
		public string CustomerName { get; set; }
		[Required]
		[Display(Name = "ProductName")]
		public string ProductName { get; set; }
		[Required]
		[Display(Name = "ProductCode")]
		public string ProductCode { get; set; }
		public string ProductImage { get; set; }
		[Required]
		[Display(Name = "QuantityInStock")]
		public string QuantityInStock { get; set; }
		[Required]
		[Display(Name = "PriceInStock")]
		public string PriceInStock { get; set; }

		[Required]
		[Display(Name = "CategoryName")]
		public string CategoryName { get; set; }

		[Required]
		[Display(Name = "UnitName")]
		public string UnitName { get; set; }

		[Required]
		[Display(Name = "ShipperName")]
		public string ShipperName { get; set; }

		[Required]
		[Display(Name = "SupplierName")]
		public string SupplierName { get; set; }
		public HttpPostedFileWrapper ImageFile { get; set; }
		public string SaleImage { get; set; }



		[Required]
		[Display(Name = "PhoneNumber")]
		public string PhoneNumber { get; set; }

		[Required]
		[Display(Name = "Gender")]
		public string Gender { get; set; }
		public string ProjectLocation { get; set; }
		public string Noted { get; set; }
		public string CustomerLocation { get; set; }
		public string LocationText { get; set; }
		public Nullable<decimal> ReceivedByABA { get; set; }
		public Nullable<decimal> NewReceivedByABA { get; set; }
		//public Nullable<decimal> TotalReceivedFromCustomer
		//{
		//    get { return ReceivedByABA + TotalReceivedFromCustomer; }
		//    set { TotalReceivedFromCustomer = value; }
		//}

		public List<SaleDetailViewModel> GetDetail { get; set; }
		public string UnitType { get; set; }
		public string ColorCode { get; set; }
		public decimal? Price { get; set; }
		public int Quantity { get; set; }
		public string Sugar { get; set; }
		public string Size { get; set; }
		public string Item { get; internal set; }

		public AppointmentModel appointment { get; set; }
		public static object GetNoDetail(string id)
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				// Fetch sales for the customer (handle both multiple and single sales)
				var salesList = db.tblSales.Where(s => s.CustomerId == id || s.Id == id).ToList();
				// If only one sale is found, return a single SaleViewModel
				if (salesList.Count == 1)
				{
					var sale = salesList.First();
					var customer = db.tblCustomers.FirstOrDefault(p => p.Id == sale.CustomerId || p.Id == sale.Id);
					var customer_location = db.tblCustomer_location.FirstOrDefault(cl => cl.id == sale.customer_location && cl.status == true);

					// Prepare customer location details
					var resultcustomer_location = customer_location?.location ?? "";
					var location_id = customer_location?.id.ToString() ?? "";

					var saleViewModel = new SaleViewModels
					{
						Id = sale.Id,
						CustomerId = customer?.Id,
						CustomerName = customer?.CustomerName,
						InvoiceStatus = sale.InvoiceStatus,
						InvoiceNo = sale.InvoiceNo,
						Description = sale.Description,
						Discount = sale.Discount.ToString(),
						RevicedFromCustomer = sale.RevicedFromCustomer.ToString(),
						ReceivedByABA = sale.ReceivedByABA ?? 0,
						Amount = sale.Amount.ToString(),
						Owe = ((sale.Amount - sale.Discount) - (sale.RevicedFromCustomer + (double)(sale.ReceivedByABA))).ToString(),
						CreatedDate = sale.CreatedDate.ToString(),
						UpdatedDate = sale.UpdatedDate.ToString(),
						Status = sale.Status,
						CustomerLocation = location_id,
						LocationText = resultcustomer_location
					};

					// Fetch sale details for this sale
					List<SaleDetailViewModel> GetDetail = db.tblSalesDetails
						.Where(w => w.SaleId == sale.Id)
						.Select(s => new SaleDetailViewModel
						{
							Id = s.Id,
							SaleId = s.SaleId,
							ProductId = s.ProductId,
							ProductIdn = s.ProductId,
							Quantity = s.Quantity.ToString(),
							Price = s.Price.ToString(),
							Total = (s.Price * s.Quantity).ToString(),
							color_code = s.color_code,
							actual_price = s.actual_price.ToString(),
							UnitTypeId = s.UnitTypeId,
							UNit = s.UnitTypeId,
							Sugar = s.Sugar,
							Size = s.Size
						}).ToList();

					// Update SaleDetailViewModel with additional details like unit names and product names
					foreach (var list in GetDetail)
					{
						// Fetch unit details
						var un = db.tblUnits.Where(u => u.Id == list.UnitTypeId).ToList();
						foreach (var u1 in un)
						{
							list.UnitTypeId = u1.Id;
							list.UNit = u1.UnitNameEng;
						}

						// Fetch product details
						var productdetail = db.tblProducts.Where(s => s.Id == list.ProductId).ToList();
						foreach (var list2 in productdetail)
						{
							list.ProductIdn = list2.ProductName;
							list.ProductId = list2.Id;
						}
					}

					// Add sale details to the sale object
					saleViewModel.GetDetail = GetDetail;

					return saleViewModel;  // Return single SaleViewModel for one sale
				}
				else
				{
					// If there are multiple sales, return a list of SaleViewModels
					List<SaleViewModels> sales = new List<SaleViewModels>();

					foreach (var sale in salesList)
					{
						var customer = db.tblCustomers.FirstOrDefault(p => p.Id == sale.CustomerId);
						var customer_location = db.tblCustomer_location.FirstOrDefault(cl => cl.id == sale.customer_location && cl.status == true);

						var resultcustomer_location = customer_location?.location ?? "";
						var location_id = customer_location?.id.ToString() ?? "";

						SaleViewModels saleViewModel = new SaleViewModels
						{
							Id = sale.Id,
							CustomerId = customer?.Id,
							CustomerName = customer?.CustomerName,
							InvoiceStatus = sale.InvoiceStatus,
							InvoiceNo = sale.InvoiceNo,
							Description = sale.Description,
							Discount = sale.Discount.ToString(),
							RevicedFromCustomer = sale.RevicedFromCustomer.ToString(),
							ReceivedByABA = sale.ReceivedByABA ?? 0,
							Amount = sale.Amount.ToString(),
							Owe = ((sale.Amount - sale.Discount) - (sale.RevicedFromCustomer + (double)(sale.ReceivedByABA))).ToString(),
							CreatedDate = sale.CreatedDate.ToString(),
							UpdatedDate = sale.UpdatedDate.ToString(),
							Status = sale.Status,
							CustomerLocation = location_id,
							LocationText = resultcustomer_location
						};

						List<SaleDetailViewModel> GetDetail = db.tblSalesDetails
							.Where(w => w.SaleId == sale.Id)
							.Select(s => new SaleDetailViewModel
							{
								Id = s.Id,
								SaleId = s.SaleId,
								ProductId = s.ProductId,
								ProductIdn = s.ProductId,
								Quantity = s.Quantity.ToString(),
								Price = s.Price.ToString(),
								Total = (s.Price * s.Quantity).ToString(),
								color_code = s.color_code,
								actual_price = s.actual_price.ToString(),
								UnitTypeId = s.UnitTypeId,
								UNit = s.UnitTypeId,
								Sugar = s.Sugar,
								Size = s.Size
							}).ToList();

						// Update SaleDetailViewModel with additional details like unit names and product names
						foreach (var list in GetDetail)
						{
							var un = db.tblUnits.Where(u => u.Id == list.UnitTypeId).ToList();
							foreach (var u1 in un)
							{
								list.UnitTypeId = u1.Id;
								list.UNit = u1.UnitNameEng;
							}

							var productdetail = db.tblProducts.Where(s => s.Id == list.ProductId).ToList();
							foreach (var list2 in productdetail)
							{
								list.ProductIdn = list2.ProductName;
								list.ProductId = list2.Id;
							}
						}

						saleViewModel.GetDetail = GetDetail;
						sales.Add(saleViewModel);
					}

					return sales;  // Return a list of SaleViewModels for multiple sales
				}
			}
		}
	}
		/* public static SaleViewModels GetNoDetail(string id )
		  {
			  using (jotunDBEntities db = new jotunDBEntities())
			  {
				  SaleViewModels sale = new SaleViewModels();
				  List<SaleDetailViewModel> GetDetail = new List<SaleDetailViewModel>();
				  tblSale ts = db.tblSales.FirstOrDefault(s => s.Id == id || s.CustomerId == id);
				  tblSalesDetail pd = new tblSalesDetail();
				  var customer = (from p in db.tblCustomers
								  where p.Id == ts.CustomerId
								  select p).FirstOrDefault();
				  var customer_location = (from cl in db.tblCustomer_location
										   where cl.id == ts.customer_location && cl.status == true
										   select cl).FirstOrDefault();
				  var resultcustomer_location = "";
				  var location_id = "";
				  if (customer_location != null)
				  {
					  resultcustomer_location = customer_location.location;
					  location_id = customer_location.id.ToString();
				  }
				  else
				  {
					  resultcustomer_location = "";
					  location_id = "";
				  }
				  var ss = db.tblSales.Where(w => string.Compare(w.Id, id) == 0 || string.Compare(w.CustomerId, id) == 0).FirstOrDefault();
				  if (ss != null)
				  {
					  ss.ReceivedByABA = ss.ReceivedByABA ?? 0;
					  ss.RevicedFromCustomer = ss.RevicedFromCustomer ?? 0;
					  sale = new SaleViewModels()
					  {
						  Id = ss.Id,
						  CustomerId = customer.Id,
						  CustomerName = customer.CustomerName,
						  Description = ss.Description,
						  Discount = ss.Discount.ToString(),
						  RevicedFromCustomer = ss.RevicedFromCustomer.ToString(),
						  ReceivedByABA = ss.ReceivedByABA ?? 0,
						  Amount = ss.Amount.ToString(),
						  ////Owe = ((s.Amount - ((s.Amount * s.Discount) / 100)) - s.RevicedFromCustomer).ToString(),
						  Owe = ((ss.Amount - ss.Discount) - (ss.RevicedFromCustomer + (double)(ss.ReceivedByABA))).ToString(),
						  CreatedDate = ss.CreatedDate.ToString(),
						  UpdatedDate = ss.UpdatedDate.ToString(),
						  Status = ss.Status,
						  CustomerLocation = location_id,
						  LocationText = resultcustomer_location,
					  };
				  }
				  GetDetail = db.tblSalesDetails.Where(w => string.Compare(w.SaleId, id) == 0).Select(s => new SaleDetailViewModel()
				  {
					  Id = s.Id,
					  SaleId = s.SaleId,
					  ProductId = s.ProductId,
					  ProductIdn = s.ProductId,
					  Quantity = s.Quantity.ToString(),
					  Price = s.Price.ToString(),
					  Total = (s.Price * s.Quantity).ToString(),
					  color_code = s.color_code,
					  actual_price = s.actual_price.ToString(),
					  UnitTypeId = s.UnitTypeId,
					  UNit = s.UnitTypeId,
					  Sugar = s.Sugar,
					  Size = s.Size,     
				  }).ToList();
				  foreach (var list in GetDetail)
				  {
					  var un = (from u in db.tblUnits
								where u.Id == list.UnitTypeId
								select u).ToList();
					  foreach (var u1 in un)
					  {
						  list.UnitTypeId = u1.Id;
						  list.UNit = u1.UnitNameEng;
					  }
					  var productdetail = (from s in db.tblProducts
										   where s.Id == list.ProductId
										   select s).ToList();

					  foreach (var list2 in productdetail)
					  {
						  list.ProductIdn = list2.ProductName;
						  list.ProductId = list2.Id;
					  }


				  }
				  List<SaleDetailViewModel> models = new List<SaleDetailViewModel>();
				  foreach (var sale_obj in GetDetail)
				  {
					  if ( !String.IsNullOrEmpty(sale_obj.actual_price))
					  {
						  if (Convert.ToDouble(sale_obj.Price) > 0 || Convert.ToDouble(sale_obj.actual_price) > 0)
						  {
							  models.Add(new SaleDetailViewModel()
							  {
								  Id = sale_obj.Id,
								  SaleId = sale_obj.SaleId,
								  ProductId = sale_obj.ProductId,
								  ProductIdn = sale_obj.ProductIdn,
								  Size = sale_obj.Size,
								  Sugar = sale_obj.Sugar,
								  Quantity = Convert.ToDouble(sale_obj.Quantity).ToString("0"),
								  Price = Convert.ToDouble(sale_obj.Price).ToString("0.00"),
								  Total = (Convert.ToDouble(sale_obj.Price ) * Convert.ToDouble(sale_obj.Quantity)).ToString("0.00"),
								  color_code = sale_obj.color_code,
								  actual_price = Convert.ToDouble(sale_obj.actual_price ).ToString("0.00"),
								  UnitTypeId = sale_obj.UnitTypeId,
								  UNit = sale_obj.UNit,
							  });
						  }
					  }
					  else
					  {
						  models.Add(new SaleDetailViewModel()
						  {

							  Id = sale_obj.Id,
							  SaleId = sale_obj.SaleId,
							  ProductId = sale_obj.ProductId,
							  ProductIdn = sale_obj.ProductIdn,
							  Size = sale_obj.Size,
							  Sugar = sale_obj.Sugar,
							  Quantity = Convert.ToDouble(sale_obj.Quantity).ToString("0"),
							  Price = Convert.ToDouble(sale_obj.Price).ToString("0.00"),
							  Total = (Convert.ToDouble(sale_obj.Price) * Convert.ToDouble(sale_obj.Quantity)).ToString("0.00"),
							  color_code = sale_obj.color_code,
							  actual_price = Convert.ToDouble(sale_obj.Price).ToString("0.00"),
							  UnitTypeId = sale_obj.UnitTypeId,
							  UNit = sale_obj.UNit,

						  });
					  }
				  }
				  sale.GetDetail = models;
				  return sale;
			  }
		  }
	  }

  }*/
		public class SaleDetailViewModel
		{
			[Key]
			public string Id { get; set; }
			public string SaleId { get; set; }
			public string ProductId { get; set; }
			public string ProductIdn { get; set; }
			public string UnitTypeId { get; set; }
			public string UNit { get; set; }
			public string Quantity { get; set; }
			public string Price { get; set; }
			public string Total { get; set; }
			public string color_code { get; set; }
			public string actual_price { get; set; }
			public string Sugar { get; set; }
			public string Size { get; set; }

			public static SaleDetailViewModel GetViewDetail(string id)
			{
				using (jotunDBEntities db = new jotunDBEntities())
				{
					return db.tblSalesDetails.Where(s => string.Compare(s.Id, id) == 0).Select(s => new SaleDetailViewModel()
					{
						Id = s.Id,
						SaleId = s.SaleId,
						ProductId = s.ProductId,
						ProductIdn = s.ProductId,
						Quantity = s.Quantity.ToString(),
						Price = s.Price.ToString(),
						UnitTypeId = s.UnitTypeId,
						UNit = "",
						// UnitTypeId = s.
					}).FirstOrDefault();
				}
			}
		}
	}

