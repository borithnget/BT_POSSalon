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
    
    public partial class tblShop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblShop()
        {
            this.tblShopModulePermissions = new HashSet<tblShopModulePermission>();
        }
    
        public int Id { get; set; }
        public string ShopCode { get; set; }
        public string ShopName { get; set; }
        public string ShopAddress { get; set; }
        public string ShopPhone { get; set; }
        public string Description { get; set; }
        public string ShopImageURL { get; set; }
        public Nullable<bool> Active { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblShopModulePermission> tblShopModulePermissions { get; set; }
    }
}
