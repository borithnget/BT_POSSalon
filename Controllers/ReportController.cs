using iTextSharp.text.pdf;
using jotun.Entities;
using jotun.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Reporting.WebForms;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace jotun.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public Boolean isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "SuperAdmin" || s[0].ToString() == "Administrator")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public ActionResult Index()
        {
			ViewData["HideHeader"] = true;
			return View();
		}
        public ActionResult Purchase(tblPurchaseBySupplierViewModel sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("SupplierId"));
            dt.Columns.Add(new DataColumn("ShipperAmount"));
            dt.Columns.Add(new DataColumn("Deposit"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("PurchaseAmount"));

            DateTime ToDates = sm.ToDate.AddHours(23.59);

            var treatmentSum = (from s1 in db.tblPurchaseBySuppliers
                                where s1.Status == 1 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates
                                select s1).ToList();

            foreach (var item in treatmentSum)
            {
                string nonull = "0.000";

                if (item.Status == 1)
                {
                    DataRow dr = dt.NewRow();

                    var sup = (from su in db.tblSuppliers
                               where su.Id == item.SupplierId
                               select su).FirstOrDefault();

                    var sip = (from si in db.tblShippers
                               where si.Id == item.ShipperId
                               select si).FirstOrDefault();

                    if (item.PurchaseAmount == null)
                    {
                        item.PurchaseAmount = Convert.ToDecimal(nonull);
                    }
                    if (item.Deposit == null)
                    {
                        item.Deposit = Convert.ToDecimal(nonull);
                    }
                    if (item.Discount == null)
                    {
                        item.Discount = Convert.ToDecimal(nonull);
                    }
                    if (item.ShipperAmount == null)
                    {
                        item.ShipperAmount = Convert.ToDecimal(nonull);
                    }


                    dr["InvoiceNo"] = item.InvoiceNo;
                    dr["SupplierId"] = sup.SupplierName;
                    dr["ShipperAmount"] = float.Parse(item.ShipperAmount.ToString()).ToString("0.000");
                    dr["Deposit"] = float.Parse(item.Deposit.ToString()).ToString("0.000");
                    dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                    dr["PurchaseAmount"] = float.Parse(item.PurchaseAmount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    //dr["CreatedDate"] = Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy");


                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Purchase.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSPurchase", dt);
            rv.LocalReport.DataSources.Add(rds);


            ViewBag.ReportViewer = rv;

            return View();
        }
        public ActionResult PurchaseDetail(tblPurchaseBySupplierViewModel sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("SupplierId"));
            dt.Columns.Add(new DataColumn("ShipperAmount"));
            dt.Columns.Add(new DataColumn("Deposit"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("TotalDiscount"));
            dt.Columns.Add(new DataColumn("PurchaseAmount"));
            dt.Columns.Add(new DataColumn("Phone"));
            dt.Columns.Add(new DataColumn("ProductName"));
            dt.Columns.Add(new DataColumn("QTY"));
            dt.Columns.Add(new DataColumn("Cost"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("CreatedDate"));
            dt.Columns.Add(new DataColumn("TotalAmount"));
            dt.Columns.Add(new DataColumn("TotalDeposit"));
            dt.Columns.Add(new DataColumn("TotalShipping"));
            dt.Columns.Add(new DataColumn("Revenue"));
            DateTime ToDates = sm.ToDate.AddHours(23.59);
            var treatmentSum = (from ps in db.tblPurchaseBySuppliers
                                orderby ps.CreatedDate descending
                                join pd in db.tblPurchaseBySupplierDetails on ps.Id equals pd.PurchaseBySupplierId
                                where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates
                                select new { ps, pd }).ToList();
            foreach (var item in treatmentSum)
            {
                string nonull = "0.000";
                if (item.ps.Status == 1)
                {
                    DataRow dr = dt.NewRow();

                    var sup = (from su in db.tblSuppliers
                               where su.Id == item.ps.SupplierId
                               select su).FirstOrDefault();

                    var sip = (from si in db.tblShippers
                               where si.Id == item.ps.ShipperId
                               select si).FirstOrDefault();
                    var pro = (from pr in db.tblProducts
                               where pr.Id == item.pd.ProductId
                               select pr).FirstOrDefault();
                    var unit = (from u in db.tblUnits
                                where u.Id == item.pd.UnitTypeId && u.Status == 1
                                select u).FirstOrDefault();
                    if (item.ps.PurchaseAmount == null)
                    {
                        item.ps.PurchaseAmount = Convert.ToDecimal(nonull);
                    }
                    if (item.ps.Deposit == null)
                    {
                        item.ps.Deposit = Convert.ToDecimal(nonull);
                    }
                    if (item.ps.Discount == null)
                    {
                        item.ps.Discount = Convert.ToDecimal(nonull);
                    }
                    if (item.ps.ShipperAmount == null)
                    {
                        item.ps.ShipperAmount = Convert.ToDecimal(nonull);
                    }
                    decimal? sum = 0;
                    for (int i = 0; i < treatmentSum.Count(); i++)
                    {
                        sum += treatmentSum[i].ps.PurchaseAmount;
                        dr["TotalAmount"] = sum;
                    };
                    dr["Id"] = item.pd.PurchaseBySupplierId;
                    dr["InvoiceNo"] = item.ps.InvoiceNo;
                    dr["SupplierId"] = sup.SupplierName;
                    dr["ShipperAmount"] = float.Parse(item.ps.ShipperAmount.ToString()).ToString("0.000");
                    dr["Deposit"] = float.Parse(item.ps.Deposit.ToString()).ToString("0.000");
                    dr["Discount"] = float.Parse(item.ps.Discount.ToString()).ToString("0.000");
                    decimal? dis = ((item.pd.Cost * item.pd.Quantity) * item.pd.Discount) / 100;
                    dr["TotalDiscount"] = float.Parse(dis.ToString()).ToString("0.000");
                    var getSomeAmount = (from ps in db.tblPurchaseBySuppliers
                                         orderby ps.CreatedDate descending
                                         where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates
                                         select ps).ToList();
                    decimal? sumDP = 0;
                    for (int i = 0; i < getSomeAmount.Count(); i++)
                    {
                        if (getSomeAmount[i].Deposit != null)
                        {
                            sumDP += getSomeAmount[i].Deposit;
                        }
                        dr["TotalDeposit"] = sumDP;

                    }
                    decimal? sumSP = 0;
                    for (int i = 0; i < getSomeAmount.Count(); i++)
                    {
                        if (getSomeAmount[i].ShipperAmount != null)
                        {
                            sumSP += getSomeAmount[i].ShipperAmount;
                        }
                        dr["TotalShipping"] = sumSP;

                    }
                    dr["PurchaseAmount"] = float.Parse((Convert.ToDecimal(item.ps.PurchaseAmount) + Convert.ToDecimal(item.ps.Discount)).ToString()).ToString("0.000");
                    //dr["PurchaseAmount"] = float.Parse((Convert.ToDecimal(item.ps.PurchaseAmount) ).ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dr["CreatedDate"] = Convert.ToDateTime(item.ps.CreatedDate).ToString("dd-MMM-yyyy");

                    dr["Phone"] = sup.ContactPhone;
                    dr["ProductName"] = pro.ProductName;
                    dr["QTY"] = item.pd.Quantity +" "+unit.UnitNameEng;
                    dr["Cost"] = item.pd.Cost;
                    dr["Total"] = (item.pd.Quantity * item.pd.Cost);
                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\PurchaseDetail.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSPurchaseDetail", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            return View();
        }
        public ActionResult Sale(SaleViewModels sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            List<tblSale> treatmentSum = new List<tblSale>();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("CustomerNameFilter"));
            dt.Columns.Add(new DataColumn("Owe"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
            dt.Columns.Add(new DataColumn("ReceivedABA"));
            DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);

            var emptyDate = new DateTime(0001, 1, 1);
            var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();
            
            if (sm.CustomerId != null)
            {
                DateTime dateFrom = Convert.ToDateTime(sm.FromDate);
                if(sm.FromDate == emptyDate)
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        where s1.Status == 1 && string.Compare(cu.CustomerName, customer_name.CustomerName)==0
                                        select s1).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        where s1.Status == 1 && string.Compare(s1.CustomerId ,sm.CustomerId)==0
                                        select s1).ToList();
                    }
                }
                else
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        where s1.Status == 1 && (s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates) && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                        select s1).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        where s1.Status == 1 && (s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates) && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                        select s1).ToList();
                    }
                }
                
            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                where s1.Status == 1 && (s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates) 
                                select s1).ToList();
            }
            foreach (var item in treatmentSum)
            {
                if (item.Status == 1)
                {
                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.CustomerId
                                    select p).ToList();
                    item.ReceivedByABA = item.ReceivedByABA ?? 0;
                    item.RevicedFromCustomer = item.RevicedFromCustomer ?? 0;

                    dr["Id"] = item.Id;
                    foreach (var i in customer)
                    {
                        dr["CustomerId"] = i.CustomerName;

                    }
                    dr["Owe"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                    dr["RevicedFromCustomer"] = float.Parse(item.RevicedFromCustomer.ToString()).ToString("0.000");
                    dr["ReceivedABA"]= float.Parse(item.ReceivedByABA.ToString()).ToString("0.000");
                    // dr["CreatedDate"] = item.CreatedDate;
                    if (item.CustomerId != null)
                    {
                        dr["CustomerNameFilter"] = customer_name?.CustomerName ?? "All";
                    }
                    else
                    {
                        dr["CustomerNameFilter"] = "All";
                    }
                    dr["InvoiceNo"] = item.InvoiceNo;
                    dr["Status"] = item.InvoiceStatus;
                    dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                    dr["Amount"] = float.Parse(item.Amount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Sale.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSale", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            return View();
        }

        public ActionResult SalesNetIncome(SaleViewModels sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            List<tblSale> treatmentSum = new List<tblSale>();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("CustomerNameFilter"));
            dt.Columns.Add(new DataColumn("Owe"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
            dt.Columns.Add(new DataColumn("NetIncome"));
            DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);


            var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();
            if (sm.CustomerId != null)
            {

                if (customer_name.is_old_data == true)
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                    where s1.Status == 1 && (s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates) && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                    select s1).ToList();
                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    where s1.Status == 1 && (s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates) && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                    select s1).ToList();
                }
            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                where s1.Status == 1 && (s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates) && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                select s1).ToList();
            }
            foreach (var item in treatmentSum)
            {
                if (item.Status == 1)
                {
                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.CustomerId
                                    select p).ToList();
                    var saledetail = (from sd in db.tblSalesDetails
                                      join pro in db.tblProducts on sd.ProductId equals pro.Id
                                      join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                      where sd.SaleId == item.Id && sd.UnitTypeId == probyunit.UnitTypeID
                                      select new { probyunit, sd }
                                      ).ToList();

                    decimal? sumNetIncome = 0;
                    for (var i = 0; i < saledetail.Count; i++)
                    {
                        sumNetIncome +=  saledetail[i].probyunit.Cost * saledetail[i].sd.Quantity;
                        //sumNetIncome += saledetail[i].Price - saledetail[i].Cost;
                    }

                    dr["Id"] = item.Id;
                    foreach (var i in customer)
                    {
                        dr["CustomerId"] = i.CustomerName;

                    }
                    dr["Owe"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                    dr["RevicedFromCustomer"] = float.Parse(item.RevicedFromCustomer.ToString()).ToString("0.000");
                    // dr["CreatedDate"] = item.CreatedDate;
                    if (item.CustomerId != null)
                    {
                        dr["CustomerNameFilter"] = customer_name?.CustomerName ?? "All";
                    }
                    else
                    {
                        dr["CustomerNameFilter"] = "All";
                    }
                    dr["InvoiceNo"] = item.InvoiceNo;
                    dr["Status"] = item.InvoiceStatus;
                    dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                    dr["Amount"] = float.Parse(item.Amount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dr["NetIncome"] = float.Parse((Convert.ToDecimal(item.Amount) - sumNetIncome).ToString()).ToString("0.000"); ;
                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleNetIncome.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleNetIncome", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            if (isAdminUser())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }


        public JsonResult getProjectLocation(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            return Json((db.tblCustomer_location.Where(c => c.status == true && c.location.ToLower().Contains(term.ToLower())).Select(a => new { label = a.location, id = a.id }).Take(100)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult getProductName(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            return Json((db.tblProducts.Where(c => c.Status == 1 && c.ProductName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.ProductName, id = a.Id }).Take(100)), JsonRequestBehavior.AllowGet);
        }


        public ActionResult SalesDetails(SaleViewModels sm)
        {

            jotunDBEntities db = new jotunDBEntities();        
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("CustomerNo"));
            dt.Columns.Add(new DataColumn("CustomerName"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("customer_location"));
            dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("ProductId"));
            dt.Columns.Add(new DataColumn("ProductName"));
            dt.Columns.Add(new DataColumn("Quantity"));
            dt.Columns.Add(new DataColumn("Refer"));
            dt.Columns.Add(new DataColumn("ColorCode"));
            dt.Columns.Add(new DataColumn("Price"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("TotalAmt"));
            dt.Columns.Add(new DataColumn("TotalDepo"));
            dt.Columns.Add(new DataColumn("TotalDis"));
            dt.Columns.Add(new DataColumn("OrderDate"));
            dt.Columns.Add(new DataColumn("PayDate"));

            DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);
            var treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates
                                select new
                                {
                                    s1,
                                    sd
                                }).ToList();
            var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();
            var emptyDate = new DateTime(0001, 1, 1);

            if (sm.CustomerId != null)
            {
                if (sm.FromDate == emptyDate)
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && cu.CustomerName == customer_name.CustomerName 
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.CustomerId == sm.CustomerId
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                }
                else
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                }
                

            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                select new
                                {
                                    s1,
                                    sd
                                }).ToList();
            }

            var resultlist = (from i in treatmentSum.ToList()
                              select i
                              ).ToList();
            if (sm.filter_project_location_name != null)
            {
                resultlist = (from i in treatmentSum.ToList()
                              join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                              where sm.filter_project_location_name == lo.location
                              select i
                              ).ToList();
            }
            else
            {
                resultlist = treatmentSum;
            }

            var resultlist2 = (from i in resultlist.ToList()
                              select i
                              ).ToList();

            if (sm.filter_product_id != null)
            {
                resultlist2 = (from i in resultlist.ToList()
                               join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                               where sm.filter_product_id == pro.Id
                                   select i
                              ).ToList();
            }
            else
            {
                resultlist2 = resultlist;
            }
            DateTime startDateTime = DateTime.Now;
            var getSomeAmount = (from ps in db.tblSales
                                 where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                 select ps).ToList();


            var resultlistsum = (from i in getSomeAmount
                                 select i).ToList();
            if (sm.filter_project_location_name != null)
            {
                resultlistsum = (from i in getSomeAmount
                                 join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                 where sm.filter_project_location_name == lo.location
                                 select i).ToList();
            }
            else
            {
                resultlistsum = getSomeAmount;
            }
            var resultlistsum2 = (from i in resultlistsum
                                  select i).ToList();
            if (sm.filter_product_id != null)
            {

                resultlistsum2 = (from i in resultlistsum
                                  join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                  where sm.filter_product_id == pd.ProductId
                                  select i).Distinct().ToList();
            }
            else
            {
                resultlistsum2 = resultlistsum;

            }


            if (resultlist2.Any())
            {
                foreach (var item in resultlist2)
                {


                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.s1.CustomerId
                                    select p).ToList();
                    var product = (from p in db.tblProducts
                                   where p.Id == item.sd.ProductId
                                   select p).ToList();
                    var unit = (from u in db.tblUnits
                                where u.Id == item.sd.UnitTypeId && u.Status == 1
                                select u).FirstOrDefault();

                    var site = (from s in db.tblCustomer_location
                                where s.id == item.s1.customer_location
                                select s).FirstOrDefault();
                    item.s1.ReceivedByABA = item.s1.ReceivedByABA ?? 0;
                    item.s1.RevicedFromCustomer = item.s1.RevicedFromCustomer ?? 0;
                    dr["Id"] = item.s1.Id;
                    foreach (var i in customer)
                    {
                        dr["CustomerNo"] = i.CustomerNo;
                        dr["CustomerName"] = i.CustomerName;
                    }
                    foreach (var i in product)
                    {
                        dr["ProductName"] = i.ProductName;
                    }
                    if (sm.CustomerId != null)
                    {
                        dr["CustomerId"] = customer_name.CustomerName;
                    }
                    else
                    {
                        dr["CustomerId"] = "All";
                    }
                    dr["InvoiceNo"] = item.s1.InvoiceNo;
                    dr["Status"] = item.s1.InvoiceStatus;
                    dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                    dr["RevicedFromCustomer"] = float.Parse((item.s1.RevicedFromCustomer +(double)item.s1.ReceivedByABA).ToString()).ToString("$0.000");
                    dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dr["ColorCode"] = item.sd.color_code;
                    if (item.s1.customer_location != null) 
                    {
                        dr["customer_location"] = site == null ? string.Empty: site.location;
                    }
					/*  dr["Quantity"] = float.Parse(item.sd.Quantity.ToString()).ToString("0.00") + " " + unit.UnitNameEng;*/
					/*dr["Quantity"] = (item.sd.Quantity != null ? float.Parse(item.sd.Quantity.ToString()).ToString("0.00") : "0.00") +
				   " " + (unit != null ? unit.UnitNameEng : "Unknown");*/
					dr["Quantity"] = (item.sd.Quantity != null ? ((int)item.sd.Quantity).ToString() : "0") +
                    " " + (unit != null ? unit.UnitNameEng : "Unknown");
					if (item.sd.actual_price != null)
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                    }
                    else
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                    }
                    dr["ProductId"] = item.sd.ProductId;
                    dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                    if (item.s1.UpdatedDate != null)
                    {
                        dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                    }
                    else
                    {
                        dr["PayDate"] = "";
                    }
                    decimal sumDP = 0;
                    for (int i = 0; i < resultlistsum2.Count(); i++)
                    {
                        if (resultlistsum2[i].Discount != null)
                        {
                            sumDP += Convert.ToDecimal(resultlistsum2[i].Discount);
                        }
                        dr["TotalDis"] = sumDP;
                    }

                    decimal sumDPO = 0;
                    for (int i = 0; i < resultlistsum2.Count(); i++)
                    {
                        if (resultlistsum2[i].RevicedFromCustomer != null)
                        {
                            sumDPO += Convert.ToDecimal(resultlistsum2[i].RevicedFromCustomer);
                        }

                        if (resultlistsum2[i].ReceivedByABA != null)
                        {
                            sumDPO += Convert.ToDecimal(resultlistsum2[i].ReceivedByABA);
                        }

                        dr["TotalDepo"] = sumDPO;
                    }

                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            DateTime endDatetime = DateTime.Now;
            var start = (endDatetime - startDateTime).TotalSeconds;

            return View();
        }      
        public ActionResult SalesDetailsCopy(SaleViewModels sm)
        {

            jotunDBEntities db = new jotunDBEntities();
            // List<string>  = new List<string>();

            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("CustomerNo"));
            dt.Columns.Add(new DataColumn("CustomerName"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("customer_location"));
            dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("ProductId"));
            dt.Columns.Add(new DataColumn("ProductName"));
            dt.Columns.Add(new DataColumn("Quantity"));
            dt.Columns.Add(new DataColumn("Refer"));
            dt.Columns.Add(new DataColumn("ColorCode"));
            dt.Columns.Add(new DataColumn("Price"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("TotalAmt"));
            dt.Columns.Add(new DataColumn("TotalDepo"));
            dt.Columns.Add(new DataColumn("TotalDis"));
            dt.Columns.Add(new DataColumn("OrderDate"));
            dt.Columns.Add(new DataColumn("PayDate"));

            DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);
            var treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates
                                select new
                                {
                                    s1,
                                    sd
                                }).ToList();
            var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();

            if (sm.CustomerId != null)
            {
                if (customer_name.is_old_data == true)
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }

            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                select new
                                {
                                    s1,
                                    sd
                                }).ToList();
            }

            var resultlist = (from i in treatmentSum.ToList()
                              select i
                              ).ToList();
            if (sm.filter_project_location_name != null)
            {
                resultlist = (from i in treatmentSum.ToList()
                              join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                              where sm.filter_project_location_name == lo.location
                              select i
                              ).ToList();
            }
            else
            {
                resultlist = treatmentSum;
            }

            var resultlist2 = (from i in resultlist.ToList()
                              select i
                              ).ToList();

            if (sm.filter_product_id != null)
            {
                resultlist2 = (from i in resultlist.ToList()
                               join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                               where sm.filter_product_id == pro.Id
                                   select i
                              ).ToList();
            }
            else
            {
                resultlist2 = resultlist;
            }
            DateTime startDateTime = DateTime.Now;


            if (resultlist2.Any())
            {
                foreach (var item in resultlist2)
                {


                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.s1.CustomerId
                                    select p).ToList();
                    var product = (from p in db.tblProducts
                                   where p.Id == item.sd.ProductId
                                   select p).ToList();
                    var unit = (from u in db.tblUnits
                                where u.Id == item.sd.UnitTypeId && u.Status == 1
                                select u).FirstOrDefault();

                    var site = (from s in db.tblCustomer_location
                                where s.id == item.s1.customer_location
                                select s).FirstOrDefault();
                    dr["Id"] = item.s1.Id;
                    foreach (var i in customer)
                    {
                        dr["CustomerNo"] = i.CustomerNo;
                        dr["CustomerName"] = i.CustomerName;
                    }
                    foreach (var i in product)
                    {
                        dr["ProductName"] = i.ProductName;
                    }
                    if (sm.CustomerId != null)
                    {
                        dr["CustomerId"] = customer_name.CustomerName;
                    }
                    else
                    {
                        dr["CustomerId"] = "All";
                    }
                    dr["InvoiceNo"] = item.s1.InvoiceNo;
                    dr["Status"] = item.s1.InvoiceStatus;
                    dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                    dr["RevicedFromCustomer"] = float.Parse(item.s1.RevicedFromCustomer.ToString()).ToString("$0.000");
                    dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dr["ColorCode"] = item.sd.color_code;
                    if (item.s1.customer_location != null)
                    {
                        dr["customer_location"] = site == null ? string.Empty: site.location;
                    }
                    dr["Quantity"] = float.Parse(item.sd.Quantity.ToString()).ToString("0.00") + " " + unit.UnitNameEng;
                    if (item.sd.actual_price != null)
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                    }
                    else
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                    }
                    dr["ProductId"] = item.sd.ProductId;
                    dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                    if (item.s1.UpdatedDate != null)
                    {
                        dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                    }
                    else
                    {
                        dr["PayDate"] = "";
                    }


                    var getSomeAmount = (from ps in db.tblSales
                                         where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                         select ps).ToList();
                    if (sm.CustomerId != null)
                    {
                        if (customer_name.is_old_data == true)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                    }
                    else
                    {
                        var cus_old_data = (from cd in db.tblCustomers
                                            where cd.Id == item.s1.CustomerId && cd.is_old_data == true
                                            select cd).FirstOrDefault();
                        if (cus_old_data != null)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (cu.CustomerName == cus_old_data.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                    }

                    var resultlistsum = (from i in getSomeAmount
                                         select i).ToList();
                    if (sm.filter_project_location_name != null)
                    {
                        resultlistsum = (from i in getSomeAmount
                                         join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                         where sm.filter_project_location_name == lo.location
                                         select i).ToList();
                    }
                    else
                    {
                        resultlistsum = getSomeAmount;
                    }
                    var resultlistsum2 = (from i in resultlistsum
                                          select i).ToList();
                    if (sm.filter_product_id != null)
                    {

                        resultlistsum2 = (from i in resultlistsum
                                          join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                          where sm.filter_product_id == pd.ProductId
                                          select i).Distinct().ToList();
                    }
                    else
                    {
                        resultlistsum2 = resultlistsum;

                    }

                    decimal sumDP = 0;
                    for (int i = 0; i < resultlistsum2.Count(); i++)
                    {
                        if (resultlistsum2[i].Discount != null)
                        {
                            sumDP += Convert.ToDecimal(resultlistsum2[i].Discount);
                        }
                        dr["TotalDis"] = sumDP;
                    }

                    decimal sumDPO = 0;
                    for (int i = 0; i < resultlistsum2.Count(); i++)
                    {
                        if (resultlistsum2[i].RevicedFromCustomer != null)
                        {
                            sumDPO += Convert.ToDecimal(resultlistsum2[i].RevicedFromCustomer);
                        }
                        dr["TotalDepo"] = sumDPO;
                    }
                    dt.Rows.Add(dr);
                }
            }


            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            DateTime endDatetime = DateTime.Now;
            var start = (endDatetime - startDateTime).TotalSeconds;

            return View();
        }
        
        
        public ActionResult SalesDetailsNetIncome(SaleViewModels sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            // List<string>  = new List<string>();

            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("CustomerNo"));
            dt.Columns.Add(new DataColumn("CustomerName"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("customer_location"));
            dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("ProductId"));
            dt.Columns.Add(new DataColumn("ProductName"));
            dt.Columns.Add(new DataColumn("Quantity"));
            dt.Columns.Add(new DataColumn("Refer"));
            dt.Columns.Add(new DataColumn("ColorCode"));
            dt.Columns.Add(new DataColumn("Price"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("TotalAmt"));
            dt.Columns.Add(new DataColumn("TotalDepo"));
            dt.Columns.Add(new DataColumn("TotalDis"));
            dt.Columns.Add(new DataColumn("OrderDate"));
            dt.Columns.Add(new DataColumn("PayDate"));
            dt.Columns.Add(new DataColumn("NetIncome"));

            DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);
            var treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                join pro in db.tblProducts on sd.ProductId equals pro.Id
                                join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates 
                                select new
                                {
                                    s1,
                                    sd,
                                    probyunit,
                                }).ToList();
            var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();
            var emptyDate = new DateTime(0001, 1, 1);


            if (sm.CustomerId != null)
            {
                if (sm.FromDate == emptyDate)
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        join pro in db.tblProducts on sd.ProductId equals pro.Id
                                        join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                        where s1.Status == 1 && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                        select new
                                        {
                                            s1,
                                            sd,
                                            probyunit,
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        join pro in db.tblProducts on sd.ProductId equals pro.Id
                                        join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                        where s1.Status == 1 && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                        select new
                                        {
                                            s1,
                                            sd,
                                            probyunit,
                                        }).ToList();
                    }
                }
                else
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        join pro in db.tblProducts on sd.ProductId equals pro.Id
                                        join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                        where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                        select new
                                        {
                                            s1,
                                            sd,
                                            probyunit,
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        join pro in db.tblProducts on sd.ProductId equals pro.Id
                                        join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                        where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                        select new
                                        {
                                            s1,
                                            sd,
                                            probyunit,
                                        }).ToList();
                    }
                }
                

            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                join pro in db.tblProducts on sd.ProductId equals pro.Id
                                join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                select new
                                {
                                    s1,
                                    sd,
                                    probyunit,
                                }).ToList();
            }

            var resultlist = (from i in treatmentSum.ToList()
                              select i
                              ).ToList();
            if (sm.filter_project_location_name != null)
            {
                resultlist = (from i in treatmentSum.ToList()
                              join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                              where sm.filter_project_location_name == lo.location
                              select i
                              ).ToList();
            }
            else
            {
                resultlist = treatmentSum;
            }

            var resultlist2 = (from i in resultlist.ToList()
                              select i
                              ).ToList();

            if (sm.filter_product_id != null)
            {
                resultlist2 = (from i in resultlist.ToList()
                               join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                               where sm.filter_product_id == pro.Id
                                   select i
                              ).ToList();
            }
            else
            {
                resultlist2 = resultlist;
            }
            if (resultlist2.Any())
            {
                foreach (var item in resultlist2)
                {


                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.s1.CustomerId
                                    select p).ToList();
                    var product = (from p in db.tblProducts
                                   where p.Id == item.sd.ProductId
                                   select p).ToList();
                    var unit = (from u in db.tblUnits
                                where u.Id == item.sd.UnitTypeId && u.Status == 1
                                select u).FirstOrDefault();

                    var site = (from s in db.tblCustomer_location
                                where s.id == item.s1.customer_location
                                select s).FirstOrDefault();
                    item.s1.ReceivedByABA = item.s1.ReceivedByABA ?? 0;
                    item.s1.RevicedFromCustomer = item.s1.RevicedFromCustomer ?? 0;

                    dr["Id"] = item.s1.Id;
                    foreach (var i in customer)
                    {
                        dr["CustomerNo"] = i.CustomerNo;
                        dr["CustomerName"] = i.CustomerName;
                    }
                    foreach (var i in product)
                    {
                        dr["ProductName"] = i.ProductName;
                    }
                    if (sm.CustomerId != null)
                    {
                        dr["CustomerId"] = customer_name.CustomerName;
                    }
                    else
                    {
                        dr["CustomerId"] = "All";
                    }
                    dr["InvoiceNo"] = item.s1.InvoiceNo;
                    dr["Status"] = item.s1.InvoiceStatus;
                    dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                    dr["RevicedFromCustomer"] = float.Parse((item.s1.RevicedFromCustomer+(double)item.s1.ReceivedByABA).ToString()).ToString("$0.000");
                    dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dr["ColorCode"] = item.sd.color_code;
                    if (item.s1.customer_location != null)
                    {
                        dr["customer_location"] = site == null ? string.Empty: site.location;
                    }
                    dr["Quantity"] = float.Parse(item.sd.Quantity.ToString()).ToString("0.00") + " " + unit.UnitNameEng;
                    if (item.sd.actual_price != null)
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                        dr["NetIncome"] = float.Parse(((item.sd.actual_price * item.sd.Quantity) - (item.probyunit.Cost * item.sd.Quantity)).ToString()).ToString("0.000");
                    }
                    else
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                        dr["NetIncome"] = float.Parse(((item.sd.Price * item.sd.Quantity) - (item.probyunit.Cost * item.sd.Quantity)).ToString()).ToString("0.000");

                    }
                    dr["ProductId"] = item.sd.ProductId;
                    dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                    if (item.s1.UpdatedDate != null)
                    {
                        dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                    }
                    else
                    {
                        dr["PayDate"] = "";
                    }

                    var getSomeAmount = (from ps in db.tblSales
                                         where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                         select ps).ToList();
                    if (sm.CustomerId != null)
                    {
                        if (customer_name.is_old_data == true)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                    }
                    else
                    {
                        var cus_old_data = (from cd in db.tblCustomers
                                            where cd.Id == item.s1.CustomerId && cd.is_old_data == true
                                            select cd).FirstOrDefault();
                        if (cus_old_data != null)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (cu.CustomerName == cus_old_data.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                    }

                    var resultlistsum = (from i in getSomeAmount
                                         select i).ToList();
                    if (sm.filter_project_location_name != null)
                    {
                        resultlistsum = (from i in getSomeAmount
                                         join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                         where sm.filter_project_location_name == lo.location
                                         select i).ToList();
                    }
                    else
                    {
                        resultlistsum = getSomeAmount;
                    }
                    var resultlistsum2 = (from i in resultlistsum
                                         select i).ToList();
                    if (sm.filter_product_id != null)
                    {

                        resultlistsum2 = (from i in resultlistsum
                                          join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                          where sm.filter_product_id == pd.ProductId
                                          select i).Distinct().ToList();
                    }
                    else
                    {
                        resultlistsum2 = resultlistsum;
 
                    }
                    decimal sumDP = 0;
                    for (int i = 0; i < resultlistsum2.Count(); i++)
                    {
                        if (resultlistsum2[i].Discount != null)
                        {
                            sumDP += Convert.ToDecimal(resultlistsum2[i].Discount);
                        }
                        dr["TotalDis"] = sumDP;
                    }

                    decimal sumDPO = 0;
                    for (int i = 0; i < resultlistsum2.Count(); i++)
                    {
                        if (resultlistsum2[i].RevicedFromCustomer != null)
                        {
                            sumDPO += Convert.ToDecimal(resultlistsum2[i].RevicedFromCustomer);
                        }
                        if (resultlistsum2[i].ReceivedByABA != null)
                        {
                            sumDPO += Convert.ToDecimal(resultlistsum2[i].ReceivedByABA);
                        }
                        dr["TotalDepo"] = sumDPO;
                    }

                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetailsNetIncome.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleDetailsNetIncome", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            return View();
        }
        
        public ActionResult SalesDetails21july2020(SaleViewModels sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            // List<string>  = new List<string>();

            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("CustomerNo"));
            dt.Columns.Add(new DataColumn("CustomerName"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("customer_location"));
            dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("ProductId"));
            dt.Columns.Add(new DataColumn("ProductName"));
            dt.Columns.Add(new DataColumn("Quantity"));
            dt.Columns.Add(new DataColumn("Refer"));
            dt.Columns.Add(new DataColumn("ColorCode"));
            dt.Columns.Add(new DataColumn("Price"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("TotalAmt"));
            dt.Columns.Add(new DataColumn("TotalDepo"));
            dt.Columns.Add(new DataColumn("TotalDis"));
            dt.Columns.Add(new DataColumn("OrderDate"));
            dt.Columns.Add(new DataColumn("PayDate"));

            DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);
            var treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                where s1.Status == 1 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates
                                select new
                                {
                                    s1,
                                    sd
                                }).ToList();
            var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();

            if (sm.CustomerId != null)
            {
                if (customer_name.is_old_data == true)
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }

            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                where s1.Status == 1 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                select new
                                {
                                    s1,
                                    sd
                                }).ToList();
            }

            var resultlist = (from i in treatmentSum.ToList()
                              select i
                              ).ToList();
            if (sm.filter_project_location_name != null)
            {
                resultlist = (from i in treatmentSum.ToList()
                              join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                              where sm.filter_project_location_name == lo.location
                              select i
                              ).ToList();
            }
            else
            {
                resultlist = treatmentSum;
            }



            if (resultlist.Any())
            {
                foreach (var item in resultlist)
                {


                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.s1.CustomerId
                                    select p).ToList();
                    var product = (from p in db.tblProducts
                                   where p.Id == item.sd.ProductId
                                   select p).ToList();
                    var unit = (from u in db.tblUnits
                                where u.Id == item.sd.UnitTypeId && u.Status == 1
                                select u).FirstOrDefault();

                    var site = (from s in db.tblCustomer_location
                                where s.id == item.s1.customer_location
                                select s).FirstOrDefault();
                    dr["Id"] = item.s1.Id;
                    foreach (var i in customer)
                    {
                        dr["CustomerNo"] = i.CustomerNo;
                        dr["CustomerName"] = i.CustomerName;
                    }
                    foreach (var i in product)
                    {
                        dr["ProductName"] = i.ProductName;
                    }
                    if (sm.CustomerId != null)
                    {
                        dr["CustomerId"] = customer_name.CustomerName;
                    }
                    else
                    {
                        dr["CustomerId"] = "All";
                    }
                    dr["InvoiceNo"] = item.s1.InvoiceNo;
                    dr["Status"] = item.s1.InvoiceStatus;
                    dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                    dr["RevicedFromCustomer"] = float.Parse(item.s1.RevicedFromCustomer.ToString()).ToString("$0.000");
                    dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dr["ColorCode"] = item.sd.color_code;
                    if (item.s1.customer_location != null)
                    {
                        dr["customer_location"] = site.location;
                    }
                    dr["Quantity"] = float.Parse(item.sd.Quantity.ToString()).ToString("0.00") + " " + unit.UnitNameEng;
                    if (item.sd.actual_price != null)
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                    }
                    else
                    {
                        dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                        dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                    }
                    dr["ProductId"] = item.sd.ProductId;
                    dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                    if (item.s1.UpdatedDate != null)
                    {
                        dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                    }
                    else
                    {
                        dr["PayDate"] = "";
                    }






                    var getSomeAmount = (from ps in db.tblSales
                                         where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                         select ps).ToList();
                    if (sm.CustomerId != null)
                    {
                        if (customer_name.is_old_data == true)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                    }
                    else
                    {
                        var cus_old_data = (from cd in db.tblCustomers
                                            where cd.Id == item.s1.CustomerId && cd.is_old_data == true
                                            select cd).FirstOrDefault();
                        if (cus_old_data != null)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates && (cu.CustomerName == cus_old_data.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.CreatedDate >= sm.FromDate && ps.CreatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                             select ps).ToList();
                        }
                    }

                    var resultlistsum = (from i in getSomeAmount
                                         select i).ToList();
                    if (sm.filter_project_location_name != null)
                    {
                        resultlistsum = (from i in getSomeAmount
                                         join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                         where sm.filter_project_location_name == lo.location
                                         select i).ToList();
                    }
                    else
                    {
                        resultlistsum = getSomeAmount;
                    }

                    decimal sumDP = 0;
                    for (int i = 0; i < resultlistsum.Count(); i++)
                    {
                        if (resultlistsum[i].Discount != null)
                        {
                            sumDP += Convert.ToDecimal(resultlistsum[i].Discount);
                        }
                        dr["TotalDis"] = sumDP;
                    }

                    decimal sumDPO = 0;
                    for (int i = 0; i < resultlistsum.Count(); i++)
                    {
                        if (resultlistsum[i].RevicedFromCustomer != null)
                        {
                            sumDPO += Convert.ToDecimal(resultlistsum[i].RevicedFromCustomer);
                        }
                        dr["TotalDepo"] = sumDPO;
                    }








                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            return View();
        }


        public ActionResult Owe(SaleViewModels sm)
        {

            jotunDBEntities db = new jotunDBEntities();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("id"));
            dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("OweBy"));
            dt.Columns.Add(new DataColumn("OweDayTotal"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Deposit"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("OwnAmount"));
            dt.Columns.Add(new DataColumn("OrderDate"));

            DateTime ToDates = sm.ToDate.AddHours(23.59);
            // DateTime toDate = Convert.ToDateTime(sm.DateTo).AddDays(1);
            var treatmentSum = new List<tblSale>();
            var emptyDate = new DateTime(0001, 1, 1);
            if (sm.FromDate == emptyDate)
            {
                treatmentSum = (from s1 in db.tblSales
                                orderby s1.CreatedDate
                                where s1.Status == 1 && (s1.Amount - s1.Discount - s1.RevicedFromCustomer) > 0 && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                select s1).ToList();
            }
            else
            {
                treatmentSum = (from s1 in db.tblSales
                                    orderby s1.CreatedDate
                                    where s1.Status == 1 && (s1.Amount - s1.Discount - s1.RevicedFromCustomer) > 0 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                    select s1).ToList();
            }

            foreach (var item in treatmentSum)
            {
                if (item.Status == 1)
                {
                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.CustomerId
                                    select p).ToList();


                    DateTime datenow = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyy")).AddDays(0);
                    DateTime datec = Convert.ToDateTime(Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy")).AddDays(0);
                    var datetotal = (datenow - datec).TotalDays;

                    if (sm.CustomerId == "" || sm.CustomerId == null)
                    {
                        dr["CustomerId"] = "All";

                    }
                    else
                    {
                        var cus = (from su in db.tblCustomers
                                   where su.Id == sm.CustomerId
                                   select su).FirstOrDefault();
                        dr["CustomerId"] = cus.CustomerName;

                    }

                    dr["id"] = item.Id;
                    item.ReceivedByABA = item.ReceivedByABA ?? 0;
                    item.RevicedFromCustomer = item.RevicedFromCustomer ?? 0;
                    //double ownAmount =Convert.ToDecimal(item.Amount - item.Discount - item.RevicedFromCustomer - (decimal)item.ReceivedByABA);
                    decimal ownAmount =Convert.ToDecimal(item.SaleAmount) - Convert.ToDecimal(item.SaleDeposit);
                    foreach (var i in customer)
                    {
                        dr["OweBy"] = i.CustomerName;
                    }
                    //  dr["OweBy"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                    dr["OweDayTotal"] = datetotal + " day(s)";
                    //dr["OweDayTotal"] = datetotal;
                    dr["OrderDate"] = Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy");
                    dr["InvoiceNo"] = item.InvoiceNo;
                    dr["Status"] = item.InvoiceStatus;
                    dr["Deposit"] = Convert.ToDecimal(item.SaleDeposit);//  float.Parse((item.RevicedFromCustomer+(float)item.ReceivedByABA).ToString()).ToString("0.000");
                    dr["Discount"] = Convert.ToDecimal(item.Discount); //float.Parse(item.Discount.ToString()).ToString("0.000");
                    dr["Amount"] = Convert.ToDecimal(item.Amount); //float.Parse(item.Amount.ToString()).ToString("0.000");
                    //dr["OwnAmount"] = float.Parse((item.Amount -item.Discount - item.RevicedFromCustomer-(float)item.ReceivedByABA).ToString()).ToString("0.000");
                    dr["OwnAmount"] = Convert.ToDecimal(ownAmount);// float.Parse(ownAmount.ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    if(ownAmount>0)
                        dt.Rows.Add(dr);
                }
            }
           
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Owe.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSOwes", dt);
            rv.LocalReport.DataSources.Add(rds);


            ViewBag.ReportViewer = rv;
           
            return View();
        }
        public ActionResult ItemsInStock(ProductViewModels sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Id"));

            dt.Columns.Add(new DataColumn("ProductName"));
            dt.Columns.Add(new DataColumn("CategoryId"));
            dt.Columns.Add(new DataColumn("QuantityInStock"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("ProductCode"));

            List<tblProduct> product = new List<tblProduct>();
            if(string.IsNullOrEmpty(sm.CategoryId) || string.Compare(sm.CategoryId,"0")==0)
                 product = (from item in db.tblProducts
                           orderby item.QuantityInStock descending
                           where item.QuantityInStock <= item.QuantityInStock && item.Status == 1
                           select item).ToList();
            else
                product = (from item in db.tblProducts
                           orderby item.QuantityInStock descending
                           where item.QuantityInStock <= item.QuantityInStock && item.Status == 1 && string.Compare(item.CategoryId,sm.CategoryId)==0
                           select item).ToList();


            foreach (var item in product)
            {
                var category = (from p1 in db.tblCategories
                                where p1.Id == item.CategoryId
                                select p1).FirstOrDefault();

                var categoryss = category.CategoryNameEng;
                if (categoryss == null)
                {
                    categoryss = category.CategoryNameKh;
                }

                DataRow dr = dt.NewRow();

                dr["Id"] = item.Id;

                dr["ProductName"] = item.ProductName;
                dr["ProductCode"] = item.ProductCode;
                dr["CategoryId"] = categoryss;
                dr["QuantityInStock"] = item.QuantityInStock;
                var getSomeAmount = (from s1 in db.tblProducts
                                     where s1.Status == 1
                                     select s1).ToList();
                decimal sumDP = 0;
                for (int i = 0; i < getSomeAmount.Count(); i++)
                {
                    if (getSomeAmount[i].QuantityInStock != null)
                    {
                        sumDP += Convert.ToDecimal(getSomeAmount[i].QuantityInStock);
                    }
                    dr["Total"] = sumDP;
                }

                dt.Rows.Add(dr);

            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\ItemsInStockReport.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSItemsInStock", dt);
            rv.LocalReport.DataSources.Add(rds);


            ViewBag.ReportViewer = rv;

            ViewBag.CategoryNamesEng = db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null")).ToList();

            return View(sm);

        }
        public ActionResult ItemNearOutOfStock(ProductViewModels sm)
        {
            jotunDBEntities db = new jotunDBEntities();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("id"));
            dt.Columns.Add(new DataColumn("item_id"));
            dt.Columns.Add(new DataColumn("qty_in_stock"));
            dt.Columns.Add(new DataColumn("qty_alert"));
            dt.Columns.Add(new DataColumn("supply_by"));
            dt.Columns.Add(new DataColumn("phone"));
            dt.Columns.Add(new DataColumn("category_id"));
            var product = (from item in db.tblProducts
                           orderby item.QuantityInStock descending
                           where item.QuantityInStock <= item.quantity_alert && item.Status == 1
                           select item).ToList();
            foreach (var result in product)
            {
                var category = (from p1 in db.tblCategories
                                where p1.Id == result.CategoryId
                                select p1).FirstOrDefault();

                var categoryss = category.CategoryNameEng;
                if (categoryss == null)
                {
                    categoryss = category.CategoryNameKh;
                }
                DataRow dr = dt.NewRow();
                dr["id"] = result.Id;
                dr["item_id"] = result.ProductName;
                dr["qty_in_stock"] = result.QuantityInStock;
                dr["qty_alert"] = result.quantity_alert;
                dr["category_id"] = categoryss;
                dt.Rows.Add(dr);
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\ItemNearOutOfStock.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSItemNearOutOfStock", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            return View();
        }
        public ActionResult OweMoreThan(SaleViewModels sm)
        {

            jotunDBEntities db = new jotunDBEntities();
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("FromDate"));
            dt.Columns.Add(new DataColumn("ToDate"));
            dt.Columns.Add(new DataColumn("id"));
            // dt.Columns.Add(new DataColumn("CustomerId"));
            dt.Columns.Add(new DataColumn("OweBy"));
            dt.Columns.Add(new DataColumn("OweDayTotal"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("Deposit"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("Amount"));
            dt.Columns.Add(new DataColumn("OwnAmount"));
            dt.Columns.Add(new DataColumn("OrderDate"));

            DateTime ToDates = sm.ToDate.AddHours(23.59);
            // DateTime toDate = Convert.ToDateTime(sm.DateTo).AddDays(1);



            var treatmentSum = (from s1 in db.tblSales
                                where s1.Status == 1
                                select s1).ToList();

            foreach (var item in treatmentSum)
            {
                if (item.Status == 1)
                {
                    DataRow dr = dt.NewRow();
                    var customer = (from p in db.tblCustomers
                                    where p.Id == item.CustomerId
                                    select p).ToList();


                    DateTime datenow = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyy")).AddDays(0);
                    DateTime datec = Convert.ToDateTime(Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy")).AddDays(0);
                    var datetotal = (datenow - datec).TotalDays;



                    dr["id"] = item.Id;
                    foreach (var i in customer)
                    {
                        dr["OweBy"] = i.CustomerName;
                    }
                    //  dr["OweBy"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                    dr["OweDayTotal"] = datetotal + " day(s)";
                    dr["OrderDate"] = Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy");
                    dr["InvoiceNo"] = item.InvoiceNo;
                    dr["Status"] = item.InvoiceStatus;
                    dr["Deposit"] = float.Parse(item.RevicedFromCustomer.ToString()).ToString("0.000");
                    dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                    dr["Amount"] = float.Parse(item.Amount.ToString()).ToString("0.000");
                    dr["OwnAmount"] = float.Parse((item.Amount - item.RevicedFromCustomer).ToString()).ToString("0.000");
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
                    dt.Rows.Add(dr);
                }
            }
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\OweMoreThan.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSOwes", dt);
            rv.LocalReport.DataSources.Add(rds);


            ViewBag.ReportViewer = rv;

            return View();
        }
        public ActionResult ExportToPDFPurchase(string datefrom, string dateto)
        {
            try
            {

                jotunDBEntities db = new jotunDBEntities();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("SupplierId"));
                dt.Columns.Add(new DataColumn("ShipperAmount"));
                dt.Columns.Add(new DataColumn("Deposit"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("PurchaseAmount"));



                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);

                var treatmentSum = (from s1 in db.tblPurchaseBySuppliers
                                    where s1.Status == 1 && s1.CreatedDate >= datef && s1.CreatedDate <= ToDates
                                    select s1).ToList();

                foreach (var item in treatmentSum)
                {
                    string nonull = "0.000";

                    if (item.Status == 1)
                    {
                        DataRow dr = dt.NewRow();

                        var sup = (from su in db.tblSuppliers
                                   where su.Id == item.SupplierId
                                   select su).FirstOrDefault();

                        var sip = (from si in db.tblShippers
                                   where si.Id == item.ShipperId
                                   select si).FirstOrDefault();

                        if (item.PurchaseAmount == null)
                        {
                            item.PurchaseAmount = Convert.ToDecimal(nonull);
                        }
                        if (item.Deposit == null)
                        {
                            item.Deposit = Convert.ToDecimal(nonull);
                        }
                        if (item.Discount == null)
                        {
                            item.Discount = Convert.ToDecimal(nonull);
                        }
                        if (item.ShipperAmount == null)
                        {
                            item.ShipperAmount = Convert.ToDecimal(nonull);
                        }


                        dr["InvoiceNo"] = item.InvoiceNo;
                        dr["SupplierId"] = sup.SupplierName;
                        dr["ShipperAmount"] = float.Parse(item.ShipperAmount.ToString()).ToString("0.000");
                        dr["Deposit"] = float.Parse(item.Deposit.ToString()).ToString("0.000");
                        dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                        dr["PurchaseAmount"] = float.Parse(item.PurchaseAmount.ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        //dr["CreatedDate"] = Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy");


                        dt.Rows.Add(dr);
                    }
                }

                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Purchase.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSPurchase", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;
                rv.ProcessingMode = ProcessingMode.Local;



                string mimeType, encoding, filenameExtension;

                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File
                string FileName = "Purchase_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        public ActionResult ExportToPDFPurchaseDetail(string datefrom, string dateto)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("SupplierId"));
                dt.Columns.Add(new DataColumn("ShipperAmount"));
                dt.Columns.Add(new DataColumn("Deposit"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("TotalDiscount"));
                dt.Columns.Add(new DataColumn("PurchaseAmount"));
                dt.Columns.Add(new DataColumn("Phone"));
                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("QTY"));
                dt.Columns.Add(new DataColumn("Cost"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("CreatedDate"));
                dt.Columns.Add(new DataColumn("TotalAmount"));
                dt.Columns.Add(new DataColumn("TotalDeposit"));
                dt.Columns.Add(new DataColumn("TotalShipping"));
                dt.Columns.Add(new DataColumn("Revenue"));
                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);
                var treatmentSum = (from ps in db.tblPurchaseBySuppliers
                                    orderby ps.CreatedDate descending
                                    join pd in db.tblPurchaseBySupplierDetails on ps.Id equals pd.PurchaseBySupplierId
                                    where ps.Status == 1 && ps.CreatedDate >= datef && ps.CreatedDate <= ToDates
                                    select new { ps, pd }).ToList();
                foreach (var item in treatmentSum)
                {
                    string nonull = "0.000";

                    if (item.ps.Status == 1)
                    {
                        DataRow dr = dt.NewRow();

                        var sup = (from su in db.tblSuppliers
                                   where su.Id == item.ps.SupplierId
                                   select su).FirstOrDefault();

                        var sip = (from si in db.tblShippers
                                   where si.Id == item.ps.ShipperId
                                   select si).FirstOrDefault();
                        var pro = (from pr in db.tblProducts
                                   where pr.Id == item.pd.ProductId
                                   select pr).FirstOrDefault();

                        var unit = (from u in db.tblUnits
                                    where u.Id == item.pd.UnitTypeId && u.Status == 1
                                    select u).FirstOrDefault();

                        if (item.ps.PurchaseAmount == null)
                        {
                            item.ps.PurchaseAmount = Convert.ToDecimal(nonull);
                        }
                        if (item.ps.Deposit == null)
                        {
                            item.ps.Deposit = Convert.ToDecimal(nonull);
                        }
                        if (item.ps.Discount == null)
                        {
                            item.ps.Discount = Convert.ToDecimal(nonull);
                        }
                        if (item.ps.ShipperAmount == null)
                        {
                            item.ps.ShipperAmount = Convert.ToDecimal(nonull);
                        }
                        decimal? sum = 0;
                        for (int i = 0; i < treatmentSum.Count(); i++)
                        {
                            sum += treatmentSum[i].ps.PurchaseAmount;
                            dr["TotalAmount"] = sum;
                        };

                        dr["Id"] = item.pd.PurchaseBySupplierId;
                        dr["InvoiceNo"] = item.ps.InvoiceNo;
                        dr["SupplierId"] = sup.SupplierName;
                        dr["ShipperAmount"] = float.Parse(item.ps.ShipperAmount.ToString()).ToString("0.000");
                        dr["Deposit"] = float.Parse(item.ps.Deposit.ToString()).ToString("0.000");
                        dr["Discount"] = float.Parse(item.ps.Discount.ToString()).ToString("0.000");
                        decimal? dis = ((item.pd.Cost * item.pd.Quantity) * item.pd.Discount) / 100;
                        dr["TotalDiscount"] = float.Parse(dis.ToString()).ToString("0.000");

                        var getSomeAmount = (from ps in db.tblPurchaseBySuppliers
                                             orderby ps.CreatedDate descending
                                             where ps.Status == 1 && ps.CreatedDate >= datef && ps.CreatedDate <= ToDates
                                             select ps).ToList();
                        decimal? sumDP = 0;
                        for (int i = 0; i < getSomeAmount.Count(); i++)
                        {
                            if (getSomeAmount[i].Deposit != null)
                            {
                                sumDP += getSomeAmount[i].Deposit;
                            }
                            dr["TotalDeposit"] = sumDP;

                        }
                        decimal? sumSP = 0;
                        for (int i = 0; i < getSomeAmount.Count(); i++)
                        {
                            if (getSomeAmount[i].ShipperAmount != null)
                            {
                                sumSP += getSomeAmount[i].ShipperAmount;
                            }
                            dr["TotalShipping"] = sumSP;

                        }
                        dr["PurchaseAmount"] = float.Parse((Convert.ToDecimal(item.ps.PurchaseAmount) + Convert.ToDecimal(item.ps.Discount)).ToString()).ToString("0.000");
                        //dr["PurchaseAmount"] = float.Parse((Convert.ToDecimal(item.ps.PurchaseAmount) ).ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dr["CreatedDate"] = Convert.ToDateTime(item.ps.CreatedDate).ToString("dd-MMM-yyyy");

                        dr["Phone"] = sup.ContactPhone;
                        dr["ProductName"] = pro.ProductName;
                        dr["QTY"] = item.pd.Quantity +" "+unit.UnitNameEng;
                        dr["Cost"] = item.pd.Cost;
                        dr["Total"] = (item.pd.Quantity * item.pd.Cost);
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\PurchaseDetail.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSPurchaseDetail", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;
                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "PurchaseDetail_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        public ActionResult ExportTOPDFSale(string datefrom, string dateto, string customerid)
        {

            try
            {
                jotunDBEntities db = new jotunDBEntities();
                List<tblSale> treatmentSum = new List<tblSale>();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNameFilter"));
                dt.Columns.Add(new DataColumn("Owe"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);

                var customer_name = db.tblCustomers.Where(x => x.Id == customerid).FirstOrDefault();
                if (customerid != "")
                {

                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        where s1.Status == 1 && (s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates) && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                        select s1).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        where s1.Status == 1 && (s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates) && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                        select s1).ToList();
                    }
                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    where s1.Status == 1 && (s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates) && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                    select s1).ToList();
                }
                foreach (var item in treatmentSum)
                {
                    if (item.Status == 1)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.CustomerId
                                        select p).ToList();

                        dr["Id"] = item.Id;
                        foreach (var i in customer)
                        {
                            dr["CustomerId"] = i.CustomerName;

                        }
                        dr["Owe"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                        dr["RevicedFromCustomer"] = float.Parse(item.RevicedFromCustomer.ToString()).ToString("0.000");
                        // dr["CreatedDate"] = item.CreatedDate;
                        if (item.CustomerId != null)
                        {
                            dr["CustomerNameFilter"] = customer_name?.CustomerName ?? "All";
                        }
                        else
                        {
                            dr["CustomerNameFilter"] = "All";
                        }
                        dr["InvoiceNo"] = item.InvoiceNo;
                        dr["Status"] = item.InvoiceStatus;
                        dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                        dr["Amount"] = float.Parse(item.Amount.ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Sale.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSale", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;

                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "Sale_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        public ActionResult ExportToPDFSaleDetail(string datefrom, string dateto, string customerid ,string projectlocation, string productid)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                // List<string>  = new List<string>();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNo"));
                dt.Columns.Add(new DataColumn("CustomerName"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("customer_location"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Refer"));
                dt.Columns.Add(new DataColumn("ColorCode"));
                dt.Columns.Add(new DataColumn("Price"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("TotalAmt"));
                dt.Columns.Add(new DataColumn("TotalDepo"));
                dt.Columns.Add(new DataColumn("TotalDis"));
                dt.Columns.Add(new DataColumn("OrderDate"));
                dt.Columns.Add(new DataColumn("PayDate"));

                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);
                var treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                var customer_name = db.tblCustomers.Where(x => x.Id == customerid).FirstOrDefault();

                if (customerid != "")
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }

                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }


                var resultlist = (from i in treatmentSum.ToList()
                                  select i
                              ).ToList();
                if (projectlocation != "")
                {
                    resultlist = (from i in treatmentSum.ToList()
                                  join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                                  where projectlocation == lo.location
                                  select i
                                  ).ToList();
                }
                else
                {
                    resultlist = treatmentSum;
                }

                var resultlist2 = (from i in resultlist.ToList()
                                   select i
                                  ).ToList();

                if (productid != "")
                {
                    resultlist2 = (from i in resultlist.ToList()
                                   join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                                   where productid == pro.Id
                                   select i
                                  ).ToList();
                }
                else
                {
                    resultlist2 = resultlist;
                }


                var getSomeAmount = (from ps in db.tblSales
                                     where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                     select ps).ToList();


                var resultlistsum = (from i in getSomeAmount
                                     select i).ToList();
                if (projectlocation != null)
                {
                    resultlistsum = (from i in getSomeAmount
                                     join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                     where projectlocation == lo.location
                                     select i).ToList();
                }
                else
                {
                    resultlistsum = getSomeAmount;
                }
                var resultlistsum2 = (from i in resultlistsum
                                      select i).ToList();
                if (productid != null)
                {

                    resultlistsum2 = (from i in resultlistsum
                                      join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                      where productid == pd.ProductId
                                      select i).Distinct().ToList();
                }
                else
                {
                    resultlistsum2 = resultlistsum;

                }



                if (resultlist2.Any())
                {
                    foreach (var item in resultlist2)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.s1.CustomerId
                                        select p).ToList();
                        var product = (from p in db.tblProducts
                                       where p.Id == item.sd.ProductId 
                                       select p).ToList();
                        var unit = (from u in db.tblUnits
                                    where u.Id == item.sd.UnitTypeId && u.Status == 1
                                    select u).FirstOrDefault();

                        var site = (from s in db.tblCustomer_location
                                    where s.id == item.s1.customer_location
                                    select s).FirstOrDefault();
                        dr["Id"] = item.s1.Id;
                        foreach (var i in customer)
                        {
                            dr["CustomerNo"] = i.CustomerNo;
                            dr["CustomerName"] = i.CustomerName;
                        }
                        foreach (var i in product)
                        {
                            dr["ProductName"] = i.ProductName;
                        }

                        if (customerid != "")
                        {
                            dr["CustomerId"] = customer_name.CustomerName;

                        }
                        else
                        {
                            dr["CustomerId"] = "All";
                        }
						dr["Quantity"] = (item.sd.Quantity != null ? ((int)item.sd.Quantity).ToString() : "0") +
					    " " + (unit != null ? unit.UnitNameEng : "Unknown");
						dr["InvoiceNo"] = item.s1.InvoiceNo;
                        dr["Status"] = item.s1.InvoiceStatus;
                        dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                        dr["RevicedFromCustomer"] = float.Parse(item.s1.RevicedFromCustomer.ToString()).ToString("$0.000");
                        dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dr["ColorCode"] = item.sd.color_code;
                        if (item.s1.customer_location != null)
                        {
                            dr["customer_location"] = site == null ? string.Empty : site.location;
                        }
                        if (item.sd.actual_price != null)
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                        }
                        else
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                        }
                        dr["ProductId"] = item.sd.ProductId;
                        dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                        if (item.s1.UpdatedDate != null)
                        {
                            dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                        }
                        else
                        {
                            dr["PayDate"] = "";
                        }

                        //var getSomeAmount = (from ps in db.tblSales
                        //                     where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                        //                     select ps).ToList();
                        //if (customerid != "")
                        //{
                        //    if (customer_name.is_old_data == true)
                        //    {
                        //        getSomeAmount = (from ps in db.tblSales
                        //                         join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                        //                         where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                        //                         select ps).ToList();
                        //    }
                        //    else
                        //    {
                        //        getSomeAmount = (from ps in db.tblSales
                        //                         where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                        //                         select ps).ToList();
                        //    }
                        //}
                        //else
                        //{
                        //    var cus_old_data = (from cd in db.tblCustomers
                        //                        where cd.Id == item.s1.CustomerId && cd.is_old_data == true
                        //                        select cd).FirstOrDefault();
                        //    if (cus_old_data != null)
                        //    {
                        //        getSomeAmount = (from ps in db.tblSales
                        //                         join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                        //                         where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (cu.CustomerName == cus_old_data.CustomerName || customerid == "" || customerid == null)
                        //                         select ps).ToList();
                        //    }
                        //    else
                        //    {
                        //        getSomeAmount = (from ps in db.tblSales
                        //                         where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                        //                         select ps).ToList();
                        //    }
                        //}




                        //var resultlistsum = (from i in getSomeAmount
                        //                     select i).ToList();
                        //if (projectlocation != "")
                        //{
                        //    resultlistsum = (from i in getSomeAmount
                        //                     join lo in db.tblCustomer_location on i.customer_location equals lo.id
                        //                     where projectlocation == lo.location
                        //                     select i).ToList();
                        //}
                        //else
                        //{
                        //    resultlistsum = getSomeAmount;
                        //}
                        //var resultlistsum2 = (from i in resultlistsum
                        //                      select i).ToList();
                        //if (productid != "")
                        //{

                        //    resultlistsum2 = (from i in resultlistsum
                        //                      join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                        //                      where productid == pd.ProductId
                        //                      select i).Distinct().ToList();
                        //}
                        //else
                        //{
                        //    resultlistsum2 = resultlistsum;

                        //}



                        decimal sumDP = 0;
                        for (int i = 0; i < resultlistsum2.Count(); i++)
                        {
                            if (resultlistsum2[i].Discount != null)
                            {
                                sumDP += Convert.ToDecimal(resultlistsum2[i].Discount);
                            }
                            dr["TotalDis"] = sumDP;
                        }

                        decimal sumDPO = 0;
                        for (int i = 0; i < resultlistsum2.Count(); i++)
                        {
                            if (resultlistsum2[i].RevicedFromCustomer != null)
                            {
                                sumDPO += Convert.ToDecimal(resultlistsum2[i].RevicedFromCustomer);
                            }
                            dr["TotalDepo"] = sumDPO;
                        }
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;
                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "SaleDetail_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        
        public ActionResult ExportToPDFSaleDetailCopy(string datefrom, string dateto, string customerid ,string projectlocation, string productid)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                // List<string>  = new List<string>();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNo"));
                dt.Columns.Add(new DataColumn("CustomerName"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("customer_location"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Refer"));
                dt.Columns.Add(new DataColumn("ColorCode"));
                dt.Columns.Add(new DataColumn("Price"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("TotalAmt"));
                dt.Columns.Add(new DataColumn("TotalDepo"));
                dt.Columns.Add(new DataColumn("TotalDis"));
                dt.Columns.Add(new DataColumn("OrderDate"));
                dt.Columns.Add(new DataColumn("PayDate"));

                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);
                var treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                var customer_name = db.tblCustomers.Where(x => x.Id == customerid).FirstOrDefault();

                if (customerid != "")
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }

                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }


                var resultlist = (from i in treatmentSum.ToList()
                                  select i
                              ).ToList();
                if (projectlocation != "")
                {
                    resultlist = (from i in treatmentSum.ToList()
                                  join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                                  where projectlocation == lo.location
                                  select i
                                  ).ToList();
                }
                else
                {
                    resultlist = treatmentSum;
                }

                var resultlist2 = (from i in resultlist.ToList()
                                   select i
                                  ).ToList();

                if (productid != "")
                {
                    resultlist2 = (from i in resultlist.ToList()
                                   join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                                   where productid == pro.Id
                                   select i
                                  ).ToList();
                }
                else
                {
                    resultlist2 = resultlist;
                }

                if (resultlist2.Any())
                {
                    foreach (var item in resultlist2)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.s1.CustomerId
                                        select p).ToList();
                        var product = (from p in db.tblProducts
                                       where p.Id == item.sd.ProductId 
                                       select p).ToList();
                        var unit = (from u in db.tblUnits
                                    where u.Id == item.sd.UnitTypeId && u.Status == 1
                                    select u).FirstOrDefault();

                        var site = (from s in db.tblCustomer_location
                                    where s.id == item.s1.customer_location
                                    select s).FirstOrDefault();
                        dr["Id"] = item.s1.Id;
                        foreach (var i in customer)
                        {
                            dr["CustomerNo"] = i.CustomerNo;
                            dr["CustomerName"] = i.CustomerName;
                        }
                        foreach (var i in product)
                        {
                            dr["ProductName"] = i.ProductName;
                        }

                        if (customerid != "")
                        {
                            dr["CustomerId"] = customer_name.CustomerName;

                        }
                        else
                        {
                            dr["CustomerId"] = "All";
                        }

                        dr["InvoiceNo"] = item.s1.InvoiceNo;
                        dr["Status"] = item.s1.InvoiceStatus;
                        dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                        dr["RevicedFromCustomer"] = float.Parse(item.s1.RevicedFromCustomer.ToString()).ToString("$0.000");
                        dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dr["ColorCode"] = item.sd.color_code;
                        if (item.s1.customer_location != null)
                        {
                            dr["customer_location"] = site == null ? string.Empty : site.location;
                        }
                        if (item.sd.actual_price != null)
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                        }
                        else
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                        }
                        dr["ProductId"] = item.sd.ProductId;
                        dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                        if (item.s1.UpdatedDate != null)
                        {
                            dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                        }
                        else
                        {
                            dr["PayDate"] = "";
                        }

                        var getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                             select ps).ToList();
                        if (customerid != "")
                        {
                            if (customer_name.is_old_data == true)
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                            else
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                        }
                        else
                        {
                            var cus_old_data = (from cd in db.tblCustomers
                                                where cd.Id == item.s1.CustomerId && cd.is_old_data == true
                                                select cd).FirstOrDefault();
                            if (cus_old_data != null)
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (cu.CustomerName == cus_old_data.CustomerName || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                            else
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                        }




                        var resultlistsum = (from i in getSomeAmount
                                             select i).ToList();
                        if (projectlocation != "")
                        {
                            resultlistsum = (from i in getSomeAmount
                                             join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                             where projectlocation == lo.location
                                             select i).ToList();
                        }
                        else
                        {
                            resultlistsum = getSomeAmount;
                        }
                        var resultlistsum2 = (from i in resultlistsum
                                              select i).ToList();
                        if (productid != "")
                        {

                            resultlistsum2 = (from i in resultlistsum
                                              join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                              where productid == pd.ProductId
                                              select i).Distinct().ToList();
                        }
                        else
                        {
                            resultlistsum2 = resultlistsum;

                        }



                        decimal sumDP = 0;
                        for (int i = 0; i < resultlistsum2.Count(); i++)
                        {
                            if (resultlistsum2[i].Discount != null)
                            {
                                sumDP += Convert.ToDecimal(resultlistsum2[i].Discount);
                            }
                            dr["TotalDis"] = sumDP;
                        }

                        decimal sumDPO = 0;
                        for (int i = 0; i < resultlistsum2.Count(); i++)
                        {
                            if (resultlistsum2[i].RevicedFromCustomer != null)
                            {
                                sumDPO += Convert.ToDecimal(resultlistsum2[i].RevicedFromCustomer);
                            }
                            dr["TotalDepo"] = sumDPO;
                        }
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;
                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "SaleDetail_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        

        public ActionResult ExportToPDFSaleDetailNetIncome(string datefrom, string dateto, string customerid ,string projectlocation, string productid)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                // List<string>  = new List<string>();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNo"));
                dt.Columns.Add(new DataColumn("CustomerName"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("customer_location"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Refer"));
                dt.Columns.Add(new DataColumn("ColorCode"));
                dt.Columns.Add(new DataColumn("Price"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("TotalAmt"));
                dt.Columns.Add(new DataColumn("TotalDepo"));
                dt.Columns.Add(new DataColumn("TotalDis"));
                dt.Columns.Add(new DataColumn("OrderDate"));
                dt.Columns.Add(new DataColumn("PayDate"));
                dt.Columns.Add(new DataColumn("NetIncome"));

                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);
                var treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    join pro in db.tblProducts on sd.ProductId equals pro.Id
                                    join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                    where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && sd.UnitTypeId == probyunit.UnitTypeID
                                    select new
                                    {
                                        s1,
                                        sd,
                                        probyunit,
                                    }).ToList();
         
                var customer_name = db.tblCustomers.Where(x => x.Id == customerid).FirstOrDefault();

                if (customerid != "")
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        join pro in db.tblProducts on sd.ProductId equals pro.Id
                                        join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                        where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                        select new
                                        {
                                            s1,
                                            sd,
                                            probyunit,
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        join pro in db.tblProducts on sd.ProductId equals pro.Id
                                        join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                        where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                        select new
                                        {
                                            s1,
                                            sd,
                                            probyunit,
                                        }).ToList();
                    }

                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    join pro in db.tblProducts on sd.ProductId equals pro.Id
                                    join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                    where s1.Status == 1 && s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null) && sd.UnitTypeId == probyunit.UnitTypeID
                                    select new
                                    {
                                        s1,
                                        sd,
                                        probyunit,
                                    }).ToList();
                }


                var resultlist = (from i in treatmentSum.ToList()
                                  select i
                              ).ToList();
                if (projectlocation != "")
                {
                    resultlist = (from i in treatmentSum.ToList()
                                  join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                                  where projectlocation == lo.location
                                  select i
                                  ).ToList();
                }
                else
                {
                    resultlist = treatmentSum;
                }

                var resultlist2 = (from i in resultlist.ToList()
                                   select i
                                  ).ToList();

                if (productid != "")
                {
                    resultlist2 = (from i in resultlist.ToList()
                                   join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                                   where productid == pro.Id
                                   select i
                                  ).ToList();
                }
                else
                {
                    resultlist2 = resultlist;
                }

                if (resultlist2.Any())
                {
                    foreach (var item in resultlist2)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.s1.CustomerId
                                        select p).ToList();
                        var product = (from p in db.tblProducts
                                       where p.Id == item.sd.ProductId 
                                       select p).ToList();
                        var unit = (from u in db.tblUnits
                                    where u.Id == item.sd.UnitTypeId && u.Status == 1
                                    select u).FirstOrDefault();

                        var site = (from s in db.tblCustomer_location
                                    where s.id == item.s1.customer_location
                                    select s).FirstOrDefault();
                        dr["Id"] = item.s1.Id;
                        foreach (var i in customer)
                        {
                            dr["CustomerNo"] = i.CustomerNo;
                            dr["CustomerName"] = i.CustomerName;
                        }
                        foreach (var i in product)
                        {
                            dr["ProductName"] = i.ProductName;
                        }

                        if (customerid != "")
                        {
                            dr["CustomerId"] = customer_name.CustomerName;

                        }
                        else
                        {
                            dr["CustomerId"] = "All";
                        }

                        dr["InvoiceNo"] = item.s1.InvoiceNo;
                        dr["Status"] = item.s1.InvoiceStatus;
                        dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                        dr["RevicedFromCustomer"] = float.Parse(item.s1.RevicedFromCustomer.ToString()).ToString("$0.000");
                        dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dr["ColorCode"] = item.sd.color_code;
                 
                        if (item.s1.customer_location != null)
                        {
                            dr["customer_location"] = site == null ? string.Empty : site.location;
                        }
                        if (item.sd.actual_price != null)
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                            dr["NetIncome"] = float.Parse(((item.sd.actual_price * item.sd.Quantity) - (item.probyunit.Cost * item.sd.Quantity)).ToString()).ToString("0.000");

                        }
                        else
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                            dr["NetIncome"] = float.Parse(((item.sd.Price * item.sd.Quantity) - (item.probyunit.Cost * item.sd.Quantity)).ToString()).ToString("0.000");

                        }
                        dr["ProductId"] = item.sd.ProductId;
                        dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                        if (item.s1.UpdatedDate != null)
                        {
                            dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                        }
                        else
                        {
                            dr["PayDate"] = "";
                        }

                        var getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                             select ps).ToList();
                        if (customerid != "")
                        {
                            if (customer_name.is_old_data == true)
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                            else
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                        }
                        else
                        {
                            var cus_old_data = (from cd in db.tblCustomers
                                                where cd.Id == item.s1.CustomerId && cd.is_old_data == true
                                                select cd).FirstOrDefault();
                            if (cus_old_data != null)
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (cu.CustomerName == cus_old_data.CustomerName || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                            else
                            {
                                getSomeAmount = (from ps in db.tblSales
                                                 where ps.Status == 1 && ps.UpdatedDate >= datef && ps.UpdatedDate <= ToDates && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                                 select ps).ToList();
                            }
                        }




                        var resultlistsum = (from i in getSomeAmount
                                             select i).ToList();
                        if (projectlocation != "")
                        {
                            resultlistsum = (from i in getSomeAmount
                                             join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                             where projectlocation == lo.location
                                             select i).ToList();
                        }
                        else
                        {
                            resultlistsum = getSomeAmount;
                        }
                        var resultlistsum2 = (from i in resultlistsum
                                              select i).ToList();
                        if (productid != "")
                        {

                            resultlistsum2 = (from i in resultlistsum
                                              join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                              where productid == pd.ProductId
                                              select i).Distinct().ToList();
                        }
                        else
                        {
                            resultlistsum2 = resultlistsum;

                        }



                        decimal sumDP = 0;
                        for (int i = 0; i < resultlistsum2.Count(); i++)
                        {
                            if (resultlistsum2[i].Discount != null)
                            {
                                sumDP += Convert.ToDecimal(resultlistsum2[i].Discount);
                            }
                            dr["TotalDis"] = sumDP;
                        }

                        decimal sumDPO = 0;
                        for (int i = 0; i < resultlistsum2.Count(); i++)
                        {
                            if (resultlistsum2[i].RevicedFromCustomer != null)
                            {
                                sumDPO += Convert.ToDecimal(resultlistsum2[i].RevicedFromCustomer);
                            }
                            dr["TotalDepo"] = sumDPO;
                        }
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetailsNetIncome.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSaleDetailsNetIncome", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;
                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "SaleDetail_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        

        public ActionResult ExportToPDFSaleDetailHistoryByCustomer(string datefrom, string dateto, string customerid)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                // List<string>  = new List<string>();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNo"));
                dt.Columns.Add(new DataColumn("CustomerName"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("customer_location"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Refer"));
                dt.Columns.Add(new DataColumn("ColorCode"));
                dt.Columns.Add(new DataColumn("Price"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("TotalAmt"));
                dt.Columns.Add(new DataColumn("TotalDepo"));
                dt.Columns.Add(new DataColumn("TotalDis"));
                dt.Columns.Add(new DataColumn("OrderDate"));
                dt.Columns.Add(new DataColumn("PayDate"));

                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);
                var treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                var customer_name = db.tblCustomers.Where(x => x.Id == customerid).FirstOrDefault();

                if (customerid != "")
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1  && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }

                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 &&  (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }

                if (treatmentSum.Any())
                {
                    foreach (var item in treatmentSum)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.s1.CustomerId
                                        select p).ToList();
                        var product = (from p in db.tblProducts
                                       where p.Id == item.sd.ProductId 
                                       select p).ToList();
                        var unit = (from u in db.tblUnits
                                    where u.Id == item.sd.UnitTypeId && u.Status == 1
                                    select u).FirstOrDefault();

                        var site = (from s in db.tblCustomer_location
                                    where s.id == item.s1.customer_location
                                    select s).FirstOrDefault();
                        dr["Id"] = item.s1.Id;
                        foreach (var i in customer)
                        {
                            dr["CustomerNo"] = i.CustomerNo;
                            dr["CustomerName"] = i.CustomerName;
                        }
                        foreach (var i in product)
                        {
                            dr["ProductName"] = i.ProductName;
                        }

                        if (customerid != "")
                        {
                            dr["CustomerId"] = customer_name.CustomerName;

                        }
                        else
                        {
                            dr["CustomerId"] = "All";
                        }

                        dr["InvoiceNo"] = item.s1.InvoiceNo;
                        dr["Status"] = item.s1.InvoiceStatus;
                        dr["Discount"] = float.Parse(item.s1.Discount.ToString()).ToString("0.000");
                        dr["RevicedFromCustomer"] = float.Parse(item.s1.RevicedFromCustomer.ToString()).ToString("$0.000");
                        dr["Amount"] = float.Parse(item.s1.Amount.ToString()).ToString("0.000");
                        dr["FromDate"] = "";
                        dr["ToDate"] = "";
                        dr["ColorCode"] = item.sd.color_code;
                        if(item.s1.customer_location != null)
                        {
                            dr["customer_location"] = site.location;
                        }
                        dr["Quantity"] = float.Parse(item.sd.Quantity.ToString()).ToString("0.00") +" "+ unit.UnitNameEng;

                        if(item.sd.actual_price != null)
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.actual_price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.actual_price * item.sd.Quantity).ToString()).ToString("0.000");
                        }
                        else
                        {
                            dr["Price"] = Convert.ToDecimal(item.sd.Price).ToString("0.000");
                            dr["Total"] = float.Parse((item.sd.Price * item.sd.Quantity).ToString()).ToString("0.000");
                        }
                        
                        
                        dr["ProductId"] = item.sd.ProductId;
                        dr["OrderDate"] = Convert.ToDateTime(item.s1.CreatedDate).ToString("dd-MMM-yyy");
                        if (item.s1.UpdatedDate != null)
                        {
                            dr["PayDate"] = Convert.ToDateTime(item.s1.UpdatedDate).ToString("dd-MMM-yyy");
                        }
                        else
                        {
                            dr["PayDate"] = "";
                        }

                        var getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                             select ps).ToList();

                        if (customer_name.is_old_data == true)
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             join cu in db.tblCustomers on ps.CustomerId equals cu.Id
                                             where ps.Status == 1 &&  (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                             select ps).ToList();
                        }
                        else
                        {
                            getSomeAmount = (from ps in db.tblSales
                                             where ps.Status == 1 && (ps.CustomerId == customerid || customerid == "" || customerid == null)
                                             select ps).ToList();
                        }
                        decimal sumDP = 0;
                        for (int i = 0; i < getSomeAmount.Count(); i++)
                        {
                            if (getSomeAmount[i].Discount != null)
                            {
                                sumDP += Convert.ToDecimal(getSomeAmount[i].Discount);
                            }
                            dr["TotalDis"] = sumDP;
                        }

                        decimal sumDPO = 0;
                        for (int i = 0; i < getSomeAmount.Count(); i++)
                        {
                            if (getSomeAmount[i].RevicedFromCustomer != null)
                            {
                                sumDPO += Convert.ToDecimal(getSomeAmount[i].RevicedFromCustomer);
                            }
                            dr["TotalDepo"] = sumDPO;
                        }
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;
                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "SaleDetail_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        public ActionResult ExportTOPDFSaleNetIncome(string datefrom, string dateto, string customerid)
        {

            try
            {
                jotunDBEntities db = new jotunDBEntities();
                List<tblSale> treatmentSum = new List<tblSale>();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNameFilter"));
                dt.Columns.Add(new DataColumn("Owe"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                dt.Columns.Add(new DataColumn("NetIncome"));
                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);

                var customer_name = db.tblCustomers.Where(x => x.Id == customerid).FirstOrDefault();
                if (customerid != "")
                {

                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        where s1.Status == 1 && (s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates) && (cu.CustomerName == customer_name.CustomerName || customerid == "" || customerid == null)
                                        select s1).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        where s1.Status == 1 && (s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates) && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                        select s1).ToList();
                    }
                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    where s1.Status == 1 && (s1.UpdatedDate >= datef && s1.UpdatedDate <= ToDates) && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                    select s1).ToList();
                }
                foreach (var item in treatmentSum)
                {
                    if (item.Status == 1)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.CustomerId
                                        select p).ToList();
                        var saledetail = (from sd in db.tblSalesDetails
                                          join pro in db.tblProducts on sd.ProductId equals pro.Id
                                          join probyunit in db.tblProductByUnits on pro.Id equals probyunit.ProductID
                                          where sd.SaleId == item.Id && sd.UnitTypeId == probyunit.UnitTypeID
                                          select new { probyunit, sd }
                                      ).ToList();
                        decimal? sumNetIncome = 0;
                        for (var i = 0; i < saledetail.Count; i++)
                        {
                            sumNetIncome += saledetail[i].probyunit.Cost * saledetail[i].sd.Quantity;
                            //sumNetIncome += saledetail[i].Price - saledetail[i].Cost;
                        }

                        dr["Id"] = item.Id;
                        foreach (var i in customer)
                        {
                            dr["CustomerId"] = i.CustomerName;

                        }
                        dr["Owe"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                        dr["RevicedFromCustomer"] = float.Parse(item.RevicedFromCustomer.ToString()).ToString("0.000");
                        // dr["CreatedDate"] = item.CreatedDate;
                        if (item.CustomerId != null)
                        {
                            dr["CustomerNameFilter"] = customer_name?.CustomerName ?? "All";
                        }
                        else
                        {
                            dr["CustomerNameFilter"] = "All";
                        }
                        dr["InvoiceNo"] = item.InvoiceNo;
                        dr["Status"] = item.InvoiceStatus;
                        dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                        dr["Amount"] = float.Parse(item.Amount.ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dr["NetIncome"] = float.Parse((Convert.ToDecimal(item.Amount) - sumNetIncome).ToString()).ToString("0.000"); ;
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleNetIncome.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSaleNetIncome", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;

                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "SaleNetIncome_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }


        public ActionResult ExportToPDFItemsInStock()
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));

                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("CategoryId"));
                dt.Columns.Add(new DataColumn("QuantityInStock"));
                dt.Columns.Add(new DataColumn("Total"));

                var product = (from item in db.tblProducts
                               orderby item.QuantityInStock descending
                               where item.QuantityInStock <= item.QuantityInStock && item.Status == 1
                               select item).ToList();

                foreach (var item in product)
                {
                    var category = (from p1 in db.tblCategories
                                    where p1.Id == item.CategoryId
                                    select p1).FirstOrDefault();

                    var categoryss = category.CategoryNameEng;
                    if (categoryss == null)
                    {
                        categoryss = category.CategoryNameKh;
                    }

                    DataRow dr = dt.NewRow();

                    dr["Id"] = item.Id;

                    dr["ProductName"] = item.ProductName;
                    dr["CategoryId"] = categoryss;
                    dr["QuantityInStock"] = item.QuantityInStock;
                    var getSomeAmount = (from s1 in db.tblProducts
                                         where s1.Status == 1
                                         select s1).ToList();
                    decimal sumDP = 0;
                    for (int i = 0; i < getSomeAmount.Count(); i++)
                    {
                        if (getSomeAmount[i].QuantityInStock != null)
                        {
                            sumDP += Convert.ToDecimal(getSomeAmount[i].QuantityInStock);
                        }
                        dr["Total"] = sumDP;
                    }

                    dt.Rows.Add(dr);

                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\ItemsInStockReport.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSItemsInStock", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;

                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "ItemsInStock_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        public ActionResult ExportToPDFItemsOutOfStock()
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("id"));
                dt.Columns.Add(new DataColumn("item_id"));
                dt.Columns.Add(new DataColumn("qty_in_stock"));
                dt.Columns.Add(new DataColumn("qty_alert"));
                dt.Columns.Add(new DataColumn("supply_by"));
                dt.Columns.Add(new DataColumn("phone"));
                dt.Columns.Add(new DataColumn("category_id"));
                var product = (from item in db.tblProducts
                               orderby item.QuantityInStock descending
                               where item.QuantityInStock <= item.quantity_alert && item.Status == 1
                               select item).ToList();
                foreach (var result in product)
                {
                    var category = (from p1 in db.tblCategories
                                    where p1.Id == result.CategoryId
                                    select p1).FirstOrDefault();

                    var categoryss = category.CategoryNameEng;
                    if (categoryss == null)
                    {
                        categoryss = category.CategoryNameKh;
                    }
                    DataRow dr = dt.NewRow();
                    dr["id"] = result.Id;
                    dr["item_id"] = result.ProductName;
                    dr["qty_in_stock"] = result.QuantityInStock;
                    dr["qty_alert"] = result.quantity_alert;
                    dr["category_id"] = categoryss;
                    dt.Rows.Add(dr);
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\ItemNearOutOfStock.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSItemNearOutOfStock", dt);
                rv.LocalReport.DataSources.Add(rds);
                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;

                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "ItemsInStock_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }
        public ActionResult ExportToPDFRemove(string id)
        {
            ViewBag.deleteSuccess = "false";
            var file2delete = id;
            var fullpathoffile = Server.MapPath("~/" + file2delete);

            if (System.IO.File.Exists(fullpathoffile))
            {
                System.IO.File.Delete(fullpathoffile);
                ViewBag.deleteSuccess = "true";
            }
            return RedirectToAction("Index");
        }
        public ActionResult ExportTOPDFOwe(string datefrom, string dateto, string customerid)
        {

            try
            {
                jotunDBEntities db = new jotunDBEntities();
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("id"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("OweBy"));
                dt.Columns.Add(new DataColumn("OweDayTotal"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Deposit"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("OwnAmount"));
                dt.Columns.Add(new DataColumn("OrderDate"));

                DateTime datet = Convert.ToDateTime(dateto);
                DateTime datef = Convert.ToDateTime(datefrom);
                DateTime ToDates = datet.Date.AddHours(23.59);



                var treatmentSum = (from s1 in db.tblSales
                                    where s1.Status == 1 && (s1.Amount -s1.Discount - s1.RevicedFromCustomer) > 0 && s1.CreatedDate >= datef && s1.CreatedDate <= ToDates && (s1.CustomerId == customerid || customerid == "" || customerid == null)
                                    select s1).ToList();



                foreach (var item in treatmentSum)
                {
                    if (item.Status == 1)
                    {
                        DataRow dr = dt.NewRow();
                        var customer = (from p in db.tblCustomers
                                        where p.Id == item.CustomerId
                                        select p).ToList();


                        DateTime datenow = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyy")).AddDays(0);
                        DateTime datec = Convert.ToDateTime(Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy")).AddDays(0);
                        var datetotal = (datenow - datec).TotalDays;

                        if (customerid == "" || customerid == null)
                        {
                            dr["CustomerId"] = "All";

                        }
                        else
                        {
                            var cus = (from su in db.tblCustomers
                                       where su.Id == customerid
                                       select su).FirstOrDefault();
                            dr["CustomerId"] = cus.CustomerName;

                        }

                        dr["id"] = item.Id;
                        foreach (var i in customer)
                        {
                            dr["OweBy"] = i.CustomerName;
                        }
                        //  dr["OweBy"] = float.Parse(item.Owe.ToString()).ToString("0.000");
                        dr["OweDayTotal"] = datetotal + " day(s)";
                        dr["OrderDate"] = Convert.ToDateTime(item.CreatedDate).ToString("dd-MMM-yyy");
                        dr["InvoiceNo"] = item.InvoiceNo;
                        dr["Status"] = item.InvoiceStatus;
                        dr["Deposit"] = float.Parse(item.RevicedFromCustomer.ToString()).ToString("0.000");
                        dr["Discount"] = float.Parse(item.Discount.ToString()).ToString("0.000");
                        dr["Amount"] = float.Parse(item.Amount.ToString()).ToString("0.000");
                        dr["OwnAmount"] = float.Parse((item.Amount -item.Discount - item.RevicedFromCustomer).ToString()).ToString("0.000");
                        dr["FromDate"] = datef.ToString("dd-MMM-yyy");
                        dr["ToDate"] = datet.ToString("dd-MMM-yyy");
                        dt.Rows.Add(dr);
                    }
                }
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Owe.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSOwes", dt);
                rv.LocalReport.DataSources.Add(rds);


                rv.ProcessingMode = ProcessingMode.Local;
                rv.ShowPrintButton = true;
                ViewBag.ReportViewer = rv;
                Warning[] warnings;
                string[] streamids;

                string mimeType, encoding, filenameExtension;

                byte[] bytes = rv.LocalReport.Render("Pdf", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                //File  
                string FileName = "Sale_" + DateTime.Now.Ticks.ToString() + ".pdf";
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
                return View("Error", new HandleErrorInfo(Ex, "Index", "Home"));

            }
        }

        public ActionResult SaleCash()
        {
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleDetails.rdlc";
            rv.LocalReport.DataSources.Clear();
            DataTable dt = new DataTable();
            ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            DateTime endDatetime = DateTime.Now;
            return View();
        }
        [HttpPost]
        public ActionResult SaleCash(ReportSaleCashFilterModel model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                DateTime dateFrom = Convert.ToDateTime(model.dateFrom);
                DateTime dateTo = Convert.ToDateTime(model.dateTo);
                DateTime filterDateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0);
                DateTime filterDateTo = dateTo.AddDays(1).AddMilliseconds(-1);

                //var salePayments = db.tbSalePayments.Where(s => s.created_at >= filterDateFrom && s.created_at <= filterDateTo).ToList();

                //jotunDBEntities db = new jotunDBEntities();
                // List<string>  = new List<string>();

                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("FromDate"));
                dt.Columns.Add(new DataColumn("ToDate"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("CustomerNo"));
                dt.Columns.Add(new DataColumn("CustomerName"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("customer_location"));
                dt.Columns.Add(new DataColumn("RevicedFromCustomer"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("ProductName"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Refer"));
                dt.Columns.Add(new DataColumn("ColorCode"));
                dt.Columns.Add(new DataColumn("Price"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("TotalAmt"));
                dt.Columns.Add(new DataColumn("TotalDepo"));
                dt.Columns.Add(new DataColumn("TotalDis"));
                dt.Columns.Add(new DataColumn("OrderDate"));
                dt.Columns.Add(new DataColumn("PayDate"));
                dt.Columns.Add(new DataColumn("CostGoodSold"));
                dt.Columns.Add(new DataColumn("NetIncome"));
                dt.Columns.Add(new DataColumn("TotalCost"));
                dt.Columns.Add(new DataColumn("TotalCostGoodSold"));
                dt.Columns.Add(new DataColumn("totalDepositCoGs"));
                dt.Columns.Add(new DataColumn("TotalSaleCoGs"));
                dt.Columns.Add(new DataColumn("TotalProfit"));
                dt.Columns.Add(new DataColumn("IsAdmin"));

                //DateTime ToDates = sm.ToDate.AddDays(1).AddMilliseconds(-1);
                //var treatmentSum = (from s1 in db.tblSales
                //                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                //                    where s1.Status == 1 && s1.UpdatedDate >= filterDateFrom && s1.UpdatedDate <= filterDateTo
                //                    select new
                //                    {
                //                        s1,
                //                        sd
                //                    }).ToList();

                var sales= (from s1 in db.tblSales
                            where s1.Status == 1 && s1.UpdatedDate >= filterDateFrom && s1.UpdatedDate <= filterDateTo
                            select s1).ToList();
                /*
                var customer_name = db.tblCustomers.Where(x => x.Id == sm.CustomerId).FirstOrDefault();

                if (sm.CustomerId != null)
                {
                    if (customer_name.is_old_data == true)
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join cu in db.tblCustomers on s1.CustomerId equals cu.Id
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (cu.CustomerName == customer_name.CustomerName || sm.CustomerId == "" || sm.CustomerId == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }
                    else
                    {
                        treatmentSum = (from s1 in db.tblSales
                                        join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                        where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                        select new
                                        {
                                            s1,
                                            sd
                                        }).ToList();
                    }

                }
                else
                {
                    treatmentSum = (from s1 in db.tblSales
                                    join sd in db.tblSalesDetails on s1.Id equals sd.SaleId
                                    where s1.Status == 1 && s1.UpdatedDate >= sm.FromDate && s1.UpdatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                    select new
                                    {
                                        s1,
                                        sd
                                    }).ToList();
                }

                var resultlist = (from i in treatmentSum.ToList()
                                  select i
                                  ).ToList();
                if (sm.filter_project_location_name != null)
                {
                    resultlist = (from i in treatmentSum.ToList()
                                  join lo in db.tblCustomer_location on i.s1.customer_location equals lo.id
                                  where sm.filter_project_location_name == lo.location
                                  select i
                                  ).ToList();
                }
                else
                {
                    resultlist = treatmentSum;
                }

                var resultlist2 = (from i in resultlist.ToList()
                                   select i
                                  ).ToList();

                if (sm.filter_product_id != null)
                {
                    resultlist2 = (from i in resultlist.ToList()
                                   join pro in db.tblProducts on i.sd.ProductId equals pro.Id
                                   where sm.filter_product_id == pro.Id
                                   select i
                                  ).ToList();
                }
                else
                {
                    resultlist2 = resultlist;
                }
                DateTime startDateTime = DateTime.Now;
                var getSomeAmount = (from ps in db.tblSales
                                     where ps.Status == 1 && ps.UpdatedDate >= sm.FromDate && ps.UpdatedDate <= ToDates && (ps.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
                                     select ps).ToList();


                var resultlistsum = (from i in getSomeAmount
                                     select i).ToList();
                if (sm.filter_project_location_name != null)
                {
                    resultlistsum = (from i in getSomeAmount
                                     join lo in db.tblCustomer_location on i.customer_location equals lo.id
                                     where sm.filter_project_location_name == lo.location
                                     select i).ToList();
                }
                else
                {
                    resultlistsum = getSomeAmount;
                }
                var resultlistsum2 = (from i in resultlistsum
                                      select i).ToList();
                if (sm.filter_product_id != null)
                {

                    resultlistsum2 = (from i in resultlistsum
                                      join pd in db.tblSalesDetails on i.Id equals pd.SaleId
                                      where sm.filter_product_id == pd.ProductId
                                      select i).Distinct().ToList();
                }
                else
                {
                    resultlistsum2 = resultlistsum;

                }
                */
                var totalDepositAmountCash = sales.Sum(s => s.RevicedFromCustomer);
                var totalDepositAmountABA = sales.Sum(s => s.ReceivedByABA);
                //var totalDeposit = Convert.ToDecimal(totalDepositAmountCash) + Convert.ToDecimal(totalDepositAmountABA);
                var totalDeposit = sales.Sum(s => s.SaleDeposit);
                var totalDiscount = sales.Sum(s => s.Discount);
                var totalSaleAmount = sales.Sum(s => s.Amount);
                var totalSaleCostGoodSold = sales.Sum(s=>s.SaleCost);
                var  totalProfit = sales.Sum(s=>s.Profit);

                decimal totalDepositCoGs = 0;

                foreach (var sl in sales)
                {                
                    var customer = (from p in db.tblCustomers
                                    where p.Id == sl.CustomerId
                                    select p).FirstOrDefault();

                    var site = (from s in db.tblCustomer_location
                                where s.id == sl.customer_location
                                select s).FirstOrDefault();

                    

              
                    sl.ReceivedByABA = sl.ReceivedByABA ?? 0;
                    sl.RevicedFromCustomer = sl.RevicedFromCustomer ?? 0;
                    sl.SaleCost = sl.SaleCost ?? 0;
                    sl.Profit = sl.Profit ?? 0;

                    decimal saleDeposit =Convert.ToDecimal(sl.ReceivedByABA) + Convert.ToDecimal(sl.RevicedFromCustomer);
                    decimal saleAmount = Convert.ToDecimal(sl.Amount) - Convert.ToDecimal(sl.Discount);

                    //decimal totalCostGoodSold = 0;
                    //decimal percentageCashRecived = saleDeposit > 0 ? (saleDeposit /saleAmount) * 100 : 0;

                    var treatmentSum = (from sd in db.tblSalesDetails
                                        where string.Compare(sd.SaleId, sl.Id) == 0
                                        select sd).ToList();

                    if (treatmentSum.Any())
                    {
                        //decimal totalReceivedCostSale = 0;
                        ////decimal receivedFromCustomer = Convert.ToDecimal(sl.RevicedFromCustomer + (double)sl.ReceivedByABA);

                        //foreach (var item in treatmentSum)
                        //{
                        //    decimal productCostGoodSold = 0;
                        //    var costGoodSold = db.tblProductByUnits.Where(s => string.Compare(s.ProductID, item.ProductId) == 0 && string.Compare(s.UnitTypeID, item.UnitTypeId) == 0).FirstOrDefault();
                        //    if (costGoodSold != null)
                        //    {
                        //        totalCostGoodSold = totalCostGoodSold + Convert.ToDecimal(costGoodSold.Cost * item.Quantity);
                        //        if (saleDeposit > 0)
                        //        {
                        //            totalDepositCoGs = totalDepositCoGs + totalCostGoodSold;
                        //        }
                        //        productCostGoodSold = Convert.ToDecimal(costGoodSold.Cost);
                        //    }

                        //    var receivedAmountByItem = ((item.actual_price * item.Quantity) * percentageCashRecived) / 100;
                        //    var receivedSoldQtyByItem = item.actual_price > 0 ? receivedAmountByItem / item.actual_price : 0;
                        //    var receivedCostPerItem = receivedSoldQtyByItem * productCostGoodSold;
                        //    totalReceivedCostSale = totalReceivedCostSale + Convert.ToDecimal(receivedCostPerItem);

                        //}

                        //decimal profit = saleDeposit - totalReceivedCostSale;

                        //totalProfit = totalProfit + profit;
                        //totalSaleCostGoodSold = totalSaleCostGoodSold + totalReceivedCostSale;

                        foreach (var item in treatmentSum)
                        {
                            DataRow dr = dt.NewRow();
                            
                            var product = (from p in db.tblProducts
                                           where p.Id == item.ProductId
                                           select p).FirstOrDefault();

                            var unit = (from u in db.tblUnits
                                        where u.Id == item.UnitTypeId && u.Status == 1
                                        select u).FirstOrDefault();
                       

                            //var costGoodSold = db.tblProductByUnits.Where(s => string.Compare(s.ProductID, item.ProductId) == 0 && string.Compare(s.UnitTypeID, item.UnitTypeId) == 0).FirstOrDefault();
                            

                            dr["Id"] = sl.Id;
                            if (customer != null)
                            {
                                dr["CustomerNo"] = customer.CustomerNo;
                                dr["CustomerName"] = customer.CustomerName;
                            }
                            if (product != null)
                            {
                                dr["ProductName"] = product.ProductName;
                            }

                            dr["CustomerId"] = "All";
                            dr["InvoiceNo"] = sl.InvoiceNo;
                            dr["Status"] = sl.InvoiceStatus;
                            dr["Discount"] = Convert.ToDecimal(sl.Discount); //float.Parse(sl.Discount.ToString()).ToString("0.000");
                            dr["RevicedFromCustomer"] = Convert.ToDecimal(saleDeposit);//.ToString("$0.000");
                            dr["Amount"] = Convert.ToDecimal(sl.Amount);  //float.Parse(sl.Amount.ToString()).ToString("0.000");
                           
                            dr["FromDate"] = dateFrom.ToString("dd-MMM-yyy");
                            dr["ToDate"] = dateTo.ToString("dd-MMM-yyy");
                            dr["ColorCode"] = item.color_code;
                            if (sl.customer_location != null)
                            {
                                dr["customer_location"] = site == null ? string.Empty : site.location;
                            }
                            dr["Quantity"] = string.Format("{0} {1}", item.Quantity, unit.UnitNameEng); //float.Parse(item.Quantity.ToString()).ToString("0.00") + " " + unit.UnitNameEng;
                            if (item.actual_price != null)
                            {
                                dr["Price"] = Convert.ToDecimal(item.actual_price).ToString("0.000");
                                dr["Total"] = float.Parse((item.actual_price * item.Quantity).ToString()).ToString("0.000");
                            }
                            else
                            {
                                dr["Price"] = Convert.ToDecimal(item.Price).ToString("0.000");
                                dr["Total"] = float.Parse((item.Price * item.Quantity).ToString()).ToString("0.000");
                            }
                            dr["ProductId"] = item.ProductId;
                            dr["OrderDate"] = Convert.ToDateTime(sl.CreatedDate).ToString("dd-MMM-yyy");
                            if (sl.UpdatedDate != null)
                            {
                                dr["PayDate"] = Convert.ToDateTime(sl.UpdatedDate).ToString("dd-MMM-yyy");
                            }
                            else
                            {
                                dr["PayDate"] = "";
                            }

                            dr["TotalDis"] = Convert.ToDecimal(totalDiscount);
                            dr["TotalDepo"] = Convert.ToDecimal(totalDeposit);
                            //if (costGoodSold != null)
                            //{

                            //    dr["CostGoodSold"] = float.Parse(costGoodSold.Cost.ToString()).ToString("0.00");
                            //    dr["TotalCost"]= float.Parse((costGoodSold.Cost * item.Quantity).ToString()).ToString("0.00");
                            //    //dr["TotalCostGoodSold"] =float.Parse(totalCostGoodSold.ToString()).ToString("0.00");
                            //    dr["TotalCostGoodSold"] = float.Parse(totalReceivedCostSale.ToString()).ToString("0.00"); 
                            //    //dr["NetIncome"] = receivedFromCustomer > 0 ? receivedFromCustomer - totalCostGoodSold : 0;
                            //    dr["NetIncome"] = profit;
                            //}
                            dr["CostGoodSold"] = Convert.ToDecimal(item.cost); //float.Parse(item.cost.ToString()).ToString("0.00");
                            dr["TotalCost"] = Convert.ToDecimal(item.cost * item.Quantity);// float.Parse((item.cost * item.Quantity).ToString()).ToString("0.00");
                            //dr["TotalCostGoodSold"] =float.Parse(totalCostGoodSold.ToString()).ToString("0.00");
                            dr["TotalCostGoodSold"] = Convert.ToDecimal(sl.SaleCost); //float.Parse(sl.SaleCost.ToString()).ToString("0.00");
                            //dr["NetIncome"] = receivedFromCustomer > 0 ? receivedFromCustomer - totalCostGoodSold : 0;
                            dr["NetIncome"] =Convert.ToDecimal(sl.Profit);
                            dr["totalDepositCoGs"] = Convert.ToDecimal(totalDepositCoGs);
                            dr["TotalAmt"] = Convert.ToDecimal(totalSaleAmount);
                            dr["TotalProfit"] = Convert.ToDecimal(totalProfit);
                            dr["TotalSaleCoGs"] = Convert.ToDecimal(totalSaleCostGoodSold);
                            dr["IsAdmin"] = User.IsInRole("SuperAdmin")==true?1:User.IsInRole("Administrator") ==true?1:0;

                            dt.Rows.Add(dr);
                        }
                    }
                }
                if (User.IsInRole("Sales"))
                {
                    rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleCashForSale.rdlc";
                }
                else
                {
                    rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleCash.rdlc";
                }
                
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DSSaleDetails", dt);
                rv.LocalReport.DataSources.Add(rds);
                ViewBag.ReportViewer = rv;
                DateTime endDatetime = DateTime.Now;
                //var start = (endDatetime - startDateTime).TotalSeconds;

            }
            catch(Exception ex)
            {

            }
            return View();
        }
        
        public ActionResult SaleSummary()
        {
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleSummary.rdlc";
            rv.LocalReport.DataSources.Clear();
            DataTable dt = new DataTable();
            ReportDataSource rds = new ReportDataSource("DataSet1", dt);
            rv.LocalReport.DataSources.Add(rds);
            ViewBag.ReportViewer = rv;
            DateTime endDatetime = DateTime.Now;
            return View();
        }
        [HttpPost]
        public ActionResult SaleSummary(ReportSaleCashFilterModel model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                DateTime dateFrom = Convert.ToDateTime(model.dateFrom);
                DateTime dateTo = Convert.ToDateTime(model.dateTo);
                DateTime filterDateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0);
                DateTime filterDateTo = dateTo.AddDays(1).AddMilliseconds(-1);

                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("SaleDate"));
                dt.Columns.Add(new DataColumn("Customer"));
                dt.Columns.Add(new DataColumn("CusomerLocation"));
                dt.Columns.Add(new DataColumn("Amount"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("DepositAmount"));
                dt.Columns.Add(new DataColumn("OutstandingAmount"));

                var treatmentSum = (from s1 in db.tblSales
                                    orderby s1.UpdatedDate
                                    where s1.Status == 1 && s1.CreatedDate >= filterDateFrom && s1.CreatedDate <= filterDateTo
                                    select s1).ToList();

                foreach(var s in treatmentSum)
                {
                    var customer = (from p in db.tblCustomers
                                    where p.Id == s.CustomerId
                                    select p).FirstOrDefault();

                    var site = (from cl in db.tblCustomer_location
                                where cl.id == s.customer_location
                                select cl).FirstOrDefault();
                    var totalReceivedFromCustomer =Convert.ToDouble(s.ReceivedByABA)+Convert.ToDouble(s.RevicedFromCustomer);
                    DataRow dr = dt.NewRow();
                    dr["Id"] = s.Id;
                    dr["SaleDate"] = Convert.ToDateTime(s.UpdatedDate).ToString("dd-MMM-yyyy");
                    dr["Customer"] = customer == null ? string.Empty : customer.CustomerName;
                    dr["CusomerLocation"] = site == null ? string.Empty : site.location;
                    dr["Amount"]= float.Parse(s.Amount.ToString()).ToString("0.000");
                    dr["Discount"] = float.Parse(s.Discount.ToString()).ToString("0.000");
                    dr["DepositAmount"] = float.Parse(totalReceivedFromCustomer.ToString()).ToString("0.000");
                    dr["OutstandingAmount"] = float.Parse(s.Owe.ToString()).ToString("0.000");
                    dt.Rows.Add(dr);
                }

                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleSummary.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("DataSet1", dt);
                rv.LocalReport.DataSources.Add(rds);
                ViewBag.ReportViewer = rv;
            }
            catch(Exception ex)
            {

            }
            return View();
        }
		public ActionResult ExportToPDFSaleSummary(string datefrom, string dateto)
		{
			try
			{
				// Initialize the database context
				jotunDBEntities db = new jotunDBEntities();

				// Parse the input date parameters
				DateTime dateFrom = Convert.ToDateTime(datefrom);  
				DateTime dateTo = Convert.ToDateTime(dateto);    			
				DateTime filterDateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0);
				DateTime filterDateTo = dateTo.AddDays(1).AddMilliseconds(-1);

				// Initialize ReportViewer and configure it
				ReportViewer rv = new ReportViewer();
				rv.ProcessingMode = ProcessingMode.Local;
				rv.SizeToReportContent = true;
				rv.Width = Unit.Percentage(100);
				rv.Height = Unit.Percentage(100);

				// Create a DataTable for report data
				DataTable dt = new DataTable();
				dt.Columns.Add(new DataColumn("Id"));
				dt.Columns.Add(new DataColumn("SaleDate"));
				dt.Columns.Add(new DataColumn("Customer"));
				dt.Columns.Add(new DataColumn("CustomerLocation"));
				dt.Columns.Add(new DataColumn("Amount"));
				dt.Columns.Add(new DataColumn("Discount"));
				dt.Columns.Add(new DataColumn("DepositAmount"));
				dt.Columns.Add(new DataColumn("OutstandingAmount"));

				// Fetch the sales data based on the date range filter
				var treatmentSum = (from s1 in db.tblSales
									where s1.Status == 1 && s1.CreatedDate >= filterDateFrom && s1.CreatedDate <= filterDateTo
									select s1).ToList();

				// Populate the DataTable with the retrieved data
				foreach (var s in treatmentSum)
				{
					var customer = db.tblCustomers.FirstOrDefault(p => p.Id == s.CustomerId);
					var site = db.tblCustomer_location.FirstOrDefault(cl => cl.id == s.customer_location);
					var totalReceivedFromCustomer = Convert.ToDouble(s.ReceivedByABA) + Convert.ToDouble(s.RevicedFromCustomer);

					DataRow dr = dt.NewRow();
					dr["Id"] = s.Id;
					dr["SaleDate"] = Convert.ToDateTime(s.UpdatedDate).ToString("dd-MMM-yyyy");
					dr["Customer"] = customer?.CustomerName ?? string.Empty;
					dr["CustomerLocation"] = site?.location ?? string.Empty;
					dr["Amount"] = ((double)s.Amount).ToString("0.000");
					dr["Discount"] = ((double)s.Discount).ToString("0.000");
					dr["DepositAmount"] = totalReceivedFromCustomer.ToString("0.000");
					dr["OutstandingAmount"] = ((double)s.Owe).ToString("0.000");
					dt.Rows.Add(dr);
				}

				// Configure the report viewer data source
				rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\SaleSummary.rdlc";
				rv.LocalReport.DataSources.Clear();
				ReportDataSource rds = new ReportDataSource("DataSet1", dt);
				rv.LocalReport.DataSources.Add(rds);

				// Render the report to a byte array (PDF format)
				byte[] renderedBytes = rv.LocalReport.Render("Pdf");

				// Ensure the pdf directory exists
				string pdfDirectory = Server.MapPath("~/pdf/");
				if (!Directory.Exists(pdfDirectory))
				{
					Directory.CreateDirectory(pdfDirectory);
				}

				// Generate file path for saving the PDF
				string fileName = "SaleSummary_" + DateTime.Now.Ticks.ToString() + ".pdf";
				string filePath = Path.Combine(pdfDirectory, fileName);

				// Write the rendered PDF to disk
				System.IO.File.WriteAllBytes(filePath, renderedBytes);

				// Set up PdfReader and PdfStamper for triggering print functionality
				using (PdfReader reader = new PdfReader(renderedBytes))
				{
					using (FileStream output = new FileStream(filePath, FileMode.Create))
					{
						string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
						PdfStamper pdfStamper = new PdfStamper(reader, output, '0', true);

						// JavaScript for triggering print, based on browser type
						if (userAgent.Contains("Firefox"))
						{
							pdfStamper.JavaScript = "var pp = this.getPrintParams(); pp.interactive = pp.constants.interactionLevel.full; this.print(pp);";
						}
						else
						{
							pdfStamper.JavaScript = "setTimeout(function() {var pp = this.getPrintParams(); pp.interactive = pp.constants.interactionLevel.full; this.print(pp);}, 200);";
						}

						pdfStamper.FormFlattening = false;
						pdfStamper.Close();
					}
				}

				// Return the path to the generated PDF file for further use
				string returnFilePath = "/pdf/" + fileName;
				return Content(returnFilePath); // This can be used in your front-end for triggering the print
			}
			catch (Exception ex)
			{
				// Handle any errors and return error view
				return View("Error", new HandleErrorInfo(ex, "Report", "ExportToPDFSaleSummary"));
			}
		}
	}
}
