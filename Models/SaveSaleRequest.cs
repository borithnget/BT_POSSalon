using jotun.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
	public class SaveSaleRequest
	{
		public tblSale client { get; set; }
		public SaleViewModels model { get; set; }
		public ProductViewModels pmodel { get; set; }
		public SaleDetailViewModel cliented { get; set; }
        public string TableNumber { get; set; }
    }

}