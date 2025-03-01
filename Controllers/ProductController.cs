using jotun.Entities;
using jotun.Functions;
using jotun.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static iTextSharp.awt.geom.Point2D;

namespace jotun.Controllers
{
    public class ProductController : Controller
    {
        CommonFunctions functions = new CommonFunctions();

        // GET: Product
        public ActionResult Index()
        {
            if (isAdminUser())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
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


        [HttpGet]
        public ActionResult Create()
        {


            using (jotunDBEntities db = new jotunDBEntities())
            {
                ViewBag.CategoryNamesEng = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                                              .ToList(), "Id", "CategoryNameEng");
                ViewBag.CategoryNamesKh = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                                              .ToList(), "Id", "CategoryNameKh");

                ViewBag.UnitNamesEng = new SelectList(db.tblUnits.Where(u => !u.Status.ToString().Contains("0") && !u.UnitNameEng.Contains("Null"))
                                              .ToList(), "Id", "UnitNameEng");
                ViewBag.UnitNamesKh = new SelectList(db.tblUnits.Where(u => !u.Status.ToString().Contains("0") && !u.UnitNameKh.Contains("Null"))
                                              .ToList(), "Id", "UnitNameKh");

                //ViewBag.ShipperNames = new SelectList(db.tblShippers.Where(u => !u.Status.ToString().Contains("0"))
                //                              .ToList(), "ShipperName", "ShipperName");

                //ViewBag.SupplierNames = new SelectList(db.tblSuppliers.Where(u => !u.Status.ToString().Contains("0"))
                //                              .ToList(), "SupplierName", "SupplierName");

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


        public JsonResult GetUnit(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                string code = db.tblUnits.Where(w => string.Compare(w.UnitNameEng, id) == 0).Select(s => s.Quantity).FirstOrDefault().ToString();
                string code2 = db.tblUnits.Where(w => string.Compare(w.UnitNameEng, id) == 0).Select(s => s.Id).FirstOrDefault().ToString();

                // string code = CommonFunction.GenerateServiceCodeNumber(id);
                return Json(new { result = Result.Success, data = code, data2 = code2 }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetUnit2(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                string code = db.tblUnits.Where(w => string.Compare(w.UnitNameEng, id) == 0).Select(s => s.Quantity).FirstOrDefault().ToString();
                string code2 = db.tblUnits.Where(w => string.Compare(w.UnitNameEng, id) == 0).Select(s => s.Id).FirstOrDefault().ToString();

                // string code = CommonFunction.GenerateServiceCodeNumber(id);
                return Json(new { result = Result.Success, data = code, data2 = code2 }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult ProductHistory()
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();

                var productHistory = db.tblProducts
                    .Select(p => new ProductHistoryViewModel
                    {
                        ProductCode = p.Id,
                        ProductName = p.ProductName,
                        LastAction = p.LastAction,
                        LastModified = p.LastModified,
                        ModifiedBy = p.ModifiedBy
                    })
                    .OrderByDescending(p => p.LastModified)
                    .ToList();

                return View(productHistory);
            }
            catch (Exception ex)
            {
                // Log exception and handle gracefully
                ViewBag.ErrorMessage = "An error occurred while retrieving product history.";
                return View(new List<ProductHistoryViewModel>());
            }
        }

        [HttpPost]
        public ActionResult Create(ProductViewModels model)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                try
                {
                    var pmodel = new tblProduct
                    {
                        Id = Guid.NewGuid().ToString(),
                        CategoryId = model.CategoryId,
                        ProductName = model.ProductName,
                        PriceInStock = Convert.ToDouble(model.PriceInStock),
                        CreatedDate = DateTime.Now,
                        Status = 1,
                        ProductImage = string.IsNullOrEmpty(model.ProductImage) ? "/Images/defaultimage.png" : "/Images/" + model.ProductImage,
                        LastAction = "Create",
                        LastModified = DateTime.Now,
                        ModifiedBy = User.Identity.Name,
                        Service = model.isService == 0 ? false : true,
                        Unitid = model.UnitId,
                        QuantityInStock = Convert.ToDouble(model.QuantityInStock),
                        
                    };
                    db.tblProducts.Add(pmodel);
                    db.SaveChanges();

                    //Price by Customer Type
                    var customerTypes = db.tblCustomerTypes.ToList();

                    foreach (var customerType in customerTypes)
                    {
                        decimal adjustedPrice = Convert.ToDecimal(pmodel.PriceInStock);
                        if (customerType.discount.HasValue && customerType.discount.Value > 0)
                        {
                            adjustedPrice -= adjustedPrice * customerType.discount.Value;
                        }

                        if (customerType.StoreDiscount.HasValue && customerType.StoreDiscount.Value > 0)
                        {
                            adjustedPrice -= customerType.StoreDiscount.Value;
                        }
                        if (adjustedPrice < 0)
                        {
                            adjustedPrice = 0;
                        }
                        var productByUnit = new tblProductByUnit
                        {
                            Id = Guid.NewGuid().ToString(),
                            ProductID = pmodel.Id,
                            UnitTypeID = pmodel.Unitid,
                            Price = adjustedPrice,
                            Cost = Convert.ToDecimal(0),                                                       
                            CustomerType = customerType.Id,
                            TypeDefault = true,
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            CreatedBy = User.Identity.Name,
                            UpdatedAt = DateTime.Now,
                        };

                        db.tblProductByUnits.Add(productByUnit);
                        db.SaveChanges();
                    }




                    //if (model.Products != null && model.Products.Any())
                    //{
                    //    foreach (var product in model.Products)
                    //    {
                    //        var category = db.tblCategories
                    //                         .FirstOrDefault(p => p.CategoryNameEng == product.CategoryName || p.CategoryNameKh == product.CategoryName);

                    //        if (category != null)
                    //        {

                    //            var pmodel = new tblProduct
                    //            {
                    //                Id = product.Id,
                    //                CategoryId = category.Id,
                    //                ProductName = product.ProductName,
                    //                PriceInStock = Convert.ToDouble(product.PriceInStock),
                    //                CreatedDate = DateTime.Now,
                    //                Status = 1,
                    //                ProductImage = string.IsNullOrEmpty(product.ProductImage) ? "/Images/defaultimage.jpg" : product.ProductImage,
                    //                LastAction = "Create",
                    //                LastModified = DateTime.Now,
                    //                ModifiedBy = User.Identity.Name,
                    //                Service = Convert.ToBoolean(product.isService),
                    //                Unitid = product.UnitId,
                    //                QuantityInStock = Convert.ToDouble(product.QuantityInStock),
                    //            };
                    //            db.tblProducts.Add(pmodel);
                    //            db.SaveChanges();
                    //            if (model.ProductAttributes != null && model.ProductAttributes.Any())
                    //            {
                    //                var productAttributes = model.ProductAttributes
                    //                    .Where(attr => attr.ProductId == product.Id)
                    //                    .Select(attr => new tblProductAttribute
                    //                    {
                    //                        ProductId = pmodel.Id,
                    //                        AttributeName = attr.AttributeName,
                    //                        AttributeValue = attr.AttributeValue,
                    //                        CreatedDate = DateTime.Now,
                    //                        ModifiedDate = DateTime.Now
                    //                    }).ToList();

                    //                db.tblProductAttributes.AddRange(productAttributes);
                    //                db.SaveChanges(); // Save attributes
                    //            }
                    //            if (model.GetDetail != null && model.GetDetail.Any())
                    //            {
                    //                foreach (var detail in model.GetDetail)
                    //                {

                    //                    if (detail.ProductID == product.Id)
                    //                    {
                    //                        var customerTypes = db.tblCustomerTypes.ToList();

                    //                        foreach (var customerType in customerTypes)
                    //                        {

                    //                            decimal adjustedPrice = Convert.ToDecimal(product.PriceInStock);

                    //                            if (customerType.discount.HasValue && customerType.discount.Value > 0)
                    //                            {
                    //                                adjustedPrice -= adjustedPrice * customerType.discount.Value;
                    //                            }

                    //                            if (customerType.StoreDiscount.HasValue && customerType.StoreDiscount.Value > 0)
                    //                            {
                    //                                adjustedPrice -= customerType.StoreDiscount.Value;
                    //                            }
                    //                            if (adjustedPrice < 0)
                    //                            {
                    //                                adjustedPrice = 0;
                    //                            }
                    //                            var productByUnit = new tblProductByUnit
                    //                            {
                    //                                Id = Guid.NewGuid().ToString(),
                    //                                Price = adjustedPrice,
                    //                                Cost = Convert.ToDecimal(detail.Cost),
                    //                                UnitTypeID = detail.UnitTypeID,
                    //                                ProductID = pmodel.Id,
                    //                                CustomerType = customerType.Id,
                    //                                TypeDefault = detail.UnitTypeID == pmodel.Unitid,
                    //                                CreatedAt = DateTime.Now,
                    //                                IsActive = true,
                    //                                CreatedBy = User.Identity.Name,
                    //                                UpdatedAt = DateTime.Now,
                    //                            };

                    //                            db.tblProductByUnits.Add(productByUnit);
                    //                        }
                    //                        db.SaveChanges();
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }

                    //    return Json(new { success = true, message = "Save successful" });
                    //}

                    return Json(new { success = true, message = "Save successful" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }

                return Json(new { success = false, message = "Model validation failed." });
            }
        }

        //[HttpPost]
        //public ActionResult Create(ProductViewModels model)
        //{		
        //	using (jotunDBEntities db = new jotunDBEntities())
        //	{
        //		try {
        //                  if (model.Products != null && model.Products.Any())
        //                  {
        //                      foreach (var product in model.Products)
        //                      {
        //                          var category = db.tblCategories
        //                                           .FirstOrDefault(p => p.CategoryNameEng == product.CategoryName || p.CategoryNameKh == product.CategoryName);

        //                          if (category != null)
        //                          {

        //                              var pmodel = new tblProduct
        //                              {
        //                                  Id = product.Id,
        //                                  CategoryId = category.Id,
        //                                  ProductName = product.ProductName,
        //                                  PriceInStock = Convert.ToDouble(product.PriceInStock),
        //                                  CreatedDate = DateTime.Now,
        //                                  Status = 1,
        //                                  ProductImage = string.IsNullOrEmpty(product.ProductImage) ? "/Images/defaultimage.jpg" : product.ProductImage,
        //                                  LastAction = "Create",
        //                                  LastModified = DateTime.Now,
        //                                  ModifiedBy = User.Identity.Name,
        //                                  Service =Convert.ToBoolean(product.isService),
        //                                  Unitid=product.UnitId,
        //                                  QuantityInStock=Convert.ToDouble(product.QuantityInStock),
        //                              };
        //                              db.tblProducts.Add(pmodel);
        //                              db.SaveChanges();
        //                              if (model.ProductAttributes != null && model.ProductAttributes.Any())
        //                              {
        //                                  var productAttributes = model.ProductAttributes
        //                                      .Where(attr => attr.ProductId == product.Id)
        //                                      .Select(attr => new tblProductAttribute
        //                                      {
        //                                          ProductId = pmodel.Id,
        //                                          AttributeName = attr.AttributeName,
        //                                          AttributeValue = attr.AttributeValue,
        //                                          CreatedDate = DateTime.Now,
        //                                          ModifiedDate = DateTime.Now
        //                                      }).ToList();

        //                                  db.tblProductAttributes.AddRange(productAttributes);
        //                                  db.SaveChanges(); // Save attributes
        //                              }
        //                              if (model.GetDetail != null && model.GetDetail.Any())
        //                              {
        //                                  foreach (var detail in model.GetDetail)
        //                                  {

        //                                      if (detail.ProductID == product.Id)
        //                                      {
        //                                          var customerTypes = db.tblCustomerTypes.ToList();

        //                                          foreach (var customerType in customerTypes)
        //                                          {

        //                                              decimal adjustedPrice = Convert.ToDecimal(product.PriceInStock);

        //                                              if (customerType.discount.HasValue && customerType.discount.Value > 0)
        //                                              {
        //                                                  adjustedPrice -= adjustedPrice * customerType.discount.Value;
        //                                              }

        //                                              if (customerType.StoreDiscount.HasValue && customerType.StoreDiscount.Value > 0)
        //                                              {
        //                                                  adjustedPrice -= customerType.StoreDiscount.Value;
        //                                              }
        //                                              if (adjustedPrice < 0)
        //                                              {
        //                                                  adjustedPrice = 0;
        //                                              }
        //                                              var productByUnit = new tblProductByUnit
        //                                              {
        //                                                  Id = Guid.NewGuid().ToString(),
        //                                                  Price = adjustedPrice,
        //                                                  Cost = Convert.ToDecimal(detail.Cost),
        //                                                  UnitTypeID = detail.UnitTypeID,
        //                                                  ProductID = pmodel.Id,
        //                                                  CustomerType = customerType.Id,
        //                                                  TypeDefault = detail.UnitTypeID == pmodel.Unitid,
        //                                                  CreatedAt = DateTime.Now,
        //                                                  IsActive = true,
        //                                                  CreatedBy = User.Identity.Name,
        //                                                  UpdatedAt = DateTime.Now,
        //                                              };

        //                                              db.tblProductByUnits.Add(productByUnit);
        //                                          }
        //                                          db.SaveChanges();
        //                                      }
        //                                  }
        //                              }
        //                          }
        //                      }

        //                      return Json(new { success = true, message = "Save successful" });
        //                  }
        //              }		
        //		catch(Exception ex)
        //              {
        //                  return Json(new { success = false, message = ex.Message });
        //              }

        //		return Json(new { success = false, message = "Model validation failed." });
        //	}
        //}
        //[HttpPost]
        //public JsonResult ImageUpload(HttpPostedFileBase ImageFile)
        //{
        //	if (ImageFile == null || ImageFile.ContentLength == 0)
        //		return Json(new { error = "No file uploaded." }, JsonRequestBehavior.AllowGet);

        //	try
        //	{
        //		var fileName = Path.GetFileName(ImageFile.FileName);
        //		var savePath = Server.MapPath("~/Images/");

        //		if (!Directory.Exists(savePath))
        //			Directory.CreateDirectory(savePath);

        //		var fullPath = Path.Combine(savePath, fileName);
        //		ImageFile.SaveAs(fullPath);

        //		return Json(new { imagePath = "/Images/" + fileName }, JsonRequestBehavior.AllowGet);
        //	}
        //	catch (Exception ex)
        //	{
        //		return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
        //	}
        //}


        [HttpPost]
        public ActionResult ImageUpload(ProductViewModels model)
        {
            string imageId = "";
            var file = model.ImageFile;
            // byte[] imagebyte = null;
            using (jotunDBEntities db = new jotunDBEntities())
            {
                if (file != null)
                {
                    file.SaveAs(Server.MapPath("/Images/" + file.FileName));

                    BinaryReader reader = new BinaryReader(file.InputStream);

                    //imagebyte = reader.ReadBytes(file.ContentLength);

                    tblProduct img = new tblProduct();
                    img.ProductImage = "/Images/" + file.FileName;

                }
            }
            return Json(imageId, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ImageRetrieve(string imgID)
        {
            jotunDBEntities db = new jotunDBEntities();

            var img = db.tblProducts.SingleOrDefault(x => x.Id == imgID);
            RedirectToAction("Index", "Product");
            return File(img.ProductImage, "image/jpg", "image/png");


        }
        public ActionResult ProductHistoryDetail(string id)
        {
            var productId = id.ToString(); // Convert the 'id' (int) to string
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var productHistory = db.tblProducts
                                       .Where(p => p.Id == productId) // Compare with the string 'Id' field
                                       .Select(p => new ProductHistoryViewModel
                                       {
                                           ProductCode = p.Id, // Use the Id field from tblProducts (which is a string)
                                           ProductName = p.ProductName,
                                           LastAction = p.LastAction,
                                           LastModified = p.LastModified,
                                           ModifiedBy = p.ModifiedBy
                                       }).FirstOrDefault();

                if (productHistory == null)
                {
                    return HttpNotFound();
                }

                return View(productHistory);
            }
        }


        [HttpGet]
        public ActionResult Detail(string id, tblProduct model)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //using (jotunDBEntities db = new jotunDBEntities())
            //{
            //    tblProduct product = db.tblProducts.Find(id);
            //    if (product == null)
            //    {
            //        return HttpNotFound();
            //    }


            //    var category = (from c in db.tblCategories
            //                    where c.Id == product.CategoryId
            //                    select c
            //                ).FirstOrDefault();

            //    var date = product.UpdatedDate;
            //    if (date == null) { date = product.CreatedDate; }
            //    else { date = product.UpdatedDate; }

            //    //var uns = unit.UnitNameEng;
            //    //if (uns == null){uns = unit.UnitNameKh;}
            //    //else{ uns = unit.UnitNameEng;}

            //    var cat = category.CategoryNameEng;
            //    if (cat == null) { cat = category.CategoryNameKh; }
            //    else { cat = category.CategoryNameEng; }

            //    //ProductViewModels getNoDetail = new ProductViewModels(id);

            //    return View(new ProductViewModels()
            //    {
            //        Id = product.Id,
            //        CreatedDate = Convert.ToDateTime(date).ToString("dd-MMM-yyyy"),
            //        ProductName = product.ProductName,
            //        ProductCode = product.ProductCode,
            //        ProductNo = product.ProductNo,
            //        QuantityInStock = Convert.ToString(product.QuantityInStock),
            //        PriceInStock = Convert.ToString(product.PriceInStock),
            //        Description = product.Description,
            //        //ShipperName = shipper.ShipperName,
            //        //SupplierName = supplier.SupplierName,
            //        //UnitName = uns,
            //        CategoryName = cat,


            //        ProductImage = product.ProductImage != null ? product.ProductImage : "/Images/defualimage.jpg",


            //    });




            //}
            //functions.convert_All_unit_to_int();
            if (isAdminUser())
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
                return View(ProductViewModels.GetNoDetail(id));
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }


        }
        [HttpGet]
        public ActionResult Edit(string id)
        {

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ViewBag.CategoryNamesEng = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameEng.Contains("Null"))
                                     .ToList(), "CategoryNameEng", "CategoryNameEng");
                ViewBag.CategoryNamesKh = new SelectList(db.tblCategories.Where(u => !u.Status.ToString().Contains("0") && !u.CategoryNameKh.Contains("Null"))
                                              .ToList(), "CategoryNameKh", "CategoryNameKh");

