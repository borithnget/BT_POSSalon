using jotun.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class ProductViewModels
    {
        public string Id { get; set; }

        public string UnitQuant { get; set; }
        public string ProductNo { get; set; }
        public string ProductCode { get; set; }

        [Required]
        [Display(Name = "ProductName")]
        public string ProductName { get; set; }

        public string ProductImage { get; set; }

        [Required]
        [Display(Name = "QuantityInStock")]
        public string QuantityInStock { get; set; }

        [Required]
        [Display(Name = "QuantityInStockRetail")]
        public string QuantityInStockRetail { get; set; }



        [Required]
        [Display(Name = "PriceInStock")]
        public string PriceInStock { get; set; }

        [Required]
        [Display(Name = "CategoryName")]
        public string CategoryName { get; set; }

        //[Required]
        [Display(Name = "CategoryId")]
        public string CategoryId { get; set; }





        [Required]
        [Display(Name = "ShipperName")]
        public string ShipperName { get; set; }

        [Required]
        [Display(Name = "SupplierName")]
        public string SupplierName { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public string quantity_alert { get; set; }

        public HttpPostedFileWrapper ImageFile { get; set; }
        public string LastAction { get; set; } 
        public DateTime? LastModified { get; set; }     
        public string ModifiedBy { get; set; } 





        public List<ProductViewModelDetailbyUnit> GetDetail { get; set; }
        public static ProductViewModels GetNoDetail(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                ProductViewModels product = new ProductViewModels();
                List<ProductViewModelDetailbyUnit> GetDetail = new List<ProductViewModelDetailbyUnit>();
                tblProduct ps = db.tblProducts.Find(id);
                tblProductByUnit pd = new tblProductByUnit();

         
                product = db.tblProducts.Where(w => string.Compare(w.Id, id) == 0).Select(s => new ProductViewModels()
                {
                    Id = s.Id,
                    ProductNo = s.ProductNo,
                    ProductCode = s.ProductCode,
                    ProductName = s.ProductName,
                    PriceInStock =s.PriceInStock.ToString(),
                    QuantityInStock =s.QuantityInStock.ToString(),
                    UnitQuant = s.Unitid,
                    QuantityInStockRetail = s.QuantityInStockRetail.ToString(),
                    //QuantityInStockRetail = s.QuantityInStockRetail.ToString()   ,
                    
                    //QuantityInStockRetail = (Convert.ToDecimal(s.QuantityInStock.ToString()) * Convert.ToDecimal(s.Unitid.ToString())).ToString(),
                    CategoryName =s.CategoryId,
                    ProductImage = s.ProductImage != null ? s.ProductImage : "/Images/defualimage.jpg",
                    CreatedDate = s.CreatedDate.ToString(),
                    Description = s.Description,
                    quantity_alert = s.quantity_alert.ToString(),
                }).FirstOrDefault();

                product.ProductImage = string.IsNullOrEmpty(product.ProductImage) ? "/Images/defualimage.jpg" : product.ProductImage;
                product.CategoryName = db.tblCategories.Where(s => string.Compare(s.Id, product.CategoryName) == 0).Select(s => s.CategoryNameEng).FirstOrDefault().ToString();
                

                if(product.UnitQuant != null)
                {
                    product.UnitQuant = db.tblUnits.Where(x => x.Id == product.UnitQuant).FirstOrDefault()==null?string.Empty: db.tblUnits.Where(x => x.Id == product.UnitQuant).FirstOrDefault().UnitNameEng;
                    //product.UnitQuant = (from produ in db.tblProductByUnits
                    //                     join unit in db.tblUnits on produ.UnitTypeID equals unit.Id
                    //                     where string.Compare(produ.Id, product.UnitQuant) == 0
                    //                     select unit.UnitNameEng).FirstOrDefault();
                }

                var pr = (from s in db.tblProductByUnits
                          where s.ProductID == id
                          select s).ToList();

                foreach(var item in pr)
                {
                    GetDetail.Add(new ProductViewModelDetailbyUnit()
                    {
                        Id = item.Id,
                        UnitTypeID = item.UnitTypeID,
                        UnitTypeIDN = item.UnitTypeID,
                        Cost = (item.Cost ?? 0 ).ToString("N"),
                        Price = (item.Price ?? 0).ToString("N"),
                        QTY = "",
                        TypeDefault = (item.TypeDefault).ToString(),

                    });
                }
                //GetDetail = (from s in db.tblProductByUnits
                //                     where s.ProductID == id
                //                     select new ProductViewModelDetailbyUnit() {
                //                         Id = s.Id,
                //                         UnitTypeID = s.UnitTypeID,
                //                         UnitTypeIDN = s.UnitTypeID,
                //                         Cost =  s.Cost.ToString(),
                //                         Price = s.Price.ToString(),
                //                         QTY = "",
                //                         TypeDefault = (s.TypeDefault).ToString(),

                //                     }).ToList();


                foreach (var list in GetDetail)
                {
                    var productdetail = (from s in db.tblUnits
                                         where s.Id == list.UnitTypeID
                                         select s).ToList();

                    foreach (var list2 in productdetail)
                    {
                        list.UnitTypeID = list2.UnitNameEng;
                        list.QTY = list2.Quantity.ToString();
                    }


                }



                //GetDetail = db.tblProductByUnits.Where(w => string.Compare(w.ProductID, id) == 0).Select(s => new ProductViewModelDetailbyUnit()
                //{
                //    Id = s.Id,
                //    ProductID = s.ProductID,
                //    UnitTypeID = s.UnitTypeID,
                //    Cost = Convert.ToDouble(s.Cost),
                //    Price = Convert.ToDouble(s.Price),
                //}).ToList();



                //GetDetail = (from prd in db.tblProductByUnits
                //             where prd.ProductID == id
                //             select

                //            //  select prd
                //              new ProductViewModelDetailbyUnit()
                //              {
                //                  Id = prd.Id,
                //                  ProductID = prd.ProductID,
                //                  UnitTypeID = prd.UnitTypeID,
                //                  Cost = Convert.ToDouble(prd.Cost),
                //                  Price = Convert.ToDouble(prd.Price),

                //              }).ToList();


                product.GetDetail = GetDetail;

                return product;
            }
        }


    }
}

public class ProductViewModelDetailbyUnit
{
    [Key]
    public string Id { get; set; }
    public string ProductID { get; set; }
    public string UnitTypeID { get; set; }
    public string UnitTypeIDN { get; set; }
    public string TypeDefault { get; set; }
    public string QTY { get; set; }
    public string Cost { get; set; }
    public string Price { get; set; }
    public static ProductViewModelDetailbyUnit GetViewDetail(string id)
    {
        using (jotunDBEntities db = new jotunDBEntities())
        {
            return db.tblProductByUnits.Where(s => string.Compare(s.Id, id) == 0).Select(s => new ProductViewModelDetailbyUnit()
            {
                Id = s.Id,
                ProductID = s.ProductID,
                UnitTypeID = s.UnitTypeID,
                UnitTypeIDN = s.UnitTypeID,
                Cost = s.Cost.ToString(),
                Price = s.Price.ToString(),
                QTY = "",
                TypeDefault=(s.TypeDefault).ToString(),
            }).FirstOrDefault();

        }
    }
}