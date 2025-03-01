using System;

namespace jotun.Controllers
{
    internal class Storedata
    {
        public string CustomerName { get; set; }
        public string CustomerLocation { get; set; }
        public string Description { get; set; }
        public string UnitType { get; set; }
        public string ColorCode { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime Date { get; set; }
    }
}