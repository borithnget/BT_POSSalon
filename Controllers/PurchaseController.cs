using iTextSharp.text.pdf;
using jotun.Entities;
using jotun.Functions;
using jotun.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace jotun.Controllers
{
    public class PurchaseController : Controller
    {
        jotunDBEntities db = new jotunDBEntities();
        CommonFunctions functions = new CommonFunctions();
        // GET: Purchase
        public ActionResult Index()
        {

            return View();
        }


        [HttpGet]
        public ActionResult Create(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {

                ViewBag.SupplierNames = new SelectList(db.tblSuppliers.Where(u => !u.Status.ToString().Contains("0"))
                                              .ToList(), "SupplierName", "SupplierName");

                ViewBag.ShipperName = new SelectList(db.tblShippers.Where(u => !u.Status.ToString().Contains("0"))
                                              .ToList(), "ShipperName", "ShipperName");

                ViewBag.UnitNamesEng = new SelectList(db.tblUnits.Where(u => !u.Status.ToString().Contains("0") && !u.UnitNameEng.Contains("Null"))
                                              .ToList(), "UnitNameEng", "UnitNameEng");

                
            };
            
            return View();
        }


        // POST: Client/Create
        [HttpPost]
        public ActionResult Create(tblPurchaseBySupplier client, tblPurchaseBySupplierViewModel model, ProductViewModels pmodel, tblProduct prod)
        {
            try
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var shipper = (from p in db.tblShippers
                                   where p.ShipperName == client.ShipperId
                                   select p).FirstOrDefault();

                    client.ShipperId = shipper.Id;

                    var supplier = (from p in db.tblSuppliers
                                    where p.SupplierName == client.SupplierId
                                    select p).FirstOrDefault();
                    client.SupplierId = supplier.Id;




                    client.Id = model.ID;
                    //client.SupplierId = pmodel.SupplierId;
                    //client.ShipperId = model.ShipperId;
                    client.Description = model.Description;
                    client.CreatedDate = DateTime.Now;
                    client.Status = 1;
                    client.Deposit = Convert.ToDecimal(model.Deposit);
                    client.PurchaseAmount = Convert.ToDecimal(model.PurchaseAmount);
                    client.ShipperAmount = Convert.ToDecimal(model.ShipperAmount);
                    client.Discount = model.Discount;
                    var owe = (client.PurchaseAmount + client.ShipperAmount) - client.Deposit;
                    if (owe <= 0)
                    {
                        client.PurchaseStatus = "Paid";
                    }
                    else
                    {
                        client.PurchaseStatus = "Not Paid";
                    }
                    db.tblPurchaseBySuppliers.Add(client);
                    db.SaveChanges();

                    if (model.GetDetail != null)
                    {
                        foreach (PurchaseViewModelDetail item in model.GetDetail)
                        {
                            var i = (from i1 in db.tblUnits
                                     where i1.UnitNameEng == item.UnitTypeId
                                     select i1).FirstOrDefault();
                            var pros = (from t in db.tblProductByUnits where t.TypeDefault == true && t.ProductID == item.ProductId select t.UnitTypeID).FirstOrDefault();
                            var unit = (from t in db.tblUnits where t.Id == pros select t.Quantity).FirstOrDefault();

                            tblPurchaseBySupplierDetail clientd = new tblPurchaseBySupplierDetail();
                            clientd.Id = Guid.NewGuid().ToString();
                            clientd.PurchaseBySupplierId = client.Id;
                            clientd.UnitTypeId = i.Id;
                            clientd.ProductId = item.ProductId;
                            clientd.Quantity = Convert.ToDecimal(item.Quantity);
                            clientd.Cost = Convert.ToDecimal(item.Cost);
                            clientd.Discount = Convert.ToDecimal(item.Discount);
                            db.tblPurchaseBySupplierDetails.Add(clientd);
                            db.SaveChanges();
                            tblProduct pp = db.tblProducts.Find(clientd.ProductId);
                            var product = (from pro in db.tblProducts
                                           where pro.Id == clientd.ProductId
                                           select pro
                            ).ToList();
                            foreach (var ii in product)
                            {
                                //var k = i.Quantity * clientd.Quantity;
                                //if (pp.QuantityInStockRetail == null )
                                //{
                                // pp.QuantityInStockRetail = 0;
                                //}
                                //// pp.QuantityInStock = i.QuantityInStock + clientd.Quantity;
                                ////var k = i.Quantity * clientd.Quantity;
                                ////pp.QuantityInStockRetail = (long)Convert.ToDecimal(pp.QuantityInStockRetail+ k) ;
                                ////pp.QuantityInStock = pp.QuantityInStockRetail / i.Quantity;
                                ////db.SaveChanges();
                                //if (pp.QuantityInStockRetail == 0)
                                //{
                                // var str = pp.QuantityInStock * unit;
                                // var newstock = (long)Convert.ToDecimal(str + k);
                                // pp.QuantityInStockRetail = newstock;
                                // pp.QuantityInStock = newstock / unit;
                                // db.SaveChanges();
                                //}
                                //else
                                //{
                                // var x = (long)Convert.ToDecimal(pp.QuantityInStockRetail + k);
                                // pp.QuantityInStockRetail = x;
                                // pp.QuantityInStock = pp.QuantityInStockRetail / unit;
                                // db.SaveChanges();
                                //}

                                functions.stock_purchase(ii.Id, Convert.ToDecimal(item.Quantity), i.Id);

                            }
                        }
                    }
                    tblPurchaseBySupplier s = db.tblPurchaseBySuppliers.Find(client.Id);
                    var ss = db.tblPurchaseBySupplierDetails.Where(ID => ID.PurchaseBySupplierId == client.Id).ToList();
                    var purchaseId = (from p in db.tblPurchaseBySuppliers where p.InvoiceNo != null && p.Status == 1 select p).ToList();
                    var maxid = (from x in db.tblPurchaseBySuppliers where x.InvoiceNo != null && x.Status == 1 select x.InvoiceId).Max();
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
                        foreach (var item in purchaseId)
                        {
                            if (item.InvoiceId == maxid)
                            {
                                string purchaseId1 = item.InvoiceNo;
                                string splits = purchaseId1;
                                string[] arraystring = splits.Split('-');
                                var j = arraystring[0];
                                var j1 = arraystring[1];

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
                    db.SaveChanges();
                    //return RedirectToAction("Index", "Purchase");

                    return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPurchase(tblPurchaseBySupplierViewModel model)
        {
            List<tblPurchaseBySupplierViewModel> models = new List<tblPurchaseBySupplierViewModel>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                //var productss = db.tblPurchaseBySuppliers.OrderBy(x => x.Id).ToList();
                var products = (from p in db.tblPurchaseBySuppliers
                                orderby p.CreatedDate descending
                                select p).ToList();
                foreach (var p in products)
                {
                    if (p.ShipperAmount == null)
                    {
                        p.ShipperAmount = 0;
                    }
                    if(p.Deposit == null)
                    {
                        p.Deposit = 0;
                    }
                    if (p.Discount == null)
                    {
                        p.Discount = 0;
                    }
                    if (p.Status == 1)
                    {
                        var shipper = (from p1 in db.tblShippers
                                       where p1.Id == p.ShipperId
                                       select p1).FirstOrDefault();
                        var supplier = (from p1 in db.tblSuppliers
                                        where p1.Id == p.SupplierId
                                        select p1).FirstOrDefault();

                        var date = p.UpdatedDate;
                        if (date == null) { date = p.CreatedDate; }
                        else { date = p.UpdatedDate; }

                        models.Add(new tblPurchaseBySupplierViewModel()
                        {
                            ID = p.Id,
                            ShipperId = shipper.ShipperName,
                            SupplierId = supplier.SupplierName,
                            Description = p.Description,
                            CreatedDate = p.CreatedDate,
                            ShipperAmount=p.ShipperAmount,
                            PurchaseAmount=p.PurchaseAmount,
                            PurchaseStatus = p.PurchaseStatus,
                            InvoiceNo = p.InvoiceNo,

                         //   Deposit = float.Parse(((decimal)p.Deposit).ToString()).ToString("0.000"),
                            Deposit = p.Deposit,


                            Discount =  p.Discount,
                            Owe = Convert.ToDouble(((p.PurchaseAmount + p.ShipperAmount)-p.Deposit)).ToString("N"),
                        });

                    }
                }

            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet]
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            return View(tblPurchaseBySupplierViewModel.GetNoDetail(id));
            

        }

        [HttpPost]
        public ActionResult Details(string id, tblPurchaseBySupplierViewModel model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                //if (!ModelState.IsValid) return View(model);

                tblPurchaseBySupplier client = db.tblPurchaseBySuppliers.Find(id);
                if (client != null)
                {
                    if (client.Deposit == null)
                    {
                        client.Deposit = 0;
                    }
                    client.Deposit = client.Deposit + Convert.ToDecimal(model.Deposit1);
                    // client.Owe = client.Owe - Convert.ToDecimal(model.RevicedFromCustomer);
                    var owe = (client.PurchaseAmount + client.ShipperAmount) - client.Deposit;
                    if (owe <= 0)
                    {
                        client.PurchaseStatus = "Paid";
                    }
                    else
                    {
                        client.PurchaseStatus = "Not Paid";
                    }

                    db.SaveChanges();


                }
                return RedirectToAction("Index", "Purchase");
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Edit(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {

                ViewBag.SupplierNames = new SelectList(db.tblSuppliers.Where(u => !u.Status.ToString().Contains("0"))
                                              .ToList(), "SupplierName", "SupplierName");

                ViewBag.ShipperNames = new SelectList(db.tblShippers.Where(u => !u.Status.ToString().Contains("0"))
                                              .ToList(), "ShipperName", "ShipperName");



            };
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            return View(tblPurchaseBySupplierViewModel.GetNoDetail(id));
        }

        // POST: Client/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, tblPurchaseBySupplierViewModel model)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                if (!ModelState.IsValid) return View(model);

                tblPurchaseBySupplier client = db.tblPurchaseBySuppliers.Find(id);
                if (client != null)
                {

                    var shipper = (from p in db.tblShippers
                                   where p.ShipperName == model.ShipperId
                                   select p).FirstOrDefault();
                    client.ShipperId = shipper.Id;

                    var supplier = (from p in db.tblSuppliers
                                    where p.SupplierName == model.SupplierId
                                    select p).FirstOrDefault();
                    client.SupplierId = supplier.Id;


                    client.Id = model.ID;
                    //client.ShipperId = model.ShipperId;
                    //client.SupplierId = model.SupplierId;
                    client.CreatedDate = model.CreatedDate;
                    client.Description = model.Description;
                    db.SaveChanges();

                    var GetDetail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();
                    if (GetDetail.Any())
                    {
                        foreach (var item in GetDetail)
                        {
                            tblPurchaseBySupplierDetail clientDetail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.Id, item.Id) == 0).FirstOrDefault();
                            clientDetail.Quantity = item.Quantity;
                            db.SaveChanges();
                            if (model.GetDetail != null)
                            {

                                var GetDetail2 = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();

                                foreach (var ii in GetDetail2)
                                {

                                    //tblProduct pp = db.tblProducts.Find(ii.ProductId);
                                    //var product = (from pro in db.tblProducts
                                    //               where pro.Id == ii.ProductId
                                    //               select pro
                                    //              ).ToList();
                                    //foreach (var i in product)
                                    //{
                                    //    var si = (from i1 in db.tblUnits
                                    //              where i1.Id == ii.UnitTypeId
                                    //              select i1).FirstOrDefault();

                                    //    // pp.QuantityInStock = i.QuantityInStock - clientd2.Quantity;
                                    //    var k = si.Quantity * ii.Quantity;
                                    //    pp.QuantityInStockRetail = (long)Convert.ToDecimal(pp.QuantityInStockRetail - k);
                                    //    pp.QuantityInStock = pp.QuantityInStockRetail / si.Quantity;
                                    //    db.SaveChanges();

                                    //}

                                    functions.revert_stock_pruchase(ii.Id);

                                    tblPurchaseBySupplierDetail clientd2 = db.tblPurchaseBySupplierDetails.Find(ii.Id);
                                    db.tblPurchaseBySupplierDetails.Remove(clientd2);
                                    db.SaveChanges();
                                }

                                foreach (PurchaseViewModelDetail items in model.GetDetail)
                                {
                                    var i = (from i1 in db.tblUnits
                                             where i1.UnitNameEng == items.UnitTypeId
                                             select i1).FirstOrDefault();

                                    tblPurchaseBySupplierDetail clientd = new tblPurchaseBySupplierDetail();
                                    clientd.Id = Guid.NewGuid().ToString();
                                    clientd.PurchaseBySupplierId = client.Id;
                                    clientd.ProductId = items.ProductId;
                                    clientd.UnitTypeId = i.Id;
                                    // clientd.Price = items.Price;
                                    clientd.Quantity = Convert.ToDecimal(items.Quantity);
                                    clientd.Cost = (long)Convert.ToDouble(items.Cost);

                                    db.tblPurchaseBySupplierDetails.Add(clientd);
                                    db.SaveChanges();

                                    tblProduct pp = db.tblProducts.Find(clientd.ProductId);
                                    var product = (from pro in db.tblProducts
                                                   where pro.Id == clientd.ProductId
                                                   select pro
                                                  ).ToList();
                                    foreach (var ii in product)
                                    {
                                        //   pp.QuantityInStock = i.QuantityInStock + clientd.Quantity;
                                        //var k = i.Quantity * clientd.Quantity;
                                        //pp.QuantityInStockRetail = (long)Convert.ToDecimal(pp.QuantityInStockRetail + k);
                                        //pp.QuantityInStock = pp.QuantityInStockRetail / i.Quantity;
                                        //db.SaveChanges();
                                        functions.stock_purchase(ii.Id, Convert.ToDecimal(items.Quantity), i.Id);
                                    }
                                }
                            }
                        }
                    }
                    //return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);



                }
                return RedirectToAction("Index", "Purchase");
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult Edit(string id, tblPurchaseBySupplierViewModel model)
        //{
        //    try
        //    {
        //        jotunDBEntities db = new jotunDBEntities();
        //        if (!ModelState.IsValid) return View(model);

        //        tblPurchaseBySupplier client = db.tblPurchaseBySuppliers.Find(id);
        //        if (client != null)
        //        {

        //            var shipper = (from p in db.tblShippers
        //                           where p.ShipperName == model.ShipperId
        //                           select p).FirstOrDefault();
        //            client.ShipperId = shipper.Id;

        //            var supplier = (from p in db.tblSuppliers
        //                            where p.SupplierName == model.SupplierId
        //                            select p).FirstOrDefault();
        //            client.SupplierId = supplier.Id;


        //            client.Id = model.ID;
        //            //client.ShipperId = model.ShipperId;
        //            //client.SupplierId = model.SupplierId;
        //            client.CreatedDate = model.CreatedDate;
        //            client.Description = model.Description;
        //            db.SaveChanges();

        //            var GetDetail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();
        //            if (GetDetail.Any())
        //            {
        //                foreach (var item in GetDetail)
        //                {
        //                    tblPurchaseBySupplierDetail clientDetail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.Id, item.Id) == 0).FirstOrDefault();
        //                    clientDetail.Quantity = item.Quantity;
        //                    db.SaveChanges();

        //                }
        //            }

        //            if (model.GetDetail != null)
        //            {

        //                var GetDetail2 = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();

        //                foreach (var ii in GetDetail2)
        //                {
        //                    tblPurchaseBySupplierDetail clientd2 = db.tblPurchaseBySupplierDetails.Find(ii.Id);
        //                    db.tblPurchaseBySupplierDetails.Remove(clientd2);
        //                    db.SaveChanges();
        //                }

        //                foreach (PurchaseViewModelDetail item in model.GetDetail)
        //                {


        //                    tblPurchaseBySupplierDetail clientd = new tblPurchaseBySupplierDetail();
        //                    clientd.Id = Guid.NewGuid().ToString();
        //                    clientd.PurchaseBySupplierId = client.Id;
        //                    clientd.ProductId = item.ProductId;
        //                    clientd.Price = item.Price;
        //                    clientd.Quantity = item.Quantity;
        //                    db.tblPurchaseBySupplierDetails.Add(clientd);
        //                    db.SaveChanges();

        //                    //tblProduct pp = db.tblProducts.Find(clientd.ProductId);
        //                    //var product = (from pro in db.tblProducts
        //                    //               where pro.Id == clientd.ProductId
        //                    //               select pro
        //                    //              ).ToList();
        //                    //foreach(var i in product)
        //                    //{
        //                    //    pp.QuantityInStock = i.QuantityInStock + clientd.Quantity;
        //                    //    db.SaveChanges();
        //                    //}

        //                }
        //            }
        //        }
        //        return RedirectToAction("Index", "Purchase");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public async Task<ActionResult> Delete(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblPurchaseBySupplier cust = await db.tblPurchaseBySuppliers.FindAsync(id);

                    var GetDetail2 = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();

                    foreach (var ii in GetDetail2)
                    {
                        tblPurchaseBySupplierDetail clientd2 = db.tblPurchaseBySupplierDetails.Find(ii.Id);
                        tblProduct pp = db.tblProducts.Find(clientd2.ProductId);
                        var product = (from pro in db.tblProducts
                                       where pro.Id == clientd2.ProductId
                                       select pro
                                      ).ToList();
                        foreach (var i in product)
                        {
                        //var si = (from i1 in db.tblUnits
                        //          where i1.Id == ii.UnitTypeId
                        //          select i1).FirstOrDefault();

                        //// pp.QuantityInStock = i.QuantityInStock - clientd2.Quantity;
                        //var k = si.Quantity * ii.Quantity;
                        //pp.QuantityInStockRetail = (long)Convert.ToDecimal(pp.QuantityInStockRetail - k);
                        //pp.QuantityInStock = pp.QuantityInStockRetail / si.Quantity;
                        //db.SaveChanges();
                        // pp.QuantityInStock = i.QuantityInStock - clientd2.Quantity;

                        functions.revert_stock_pruchase(i.Id);
                        }
                        clientd2.Quantity = 0;

                        db.SaveChanges();
                    }

                  
                cust.Status = 0;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");


            }
        }
        public ActionResult GetViewDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Json(new { result = "error" }, JsonRequestBehavior.AllowGet);
            return Json(new { result = "success", data = tblPurchaseBySupplierViewModel.GetNoDetail(id) }, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult getpro(string term)
        //{
        //    jotunDBEntities db = new jotunDBEntities();
        //    return Json((db.tblProducts.Where(c => c.ProductName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.ProductName, id = a.Id, pr = a.PriceInStock }).Take(100)), JsonRequestBehavior.AllowGet);
        //}
        public JsonResult getsup(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            return Json((db.tblSuppliers.Where(c => c.SupplierName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.SupplierName, id = a.Id }).Take(100)), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getpro(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            var EmpName = (from e in db.tblProducts
                           //where e.ProductCode.ToLower().Contains(term.ToLower())
                           where e.Status == 1 && (e.ProductName.ToLower().Contains(term.ToLower()) ) || (e.ProductCode.ToLower().Contains(term.ToLower()))
                           select new
                           {
                               label = e.ProductName + " Code: " + e.ProductCode,
                               id = e.ProductName,
                               pid = e.Id,
                               pn = e.ProductName,
                               pr = e.PriceInStock,
                           });
          
           return Json(EmpName, JsonRequestBehavior.AllowGet);
           //return Json((db.tblProducts.Where(c => c.ProductName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.ProductName, id = a.Id, }).Take(100)), JsonRequestBehavior.AllowGet);
        }



        public JsonResult getship(string term)
        {
            jotunDBEntities db = new jotunDBEntities();
            return Json((db.tblShippers.Where(c => c.ShipperName.ToLower().Contains(term.ToLower())).Select(a => new { label = a.ShipperName, id = a.Id }).Take(100)), JsonRequestBehavior.AllowGet);
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

                            return Json(new { result = Result.Success, data = i.Cost, data1 = uni.UnitNameEng }, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                return Json(new { result = Result.Success }, JsonRequestBehavior.AllowGet);

            }
        }

        public JsonResult GetUnit2(string id)

        {
            

            using (jotunDBEntities db = new jotunDBEntities())
            {
                //string code = "1";
                //string code = db.tblProductByUnits.Where(w => string.Compare(w.Id, id) == 0).Select(s => s.Cost).FirstOrDefault().ToString();

                string sampleString = id;
                string[] arrayString = sampleString.Split('^');
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

                        return Json(new { result = Result.Success, data = i.Cost ,data1=uni.UnitNameEng}, JsonRequestBehavior.AllowGet);

                    }
                }





                // string code = CommonFunction.GenerateServiceCodeNumber(id);
                return Json(new { result = Result.Success }, JsonRequestBehavior.AllowGet);

            }

        }

        //public JsonResult GetUnit(string id)
        //{

        //    using (jotunDBEntities db = new jotunDBEntities())
        //    {
        //        //string code = "1";
        //        string code = db.tblProductByUnits.Where(w => string.Compare(w.Id, id) == 0).Select(s => s.Cost).FirstOrDefault().ToString();

        //        var codes = (from i in db.tblProducts
        //                     join i1 in db.tblProductByUnits on i.Id equals i1.ProductID
        //                     where (i1.ProductID == i.Id) && (i1.UnitTypeID == id)
        //                     select i1.Cost


        //                        ).ToList();

        //        // string code = CommonFunction.GenerateServiceCodeNumber(id);
        //        return Json(new { result = Result.Success, data = codes }, JsonRequestBehavior.AllowGet);
        //    }

        //}
        public ActionResult InvoiceReport(string id)
        {
            var aa=0;
            jotunDBEntities db = new jotunDBEntities();

            tblPurchaseBySupplier s = db.tblPurchaseBySuppliers.Find(id);
            var ss = db.tblPurchaseBySupplierDetails.Where(ID => ID.PurchaseBySupplierId == id).ToList();
            var purchaseId = (from p in db.tblPurchaseBySuppliers where p.InvoiceNo != null && p.Status == 1 select p).ToList();
            var maxid = (from x in db.tblPurchaseBySuppliers where x.InvoiceNo != null && x.Status == 1 select x.InvoiceId).Max();
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
                foreach(var item in purchaseId)
                {
                    if(item.InvoiceId == maxid)
                    {
                        string purchaseId1 = item.InvoiceNo;
                        string splits = purchaseId1;
                        string[] arraystring = splits.Split('-');
                        var j = arraystring[0];
                        var j1 = arraystring[1];

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
            if(s.Deposit == null)
            {
                s.Deposit = aa;
            }
            if (s.ShipperAmount == null)
            {
                s.ShipperAmount = aa;
            }
            if (s.Discount == null)
            {
                s.Discount = aa;
            }
            if (s.PurchaseAmount == null)
            {
                s.PurchaseAmount = aa;
            }
            decimal k = (decimal)(((s.PurchaseAmount + s.ShipperAmount) - s.Discount) - s.Deposit);
            if (k <= 0)
            {
                s.PurchaseStatus = "Paid";
            }
            else
            {
                s.PurchaseStatus = "Not Paid";
            }

            db.SaveChanges();
           

            ReportViewer rv = new ReportViewer();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.SizeToReportContent = true;
            rv.Width = Unit.Percentage(100);
            rv.Height = Unit.Percentage(100);
            DataTable dt = new DataTable();


            dt.Columns.Add(new DataColumn("Id"));
            dt.Columns.Add(new DataColumn("SupplierId"));
            dt.Columns.Add(new DataColumn("ShipperId"));
            dt.Columns.Add(new DataColumn("Description"));
            dt.Columns.Add(new DataColumn("CreatedDate"));
            dt.Columns.Add(new DataColumn("UpdatedDate"));
            dt.Columns.Add(new DataColumn("Status"));
            dt.Columns.Add(new DataColumn("ShipperAmount"));
            dt.Columns.Add(new DataColumn("PurchaseAmount"));
            dt.Columns.Add(new DataColumn("Deposit"));
            dt.Columns.Add(new DataColumn("Discount"));
            dt.Columns.Add(new DataColumn("PurchaseStatus"));
            dt.Columns.Add(new DataColumn("InvoiceNo"));
            dt.Columns.Add(new DataColumn("ProductId"));
            dt.Columns.Add(new DataColumn("UnitTypeId"));
            dt.Columns.Add(new DataColumn("Quantity"));
            dt.Columns.Add(new DataColumn("Cost"));
            dt.Columns.Add(new DataColumn("Date"));
            dt.Columns.Add(new DataColumn("ExpiredDate"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("Owe"));

            var purd = db.tblPurchaseBySupplierDetails.Where(ID => ID.PurchaseBySupplierId == id);
            var pur = db.tblPurchaseBySuppliers.Where(ID => ID.Id == id);
            if (purd.Any())
            {
                foreach (var purs in pur)
                {
                    foreach (var ii in purd)
                    {

                        var proname = (from p in db.tblProducts
                                       where p.Id == ii.ProductId
                                       select p).FirstOrDefault();

                        var supplier = (from p in db.tblSuppliers
                                       where p.Id == purs.SupplierId
                                       select p).FirstOrDefault();

                        var unit = (from u in db.tblUnits
                                    where u.Id == ii.UnitTypeId
                                    select u).FirstOrDefault();


                        DataRow dr = dt.NewRow();

                        dr["Id"] = ii.Id;
                        dr["SupplierId"] = supplier.SupplierName;
                        dr["ShipperId"] = ii.Id;
                        dr["Description"] = ii.Id;
                        dr["CreatedDate"] = ii.Id;
                        dr["UpdatedDate"] = ii.Id;
                        dr["Status"] = ii.Id;
                        dr["ShipperAmount"] = float.Parse((purs.ShipperAmount).ToString()).ToString("0.00");
                        dr["PurchaseAmount"] = float.Parse((purs.PurchaseAmount).ToString()).ToString("0.00");
                        dr["Deposit"] = float.Parse((purs.Deposit).ToString()).ToString("0.00");
                        dr["Discount"] = float.Parse((purs.Discount).ToString()).ToString("0.00") ;
                        dr["PurchaseStatus"] = ii.Id;
                        dr["InvoiceNo"] = purs.InvoiceNo;
                        dr["ProductId"] = proname.ProductName;
                        dr["UnitTypeId"] = unit.UnitNameEng;
                        dr["Quantity"] =float.Parse((ii.Quantity).ToString());
                        dr["Cost"] = float.Parse((ii.Cost).ToString()).ToString("0.00");
                        dr["Date"] = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                        dr["ExpiredDate"] = ii.Id;
                        dr["Total"] = float.Parse((ii.Quantity * ii.Cost).ToString()).ToString("0.00");
                        dr["Owe"] = float.Parse((((purs.PurchaseAmount + purs.ShipperAmount) - purs.Deposit)).ToString()).ToString("0.00");

                        dt.Rows.Add(dr);
                    }

                }

            }


            rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\PurchaseInvoice.rdlc";
            rv.LocalReport.DataSources.Clear();
            ReportDataSource rds = new ReportDataSource("PurchaseInvoice", dt);
            rv.LocalReport.DataSources.Add(rds);


            ViewBag.ReportViewer = rv;

            return View();
        }

        public ActionResult ExportToPDFPurchase(string id)
        {
            try
            {
                var aa = 0;
                jotunDBEntities db = new jotunDBEntities();

                tblPurchaseBySupplier s = db.tblPurchaseBySuppliers.Find(id);
                var ss = db.tblPurchaseBySupplierDetails.Where(ID => ID.PurchaseBySupplierId == id).ToList();
                var purchaseId = (from p in db.tblPurchaseBySuppliers where p.InvoiceNo != null && p.Status == 1 select p).ToList();
                var maxid = (from x in db.tblPurchaseBySuppliers where x.InvoiceNo != null && x.Status == 1 select x.InvoiceId).Max();
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
                    foreach (var item in purchaseId)
                    {
                        if (item.InvoiceId == maxid)
                        {
                            string purchaseId1 = item.InvoiceNo;
                            string splits = purchaseId1;
                            string[] arraystring = splits.Split('-');
                            var j = arraystring[0];
                            var j1 = arraystring[1];

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
                if (s.Deposit == null)
                {
                    s.Deposit = aa;
                }
                if (s.ShipperAmount == null)
                {
                    s.ShipperAmount = aa;
                }
                if (s.Discount == null)
                {
                    s.Discount = aa;
                }
                if (s.PurchaseAmount == null)
                {
                    s.PurchaseAmount = aa;
                }
                decimal k = (decimal)(((s.PurchaseAmount + s.ShipperAmount) - s.Discount) - s.Deposit);
                if (k <= 0)
                {
                    s.PurchaseStatus = "Paid";
                }
                else
                {
                    s.PurchaseStatus = "Not Paid";
                }

                db.SaveChanges();


                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                rv.SizeToReportContent = true;
                rv.Width = Unit.Percentage(100);
                rv.Height = Unit.Percentage(100);
                DataTable dt = new DataTable();


                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("SupplierId"));
                dt.Columns.Add(new DataColumn("ShipperId"));
                dt.Columns.Add(new DataColumn("Description"));
                dt.Columns.Add(new DataColumn("CreatedDate"));
                dt.Columns.Add(new DataColumn("UpdatedDate"));
                dt.Columns.Add(new DataColumn("Status"));
                dt.Columns.Add(new DataColumn("ShipperAmount"));
                dt.Columns.Add(new DataColumn("PurchaseAmount"));
                dt.Columns.Add(new DataColumn("Deposit"));
                dt.Columns.Add(new DataColumn("Discount"));
                dt.Columns.Add(new DataColumn("PurchaseStatus"));
                dt.Columns.Add(new DataColumn("InvoiceNo"));
                dt.Columns.Add(new DataColumn("ProductId"));
                dt.Columns.Add(new DataColumn("UnitTypeId"));
                dt.Columns.Add(new DataColumn("Quantity"));
                dt.Columns.Add(new DataColumn("Cost"));
                dt.Columns.Add(new DataColumn("Date"));
                dt.Columns.Add(new DataColumn("ExpiredDate"));
                dt.Columns.Add(new DataColumn("Total"));
                dt.Columns.Add(new DataColumn("Owe"));

                var purd = db.tblPurchaseBySupplierDetails.Where(ID => ID.PurchaseBySupplierId == id);
                var pur = db.tblPurchaseBySuppliers.Where(ID => ID.Id == id);
                if (purd.Any())
                {
                    foreach (var purs in pur)
                    {
                        foreach (var ii in purd)
                        {

                            var proname = (from p in db.tblProducts
                                           where p.Id == ii.ProductId
                                           select p).FirstOrDefault();

                            var supplier = (from p in db.tblSuppliers
                                            where p.Id == purs.SupplierId
                                            select p).FirstOrDefault();

                            var unit = (from u in db.tblUnits
                                        where u.Id == ii.UnitTypeId
                                        select u).FirstOrDefault();


                            DataRow dr = dt.NewRow();

                            dr["Id"] = ii.Id;
                            dr["SupplierId"] = supplier.SupplierName;
                            dr["ShipperId"] = ii.Id;
                            dr["Description"] = ii.Id;
                            dr["CreatedDate"] = ii.Id;
                            dr["UpdatedDate"] = ii.Id;
                            dr["Status"] = ii.Id;
                            dr["ShipperAmount"] = float.Parse((purs.ShipperAmount).ToString()).ToString("0.00");
                            dr["PurchaseAmount"] = float.Parse((purs.PurchaseAmount).ToString()).ToString("0.00");
                            dr["Deposit"] = float.Parse((purs.Deposit).ToString()).ToString("0.00");
                            dr["Discount"] = float.Parse((purs.Discount).ToString()).ToString("0.00");
                            dr["PurchaseStatus"] = ii.Id;
                            dr["InvoiceNo"] = purs.InvoiceNo;
                            dr["ProductId"] = proname.ProductName;
                            dr["UnitTypeId"] = unit.UnitNameEng;
                            dr["Quantity"] = float.Parse((ii.Quantity).ToString());
                            dr["Cost"] = float.Parse((ii.Cost).ToString()).ToString("0.00");
                            dr["Date"] = Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy");
                            dr["ExpiredDate"] = ii.Id;
                            dr["Total"] = float.Parse((ii.Quantity * ii.Cost).ToString()).ToString("0.00");
                            dr["Owe"] = float.Parse((((purs.PurchaseAmount + purs.ShipperAmount) - purs.Deposit)).ToString()).ToString("0.00");

                            dt.Rows.Add(dr);
                        }

                    }

                }


                rv.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"ReportDesign\PurchaseInvoice.rdlc";
                rv.LocalReport.DataSources.Clear();
                ReportDataSource rds = new ReportDataSource("PurchaseInvoice", dt);
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
    }
}