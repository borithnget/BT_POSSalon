using iTextSharp.text.pdf;
using jotun.Entities;
using jotun.Functions;
using jotun.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Data.Entity.Core;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using static iTextSharp.text.pdf.AcroFields;
using System.Windows.Media;

namespace jotun.Controllers
{
    public class SaleController : Controller
    {
        jotunDBEntities db = new jotunDBEntities();

        public Boolean isAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "SuperAdmin" || s[0].ToString() == "Administrator" || s[0].ToString() == "Sales")
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
        // GET: Sale
        public ActionResult Index()
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ViewBag.CategoryNamesEng = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                                              .ToList(), "CategoryNameEng", "CategoryNameEng");
                ViewBag.CategoryNamesKh = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                                              .ToList(), "CategoryNameKh", "CategoryNameKh");
                ViewBag.CustomerName = new SelectList(db.tblCustomers.Where(u => !u.Status.ToString().Contains("0") && !u.CustomerName.Contains("Null"))
                                              .ToList(), "CustomerName", "CustomerName");
            };
            if (isAdminUser())
            {

                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult IndexSale()
        {
            return View();
        }

        public JsonResult GetSale(SaleViewModels model)
        {
            string xx = "";
            string yy;
            string zz;
            try
            {
                List<SaleViewModels> models = new List<SaleViewModels>();
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var dateFrom = CommonFunctions.ToLocalTime(model.FromDate);
                    var dt = CommonFunctions.ToLocalTime(model.ToDate).AddDays(1).AddMilliseconds(-1);
                    var filterDateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0);



                    yy = CommonFunctions.ToLocalTime(Convert.ToDateTime(model.FromDate)).ToString("yyyy-MM-dd");
                    zz = CommonFunctions.ToLocalTime(Convert.ToDateTime(model.ToDate)).ToString("yyyy-MM-dd");
                    model.FromDate = CommonFunctions.ToLocalTime(model.FromDate);

                    if (yy != "0001-01-01" && zz != "0001-01-01")
                    {
                        DateTime ToDates = CommonFunctions.ToLocalTime(model.ToDate.AddHours(23.59));

                        var treatmentSum = (from s1 in db.tblSales
                                            orderby s1.UpdatedDate descending
                                            where s1.Status == 1 && s1.UpdatedDate >= filterDateFrom && s1.UpdatedDate <= dt
                                            select s1).ToList();
                        foreach (var b in treatmentSum)
                        {
                            var date = b.UpdatedDate;
                            if (date == null) { date = b.CreatedDate; }
                            else { date = b.UpdatedDate; }
                            var customer = (from p1 in db.tblCustomers
                                            where p1.Id == b.CustomerId
                                            select p1).ToList();
                            b.ReceivedByABA = b.ReceivedByABA ?? 0;

                            models.Add(new SaleViewModels()
                            {
                                Id = b.Id,
                                //CustomerId = x1.CustomerName,
                                CustomerId=b.CustomerId,
                                SaleImage = b.SaleImage,
                                Owe = (b.Amount - (b.Discount + b.RevicedFromCustomer + (double)b.ReceivedByABA) ?? 0).ToString("0.00"),
                                Amount = float.Parse((b.Amount).ToString()).ToString("0.00"),
                                Discount = b.Discount.ToString(),
                                RevicedFromCustomer = float.Parse((b.RevicedFromCustomer + (double)b.ReceivedByABA).ToString()).ToString("0.00"),
                                InvoiceStatus = b.InvoiceStatus,
                                InvoiceNo = b.InvoiceNo,
                                Description = b.Description,
                                CreatedDate = Convert.ToDateTime(b.UpdatedDate.ToString()).ToString("yyyy-MM-dd"),
                                //ProjectLocation = location,
                            });
                        }
                    }
                    else
                    {
                        var sale = (from s in db.tblSales
                                    orderby s.UpdatedDate descending
                                    where s.Status == 1 && s.UpdatedDate >= filterDateFrom && s.UpdatedDate <= dt
                                    select s).ToList();
                        foreach (var item in sale)
                        {
                            string date3 = CommonFunctions.ToLocalTime(Convert.ToDateTime((item.UpdatedDate).ToString())).ToString("yyyy-MM-dd");
                            string date4 = CommonFunctions.ToLocalTime(Convert.ToDateTime((DateTime.Now).ToString())).ToString("yyyy-MM-dd");
                            string date5 = CommonFunctions.ToLocalTime(Convert.ToDateTime(((DateTime.Now).AddDays(-1)).ToString())).ToString("yyyy-MM-dd");
                            var aa = item.Description;
                            if (aa == null)
                            {
                                aa = xx;
                            }
                            if ((item.Status == 1 && date3 == date4) || date3 == date5)
                            {
                                var customer = (from p1 in db.tblCustomers
                                                where p1.Id == item.CustomerId
                                                select p1).ToList();
                                foreach (var i in customer)
                                {
                                    string location = "";
                                    var customer_location_obj = db.tblCustomer_location.Where(l => l.id == item.customer_location).FirstOrDefault();
                                    if (customer_location_obj != null)
                                    {
                                        location = customer_location_obj.location;
                                    }
                                    var date = item.UpdatedDate;
                                    if (date == null) { date = item.CreatedDate; }
                                    else { date = item.UpdatedDate; }
                                    models.Add(new SaleViewModels()
                                    {
                                        Id = item.Id,
                                        CustomerId = i.CustomerName,
                                        SaleImage = item.SaleImage,
                                        //Owe = float.Parse((item.Owe - item.RevicedFromCustomer).ToString()).ToString("0.000"),
                                        Owe = (item.Amount - (((item.Amount * item.Discount) / 100) + (item.RevicedFromCustomer + (double)item.ReceivedByABA))).ToString(),
                                        Amount = float.Parse((item.Amount).ToString()).ToString("0.00"),
                                        Discount = item.Discount.ToString(),
                                        RevicedFromCustomer = float.Parse((item.RevicedFromCustomer).ToString()).ToString("0.00"),
                                        InvoiceStatus = item.InvoiceStatus,
                                        InvoiceNo = item.InvoiceNo,
                                        Description = aa,
                                        CreatedDate = Convert.ToDateTime(item.UpdatedDate.ToString()).ToString("yyyy-MM-dd"),
                                        ProjectLocation = location,
                                    });
                                }
                            }
                        }
                    }
                }
                if (models != null && models.Count > 0)
                {
                    var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                var jsonResult2 = Json(new { data = "" }, JsonRequestBehavior.AllowGet);
                jsonResult2.MaxJsonLength = int.MaxValue;
                return jsonResult2;
            }
            catch (EntityCommandExecutionException ex)
            {
                // Log or inspect the inner exception
                Console.WriteLine(ex.InnerException?.Message);

                // Ensure the method returns something if an exception occurs
                var errorResult = Json(new { error = "An error occurred while fetching the sale data." }, JsonRequestBehavior.AllowGet);
                errorResult.MaxJsonLength = int.MaxValue;
                return errorResult;  // Returning an error response
            }
        }
        /*  [HttpPost]
          public ActionResult Index(string customerName, string customerLocation, string description, string unitType, string colorCode, decimal? price, int quantity, string invoiceNo, DateTime? date, string item)
          {
              var model = new SaleViewModels
              {
                  CustomerName = customerName,
                  CustomerLocation = customerLocation,
                  Description = description,
                  UnitType = unitType,
                  ColorCode = colorCode,
                  Price = price,
                  Quantity = quantity,
                  InvoiceNo = invoiceNo,
                  Date = (DateTime)date,
                  Item = item
              };

              // Optionally, save to database or perform calculations here

              // Pass the model data to View B
              TempData["ProcessedData"] = model;

              // Redirect to View B
              return RedirectToAction("Create");
          }
  */

        public ActionResult Create()
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ViewBag.CategoryNamesEng = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                                              .ToList(), "CategoryNameEng", "CategoryNameEng");
                ViewBag.CategoryNamesKh = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                                              .ToList(), "CategoryNameKh", "CategoryNameKh");
                ViewBag.CustomerName = new SelectList(db.tblCustomers.Where(u => !u.Status.ToString().Contains("0") && !u.CustomerName.Contains("Null"))
                                              .ToList(), "CustomerName", "CustomerName");
            };
            return View();

        }
        [HttpPost]
        public ActionResult Create(tblSale client, SaleViewModels model, ProductViewModels pmodel, tblProduct prod)
        {
            try
            {
                /* model = TempData["ProcessedData"] as SaleViewModels;*/


                using (jotunDBEntities db = new jotunDBEntities())
                {
                    CommonFunctions functions = new CommonFunctions();
                    client.Amount = Convert.ToDouble(model.Amount);
                    client.Discount = Convert.ToDouble(model.Discount);
                    client.RevicedFromCustomer = Convert.ToDouble(model.RevicedFromCustomer);
                    client.ReceivedByABA = model.ReceivedByABA;
                    var discount = client.Discount;
                    client.Owe = (client.Amount - discount) - (client.RevicedFromCustomer + Convert.ToDouble(client.ReceivedByABA));
                    var ids = Guid.NewGuid().ToString();
                    client.Id = ids;
                    client.Description = model.Description;
                    client.CreatedDate = CommonFunctions.ToLocalTime(DateTime.Now);
                    client.UpdatedDate = CommonFunctions.ToLocalTime(DateTime.Now);

                    client.SaleAmount = Convert.ToDecimal(client.Amount) - Convert.ToDecimal(discount);
                    client.SaleDeposit = Convert.ToDecimal(client.ReceivedByABA) + Convert.ToDecimal(client.RevicedFromCustomer);
                    client.SaleImage = string.IsNullOrEmpty(client.SaleImage)
                        ? "/Images/defaultimage.jpg"
                        : "/Images/" + model.SaleImage;

                    //if(client.customer_location == null || client.customer_location == 0)
                    //{
                    //    tblCustomer_location location = new tblCustomer_location();
                    //    location.location = model.LocationText;
                    //    location.updated_date = DateTime.Now;
                    //    location.created_date = DateTime.Now;
                    //    location.customer_id = model.CustomerId;
                    //    location.status = true;
                    //    db.tblCustomer_location.Add(location);
                    //    db.SaveChanges();
                    //    client.customer_location = location.id;
                    //}

                    //var getIsLocation = (from l in db.tblCustomer_location
                    //                     where l.customer_id == client.CustomerId && l.location == model.LocationText
                    //                     select l).FirstOrDefault();
                    //if (getIsLocation == null)
                    //{
                    //    tblCustomer_location location = new tblCustomer_location();
                    //    location.location = model.LocationText;
                    //    location.updated_date = CommonFunctions.ToLocalTime(DateTime.Now);
                    //    location.created_date = CommonFunctions.ToLocalTime(DateTime.Now);
                    //    location.customer_id = model.CustomerId;
                    //    location.status = true;
                    //    db.tblCustomer_location.Add(location);
                    //    db.SaveChanges();
                    //    client.customer_location = location.id;
                    //}
                    //else
                    //{
                    //    if (client.customer_location != null)
                    //    {
                    //        client.customer_location = getIsLocation.id;
                    //    }
                    //    else
                    //    {
                    //        client.customer_location = Convert.ToInt32(model.CustomerLocation);
                    //    }
                    //}

                    if (string.IsNullOrEmpty(model.CustomerId))
                    {
                        if (string.IsNullOrEmpty(model.CustomerName))
                        {
                            client.CustomerId = "1";
                        }
                        else
                        {
                            //Create new customer
                            tblCustomer customer = new tblCustomer();
                            customer.Id = Guid.NewGuid().ToString();
                            customer.CustomerTypeId = 1;
                            customer.CustomerName = model.CustomerName;
                            customer.PhoneNumber = model.PhoneNumber;
                            customer.Status = 1;
                            customer.CreatedDate = DateTime.Now;
                            customer.UpdatedDate = DateTime.Now;
                            customer.TotalPoint = 0;
                            customer.TotalPointAmount = 0;
                            db.tblCustomers.Add(customer);
                            db.SaveChanges();
                            client.CustomerId = customer.Id;
                        }
                    }
                    else
                        client.CustomerId = model.CustomerId;

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
                            //var pros = (from t in db.tblProductByUnits where t.TypeDefault == true && t.ProductID == item.ProductId select t.UnitTypeID).SingleOrDefault();
                            //var unit = (from t in db.tblUnits where t.Id == pros select t.Quantity).SingleOrDefault();



                            tblSalesDetail clientd = new tblSalesDetail();
                            clientd.Id = Guid.NewGuid().ToString();
                            clientd.SaleId = client.Id;
                            clientd.UnitTypeId = i.Id;
                            clientd.ProductId = item.ProductId;
                            clientd.color_code = item.color_code;
                            clientd.Quantity = Convert.ToDecimal(item.Quantity);
                            clientd.Price = Convert.ToDecimal(item.Price);
                            clientd.actual_price = Convert.ToDecimal(item.actual_price);

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
                            functions.stock_adjustment(item.ProductId, Convert.ToDecimal(item.Quantity), i.Id);
                        }
                    }
                    decimal profit = Convert.ToDecimal(client.SaleDeposit) - Convert.ToDecimal(totalReceivedCostSale);

                    updateSaleProfit(client.Id, totalReceivedCostSale, profit);
                    SaveSalePayment(client);
                    createInvoiceNo(ids);

                    return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ImageUpload(HttpPostedFileBase ImageFile, string Id)
        {
            if (ImageFile == null || Id == null)
            {
                return Json(new { success = false, message = "No file or row ID provided." });
            }

            try
            {
                // Save the file to the server
                string fileName = Path.GetFileName(ImageFile.FileName);
                string filePath = Path.Combine(Server.MapPath("~/Images/"), fileName);
                ImageFile.SaveAs(filePath);

                using (var db = new jotunDBEntities())
                {
                    var sale = db.tblSales.FirstOrDefault(s => s.Id == Id);
                    if (sale == null)
                    {
                        return Json(new { success = false, message = "Record not found." });
                    }

                    // Update the SaleImage field
                    sale.SaleImage = "/Images/" + fileName;
                    db.SaveChanges();
                }

                return Json(new { success = true, message = "Image uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        public ActionResult ImageRetrieve(string Id)
        {
            using (var db = new jotunDBEntities())
            {
                var sale = db.tblSales.SingleOrDefault(x => x.Id == Id);
                if (sale == null || string.IsNullOrEmpty(sale.SaleImage))
                {
                    return HttpNotFound("Image not found.");
                }

                // Assuming the image is stored in the /Images folder on your web server
                string imageUrl = Url.Content(sale.SaleImage);  // Returns relative URL to the image

                // If you are expecting absolute URL, construct it like this
                // string imageUrl = "http://yourwebsite.com" + sale.SaleImage;

                return Json(new { imageUrl = imageUrl }, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpPost]
        //public ActionResult ImageUpload(tblSale model)
        //{
        //    string imageId = string.Empty;

        //    // Get the file from the model
        //    var file = model.ImageFile;

        //    if (file != null && file.ContentLength > 0)
        //    {
        //        // Generate a unique filename (to avoid name conflicts)
        //        string fileName = Path.GetFileName(file.FileName);
        //        string filePath = Path.Combine(Server.MapPath("/Images/"), fileName);

        //        // Save the file to the Images folder
        //        file.SaveAs(filePath);

        //        using (jotunDBEntities db = new jotunDBEntities())
        //        {
        //            // Find the record to update (using SaleId or another identifier)
        //            var sale = db.tblSales.SingleOrDefault(x => x.Id == model.Id);

        //            if (sale != null)
        //            {
        //                // Save the image path in the database
        //                sale.SaleImage = "/Images/" + fileName;
        //                db.SaveChanges();
        //            }
        //        }
        //    }

        //    return Json(new { success = true, message = "Image uploaded successfully." }, JsonRequestBehavior.AllowGet);
        //}


        //public ActionResult ImageRetrieve(string imgID)
        //{
        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        var sale = db.tblSales.SingleOrDefault(x => x.SaleImage.Contains(imgID));

        //        if (sale != null && !string.IsNullOrEmpty(sale.SaleImage))
        //        {
        //            string imagePath = Server.MapPath(sale.SaleImage);

        //            if (System.IO.File.Exists(imagePath))
        //            {
        //                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
        //                return File(imageBytes, "image/jpeg");
        //            }
        //        }
        //    }

        //    return HttpNotFound();
        //}



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

        public async Task<ActionResult> Delete(string id)
        {
            CommonFunctions function = new CommonFunctions();
            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblSale cust = await db.tblSales.FindAsync(id);

                var GetDetail2 = db.tblSalesDetails.Where(w => string.Compare(w.SaleId, id) == 0).ToList();

                foreach (var ii in GetDetail2)
                {
                    tblSalesDetail clientd2 = db.tblSalesDetails.Find(ii.Id);

                    function.revert_stock_balance(clientd2.Id);

                    //tblProduct pp = db.tblProducts.Find(clientd2.ProductId);
                    //var product = (from pro in db.tblProducts
                    //               where pro.Id == clientd2.ProductId
                    //               select pro
                    //              ).ToList();
                    //foreach (var i in product)
                    //{
                    //    // pp.QuantityInStock = i.QuantityInStock + clientd2.Quantity;
                    //    // db.SaveChanges();


                    //    var si = (from i1 in db.tblUnits
                    //              where i1.Id == ii.UnitTypeId
                    //              select i1).FirstOrDefault();

                    //    // pp.QuantityInStock = i.QuantityInStock - clientd2.Quantity;
                    //    var k = si.Quantity * ii.Quantity;
                    //    pp.QuantityInStockRetail = (long)Convert.ToDecimal(pp.QuantityInStockRetail + k);
                    //    pp.QuantityInStock = pp.QuantityInStockRetail / si.Quantity;
                    //    db.SaveChanges();

                    //}
                    //clientd2.Quantity = 0;

                    //db.SaveChanges();
                }
                cust.Status = 0;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");


            }
        }
        public ActionResult Edit(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ViewBag.CategoryNamesEng = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                                              .ToList(), "CategoryNameEng", "CategoryNameEng");
                ViewBag.CategoryNamesKh = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                                              .ToList(), "CategoryNameKh", "CategoryNameKh");
                ViewBag.Customers = new SelectList(db.tblCustomers.Where(u => !u.Status.ToString().Contains("0"))
                                              .ToList(), "CustomerName", "CustomerName");

            };
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            return View(SaleViewModels.GetNoDetail(id));
        }

        // POST: Client/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, SaleViewModels model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                //if (!ModelState.IsValid) return View(model);
                CommonFunctions functions = new CommonFunctions();
                tblSale client = db.tblSales.Find(id);
                if (client != null)
                {

                    var customer = (from p in db.tblCustomers
                                    where p.CustomerName == model.CustomerId
                                    select p).FirstOrDefault();
                    client.CustomerId = customer.Id;

                    client.Id = model.Id;
                    client.CreatedDate = DateTime.Now;
                    client.Description = model.Description;

                    db.SaveChanges();

                    var GetDetail = db.tblSalesDetails.Where(w => string.Compare(w.SaleId, id) == 0).ToList();
                    if (GetDetail.Any())
                    {
                        foreach (var item in GetDetail)
                        {
                            tblSalesDetail clientDetail = db.tblSalesDetails.Where(w => string.Compare(w.Id, item.Id) == 0).FirstOrDefault();
                            clientDetail.Quantity = item.Quantity;
                            db.SaveChanges();
                            if (model.GetDetail != null)
                            {

                                var GetDetail2 = db.tblSalesDetails.Where(w => string.Compare(w.SaleId, id) == 0).ToList();

                                foreach (var ii in GetDetail2)
                                {
                                    tblSalesDetail clientd2 = db.tblSalesDetails.Find(ii.Id);

                                    functions.revert_stock_balance(clientd2.Id);

                                    db.tblSalesDetails.Remove(clientd2);
                                    db.SaveChanges();
                                }

                                foreach (SaleDetailViewModel items in model.GetDetail)
                                {
                                    tblSalesDetail clientd = new tblSalesDetail();
                                    clientd.Id = Guid.NewGuid().ToString();
                                    clientd.SaleId = client.Id;
                                    clientd.ProductId = items.ProductId;
                                    clientd.UnitTypeId = items.UnitTypeId;
                                    clientd.Price = Convert.ToDecimal(items.Price);
                                    clientd.Quantity = Convert.ToDecimal(items.Quantity);
                                    db.tblSalesDetails.Add(clientd);
                                    db.SaveChanges();
                                    functions.stock_adjustment(items.ProductId, Convert.ToDecimal(items.Quantity), items.UnitTypeId);
                                }
                            }
                        }
                    }
                }
                return RedirectToAction("Index", "Sale");
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditNew(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            return View(SaleViewModels.GetNoDetail(id));
        }
        [HttpPost]
        public ActionResult EditNew(string id, SaleViewModels model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                CommonFunctions functions = new CommonFunctions();
                tblSale client = db.tblSales.Find(id);
                if (client != null)
                {
                    var customer = (from p in db.tblCustomers
                                    where p.Id == model.CustomerId
                                    select p).FirstOrDefault();
                    client.CustomerId = customer.Id;
                    client.Id = model.Id;
                    client.Amount = Convert.ToDouble(model.Amount);
                    client.Discount = Convert.ToDouble(model.Discount);
                    client.RevicedFromCustomer = Convert.ToDouble(model.RevicedFromCustomer) + Convert.ToDouble(model.RevicedFromCustomer1); ;
                    var discount = client.Discount;
                    client.Owe = (client.Amount - discount) - client.RevicedFromCustomer;
                    client.Description = model.Description;
                    client.CreatedDate = DateTime.Now;
                    client.UpdatedDate = DateTime.Now;
                    client.customer_location = Convert.ToInt32(model.CustomerLocation);
                    var getIsLocation = (from l in db.tblCustomer_location
                                         where l.customer_id == customer.Id && l.location == model.LocationText
                                         select l).FirstOrDefault();
                    if (getIsLocation == null)
                    {
                        tblCustomer_location location = new tblCustomer_location();
                        location.location = model.LocationText;
                        location.updated_date = DateTime.Now;
                        location.created_date = DateTime.Now;
                        location.customer_id = model.CustomerId;
                        location.status = true;
                        db.tblCustomer_location.Add(location);
                        db.SaveChanges();
                        client.customer_location = location.id;
                    }

                    db.SaveChanges();

                    var GetDetail = db.tblSalesDetails.Where(w => string.Compare(w.SaleId, id) == 0).ToList();
                    if (GetDetail.Any())
                    {
                        foreach (var item in GetDetail)
                        {

                            if (model.GetDetail != null)
                            {

                                var GetDetail2 = db.tblSalesDetails.Where(w => string.Compare(w.SaleId, id) == 0).ToList();

                                foreach (var ii in GetDetail2)
                                {
                                    tblSalesDetail clientd2 = db.tblSalesDetails.Find(ii.Id);
                                    functions.revert_stock_balance(clientd2.Id);
                                    db.tblSalesDetails.Remove(clientd2);
                                    db.SaveChanges();
                                }

                                foreach (SaleDetailViewModel items in model.GetDetail)
                                {
                                    tblSalesDetail clientd = new tblSalesDetail();
                                    clientd.Id = Guid.NewGuid().ToString();
                                    clientd.SaleId = client.Id;
                                    clientd.ProductId = items.ProductId;
                                    clientd.UnitTypeId = items.UnitTypeId;
                                    clientd.color_code = items.color_code;
                                    clientd.actual_price = Convert.ToDecimal(items.actual_price);
                                    clientd.Quantity = Convert.ToDecimal(items.Quantity);
                                    db.tblSalesDetails.Add(clientd);
                                    db.SaveChanges();

                                    functions.stock_adjustment(items.ProductId, Convert.ToDecimal(items.Quantity), items.UnitTypeId);
                                }
                            }
                        }
                    }
                }
                return RedirectToAction("Index", "Sale");
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //public JsonResult getCust(string term)
        //{
        //    var Cust = (from e in db.tblCustomers
        //                where e.CustomerName.ToLower().Contains(term.ToLower())
        //                select new
        //                {
        //                    label = e.CustomerName,
        //                    id = e.CustomerName,
        //                    pid = e.Id,
        //                    pn = e.CustomerName


        //                });

        //    return Json(Cust, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult getCust(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            return Json((db.tblCustomers.Where(c => c.Status == 1 && c.CustomerName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.CustomerName, label2 = a.PhoneNumber, id = a.Id, status = a.is_old_data }).Take(100)), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getCustPhone(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            return Json((db.tblCustomers.Where(c => c.Status == 1 && c.PhoneNumber.ToLower().Contains(term.ToLower())).Select(a => new { label = a.PhoneNumber, label2 = a.CustomerName, id = a.Id, status = a.is_old_data }).Take(100)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult get_Location(string term, string customer_id)
        {
            jotunDBEntities db = new jotunDBEntities();
            //return Json((db.tblCustomer_location.Where(c => c.status == true && c.customer_id == customer_id && c.location.ToLower().Contains(term.ToLower())).Select(a => new { label = a.location, id = a.id }).Take(100)), JsonRequestBehavior.AllowGet);
            var results = db.tblCustomer_location
                .Where(c => c.status == true && c.customer_id == customer_id && c.location.ToLower().Contains(term.ToLower()))
                .Select(a => new { label = a.location, id = a.id })
                .GroupBy(customer => customer.label)
                .Select(group => group.FirstOrDefault())
                .Take(100);

            return Json(results, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getpro(string term)
        {

            jotunDBEntities db = new jotunDBEntities();
            var EmpName = (from e in db.tblProducts
                               //where e.ProductCode.ToLower().Contains(term.ToLower())
                           where e.Status == 1 && (e.ProductName.ToLower().Contains(term.ToLower())) || (e.ProductCode.ToLower().Contains(term.ToLower()))
                           select new
                           {
                               label = e.ProductName + " Code: " + e.ProductCode,
                               id = e.ProductName,
                               pid = e.Id,
                               pn = e.ProductName,
                               pr = e.PriceInStock,
                               qs = e.QuantityInStockRetail,
                               qsn = e.QuantityInStock,

                           });

            return Json(EmpName, JsonRequestBehavior.AllowGet);
            //return Json((db.tblProducts.Where(c => c.ProductName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.ProductName, id = a.Id, }).Take(100)), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStateList(string UnId)
        {
            db.Configuration.ProxyCreationEnabled = false;

            List<tblUnit> statelist = (from k in db.tblUnits
                                       join k2 in db.tblProductByUnits on k.Id equals k2.UnitTypeID

                                       join k1 in db.tblProducts on k2.ProductID equals k1.Id
                                       where k1.ProductName == UnId
                                       // where !k.UnitNameEng.Contains("Null") && !k.Status.ToString().Contains("0")
                                       select k).ToList();

            return Json(statelist, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetUnit(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                //string code = "1";
                //string code = db.tblProductByUnits.Where(w => string.Compare(w.Id, id) == 0).Select(s => s.Cost).FirstOrDefault().ToString();

                string sampleString = id;
                string[] arrayString = sampleString.Split('^');
                if (sampleString == "")
                {
                    return Json(new { result = Result.Success }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var j = arrayString[0];
                    var j1 = arrayString[1];




                    var pr = (from pro in db.tblProducts
                              where pro.ProductName == j
                              select pro).ToList();

                    foreach (var kk in pr)
                    {
                        var ko = (from ko1 in db.tblProductByUnits
                                  where ko1.ProductID == kk.Id && ko1.UnitTypeID == j1
                                  select ko1).ToList();
                        foreach (var i in ko)
                        {
                            var uni = (from un in db.tblUnits
                                       where un.Id == i.UnitTypeID
                                       select un).FirstOrDefault();

                            return Json(new { result = Result.Success, data = i.Price, data1 = uni.Id, data2 = uni.Quantity, data3 = uni.UnitNameEng }, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                // string code = CommonFunction.GenerateServiceCodeNumber(id);
                return Json(new { result = Result.Success }, JsonRequestBehavior.AllowGet);

            }

        }
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            return View(SaleViewModels.GetNoDetail(id));
        }
        // POST: Client/Edit/5
        [HttpPost]
        public ActionResult Details(string id, SaleViewModels model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                //if (!ModelState.IsValid) return View(model);

                tblSale client = db.tblSales.Find(id);
                if (client != null)
                {
                    client.RevicedFromCustomer = client.RevicedFromCustomer ?? 0;
                    client.ReceivedByABA = client.ReceivedByABA ?? 0;

                    client.RevicedFromCustomer = client.RevicedFromCustomer + Convert.ToDouble(model.RevicedFromCustomer1);
                    client.ReceivedByABA = client.ReceivedByABA + Convert.ToDecimal(model.NewReceivedByABA);
                    client.Owe = client.Amount - client.RevicedFromCustomer - Convert.ToDouble(model.Discount) - (double)client.ReceivedByABA;
                    if (client.Owe <= 0)
                    {
                        client.InvoiceStatus = "Paid";
                    }
                    else
                    {
                        client.InvoiceStatus = "Not Paid";
                    }
                    client.Discount = Convert.ToDouble(model.Discount);
                    if ((client.RevicedFromCustomer + (double)client.ReceivedByABA) > 0)
                    {
                        client.UpdatedDate = CommonFunctions.ToLocalTime(DateTime.Now);

                    }
                    client.SaleDeposit = Convert.ToDecimal(client.ReceivedByABA) + Convert.ToDecimal(client.RevicedFromCustomer);
                    client.SaleAmount = Convert.ToDecimal(client.Amount) - Convert.ToDecimal(client.Discount);
                    db.SaveChanges();
                    client.RevicedFromCustomer = Convert.ToDouble(model.RevicedFromCustomer1);
                    client.ReceivedByABA = model.NewReceivedByABA;
                    SaveSalePayment(client);

                    decimal totalCostGoodSold = 0;
                    decimal totalDepositCoGs = 0;
                    decimal totalReceivedCostSale = 0;
                    decimal percentageCashRecived = client.SaleDeposit > 0 ? Convert.ToDecimal((client.SaleDeposit / client.SaleAmount) * 100) : 0;

                    var saleDetails = db.tblSalesDetails.Where(s => string.Compare(s.SaleId, client.Id) == 0).ToList();
                    foreach (var clientd in saleDetails)
                    {
                        decimal productCostGoodSold = 0;
                        var costGoodSold = db.tblProductByUnits.Where(s => string.Compare(s.ProductID, clientd.ProductId) == 0 && string.Compare(s.UnitTypeID, clientd.UnitTypeId) == 0).FirstOrDefault();
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
                    }

                    decimal profit = Convert.ToDecimal(client.SaleDeposit) - Convert.ToDecimal(totalReceivedCostSale);

                    updateSaleProfit(client.Id, totalReceivedCostSale, profit);
                }
                return RedirectToAction("Index", "Sale");
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetProductDataForSale(SaleViewModels model)
        {
            List<SaleViewModels> models = new List<SaleViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();

                var products = db.tblProducts.OrderBy(x => x.Id).ToList();
                foreach (var p in products)
                {
                    if (p.Status == 1)
                    {
                        var category = (from p1 in db.tblCategories
                                        where p1.Id == p.CategoryId
                                        select p1).FirstOrDefault();
                        var unit = (from p1 in db.tblUnits
                                    where p1.Id == p.Unitid
                                    select p1).FirstOrDefault();
                        var unitss = unit.UnitNameEng;
                        if (unitss == null)
                        {
                            unitss = unit.UnitNameKh;
                        }
                        var categoryss = category.CategoryNameEng;
                        if (categoryss == null)
                        {
                            categoryss = category.CategoryNameKh;
                        }
                        var date = p.UpdatedDate;
                        if (date == null) { date = p.CreatedDate; }
                        else { date = p.UpdatedDate; }

                        models.Add(new SaleViewModels()
                        {
                            Id = p.Id.ToString(),
                            CategoryName = categoryss,
                            UnitName = unitss,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            Description = p.Description,
                            ProductImage = p.ProductImage,
                            QuantityInStock = Convert.ToString(p.QuantityInStock),
                            PriceInStock = Convert.ToString(p.PriceInStock),
                            CreatedDate = Convert.ToDateTime(date).ToString("dd-MMM-yyyy"),
                        });

                    }
                }

            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult SubmitNewCustomer(tblCustomer cmodel, string[] locations)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                cmodel.CreatedDate = DateTime.Now;
                //status 0 = customer enabled
                cmodel.Id = Guid.NewGuid().ToString();
                cmodel.Status = 1;
                cmodel.is_old_data = false;
                db.tblCustomers.Add(cmodel);
                db.SaveChanges();
                foreach (var l in locations)
                {
                    ///Add Multiple Location
                    if (!String.IsNullOrEmpty(l))
                    {
                        tblCustomer_location location = new tblCustomer_location();
                        location.location = l;
                        location.updated_date = DateTime.Now;
                        location.created_date = DateTime.Now;
                        location.customer_id = cmodel.Id;
                        location.status = true;

                        db.tblCustomer_location.Add(location);
                        db.SaveChanges();
                    }
                    ///End Multiple Location
                }
                //return RedirectToAction("Create", "Sale");

                return Json(new { data = cmodel }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetCustomerDataForSale()
        {
            List<CustomerViewModels> models = new List<CustomerViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var customers = db.tblCustomers.OrderBy(x => x.CustomerName).ToList();
                foreach (var c in customers)
                {
                    if (c.Status == 1)
                    {

                        if (c.UpdatedDate == null)
                        {
                            models.Add(new CustomerViewModels()
                            {
                                Id = c.Id.ToString(),
                                CustomerNo = c.CustomerNo,
                                CustomerName = c.CustomerName,
                                PhoneNumber = c.PhoneNumber,
                                ProjectLocation = c.ProjectLocation,
                                Gender = c.Gender,
                                Noted = c.Noted,
                                Status = Convert.ToString(c.Status),
                                CreatedDate = Convert.ToDateTime(c.CreatedDate).ToString("dd-MMM-yyyy"),

                            });
                        }
                        else
                        {
                            models.Add(new CustomerViewModels()
                            {
                                Id = c.Id.ToString(),
                                CustomerNo = c.CustomerNo,
                                CustomerName = c.CustomerName,
                                PhoneNumber = c.PhoneNumber,
                                ProjectLocation = c.ProjectLocation,
                                Gender = c.Gender,
                                Noted = c.Noted,
                                Status = Convert.ToString(c.Status),
                                CreatedDate = Convert.ToDateTime(c.UpdatedDate).ToString("dd-MMM-yyyy"),

                            });
                        }
                    }
                }
            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        public ActionResult InvoiceReportWithPaid(string id)
        {

            jotunDBEntities db = new jotunDBEntities();

            tblSale s = db.tblSales.Find(id);
            var ss = db.tblSalesDetails.Where(ID => ID.SaleId == id).ToList();
            foreach (var si in ss)
            {
                var firstId = (from p in db.tblSales select p.InvoiceNo).Max();

                if (s.InvoiceNo != null)
                {
                    s.InvoiceNo = s.InvoiceNo;
                }
                else
                {
                    s.InvoiceNo = (Convert.ToInt32(firstId) + 1).ToString();

                }
                s.InvoiceStatus = "Paid";
                s.Owe = 0;
                //   db.tblSales.Add(s);
                db.SaveChanges();
            }
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



                        DataRow dr = dt.NewRow();

                        //dr["CustomerId"] = cus.CustomerName;
                        //dr["Date"] = DateTime.Now;
                        //dr["Id"] = ii.Id;
                        //dr["ProductId"] = proname.ProductName;
                        //dr["Quantity"] = ii.Quantity;
                        //dr["Price"] = ii.Price;
                        //dr["Total"] = ii.Price * ii.Quantity;
                        //dr["Totals"] = ii.Price;
                        //dr["VAT"] = ii.Price;
                        dr["CustomerId"] = cus.CustomerName;
                        dr["Date"] = DateTime.Now;
                        dr["Id"] = ii.Id;
                        dr["ProductId"] = proname.ProductName;
                        dr["Quantity"] = ii.Quantity;
                        dr["Price"] = ii.actual_price;
                        dr["Total"] = ii.actual_price * ii.Quantity;
                        dr["Totals"] = ii.actual_price;
                        dr["VAT"] = ii.actual_price;

                        dt.Rows.Add(dr);
                    }

                }

            }


            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleInvoice", dt);
            rv.LocalReport.DataSources.Add(rds);


            ViewBag.ReportViewer = rv;

            return View();
        }
        /* public ActionResult CustomerHistory(string customer_id)
         {

             using (jotunDBEntities db = new jotunDBEntities())
             {
                 ViewBag.CategoryNamesEng = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                                               .ToList(), "CategoryNameEng", "CategoryNameEng");
                 ViewBag.CategoryNamesKh = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                                               .ToList(), "CategoryNameKh", "CategoryNameKh");
                 ViewBag.CustomerName = new SelectList(db.tblCustomers.Where(u => !u.Status.ToString().Contains("0") && !u.CustomerName.Contains("Null"))
                                               .ToList(), "CustomerName", "CustomerName");
             };
             if (isAdminUser())
             {

                 return View();
             }
             else
             {
                 return RedirectToAction("Login", "Account");
             }
         }*/
        [HttpGet]
        public ActionResult CustomerHistory(string id, string CustomerName = "", string PhoneNumber = "")
        {
            PopulateDropdowns();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                var query = db.tblCustomers.Where(c => c.Status != 0);
                if (!string.IsNullOrEmpty(id))
                {
                    query = query.Where(c => c.Id.ToString() == id);
                }
                if (!string.IsNullOrEmpty(CustomerName))
                {
                    query = query.Where(c => c.CustomerName.Contains(CustomerName));
                }
                if (!string.IsNullOrEmpty(PhoneNumber))
                {
                    query = query.Where(c => c.PhoneNumber.Contains(PhoneNumber));
                }
                var customers = query.ToList();
                ViewBag.Customers = customers;
                if (!string.IsNullOrEmpty(id) && customers.Any())
                {
                    var customer = customers.FirstOrDefault();
                    ViewBag.CustomerName = customer.CustomerName;
                    ViewBag.PhoneNumber = customer.PhoneNumber;
                    ViewBag.CustomerId = id;
                }
                else if (!string.IsNullOrEmpty(id))
                {
                    ViewBag.ErrorMessage = "No customer found with the provided ID.";
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult CustomerHistory(FormCollection form)
        {
            string inputId = form["customer_id"];
            string nameEng = form["name_eng"];
            string telephone = form["telephone"];
            bool oldRecord = Convert.ToBoolean(Request.QueryString["old_record"] ?? "false");
            ViewBag.Status = oldRecord;

            using (jotunDBEntities db = new jotunDBEntities())
            {
                var query = db.tblCustomers.Where(c => c.Status != 0);

                if (!string.IsNullOrEmpty(inputId))
                {
                    query = query.Where(c => c.Id.ToString() == inputId);
                }
                if (!string.IsNullOrEmpty(nameEng))
                {
                    query = query.Where(c => c.CustomerName.Contains(nameEng));
                }
                if (!string.IsNullOrEmpty(telephone))
                {
                    query = query.Where(c => c.PhoneNumber.Contains(telephone));
                }
                var customers = query.ToList();
                ViewBag.Customers = customers;

                if (customers.Any())
                {
                    var matchedCustomer = customers.FirstOrDefault();
                    ViewBag.CustomerId = matchedCustomer.Id.ToString();
                    ViewBag.CustomerName = matchedCustomer.CustomerName;
                    ViewBag.PhoneNumber = matchedCustomer.PhoneNumber;
                }
                else
                {
                    ViewBag.CustomerId = inputId;
                    ViewBag.CustomerName = nameEng;
                    ViewBag.PhoneNumber = telephone;

                    ViewBag.ErrorMessage = "No customers found matching the criteria.";
                }
            }
            PopulateDropdowns();
            return View();
        }
        private void PopulateDropdowns()
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ViewBag.CategoryNamesEng = new SelectList(
                    db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                    .ToList(), "CategoryNameEng", "CategoryNameEng");

                ViewBag.CategoryNamesKh = new SelectList(
                    db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                    .ToList(), "CategoryNameKh", "CategoryNameKh");

                ViewBag.CustomerNameList = new SelectList(
                    db.tblCustomers.Where(u => !u.Status.ToString().Contains("0") && !u.CustomerName.Contains("Null"))
                    .ToList(), "CustomerID", "CustomerName");
            }
        }

        public JsonResult getCustomerHistory(string customer_id, bool old_record = false)
        {
            List<SaleViewModels> models = new List<SaleViewModels>();
            List<tblSale> sale_list = new List<tblSale>();
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var customer_name = db.tblCustomers.Where(x => x.Id == customer_id).FirstOrDefault();
                if (old_record)
                {

					/* sale_list = (from s in db.tblSales
                                      join c in db.tblCustomers on s.CustomerId equals c.Id
                                      where c.CustomerName == customer_name.CustomerName && s.Status == 1
                                      select s)
                                      .ToList();  */
					 sale_list = db.tblSales
								 .Where(s => s.Status == 1 && s.CustomerId == customer_name.Id)
								 .Join(db.tblCustomers,
								  s => s.CustomerId, c => c.Id, (s, c) => new { Sale = s, Customer = c })
								 .Where(sc => sc.Customer.CustomerName == customer_name.CustomerName)
								 .Select(sc => sc.Sale)
								 .ToList();
				}
				else
                {
                    sale_list = (from s in db.tblSales
                                 where s.CustomerId == customer_id && s.Status == 1 
								 select s).ToList();
                }
                foreach (var sale in sale_list)
                {
                    string location = "";
                    var customer_location_obj = db.tblCustomer_location.Where(l => l.id == sale.customer_location).FirstOrDefault();
                    if (customer_location_obj != null)
                    {
                        location = customer_location_obj.location;
                    }
                    models.Add(new SaleViewModels()
                    {
                        Id = sale.Id,
                        CustomerId = customer_name.CustomerName,
                        SaleImage = string.IsNullOrEmpty(sale.SaleImage) ? "/Images/defaultimage.jpg" : sale.SaleImage,
                        //Owe = float.Parse((item.Owe - item.RevicedFromCustomer).ToString()).ToString("0.000"),
                        Owe = ((sale.Amount - (sale.Discount + sale.RevicedFromCustomer)) ?? 0).ToString("0.00"),
                        Amount = (sale.Amount ?? 0).ToString("0.00"),
                        Discount = (sale.Discount ?? 0).ToString("0.00"),
                        RevicedFromCustomer = (sale.RevicedFromCustomer ?? 0).ToString("0.00"),
                        InvoiceStatus = sale.InvoiceStatus,
                        InvoiceNo = sale.InvoiceNo,
                        Description = sale.Description,
                        CreatedDate = Convert.ToDateTime(sale.CreatedDate.ToString()).ToString("yyyy-MM-dd"),
                        ProjectLocation = location,
                    });
                }
            };

            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        /* public ActionResult InvoiceReport(string id)
         {
             *//* jotunDBEntities db = new jotunDBEntities();
              var aa = 0;
              tblSale s = db.tblSales.Find(id);
              var ss = db.tblSalesDetails.Where(ID => ID.SaleId == id).ToList();
              var firstId = (from p in db.tblSales where p.InvoiceNo != null && p.Status == 1 select p).ToList();
              var maxid = (from x in db.tblSales where x.InvoiceNo != null && x.Status == 1 select x.InvoiceId).Max();
              string imageUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content(s.SaleImage);
  *//*
             jotunDBEntities db = new jotunDBEntities();
             var aa = 0;
             var idList = id.Split(',')
                 .Select(i => i.Trim())
                 .Where(i => int.TryParse(i, out _))  // Filter only valid integers
                 .Select(i => int.Parse(i))
                 .ToList();
             var invoiceDetailsList = new List<object>();
             foreach (var singleIdStr in idList)
             {
                 string singleIdAsString = singleIdStr.ToString();
                 tblSale s = db.tblSales.Find(singleIdStr);
                 if (s == null)
                 {
                     return HttpNotFound($"Sale with ID {singleIdStr} not found.");
                 }
                 var ss = db.tblSalesDetails.Where(ID => ID.SaleId == singleIdAsString).ToList();
                 var firstId = (from p in db.tblSales
                                where p.InvoiceNo != null && p.Status == 1
                                select p).ToList();
                 var maxid = (from x in db.tblSales
                              where x.InvoiceNo != null && x.Status == 1
                              select x.InvoiceId).Max();
                 string imageUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content(s.SaleImage);
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
                     }
                 }

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
                 dt.Columns.Add(new DataColumn("SaleImage", typeof(string)));
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
                             dr["CustomerId"] = cus.CustomerName;
                             dr["Date"] = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                             dr["Id"] = ii.Id;
                             dr["ProductId"] = proname.ProductName;
                             dr["Quantity"] = float.Parse((ii.Quantity).ToString());
                             dr["Price"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                             dr["Total"] = float.Parse((ii.actual_price * ii.Quantity).ToString()).ToString("N");
                             dr["Totals"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                             dr["VAT"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                             dr["Phone"] = cus.PhoneNumber;
                             dr["Address"] = cus.ProjectLocation;
                             dr["Discount"] = cuss.Discount;
                             dr["RevicedFromCustomer"] = float.Parse((cuss.RevicedFromCustomer).ToString()).ToString("N");
                             //dr["Owe"] = float.Parse((cuss.Owe).ToString()).ToString("0.000");
                             dr["Owe"] = cuss.Amount - (((cuss.Amount * cuss.Discount) / 100) + cuss.RevicedFromCustomer);
                             dr["InvoiceNo"] = cuss.InvoiceNo;
                             dr["ColorCode"] = ii.color_code;
                             dr["UnitId"] = unit.UnitNameEng;
                             dr["SaleImage"] = imageUrl;

                             dt.Rows.Add(dr);
                         }
                     }
                 }
                 rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
                 rv.LocalReport.DataSources.Clear();
                 ReportDataSource rds = new ReportDataSource("DSSaleInvoice", dt);
                 rv.LocalReport.DataSources.Add(rds);
                 rv.ShowPrintButton = true;
                 rv.ShowRefreshButton = true;
                 rv.LocalReport.EnableExternalImages = true;
                 ViewBag.ReportViewer = rv;
             }
                 return View();

         }*/
        [HttpPost]
        public ActionResult StoreIds(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return new HttpStatusCodeResult(400, "No IDs provided.");
            }
            var idList = ids.Split(',')
                 .Select(i => i.Trim())
                 .Where(i => !string.IsNullOrEmpty(i))
                 .Where(i => Guid.TryParse(i, out _) || int.TryParse(i, out _))
                 .ToList();
            Session["SelectedIds"] = idList;
            return RedirectToAction("InvoiceReport");
        }
        public ActionResult InvoiceReport(bool isFirstRecord = true)
        {
            jotunDBEntities db = new jotunDBEntities();
            /*var aa = 0;*/
            var idListString = Session["SelectedIds"] as List<string>;
            var idList = idListString.ToList();
            var allInvoiceData = new List<DataRow>();
            string imageUrl = "/Images/defaultimage.jpg";
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
            dt.Columns.Add(new DataColumn("SaleImage", typeof(string)));

            foreach (var singleIdStr in idList)
            {
                tblSale s = db.tblSales.Find(singleIdStr);
                if (s == null)
                {
                    return HttpNotFound($"Sale with ID {singleIdStr} not found.");
                }
                if (isFirstRecord)
                {
                    imageUrl = string.IsNullOrEmpty(s.SaleImage) ? "/Images/defaultimage.jpg" :
                        Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content(s.SaleImage);
                    isFirstRecord = false;
                }
                var saless = db.tblSalesDetails.Where(ID => ID.SaleId == singleIdStr.ToString());
                var customer = db.tblSales.Where(ID => ID.Id == singleIdStr.ToString());
                if (saless.Any())
                {
                    foreach (var cuss in customer)
                    {
                        foreach (var ii in saless)
                        {
                            var proname = db.tblProducts.FirstOrDefault(p => p.Id == ii.ProductId);
                            var cus = db.tblCustomers.FirstOrDefault(c => c.Id == cuss.CustomerId);
                            var unit = db.tblUnits.FirstOrDefault(u => u.Id == ii.UnitTypeId);
                            DataRow dr = dt.NewRow();
                            dr["CustomerId"] = cus?.CustomerName ?? "N/A";
                            dr["Date"] = DateTime.Now.ToString("dd-MMM-yyyy");
                            dr["Id"] = ii.Id;
                            dr["ProductId"] = proname?.ProductName ?? "Unknown Product";
                            dr["Quantity"] = ii.Quantity;
                            dr["Price"] = ii.actual_price.HasValue ? ii.actual_price.Value.ToString("N") : "0.00";
                            dr["Total"] = (ii.actual_price.HasValue ? ii.actual_price.Value : 0) * ii.Quantity;
                            dr["Totals"] = ii.actual_price.HasValue ? ii.actual_price.Value.ToString("N") : "0.00";
                            dr["VAT"] = ii.actual_price.HasValue ? ii.actual_price.Value.ToString("N") : "0.00";
                            dr["Phone"] = cus?.PhoneNumber ?? "N/A";
                            dr["Address"] = cus?.ProjectLocation ?? "N/A";
                            dr["Discount"] = cuss.Discount;
                            dr["RevicedFromCustomer"] = cuss.RevicedFromCustomer;
                            dr["Owe"] = cuss.Amount - ((cuss.Amount * cuss.Discount) / 100 + cuss.RevicedFromCustomer);
                            dr["InvoiceNo"] = cuss.InvoiceNo;
                            dr["ColorCode"] = ii.color_code;
                            dr["UnitId"] = unit?.UnitNameEng ?? "Unknown Unit";
                            dr["SaleImage"] = imageUrl;
                            dt.Rows.Add(dr);
                        }
                    }
                }
            }
            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleInvoice", dt);
            rv.LocalReport.DataSources.Add(rds);
            rv.ShowPrintButton = true;
            rv.ShowRefreshButton = true;
            rv.LocalReport.EnableExternalImages = true;

            ViewBag.ReportViewer = rv;
            return View();
        }
        public ActionResult ExportToPDFOriginal(string id)
        {
            //id = "07f3b5ec-f01f-4d41-a48c-57597b247754";
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
            dt.Columns.Add(new DataColumn("SaleImage", typeof(string)));
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

                        DataRow dr = dt.NewRow();
                        dr["CustomerId"] = cus.CustomerName;
                        dr["Date"] = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                        dr["Id"] = ii.Id;
                        dr["ProductId"] = proname.ProductName;
                        dr["Quantity"] = float.Parse((ii.Quantity).ToString());
                        dr["Price"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                        dr["Total"] = float.Parse((ii.actual_price * ii.Quantity).ToString()).ToString("N");
                        dr["Totals"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                        dr["VAT"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                        dr["Phone"] = cus.PhoneNumber;
                        dr["Address"] = cus.ProjectLocation;
                        dr["Discount"] = cuss.Discount;
                        dr["RevicedFromCustomer"] = float.Parse((cuss.RevicedFromCustomer).ToString()).ToString("N");
                        dr["Owe"] = cuss.Amount - (((cuss.Amount * cuss.Discount) / 100) + cuss.RevicedFromCustomer);
                        dr["InvoiceNo"] = cuss.InvoiceNo;
                        dr["ColorCode"] = ii.color_code;
                        dr["SaleImage"] = cuss.SaleImage;
                        dt.Rows.Add(dr);

                    }

                }

            }
            rv.ProcessingMode = ProcessingMode.Local;
            //reportViewer.LocalReport.ReportPath = Server.MapPath(@"~\Invoice - Copy.rdlc");
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
            //rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\OweMoreThan.rdlc";
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
        public ActionResult ExportToPDF()
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                var idListString = Session["SelectedIds"] as List<string>;
                var idList = idListString.ToList();
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Price"));
                dt.Columns.Add(new DataColumn("CustomerId"));
                dt.Columns.Add(new DataColumn("Date"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("Phone"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("SaleImage", typeof(string)));
                string imageUrl = "/Images/defaultimage.jpg";
                foreach (var singleIdStr in idList)
                {
                    tblSale s = db.tblSales.Find(singleIdStr);
                    if (s == null) continue;
                    var saless = db.tblSalesDetails.Where(d => d.SaleId == singleIdStr.ToString());
                    var customer = db.tblSales.Where(c => c.Id == singleIdStr.ToString());
                    foreach (var cuss in customer)
                    {
                        foreach (var ii in saless)
                        {
                            var proname = db.tblProducts.FirstOrDefault(p => p.Id == ii.ProductId);
                            var cus = db.tblCustomers.FirstOrDefault(c => c.Id == cuss.CustomerId);
                            DataRow dr = dt.NewRow();
                            dr["Id"] = ii.Id;
                            dr["ProductId"] = proname?.ProductName ?? "Unknown Product";
                            dr["Quantity"] = ii.Quantity;
                            dr["Price"] = ii.actual_price.HasValue ? ii.actual_price.Value.ToString("N") : "0.00";
                            dr["Total"] = (ii.actual_price.HasValue ? ii.actual_price.Value : 0) * ii.Quantity;
                            dr["Discount"] = cuss.Discount;
                            dr["InvoiceNo"] = cuss.InvoiceNo;
                            dr["Phone"] = cus.PhoneNumber;
                            dr["SaleImage"] = string.IsNullOrEmpty(s.SaleImage) ? imageUrl : Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content(s.SaleImage);
                            dr["CustomerId"] = cus?.CustomerName ?? "Unknown";
                            dr["Date"] = DateTime.Now.ToString("dd-MMM-yyyy");

                            dt.Rows.Add(dr);
                        }
                    }
                }
                ReportViewer rv = new ReportViewer
                {
                    ProcessingMode = ProcessingMode.Local,
                    SizeToReportContent = true,
                    Width = Unit.Percentage(100),
                    Height = Unit.Percentage(100)
                };
                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.DataSources.Add(new ReportDataSource("DSSaleInvoice", dt));
                rv.LocalReport.EnableExternalImages = true;
                Warning[] warnings;
                string[] streamIds;
                string mimeType, encoding, filenameExtension;
                byte[] pdfBytes = rv.LocalReport.Render(
                    format: "PDF",
                    deviceInfo: null,
                    mimeType: out mimeType,
                    encoding: out encoding,
                    fileNameExtension: out filenameExtension,
                    streams: out streamIds,
                    warnings: out warnings);
                string fileName = $"Invoice_{DateTime.Now.Ticks}.pdf";
                string filePath = Server.MapPath("~/pdf/") + fileName;
                System.IO.File.WriteAllBytes(filePath, pdfBytes);
                return Content(Url.Content($"~/pdf/{fileName}"));
            }
            catch (Exception ex)
            {
                return View("Error", new HandleErrorInfo(ex, "ExportToPDF", "InvoiceReport"));
            }
        }

        /* public ActionResult ExportToPDF(string id)
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
                 dt.Columns.Add(new DataColumn("SaleImage", typeof(string)));
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
                             dr["CustomerId"] = cus.CustomerName;
                             dr["Date"] = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                             dr["Id"] = ii.Id;
                             dr["ProductId"] = proname.ProductName;
                             dr["Quantity"] = float.Parse((ii.Quantity).ToString());
                             dr["Price"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                             dr["Total"] = float.Parse((ii.actual_price * ii.Quantity).ToString()).ToString("N");
                             dr["Totals"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                             dr["VAT"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                             dr["Phone"] = cus.PhoneNumber;
                             dr["Address"] = cus.ProjectLocation;
                             dr["Discount"] = cuss.Discount;
                             dr["RevicedFromCustomer"] = float.Parse((cuss.RevicedFromCustomer).ToString()).ToString("N");
                             dr["Owe"] = cuss.Amount - (((cuss.Amount * cuss.Discount) / 100) + cuss.RevicedFromCustomer);
                             dr["InvoiceNo"] = cuss.InvoiceNo;
                             dr["ColorCode"] = ii.color_code;
                             dr["UnitId"] = unit.UnitNameEng;
                             dr["SaleImage"] = cuss.SaleImage;
                             dt.Rows.Add(dr);

                         }

                     }

                 }
                 rv.ProcessingMode = ProcessingMode.Local;
                 //reportViewer.LocalReport.ReportPath = Server.MapPath(@"~\Invoice - Copy.rdlc");
                 rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
                 //rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\OweMoreThan.rdlc";
                 rv.LocalReport.DataSources.Clear();
                 ReportDataSource rds = new ReportDataSource("DSSaleInvoice", dt);
                 rv.LocalReport.DataSources.Add(rds);
                 rv.ShowPrintButton = true;
                 ViewBag.ReportViewer = rv;

                 Warning[] warnings;
                 string[] streamids;
                 //string deviceInfo = @"<DeviceInfo>              
                 // <OutputFormat>PDF</OutputFormat>              
                 // <PageWidth>8.27in</PageWidth>              
                 // <PageHeight>11.69in</PageHeight>          
                 // <MarginTop>0.45in</MarginTop>          
                 // <MarginLeft>0.45in</MarginLeft>            
                 // <MarginRight>0.45in</MarginRight>       
                 // <MarginBottom>0.45in</MarginBottom></DeviceInfo>";
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
         }*/
        public static void ReadError()
        {
            string strPath = @"D:\Rekha\Log.txt";
            using (StreamReader sr = new StreamReader(strPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
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
        public ActionResult ExportToPDFcopy(string id)
        {
            //id = "07f3b5ec-f01f-4d41-a48c-57597b247754";
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
            db.SaveChanges();

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

                        DataRow dr = dt.NewRow();
                        dr["CustomerId"] = cus.CustomerName;
                        dr["Date"] = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                        dr["Id"] = ii.Id;
                        dr["ProductId"] = proname.ProductName;
                        dr["Quantity"] = float.Parse((ii.Quantity).ToString());
                        dr["Price"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                        dr["Total"] = float.Parse((ii.actual_price * ii.Quantity).ToString()).ToString("N");
                        dr["Totals"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                        dr["VAT"] = float.Parse((ii.actual_price).ToString()).ToString("N");
                        dr["Phone"] = cus.PhoneNumber;
                        dr["Address"] = cus.ProjectLocation;
                        dr["Discount"] = cuss.Discount;
                        dr["RevicedFromCustomer"] = float.Parse((cuss.RevicedFromCustomer).ToString()).ToString("N");
                        dr["Owe"] = cuss.Amount - (((cuss.Amount * cuss.Discount) / 100) + cuss.RevicedFromCustomer);
                        dr["InvoiceNo"] = cuss.InvoiceNo;
                        dr["ColorCode"] = ii.color_code;
                        dt.Rows.Add(dr);

                    }

                }

            }
            ReportViewer rv = new ReportViewer();

            rv.ProcessingMode = ProcessingMode.Local;
            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\Invoice.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("DSSaleInvoice", dt);
            rv.LocalReport.DataSources.Add(rds);
            rv.ShowPrintButton = true;
            ViewBag.ReportViewer = rv;

            Warning[] warnings;
            string[] streamids;
            string deviceInfo = @"<DeviceInfo>              
                 <OutputFormat>PDF</OutputFormat>              
                 <PageWidth>8.5in</PageWidth>              
                 <PageHeight>11in</PageHeight>          
                 <MarginTop>0.25in</MarginTop>          
                 <MarginLeft>0.45in</MarginLeft>            
                 <MarginRight>0.45in</MarginRight>       
                 <MarginBottom>0.25in</MarginBottom></DeviceInfo>";
            string mimeType, encoding, filenameExtension;

            byte[] bytess = rv.LocalReport.Render("Pdf", deviceInfo, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
            //File  
            string FileName = "Invoice_" + DateTime.Now.Ticks.ToString() + ".pdf";
            string FilePath = HttpContext.Server.MapPath(@"~\pdf\") + FileName;

            //create and set PdfReader  
            PdfReader reader = new PdfReader(bytess);
            //FileStream output = new FileStream(FilePath, FileMode.Create);

            var doc = new iTextSharp.text.Document();
            using (FileStream fs = new FileStream(FilePath, FileMode.Create))
            {
                PdfStamper stamper = new PdfStamper(reader, fs);
                string Printer = "Xerox Phaser 3635MFP PCL6";
                // This is the script for automatically printing the pdf in acrobat viewer
                stamper.JavaScript = "var pp = getPrintParams();pp.interactive =pp.constants.interactionLevel.automatic; pp.printerName = " +
                               Printer + ";print(pp);\r";
                stamper.Close();
            }
            reader.Close();
            FileStream fss = new FileStream(FilePath, FileMode.Open);
            byte[] bytes = new byte[fss.Length];
            fss.Read(bytes, 0, Convert.ToInt32(fss.Length));
            fss.Close();
            System.IO.File.Delete(FilePath);

            //Here we returns the file result for view(PDF)
            ModelState.Clear();
            Session.Clear(); //Clears the session variable for reuse 
            return File(bytes, "application/pdf");

        }
        public void SaveSalePayment(tblSale sale)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                tbSalePayment salePayment = new tbSalePayment();
                salePayment.sale_payment_id = Guid.NewGuid().ToString();
                salePayment.sale_id = sale.Id;
                salePayment.payment_amount = Convert.ToDecimal(sale.RevicedFromCustomer);
                salePayment.owe_amount = Convert.ToDecimal(sale.Owe);
                salePayment.created_at = CommonFunctions.ToLocalTime(DateTime.Now);
                salePayment.created_by = User.Identity.GetUserId();
                salePayment.payment_amount_aba = sale.ReceivedByABA;
                db.tbSalePayments.Add(salePayment);
                db.SaveChanges();
            }
        }

        public ActionResult MigrateSaleCost()
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                var sales = db.tblSales.Where(s => s.Status == 1 && s.SaleCost == null).ToList();
                //var sales = db.tblSales.Where(s => s.Status == 1 && s.SaleDeposit != null).ToList();
                foreach (var sale in sales)
                {
                    decimal totalCostGoodSold = 0;
                    decimal totalDepositCoGs = 0;
                    decimal totalReceivedCostSale = 0;
                    decimal percentageCashRecived = sale.SaleDeposit > 0 ? Convert.ToDecimal((sale.SaleDeposit / sale.SaleAmount) * 100) : 0;
                    var saleDetails = db.tblSalesDetails.Where(s => string.Compare(s.SaleId, sale.Id) == 0).ToList();
                    foreach (var sd in saleDetails)
                    {
                        decimal productCostGoodSold = 0;
                        totalCostGoodSold = totalCostGoodSold + Convert.ToDecimal(sd.cost * sd.Quantity);
                        if (sale.SaleDeposit > 0)
                        {
                            totalDepositCoGs = totalDepositCoGs + totalCostGoodSold;
                        }
                        productCostGoodSold = Convert.ToDecimal(sd.cost);

                        var receivedAmountByItem = ((sd.actual_price * sd.Quantity) * percentageCashRecived) / 100;
                        var receivedSoldQtyByItem = sd.actual_price > 0 ? receivedAmountByItem / sd.actual_price : 0;
                        var receivedCostPerItem = receivedSoldQtyByItem * productCostGoodSold;
                        totalReceivedCostSale = totalReceivedCostSale + Convert.ToDecimal(receivedCostPerItem);
                    }
                    decimal profit = Convert.ToDecimal(sale.SaleDeposit) - Convert.ToDecimal(totalReceivedCostSale);

                    updateSaleProfit(sale.Id, totalReceivedCostSale, profit);
                }

            }
            catch (Exception ex)
            {

            }
            return View();
        }
        public ActionResult SynceProductStockbySaleProductId(string id)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                CommonFunctions functions = new CommonFunctions();

                var saleDetails = (from sd in db.tblSalesDetails
                                   join s in db.tblSales on sd.SaleId equals s.Id
                                   orderby s.CreatedDate
                                   where string.Compare(sd.ProductId, id) == 0 && s.Status == 1

                                   select new { sd, s }).ToList();

                foreach (var sd in saleDetails)
                {
                    functions.stock_adjustment(sd.sd.ProductId, Convert.ToDecimal(sd.sd.Quantity), sd.sd.UnitTypeId);
                }

            }
            catch (Exception ex)
            {

            }
            return View();
        }
        public ActionResult ImportExcel()
        {
            return View();
        }
        // Import
        [HttpPost]
        public ActionResult ImportExcel(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                // Save images to the server
                var saveDirectory = Server.MapPath("~/Images");
                Directory.CreateDirectory(saveDirectory);

                using (var memoryStream = new MemoryStream())
                {
                    file.InputStream.CopyTo(memoryStream);
                    var result = Utility.ProcessExcelWithImages<ExcelData>(memoryStream, saveDirectory);
                    DataTable dataTable = result.Item1;
                    List<ExcelData> dataList = result.Item2;

                    // Get the most recent image if it exists
                    var latestImagePath = Directory.GetFiles(saveDirectory, "*.png")
                        .OrderByDescending(f => System.IO.File.GetCreationTime(f))
                        .Select(f => "/Images/" + Path.GetFileName(f))
                        .FirstOrDefault();
                    if (!string.IsNullOrEmpty(latestImagePath))
                    {
                        foreach (var data in dataList)
                        {
                            if (string.IsNullOrEmpty(data.ImagePath))
                            {
                                data.ImagePath = latestImagePath;
                            }
                        }
                    }

                    // Save the imported data to the database
                    using (jotunDBEntities db = new jotunDBEntities())
                    {
                        // Validate the DataTable structure
                        if (dataTable.Rows.Count > 9 && dataTable.Columns.Count > 4)
                        {
                            string name = dataTable.Rows[3][4]?.ToString().Trim();
                            string customer = dataTable.Rows[2][4]?.ToString().Trim();
                            string category = dataTable.Rows[4][4]?.ToString().Trim();
                            string color = dataTable.Rows[5][4]?.ToString().Trim();
                            string finishWeight = dataTable.Rows[6][4]?.ToString() ?? "0";
                            string labourCost = dataTable.Rows[7][4]?.ToString() ?? "0";
                            string labourDesignCost = dataTable.Rows[8][4]?.ToString() ?? "0";
                            string weight = dataTable.Rows[9][4]?.ToString() ?? "0";

                            var existingJeweler = db.tblJewelers.FirstOrDefault(J => J.Name.Trim() == name);
                            string jewelerId = existingJeweler?.JewelerId ?? Guid.NewGuid().ToString();
                            foreach (var record in dataList)
                            {
                                record.JewelerId = jewelerId;

                            }
                            var existingJewelerIds = db.tblJewelers.Select(j => j.JewelerId).ToHashSet();
                            var existingSaleLabours = db.tblSaleLabours
                                .Where(s => s.CustomerId != null)
                                .Select(s => new { s.JewelerId, s.CustomerId })
                                .ToHashSet();

                            var newRecords = dataList
                                .Where(record => !existingSaleLabours.Any(s => s.JewelerId == record.JewelerId && s.CustomerId == record.CustomerId))
                                .ToList();
                            if (existingJeweler != null)
                            {
                                jewelerId = existingJeweler.JewelerId;
                            }
                            else
                            {
                                var jewelersToAdd = new List<tblJeweler>
                                {
                                    new tblJeweler
                                    {
                                        JewelerId = jewelerId,
                                        Name = name,
                                        IsActive = true,
                                        CreatedBy = User?.Identity?.Name ?? "System",
                                        CreatedAt = DateTime.Now,
                                    }
                                };
                                db.tblJewelers.AddRange(jewelersToAdd);
                                db.SaveChanges();
                            }
                            var existingCustomer = db.tblCustomers.FirstOrDefault(c => c.CustomerName.Trim() == customer);
                            string customerId;

                            if (existingCustomer != null)
                            {
                                customerId = existingCustomer.Id;
                            }
                            else
                            {
                                var newCustomer = new tblCustomer
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    CustomerName = customer,
                                    Status = 1,
                                    CreatedDate = DateTime.Now,
                                };

                                db.tblCustomers.Add(newCustomer);
                                db.SaveChanges();
                                customerId = newCustomer.Id;
                            }
                            var saleLaboursToAdd = newRecords
                                .GroupBy(record => record.JewelerId)
                                .Select(group => new tblSaleLabour
                                {
                                    CustomerId = customerId,
                                    JewelerId = group.Key,
                                    Category = category,
                                    Color = color,
                                    Weight = weight,
                                    FinishWeight = finishWeight,
                                    LabourCost = labourCost,
                                    LabourDesignCost = labourDesignCost,
                                    Length = group.First().Length,
                                    Size = group.First().Size,
                                    IsActive = true,
                                    CreatedBy = User?.Identity?.Name ?? "System",
                                    CreatedAt = DateTime.Now,
                                    ImagePath = string.IsNullOrEmpty(group.First().ImagePath) ? null : group.First().ImagePath
                                }).ToList();
                            db.tblSaleLabours.AddRange(saleLaboursToAdd);
                            db.SaveChanges();

                            var saleLabourIds = saleLaboursToAdd
                                .Select(sl => sl.Id)
                                .ToList();

                            var saleLabourDetailsToAdd = newRecords
                                .GroupBy(record => record.JewelerId)
                                .SelectMany(group =>
                                {
                                    var detailsList = group.ToList();
                                    var resultList = new List<tblSaleLabourDetail>();
                                    bool? isCustomerFlag = false;

                                    (string size, string price) ExtractSizeAndPrice(string columnValue)
                                    {
                                        if (string.IsNullOrEmpty(columnValue) || !columnValue.Contains("="))
                                            return (string.Empty, string.Empty);

                                        var parts = columnValue.Split('=');
                                        string size = parts[0].Trim();
                                        string price = new string(parts[1].Trim().Where(char.IsDigit).ToArray());
                                        return (size, price);
                                    }

                                        for (int i = 0; i < detailsList.Count; i++)
                                        {
                                            var record = detailsList[i];
                                            string column1Value = record.Seeds?.Trim() ?? string.Empty;
                                            string column2Value = record.HeSeeds?.Trim() ?? string.Empty;
                                            if (string.IsNullOrWhiteSpace(column1Value) && string.IsNullOrWhiteSpace(column2Value))
                                            {
                                                continue;
                                            }
                                            if (column1Value == "ទំហំ​នឺងចំនួនពេជ្រ" && column2Value == "ទំហំ​នឺងចំនួនពេជ្រ")
                                            {
                                                continue;
                                            }
                                        if (column1Value == "គ្រាប់ដាំ" && column2Value == "គ្រាប់គាត់")
                                        {
                                            if (i + 1 < detailsList.Count)
                                            {
                                                var nextRow = detailsList[i + 1];

                                                // Check next row for values in Seeds and HeSeeds
                                                bool hasSeeds = !string.IsNullOrWhiteSpace(nextRow.Seeds?.Trim());
                                                bool hasHeSeeds = !string.IsNullOrWhiteSpace(nextRow.HeSeeds?.Trim());

                                                // Prioritize Seeds over HeSeeds
                                                if (hasSeeds)
                                                {
                                                    isCustomerFlag = false;
                                                }
                                                else if (hasHeSeeds)
                                                {
                                                    isCustomerFlag = true;
                                                }
                                            }
                                            continue;
                                        }
                                        var (size1, priceFromColumn1) = ExtractSizeAndPrice(column1Value);
                                        var (size2, priceFromColumn2) = ExtractSizeAndPrice(column2Value);

                                        string finalPrice = !string.IsNullOrEmpty(priceFromColumn1)
                                         ? priceFromColumn1
                                         : priceFromColumn2;
                                        var saleLabourDetail = new tblSaleLabourDetail
                                        {
                                            SaleLabourId = saleLabourIds.FirstOrDefault(),
                                            ProductName = string.IsNullOrEmpty(size1) ? column1Value : size1,
                                            ProductNameHe = string.IsNullOrEmpty(size2) ? column2Value : size2,
                                            Qty = 2,
                                            Price = decimal.TryParse(finalPrice, out decimal parsedPrice) ? parsedPrice : 0,
                                            IsCustomer = isCustomerFlag 
                                        };

                                        resultList.Add(saleLabourDetail);

                                    }

                                    return resultList;
                                }).ToList();


                            db.tblSaleLabourDetails.AddRange(saleLabourDetailsToAdd);
                            db.SaveChanges();

                        }
                        else
                        {
                            throw new Exception("DataTable does not have the required number of rows or columns.");
                        }
                    }
                    ViewBag.Data = dataTable;
                    ViewBag.DataList = dataList;
                    ViewBag.Images = latestImagePath;

                    return View();
                }
            }
            return View();
        }
        // Read
         public ActionResult ListDataLD()
         {
             try
             {
                 using (var db = new jotunDBEntities())
                 {
                     var dataList = (from detail in db.tblSaleLabourDetails
                                     join saleLabour in db.tblSaleLabours on detail.SaleLabourId equals saleLabour.Id
                                     join customer in db.tblCustomers on saleLabour.CustomerId equals customer.Id
                                     join jeweler in db.tblJewelers on saleLabour.JewelerId equals jeweler.JewelerId
                                     select new ExcelData
                                     {
                                         ProductName = detail.ProductName,
                                         ProductNameHe = detail.ProductNameHe,
                                         Price = (int)detail.Price,
                                         Category = saleLabour.Category,
                                         Color = saleLabour.Color,
                                         FinishWeight = saleLabour.FinishWeight,
                                         Weight = saleLabour.Weight,
                                         LabourCost = saleLabour.LabourCost,
                                         LabourDesignCost = saleLabour.LabourDesignCost,
                                         JewelerName = jeweler.Name,
                                         ImagePath = saleLabour.ImagePath,
                                         JewelerId = jeweler.JewelerId,
                                         CustomerId = customer.Id.ToString(),
                                         Id = saleLabour.Id.ToString(),
                                         Date = saleLabour.CreatedAt,
                                         CustomerName = customer.CustomerName,
                                         IsCustomer = (bool)detail.IsCustomer,
                                     }).ToList();
                     var formattedList = dataList
                         .GroupBy(item => new
                         {
                             item.ProductName,
                             item.ProductNameHe,
                             item.CustomerId,
                             item.JewelerName,
                             item.Category,
                             item.Color,
                             item.Weight,
                             item.LabourCost,
                             item.LabourDesignCost,
                             item.FinishWeight,
                             item.CustomerName,
                             item.Id,
                             item.ImagePath,
                             item.JewelerId,
                             item.Date,
                             item.IsCustomer,
                         })
                         .Select(group => group.First())
                         .Select(item => new
                         {
                             item.IsCustomer,
                             Seeds = item.IsCustomer == false ? $"{item.ProductName}={item.Price}p" : $"{item.ProductName}",
                             HeSeeds = item.IsCustomer == true ? $"{item.ProductNameHe}={item.Price}p" : $"{item.ProductNameHe}",
                             Date = item.Date.HasValue ? item.Date.Value.ToString("yyyy-MM-dd") : "N/A",
                             CustomerId = item.CustomerId,
                             Id= item.Id,
                             JewelerName = item.JewelerName,
                             Category = item.Category,
                             Color = item.Color,
                             Weight = item.Weight,
                             LabourCost = item.LabourCost,
                             LabourDesignCost = item.LabourDesignCost,
                             FinishWeight = item.FinishWeight,
                             CustomerName = item.CustomerName,
                             ImagePath = item.ImagePath,
                             JewelerId = item.JewelerId
                         })
                         .ToList();
                     string jsonData = JsonConvert.SerializeObject(formattedList);
                     ViewBag.DataList = jsonData;
                 }
             }
             catch (Exception ex)
             {
                 ViewBag.Message = "Error occurred while retrieving data from the database: " + ex.Message;
             }

             return View();
         }
        // Search
        public ActionResult SearchListDataLD(string searchCustomerName, string searchJewelerName, string searchDate)
        {
            try
            {
                using (var db = new jotunDBEntities())
                {
                    var dataListQuery = from detail in db.tblSaleLabourDetails
                                        join saleLabour in db.tblSaleLabours on detail.SaleLabourId equals saleLabour.Id
                                        join customer in db.tblCustomers on saleLabour.CustomerId equals customer.Id
                                        join jeweler in db.tblJewelers on saleLabour.JewelerId equals jeweler.JewelerId
                                        select new
                                        {
                                            ProductName = detail.ProductName,
                                            ProductNameHe = detail.ProductNameHe,
                                            Price = (int)detail.Price,
                                            Category = saleLabour.Category,
                                            Color = saleLabour.Color,
                                            FinishWeight = saleLabour.FinishWeight,
                                            Weight = saleLabour.Weight,
                                            LabourCost = saleLabour.LabourCost,
                                            LabourDesignCost = saleLabour.LabourDesignCost,
                                            JewelerName = jeweler.Name,
                                            ImagePath = saleLabour.ImagePath,
                                            JewelerId = jeweler.JewelerId,
                                            CustomerId = customer.Id.ToString(),
                                            Id = saleLabour.Id.ToString(),
                                            Date = saleLabour.CreatedAt,
                                            CustomerName = customer.CustomerName,
                                            IsCustomer = (bool)detail.IsCustomer,
                                        };
                    if (!string.IsNullOrEmpty(searchCustomerName))
                    {
                        dataListQuery = dataListQuery.Where(x => x.CustomerName.Contains(searchCustomerName));
                    }
                    if (!string.IsNullOrEmpty(searchJewelerName))
                    {
                        dataListQuery = dataListQuery.Where(x => x.JewelerName.Contains(searchJewelerName));
                    }
                    if (!string.IsNullOrEmpty(searchDate))
                    {
                        DateTime date = DateTime.Parse(searchDate);
                        DateTime startOfDay = date.Date; 
                        DateTime endOfDay = date.Date.AddDays(1); 
                        dataListQuery = dataListQuery.Where(x => x.Date.HasValue && x.Date.Value >= startOfDay && x.Date.Value < endOfDay);
                    }
                    var dataList = dataListQuery.ToList();

                    var formattedList = dataList
                        .GroupBy(item => new
                        {
                            item.ProductName,
                            item.ProductNameHe,
                            item.CustomerId,
                            item.Id,
                            item.JewelerName,
                            item.Category,
                            item.Color,
                            item.Weight,
                            item.LabourCost,
                            item.LabourDesignCost,
                            item.FinishWeight,
                            item.CustomerName,
                            item.ImagePath,
                            item.JewelerId,
                            item.Date,
                            item.IsCustomer
                        })
                        .Select(group => group.First())
                        .Select(item => new
                        {
                            item.IsCustomer,
                            Seeds = item.IsCustomer == false ? $"{item.ProductName}={item.Price}p" : $"{item.ProductName}",
                            HeSeeds = item.IsCustomer == true ? $"{item.ProductNameHe}={item.Price}p" : $"{item.ProductNameHe}",
                            Date = item.Date.HasValue ? item.Date.Value.ToString("yyyy-MM-dd") : "N/A",
                            CustomerId = item.CustomerId,
                            JewelerName = item.JewelerName,
                            Category = item.Category,
                            Color = item.Color,
                            Weight = item.Weight,
                            LabourCost = item.LabourCost,
                            LabourDesignCost = item.LabourDesignCost,
                            FinishWeight = item.FinishWeight,
                            CustomerName = item.CustomerName,
                            ImagePath = item.ImagePath,
                            JewelerId = item.JewelerId,
                            Id= item.Id,
                        })
                        .ToList();
                    return Json(formattedList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        //Detail
        [HttpGet]
        public ActionResult DetailSaleLabour(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound("CustomerId is required.");
            }
            using (var db = new jotunDBEntities())
            {

                var saleLabourDetails = (from saleLabour in db.tblSaleLabours
                                         join detail in db.tblSaleLabourDetails on saleLabour.Id equals detail.SaleLabourId
                                         join customer in db.tblCustomers on saleLabour.CustomerId equals customer.Id
                                         join jeweler in db.tblJewelers on saleLabour.JewelerId equals jeweler.JewelerId
                                         where customer.Id == id
                                         select new
                                         {
                                             CustomerId = customer.Id,
                                             JewelerId = jeweler.JewelerId,
                                             SaleLabourId = saleLabour.Id,
                                             Category = saleLabour.Category,
                                             Color = saleLabour.Color,
                                             FinishWeight = saleLabour.FinishWeight,
                                             Weight = saleLabour.Weight,
                                             LabourCost = saleLabour.LabourCost,
                                             LabourDesignCost = saleLabour.LabourDesignCost,
                                             JewelerName = jeweler.Name,
                                             CustomerName = customer.CustomerName,
                                             ImagePath = saleLabour.ImagePath,
                                             ProductName = detail.ProductName,
                                             ProductNameHe = detail.ProductNameHe,
                                             Price = detail.Price,
                                             SaleLabourDetailId = detail.SaleLabourDetailId
                                         }).ToList();

                var groupedData = saleLabourDetails
                    .GroupBy(x => x.SaleLabourId)
                    .Select(group => new SaleLabourViewModel
                    {
                        SaleLabourId = group.Key,
                        CustomerId = group.First().CustomerId,
                        JewelerId = group.First().JewelerId,
                        Category = group.First().Category,
                        Color = group.First().Color,
                        FinishWeight = ParseDecimal(group.First().FinishWeight),
                        Weight = ParseDecimal(group.First().Weight),
                        LabourCost = ParseDecimal(group.First().LabourCost),
                        LabourDesignCost = ParseDecimal(group.First().LabourDesignCost),
                        JewelerName = group.First().JewelerName,
                        CustomerName = group.First().CustomerName,
                        ImagePath = group.First().ImagePath,
                        ProductDetails = group.Select(g => new ProductDetail
                        {
                            ProductName = g.ProductName ?? string.Empty,
                            ProductNameHe = g.ProductNameHe ?? string.Empty,
                            Price = g.Price ?? 0,
                            SaleLabourDetailId = g.SaleLabourDetailId
                        }).ToList()
                    }).ToList();

                if (!groupedData.Any())
                {
                    return HttpNotFound();
                }
                return View(groupedData);
            }
        }
        // Edit
        [HttpGet]
        public ActionResult EditSaleLabour(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound("CustomerId and SaleLabourId are required.");
            }
            var ids = id.Split(',');

            if (ids.Length != 2)
            {
                return HttpNotFound("Invalid format. Both CustomerId and SaleLabourId are required.");
            }
            string customerId = ids[0]; 
            if (!long.TryParse(ids[1], out long saleLabourId))
            {
                return HttpNotFound("Invalid SaleLabourId.");
            }

            using (var db = new jotunDBEntities())
            {
                var saleLabourDetails = (from saleLabour in db.tblSaleLabours
                                         join detail in db.tblSaleLabourDetails on saleLabour.Id equals detail.SaleLabourId
                                         join customer in db.tblCustomers on saleLabour.CustomerId equals customer.Id
                                         join jeweler in db.tblJewelers on saleLabour.JewelerId equals jeweler.JewelerId
                                         where customer.Id == customerId && saleLabour.Id == saleLabourId
                                         select new
                                         {
                                             CustomerId = customer.Id,
                                             JewelerId = jeweler.JewelerId,
                                             SaleLabourId = saleLabour.Id,
                                             Category = saleLabour.Category,
                                             Color = saleLabour.Color,
                                             FinishWeight = saleLabour.FinishWeight,
                                             Weight = saleLabour.Weight,
                                             LabourCost = saleLabour.LabourCost,
                                             LabourDesignCost = saleLabour.LabourDesignCost,
                                             JewelerName = jeweler.Name,
                                             CustomerName = customer.CustomerName,
                                             ImagePath = saleLabour.ImagePath,
                                             ProductName = detail.ProductName,
                                             ProductNameHe = detail.ProductNameHe,
                                             Price = detail.Price,
                                             SaleLabourDetailId = detail.SaleLabourDetailId
                                         }).ToList();

                if (!saleLabourDetails.Any())
                {
                    return HttpNotFound("No matching data found for the provided CustomerId and SaleLabourId.");
                }

                var groupedData = saleLabourDetails
                    .GroupBy(x => x.SaleLabourId)
                    .Select(group => new SaleLabourViewModel
                    {
                        SaleLabourId = group.Key,
                        CustomerId = group.First().CustomerId,
                        JewelerId = group.First().JewelerId,
                        Category = group.First().Category,
                        Color = group.First().Color,
                        FinishWeight = ParseDecimal(group.First().FinishWeight),
                        Weight = ParseDecimal(group.First().Weight),
                        LabourCost = ParseDecimal(group.First().LabourCost),
                        LabourDesignCost = ParseDecimal(group.First().LabourDesignCost),
                        JewelerName = group.First().JewelerName,
                        CustomerName = group.First().CustomerName,
                        ImagePath = group.First().ImagePath,
                        ProductDetails = group.Select(g => new ProductDetail
                        {
                            ProductName = g.ProductName ?? string.Empty,
                            ProductNameHe = g.ProductNameHe ?? string.Empty,
                            Price = g.Price ?? 0,
                            SaleLabourDetailId = g.SaleLabourDetailId
                        }).ToList()
                    }).FirstOrDefault();

                return View(new List<SaleLabourViewModel> { groupedData });
            }
        }
        private decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            var regex = new Regex(@"\(([^)]+)\)"); 
            var matches = regex.Matches(input);

            decimal expressionResult = 0;

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var expression = match.Groups[1].Value;
                    try
                    {
                        expressionResult = EvaluateExpression(expression);
                        input = input.Replace(match.Value, expressionResult.ToString());
                    }
                    catch (Exception)
                    {
                      
                        expressionResult = 0;
                    }
                }
            }
            string cleanedInput = new string(input
                .Where(c => char.IsDigit(c) || c == '.' || c == ',') 
                .ToArray());
            if (string.IsNullOrWhiteSpace(cleanedInput))
                return 0;
            cleanedInput = cleanedInput.Replace(',', '.');
            return decimal.TryParse(cleanedInput, out var result) ? result : 0;
        }
        private decimal EvaluateExpression(string expression)
        {
            var result = new System.Data.DataTable().Compute(expression, null);
            return Convert.ToDecimal(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSaleLabour(List<SaleLabourViewModel> model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new jotunDBEntities())
                {
                    foreach (var saleLabourViewModel in model)
                    {
                        var saleLabour = db.tblSaleLabours.FirstOrDefault(sl => sl.Id == saleLabourViewModel.SaleLabourId);
                        if (saleLabour != null)
                        {
                            saleLabour.Weight = saleLabourViewModel.Weight.ToString();
                            saleLabour.Category = saleLabourViewModel.Category;
                            saleLabour.Color = saleLabourViewModel.Color;
                            saleLabour.FinishWeight = saleLabourViewModel.FinishWeight.ToString();
                            saleLabour.LabourCost = saleLabourViewModel.LabourCost.ToString();
                            saleLabour.LabourDesignCost = saleLabourViewModel.LabourDesignCost.ToString();
                            saleLabour.ImagePath = saleLabourViewModel.ImagePath;
                            saleLabour.UpdatedAt = DateTime.Now;
                            saleLabour.UpdatedBy = User?.Identity?.Name ?? "System";
                           
                            foreach (var productDetail in saleLabourViewModel.ProductDetails)
                            {
                                var saleLabourDetail = db.tblSaleLabourDetails.FirstOrDefault(sld => sld.SaleLabourDetailId == productDetail.SaleLabourDetailId);
                                if (saleLabourDetail != null)
                                {
                                    saleLabourDetail.ProductName = productDetail.ProductName;
                                    saleLabourDetail.ProductNameHe = productDetail.ProductNameHe;
                                    saleLabourDetail.Price = productDetail.Price;
                                }
                            }

                           
                            var customer = db.tblCustomers.FirstOrDefault(c => c.Id == saleLabourViewModel.CustomerId);
                            if (customer != null)
                            {
                                customer.CustomerName = saleLabourViewModel.CustomerName;
                            }

                           
                            var jeweler = db.tblJewelers.FirstOrDefault(j => j.JewelerId == saleLabourViewModel.JewelerId);
                            if (jeweler != null)
                            {
                                jeweler.Name = saleLabourViewModel.JewelerName;
                                jeweler.UpdatedAt = DateTime.Now;
                                jeweler.UpdatedBy = User?.Identity?.Name ?? "System";
                            }
                        }
                        else
                        {
                            return HttpNotFound($"Sale Labour record with ID {saleLabourViewModel.SaleLabourId} not found.");
                        }
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("ListDataLD");
            }
            return View(model);
        }
        [HttpGet]
        public ActionResult DeleteSaleLabour(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound("CustomerId and SaleLabourId are required.");
            }
            var ids = id.Split(',');

            if (ids.Length != 2)
            {
                return HttpNotFound("Invalid format. Both CustomerId and SaleLabourId are required.");
            }
            string customerId = ids[0];
            long saleLabourId;

            if (!long.TryParse(ids[1], out saleLabourId))
            {
                return HttpNotFound("Invalid SaleLabourId.");
            }

            using (var db = new jotunDBEntities())
            {
                var saleLabourDetails = (from saleLabour in db.tblSaleLabours
                                         join detail in db.tblSaleLabourDetails on saleLabour.Id equals detail.SaleLabourId
                                         join customer in db.tblCustomers on saleLabour.CustomerId equals customer.Id
                                         join jeweler in db.tblJewelers on saleLabour.JewelerId equals jeweler.JewelerId
                                         where customer.Id == customerId && saleLabour.Id == saleLabourId
                                         select new
                                         {
                                             CustomerId = customer.Id,
                                             JewelerId = jeweler.JewelerId,
                                             SaleLabourId = saleLabour.Id,
                                             Category = saleLabour.Category,
                                             Color = saleLabour.Color,
                                             FinishWeight = saleLabour.FinishWeight,
                                             Weight = saleLabour.Weight,
                                             LabourCost = saleLabour.LabourCost,
                                             LabourDesignCost = saleLabour.LabourDesignCost,
                                             JewelerName = jeweler.Name,
                                             CustomerName = customer.CustomerName,
                                             ImagePath = saleLabour.ImagePath,
                                             ProductName = detail.ProductName,
                                             ProductNameHe = detail.ProductNameHe,
                                             Price = detail.Price,
                                             SaleLabourDetailId = detail.SaleLabourDetailId
                                         }).ToList();

                if (!saleLabourDetails.Any())
                {
                    return HttpNotFound("No matching data found for the provided CustomerId and SaleLabourId.");
                }

                var groupedData = saleLabourDetails
                    .GroupBy(x => x.SaleLabourId)
                    .Select(group => new SaleLabourViewModel
                    {
                        SaleLabourId = group.Key,
                        CustomerId = group.First().CustomerId,
                        JewelerId = group.First().JewelerId,
                        Category = group.First().Category,
                        Color = group.First().Color,
                        FinishWeight = ParseDecimal(group.First().FinishWeight),
                        Weight = ParseDecimal(group.First().Weight),
                        LabourCost = ParseDecimal(group.First().LabourCost),
                        LabourDesignCost = ParseDecimal(group.First().LabourDesignCost),
                        JewelerName = group.First().JewelerName,
                        CustomerName = group.First().CustomerName,
                        ImagePath = group.First().ImagePath,
                        ProductDetails = group.Select(g => new ProductDetail
                        {
                            ProductName = g.ProductName ?? string.Empty,
                            ProductNameHe = g.ProductNameHe ?? string.Empty,
                            Price = g.Price ?? 0,
                            SaleLabourDetailId = g.SaleLabourDetailId
                        }).ToList()
                    }).FirstOrDefault();

                return View(new List<SaleLabourViewModel> { groupedData });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSaleLabour(List<SaleLabourViewModel> model)
        {
            if (model == null || model.Count == 0 || string.IsNullOrEmpty(model[0].CustomerId))
            {
                return Json(new { success = false, message = "CustomerId is required." });
            }

            using (var db = new jotunDBEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var customerId = model[0].CustomerId; 
                        var saleLabour = db.tblSaleLabours.FirstOrDefault(s => s.CustomerId.ToString() == customerId);

                        if (saleLabour == null)
                        {
                            return Json(new { success = false, message = $"Sale Labour for Customer with ID {customerId} not found." });
                        }
                        var relatedDetails = db.tblSaleLabourDetails.Where(d => d.SaleLabourId == saleLabour.Id).ToList();
                        db.tblSaleLabourDetails.RemoveRange(relatedDetails);
                        db.tblSaleLabours.Remove(saleLabour);
                        db.SaveChanges();
                        transaction.Commit();
                        return RedirectToAction("ListDataLD");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = $"Error deleting Sale Labour: {ex.Message}" });
                    }
                }
            }
        }
    }
}
