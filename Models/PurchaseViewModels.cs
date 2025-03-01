using jotun.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Models
{
    public class tblPurchaseBySupplierViewModel
    {
        public string ID { get; set; }
        public string SupplierId { get; set; }
        public string unittype { get; set; }
        public string UnitNameEng { get; set; }
       // public string UnitTypeID { get; set; }
        //public List<SelectListItem> unittypes { get; set; }

        public string SupplierName { get; set; }
        public string ShipperName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ShipperId { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> ShipperAmount { get; set; }
        public Nullable<decimal> PurchaseAmount { get; set; }
        public Nullable<decimal> Deposit { get; set; }
        public Nullable<decimal> Deposit1 { get; set; }
        public string Owe { get; set; }
        public string GrandTotal { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public string PurchaseStatus { get; set; }
        public string InvoiceNo { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> Status { get; set; }
        public List<PurchaseViewModelDetail> GetDetail { get; set; }
        public static tblPurchaseBySupplierViewModel GetNoDetail(string id)
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                tblPurchaseBySupplierViewModel purchase = new tblPurchaseBySupplierViewModel();
                List<PurchaseViewModelDetail> GetDetail = new List<PurchaseViewModelDetail>();
                tblPurchaseBySupplier ps = db.tblPurchaseBySuppliers.Find(id);
                tblPurchaseBySupplierDetail pd = new tblPurchaseBySupplierDetail();

                var supplier = (from p in db.tblSuppliers
                                where p.Id == ps.SupplierId
                                select p).FirstOrDefault();
                var shipper = (from s in db.tblShippers
                               where s.Id == ps.ShipperId
                               select s).FirstOrDefault();

                var sh = ps.ShipperAmount;
                if (sh == null) {
                    sh = 0; }
                else
                {
                    sh = ps.ShipperAmount;
                }
                var de = ps.Deposit;
                if (de == null)
                {
                    de = 0;
                }
                else
                {
                    de = ps.Deposit;
                }
                purchase = db.tblPurchaseBySuppliers.Where(w => string.Compare(w.Id, id) == 0).Select(s => new tblPurchaseBySupplierViewModel()
                {
                    ID = s.Id,
                    SupplierId = supplier.SupplierName,
                    SupplierName = supplier.SupplierName,
                    ShipperId = shipper.ShipperName,
                    ShipperName = shipper.ShipperName,
                    ShipperAmount = s.ShipperAmount,
                    PurchaseAmount = s.PurchaseAmount,
                    
                    Deposit=s.Deposit,
                    Description = s.Description,
                    Discount = s.Discount,
                    Owe = ((s.PurchaseAmount + sh)-de).ToString(),
                   
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate,
                    Status = s.Status
                }).FirstOrDefault();



                //GetDetail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).Select(s => new PurchaseViewModelDetail()
                //{
                //    Id = s.Id,
                //    PurchaseBySupplierId = s.PurchaseBySupplierId,
                //    ProductId = s.ProductId,
                //    ProductIdN = s.ProductId,
                //    UnitTypeId = s.UnitTypeId,
                //    Quantity = s.Quantity.ToString(),
                //    Cost = s.Cost.ToString(),
                //    Discount = s.Discount.ToString(),
                //    Discountdolar = (((s.Quantity * s.Cost) * s.Discount) / 100).ToString(),
                //    Total = ((s.Cost * s.Quantity) - (((s.Quantity * s.Cost) * s.Discount) / 100)).ToString(),

                //}).ToList();
                //GetDetail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();

                var purchase_detail = db.tblPurchaseBySupplierDetails.Where(w => string.Compare(w.PurchaseBySupplierId, id) == 0).ToList();

                List<PurchaseViewModelDetail> model = new List<PurchaseViewModelDetail>();

                foreach(var list in purchase_detail)
                {
                    model.Add(new PurchaseViewModelDetail()
                    {
                        Id = list.Id,
                        PurchaseBySupplierId = list.PurchaseBySupplierId,
                        ProductId = list.ProductId,
                        ProductIdN = list.ProductId,
                        UnitTypeId = list.UnitTypeId,
                        Quantity = Convert.ToDouble(list.Quantity).ToString("N"),
                        Cost = Convert.ToDouble(list.Cost).ToString("N"),
                        Discount = Convert.ToDouble(list.Discount).ToString("N"),
                        Discountdolar = Convert.ToDouble( (((list.Quantity * list.Cost) * list.Discount) / 100)).ToString("N"),
                        Total = Convert.ToDouble(((list.Cost * list.Quantity) - (((list.Quantity * list.Cost) * list.Discount) / 100))).ToString("N"),
                    });
                }


                //foreach (var list in GetDetail)
                foreach (var list in model)
                {

                    var un = (from u in db.tblUnits
                              where u.Id == list.UnitTypeId
                              select u).ToList();
                    foreach (var u1 in un)
                    {
                        list.UnitTypeId = u1.UnitNameEng;
                    }

                    var productdetail = (from s in db.tblProducts
                                         where s.Id == list.ProductId
                                         select s).ToList();

                    foreach (var list2 in productdetail)
                    {
                        list.ProductId = list2.ProductName;
                    }


                }


                //purchase.GetDetail = GetDetail;
                purchase.GetDetail = model;
                return purchase;
            }
        }
    }
}
public class PurchaseViewModelDetail
{
    [Key]
    public string Id { get; set; }
    public string PurchaseBySupplierId { get; set; }
    public string ProductId { get; set; }
    public string ProductIdN { get; set; }
    public string UnitTypeId { get; set; }
    public string TypeDefault { get; set; }
    public string Discount { get; set; }
    public string Discountdolar { get; set; }
    public string Quantity { get; set; }
    public string Cost { get; set; }
    public string Total { get; set; }

    public static PurchaseViewModelDetail GetViewDetail(string id)
    {
        using (jotunDBEntities db = new jotunDBEntities())
        {
            return db.tblPurchaseBySupplierDetails.Where(s => string.Compare(s.Id, id) == 0).Select(s => new PurchaseViewModelDetail()
            {
                Id = s.Id,
                PurchaseBySupplierId = s.PurchaseBySupplierId,
                ProductId = s.ProductId,
                ProductIdN = s.ProductId,
                UnitTypeId = s.UnitTypeId,
                Quantity = s.Quantity.ToString(),
                Cost = s.Cost.ToString(),
                Discount=s.Discount.ToString(),
            }).FirstOrDefault();
        }
    }

}
