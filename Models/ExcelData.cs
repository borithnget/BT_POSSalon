using jotun.Entities;
using System;
using System.Collections.Generic;
namespace jotun.Models
{
    public class ExcelData
    {
        

        public string Seeds { get; set; }
        public string HeSeeds { get; set; }
        public string Column3 { get; set; }
        public string ImagePath { get; set; }
        public string JewelerId { get;  set; }
        public string Length { get;  set; }
        public string Size { get;  set; }
        public string CustomerId { get; set; }
        public string Id { get; set; }
        public string Category { get; set; } 
        public string Color { get; set; } 
        public string FinishWeight { get; set; } 
        public string Weight { get; set; } 
        public string LabourCost { get; set; }
        public string LabourDesignCost { get; set; } 
        public string JewelerName { get; set; }
        public decimal Price { get; set; }
        public DateTime? Date;
        public bool IsCustomer;
        public string CustomerName { get; set; }
        public string ProductNameHe { get; set; }
        public string ProductName { get; set; }
        public List<tblSaleLabourDetail> Details { get; set; }
        
    }

}