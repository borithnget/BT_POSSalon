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
    
    public partial class tblSupplier
    {
        public string Id { get; set; }
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}
