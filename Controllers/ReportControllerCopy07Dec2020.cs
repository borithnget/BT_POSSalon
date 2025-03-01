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

            if (sm.CustomerId != null)
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



            var treatmentSum = (from s1 in db.tblSales
                                where s1.Status == 1 && (s1.Amount -s1.Discount - s1.RevicedFromCustomer) > 0 && s1.CreatedDate >= sm.FromDate && s1.CreatedDate <= ToDates && (s1.CustomerId == sm.CustomerId || sm.CustomerId == "" || sm.CustomerId == null)
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
                    dr["FromDate"] = sm.FromDate.ToString("dd-MMM-yyy");
                    dr["ToDate"] = sm.ToDate.ToString("dd-MMM-yyy");
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


            ViewBag.ReportViewer = rv;

            return View();

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

    }
}