                ViewBag.UnitNamesEng = new SelectList(db.tblUnits.Where(u => !u.Status.ToString().Contains("0") && !u.UnitNameEng.Contains("Null"))
                                              .ToList(), "UnitNameEng", "UnitNameEng");

                var product = ProductViewModels.GetNoDetail(id);

                if (product.UnitQuant != null)
                {
                    //var selected = db.tblUnits.Where(u => u.Id == product.UnitQuant).FirstOrDefault().UnitNameEng;
                    var selected = product.UnitQuant;

                    ViewBag.BasedUnit = new SelectList(db.tblUnits.Where(u => !u.Status.ToString().Contains("0") && !u.UnitNameEng.Contains("Null"))
                                                  .ToList(), "UnitNameEng", "UnitNameEng", selected);
                }
                else
                {
                    ViewBag.BasedUnit = ViewBag.UnitNamesEng;
                }
                if (isAdminUser())
                {
                    if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
                    return View(ProductViewModels.GetNoDetail(id));
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
        }

        [HttpPost]
        public ActionResult Edit(string id, tblProduct pmodel, ProductViewModels model)
        {

            try
            {
                jotunDBEntities db = new jotunDBEntities();
                //  if (!ModelState.IsValid) return View(model);

                tblProduct client = db.tblProducts.Find(id);
                if (client != null)
                {

                    var category = (from p in db.tblCategories
                                    where (p.CategoryNameEng == model.CategoryName) || (p.CategoryNameKh == model.CategoryName)
                                    select p).FirstOrDefault();

                    client.CategoryId = category.Id;


                    client.Id = model.Id;
                    //client.CreatedDate = Convert.ToDateTime(model.CreatedDate);
                    client.QuantityInStock = (long)Convert.ToDouble(model.QuantityInStock);
                    client.QuantityInStockRetail = (long)Convert.ToDouble(model.QuantityInStockRetail);
                    client.Description = model.Description;
                    client.ProductName = model.ProductName;
                    client.Unitid = model.UnitQuant;
                    client.quantity_alert = Convert.ToDouble(model.quantity_alert);
                    //compare = (Convert.ToDecimal(client.QuantityInStockRetail) / Convert.ToDecimal(model.QuantityInStock)).ToString();
                    // client.ProductImage = model.ProductImage;
                    if (client.ProductImage == null) { client.ProductImage = "/Images/defualimage.jpg" + model.ProductImage; }
                    else
                    {
                        if (model.ProductImage == null) { client.ProductImage = client.ProductImage; }
                        else { client.ProductImage = "/Images/" + model.ProductImage; }

                    }
                    client.LastAction = "Edit";
                    client.LastModified = DateTime.Now;
                    client.ModifiedBy = User.Identity.Name;

                    db.SaveChanges();

                    var GetDetail = db.tblProductByUnits.Where(w => string.Compare(w.ProductID, id) == 0).ToList();
                    if (GetDetail.Any())
                    {
                        foreach (var item in GetDetail)
                        {
                            tblProductByUnit clientDetail = db.tblProductByUnits.Where(w => string.Compare(w.Id, item.Id) == 0).FirstOrDefault();
                            if (clientDetail != null)
                            {
                                clientDetail.Cost = item.Cost;
                                clientDetail.Price = item.Price;
                                clientDetail.TypeDefault = item.TypeDefault;

                                db.SaveChanges();
                            }

                        }
                    }
                    if (model.GetDetail != null)
                    {

                        var GetDetail2 = db.tblProductByUnits.Where(w => string.Compare(w.ProductID, id) == 0).ToList();

                        foreach (var ii in GetDetail2)
                        {
                            tblProductByUnit clientd2 = db.tblProductByUnits.Find(ii.Id);
                            db.tblProductByUnits.Remove(clientd2);
                            db.SaveChanges();
                            //  tblPurchaseBySupplierDetail clientd = new tblPurchaseBySupplierDetail();
                            // db.SaveChanges();

                        }

                        foreach (ProductViewModelDetailbyUnit items in model.GetDetail)
                        {
                            double x = Convert.ToDouble(model.UnitQuant);
                            if (items.QTY == x.ToString())
                            {
                                tblProductByUnit clientd = new tblProductByUnit();
                                clientd.Id = Guid.NewGuid().ToString();
                                // clientd.ProductID = client.Id;
                                clientd.ProductID = pmodel.Id;

                                clientd.UnitTypeID = items.UnitTypeID;
                                clientd.TypeDefault = Convert.ToBoolean(1);
                                clientd.Price = Convert.ToDecimal(items.Price);
                                clientd.Cost = Convert.ToDecimal(items.Cost);
                                db.tblProductByUnits.Add(clientd);
                                db.SaveChanges();
                            }
                            else
                            {
                                tblProductByUnit productbyunit = new tblProductByUnit();
                                productbyunit.Id = Guid.NewGuid().ToString();
                                productbyunit.Price = Convert.ToDecimal(items.Price);
                                productbyunit.Cost = Convert.ToDecimal(items.Cost);
                                productbyunit.UnitTypeID = items.UnitTypeID;
                                productbyunit.ProductID = pmodel.Id;
                                productbyunit.TypeDefault = Convert.ToBoolean(0);
                                db.tblProductByUnits.Add(productbyunit);
                                db.SaveChanges();
                            }




                        }
                    }
                    //return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);



                }
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult CheckUsernameAvailability(string userdata)
        {
            jotunDBEntities db = new jotunDBEntities();
            System.Threading.Thread.Sleep(200);
            var SeachData = db.tblProducts.Where(x => x.ProductName == userdata && x.Status == 1).SingleOrDefault();
            if (SeachData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }

         public async Task<ActionResult> Delete(string id)
         {

             using (jotunDBEntities db = new jotunDBEntities())
             {
                 tblProduct cust = await db.tblProducts.FindAsync(id);

                 if (isAdminUser())
                 {
                     cust.Status = 0;
                     //db.tblShippers.Remove(cust);
                     cust.LastAction = "Delete"; 
                     cust.LastModified = DateTime.Now; 
                     cust.ModifiedBy = User.Identity.Name;
                     await db.SaveChangesAsync();
                     return RedirectToAction("Index");
                 }
                 else
                 {
                     return RedirectToAction("Login", "Account");
                 }


             }
         }
        [HttpPost]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            { 
                var product = await db.tblProducts.FindAsync(id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Remove the product from the database
                db.tblProducts.Remove(product);

               
                product.LastAction = "Deleted";
                product.LastModified = DateTime.Now;
                product.ModifiedBy = User.Identity.IsAuthenticated ? User.Identity.Name : "Unknown"; 
                await db.SaveChangesAsync();


                /*return RedirectToAction("Index","Product");*/
                return RedirectToAction("ProductHistory", "Product", new { id = id });
            }
        }

        public JsonResult GetProductHistoryData()
        {
            try
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var products = db.tblProducts
                        .Select(p => new
                        {
                            Id = p.Id,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            LastAction = p.LastAction,
                            LastModified = p.LastModified, 
                            ModifiedBy = p.ModifiedBy
                        })
                        .ToList();
                    var formattedProducts = products.Select(p => new
                    {
                        p.Id,
                        p.ProductCode,
                        p.ProductName,
                        p.LastAction,
                        LastModified = p.LastModified.HasValue
                            ? p.LastModified.Value.ToString("dd-MMM-yyyy")
                            : null, 
                        p.ModifiedBy
                    }).ToList();

                    return Json(new { data = formattedProducts }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (if needed)
                Console.WriteLine($"Error: {ex.Message}");

                return Json(new { data = new List<object>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetData()
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                List<tblProduct> employeeList = db.tblProducts.ToList<tblProduct>();
                return Json(new { data = employeeList }, JsonRequestBehavior.AllowGet);
            }
        }
		public JsonResult GetProductData(ProductViewModels model, string categoryFilter = null, int pageNumber = 1, int pageSize = 23)
		{
			string descrip = "";
			List<ProductViewModels> models = new List<ProductViewModels>();

			using (jotunDBEntities db = new jotunDBEntities())
			{
				// Calculate the skip value for pagination
				int skip = (pageNumber - 1) * pageSize;

				// Base query for active products created in 2025
				var query = db.tblProducts
							  .Where(p => p.Status == 1 &&
										  p.CreatedDate.HasValue &&
										  p.CreatedDate.Value.Year == 2025);

				// Apply category filter if provided
				if (!string.IsNullOrEmpty(categoryFilter))
				{
					// Split categoryFilter into a list (supports multiple categories)
					var categoryIds = categoryFilter.Split(',').ToList();
					query = query.Where(p => categoryIds.Contains(p.CategoryId)); // String comparison
				}

				// Fetch paginated data
				var products = query.OrderByDescending(p => p.CreatedDate)
									.Skip(skip)
									.Take(pageSize)
									.ToList();

				foreach (var p in products)
				{
					var category = db.tblCategories.FirstOrDefault(c => c.Id == p.CategoryId);
					var categoryss = category?.CategoryNameEng ?? category?.CategoryNameKh;
					var date = p.UpdatedDate ?? p.CreatedDate;
					var dd = p.Description ?? descrip;

					models.Add(new ProductViewModels()
					{
						Id = p.Id.ToString(),
						ProductNo = p.ProductNo,
						ProductCode = p.ProductCode,
						CategoryName = categoryss,
						ProductName = p.ProductName,
						Description = p.Description,
						ProductImage = p.ProductImage,
						QuantityInStock = Convert.ToString(p.QuantityInStock),
						PriceInStock = Convert.ToString(p.PriceInStock),
						CreatedDate = Convert.ToDateTime(date).ToString("dd-MMM-yyyy"),
						quantity_alert = Convert.ToString(p.quantity_alert),
					});
				}
			}

			var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
			jsonResult.MaxJsonLength = int.MaxValue;
			return jsonResult;
		}
		public JsonResult GetCategories()
		{
			using (jotunDBEntities db = new jotunDBEntities())
			{
				// Get all active categories
				var categories = db.tblCategories
								   .Where(c => c.Status == 1 &&
										  c.CreatedDate.HasValue &&
										  c.CreatedDate.Value.Year == 2025) 
								   .Select(c => new { c.Id, c.CategoryNameEng, c.CategoryNameKh })
								   .ToList();

				return Json(categories, JsonRequestBehavior.AllowGet);
			}
		}
		public ActionResult CreateSale()
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
		public ActionResult CreateSale([FromBody]SaveSaleRequest request)
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
					client.RevicedFromCustomer = Convert.ToDouble(model.Amount);
					client.ReceivedByABA = model.ReceivedByABA;
					var discount = client.Discount;
					client.Owe = (client.Amount) - (client.RevicedFromCustomer + Convert.ToDouble(client.ReceivedByABA));
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

					return Json(new { result = "success" }, JsonRequestBehavior.AllowGet);
				}
			}
			catch (Exception ex)
			{
				throw ex;
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
		/*public JsonResult GetProductData(ProductViewModels model)
		{
			string descrip = "";

			List<ProductViewModels> models = new List<ProductViewModels>();

			using (jotunDBEntities db = new jotunDBEntities())
			{
				ApplicationDbContext context = new ApplicationDbContext();
				//var productss = db.tblProducts.OrderBy(x => x.Id).ToList();
				var products = (from p in db.tblProducts
								orderby p.CreatedDate descending
								select p).ToList();
				foreach (var p in products)
				{
					if (p.Status == 1)
					{
						var category = (from p1 in db.tblCategories
										where p1.Id == p.CategoryId
										select p1).FirstOrDefault();
						//var unit = (from p1 in db.tblUnits
						//            where p1.Id == p.Unitid
						//            select p1).FirstOrDefault();
						////var shipper = (from p1 in db.tblShippers
						////            where p1.Id == p.ShipperId
						////            select p1).FirstOrDefault();
						////var supplier = (from p1 in db.tblSuppliers
						////            where p1.Id == p.SupplierId
						////            select p1).FirstOrDefault();
						//var unitss = unit.UnitNameEng;
						//if (unitss == null)
						//{
						//    unitss = unit.UnitNameKh;
						//}

						var categoryss = category.CategoryNameEng;
						if (categoryss == null)
						{
							categoryss = category.CategoryNameKh;
						}
						var date = p.UpdatedDate;
						if (date == null) { date = p.CreatedDate; }
						else { date = p.UpdatedDate; }
						var dd = p.Description;
						if (dd == null)
						{
							p.Description = descrip;
						}
						models.Add(new ProductViewModels()
						{
							Id = p.Id.ToString(),
							ProductNo = p.ProductNo,
							ProductCode = p.ProductCode,
							//ProductImage = p.ProductImage,
							CategoryName = categoryss,
							// UnitName = unitss,

							//ShipperName = shipper.ShipperName,
							//SupplierName = supplier.SupplierName,
							ProductName = p.ProductName,
							Description = p.Description,
							ProductImage = p.ProductImage,
							QuantityInStock = Convert.ToString(p.QuantityInStock),
							PriceInStock = Convert.ToString(p.PriceInStock),
							CreatedDate = Convert.ToDateTime(date).ToString("dd-MMM-yyyy"),
							quantity_alert = Convert.ToString(p.quantity_alert),
						});

					}
				}

			}
			var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
			jsonResult.MaxJsonLength = int.MaxValue;
			return jsonResult;
		}*/


		public JsonResult get_product_quantity_alert()
        {
            string descrip = "";

            List<ProductViewModels> models = new List<ProductViewModels>();

            using (jotunDBEntities db = new jotunDBEntities())
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var products = db.tblProducts.Where(x => x.QuantityInStock <= x.quantity_alert).OrderBy(x => x.Id).ToList();
                foreach (var p in products)
                {
                    if (p.Status == 1)
                    {
                        var category = (from p1 in db.tblCategories
                                        where p1.Id == p.CategoryId
                                        select p1).FirstOrDefault();

                        var categoryss = category.CategoryNameEng;
                        if (categoryss == null)
                        {
                            categoryss = category.CategoryNameKh;
                        }
                        var date = p.UpdatedDate;
                        if (date == null) { date = p.CreatedDate; }
                        else { date = p.UpdatedDate; }
                        var dd = p.Description;
                        if (dd == null)
                        {
                            p.Description = descrip;
                        }
                        models.Add(new ProductViewModels()
                        {
                            Id = p.Id.ToString(),
                            ProductNo = p.ProductNo,
                            ProductCode = p.ProductCode,

                            CategoryName = categoryss,

                            ProductName = p.ProductName,
                            Description = p.Description,
                            ProductImage = p.ProductImage,
                            QuantityInStock = Convert.ToString(p.QuantityInStock),
                            PriceInStock = Convert.ToString(p.PriceInStock),
                            CreatedDate = Convert.ToDateTime(date).ToString("dd-MMM-yyyy"),
                            quantity_alert = Convert.ToString(p.quantity_alert)
                        });

                    }
                }

            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

		private class FromBodyAttribute : Attribute
		{
		}
	}
}