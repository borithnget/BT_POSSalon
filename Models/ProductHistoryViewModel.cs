using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jotun.Models
{
    public class ProductHistoryViewModel
    {
        
            public string ProductCode { get; set; }
            public string ProductName { get; set; }
            public string LastAction { get; set; }
            public DateTime? LastModified { get; set; }
            public string ModifiedBy { get; set; }


    }
}