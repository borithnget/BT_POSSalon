using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class UnitViewModels
    {
        public string Id { get; set; }
        //public string CategoryNo { get; set; }
        //public string CategoryCode { get; set; }
        //[Required]
        //[Display(Name = "Unit Name")]

        [Required]
        [Display(Name = "UnitNameEng")]
        public string UnitNameEng { get; set; }
        public string UnitNameKh { get; set; }


        //[StringLength(5)]
        [Required]
        [Display(Name = "Quantity")]
        public Nullable<long> Quantity { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
    }
}