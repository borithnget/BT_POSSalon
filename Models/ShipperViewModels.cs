using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class ShipperViewModels
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "ShipperName")]
        public string ShipperName { get; set; }
        [Required]
        [Display(Name = "ContactName")]
        public string ContactName { get; set; }
        [Required]
        [Display(Name = "ContactPhone")]
        public string ContactPhone { get; set; }      
        public string Gender { get; set; }       
        public string Address { get; set; }
        public string Noted { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }



    }
}