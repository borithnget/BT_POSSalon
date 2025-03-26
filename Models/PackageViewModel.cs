using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
	public class PackageViewModel
	{
		public int Id { get; set; }
		public string PackageName { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public int Status { get; set; }
		public List<ServiceModel> Services { get; set; }
		public List<ProductViewModels> Products { get; set; }
	}
}