using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class CategoryViewModels
    {
        public string Id { get; set; }
        //public string CategoryNo { get; set; }
        //public string CategoryCode { get; set; }

        [Required]
        [Display(Name = "CategoryNameEng")]
        public string CategoryNameEng { get; set; }
        public string CategoryNameKh { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
    }
}