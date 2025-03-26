using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Models
{
    public class CreateServiceViewModel
    {
		public Guid ServiceId { get; set; }
		public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
		public string ServiceTypeName { get; set; }
		public DateTime CreatedAt { get; set; }
		public Guid ServiceTypeId { get; set; }
        public List<SelectListItem> ServiceTypes { get; set; }
        public List<ServiceProductViewModel> Products { get; set; }
        public List<SelectListItem> AvailableProducts { get; set; }
    }
    public class ServiceProductViewModel
    {
        public string ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
        public string Quality { get; set; }
        public string Unit { get; set; }
		public List<SelectListItem> AvailableUnits { get; set; }
	}
}