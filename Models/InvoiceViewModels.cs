using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class InvoiceViewModels
    {
        public string Id { get; set; }
        public string SaleId { get; set; }
        public string OutstandingInvoiceAmount { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
    }
}