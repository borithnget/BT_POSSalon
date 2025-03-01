using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using jotun.Entities;

namespace jotun.Models
{
    public class CustomerViewModels
    {
        public string Id { get; set; }

        
        public string CustomerNo { get; set; }

        [Required]
        [Display(Name = "CustomerName")]
        public string CustomerName { get; set; }

        [Required]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        public string Gender { get; set; }
        public string ProjectLocation { get; set; }
        public string Noted { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public string Status { get; set; }

        public List<tblCustomer_location> locations;
    }


    public class LocationDetails 
    { 
        
    }
}