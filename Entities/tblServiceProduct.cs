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
    
    public partial class tblServiceProduct
    {
        public System.Guid ServiceProductId { get; set; }
        public System.Guid ServiceId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Quality { get; set; }
        public string Notes { get; set; }
        public string Unit { get; set; }
    
        public virtual tblProduct tblProduct { get; set; }
        public virtual tblService tblService { get; set; }
    }
}
