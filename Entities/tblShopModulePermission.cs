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
    
    public partial class tblShopModulePermission
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int ModuleId { get; set; }
        public bool IsActive { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    
        public virtual tblModule tblModule { get; set; }
        public virtual tblShop tblShop { get; set; }
    }
}
