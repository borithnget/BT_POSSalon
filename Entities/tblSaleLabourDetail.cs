//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace jotun.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblSaleLabourDetail
    {
        public long SaleLabourDetailId { get; set; }
        public Nullable<long> SaleLabourId { get; set; }
        public string ProductName { get; set; }
        public Nullable<decimal> Qty { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<bool> IsCustomer { get; set; }
        public string ProductNameHe { get; set; }
    
        public virtual tblSaleLabour tblSaleLabour { get; set; }
    }
}
