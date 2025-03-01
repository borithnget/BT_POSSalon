using System.Collections.Generic;

namespace jotun.Models
{
    public class SaleLabourViewModel
    {
        public string Category { get; set; }
        public string Color { get; set; }
        public decimal FinishWeight { get; set; }
        public decimal Weight { get; set; }
        public decimal LabourCost { get; set; }
        public decimal LabourDesignCost { get; set; }
        public string JewelerName { get; set; }
        public string CustomerName { get; set; }
        public string ImagePath { get; set; }
        public long SaleLabourId { get; set; }
        public string JewelerId { get; set; }
        public string CustomerId { get; set; }
        public long SaleLabourDetailId { get; set; }
        // Updated to use custom ProductDetail class
        public List<ProductDetail> ProductDetails { get; set; }
    }
    // Renamed to ProductDetail (singular)
    public class ProductDetail
    {
        public string ProductName { get; set; }
        public string ProductNameHe { get; set; }
        public decimal Price { get; set; }
        public long SaleLabourDetailId { get; set; }
    }
}
