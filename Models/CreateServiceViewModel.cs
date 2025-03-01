using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Models
{
    public class CreateServiceViewModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public Guid ServiceTypeId { get; set; }
        public List<SelectListItem> ServiceTypes { get; set; }
        public List<ServiceProductViewModel> Products { get; set; }
        public List<SelectListItem> AvailableProducts { get; set; }
    }

    public class ServiceProductViewModel
    {
        public Nullable<Guid> ProductId { get; set; }
        public int Quantity { get; set; }
        public string Quality { get; set; }
    }
}