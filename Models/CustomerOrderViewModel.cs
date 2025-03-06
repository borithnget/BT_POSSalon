using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
	public class CustomerOrderViewModel
	{
		public CustomerViewModels Customer { get; set; }
		public List<SaleViewModels> OrderHistory { get; set; }
	}

}