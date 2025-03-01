using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class ReportModel
    {
    }
    public class ReportSaleCashFilterModel
    {
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public string customerId { get; set; }
        public string customterName { get; set; }
        public string customerPhone { get; set; }

    }
}