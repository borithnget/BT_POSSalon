using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Models
{
	public class CreatePackageViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string CreateBy { get; set; }
		public string UpdateBy { get; set; }
		public bool IncludeServices { get; set; }
		public bool IncludeProducts { get; set; }
		public List<ServiceSelectionViewModel> SelectedServices { get; set; } = new List<ServiceSelectionViewModel>();
		public List<ProductSelectionViewModel> SelectedProducts { get; set; } = new List<ProductSelectionViewModel>();

		public List<SelectListItem> AvailableServices { get; set; } = new List<SelectListItem>();
		public List<SelectListItem> AvailableProducts { get; set; } = new List<SelectListItem>();
		public string PackageType { get; internal set; }
	}
	public class ServiceSelectionViewModel
	{
		public Guid Id { get; set; }  
		public string ServiceName { get; set; } 
		public int Quantity { get; set; }
	}
	public class ProductSelectionViewModel
	{
		public string Id { get; set; }  
		public string ProductName { get; set; } 
		public int Quantity { get; set; }
		public string Unit { get; set; } 
		public int Qty { get; set; }
		public string ProductImage {  get; set; }
		public List<SelectListItem> AvailableUnits { get; set; }
	}
}