using jotun.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace jotun.Models
{
    public class ServiceModel
    {
        public string ServiceId { get; set; }
        public string ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public string Name { get; set; }
        public Nullable<double> Price { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedAt { get; set; }

        public static List<ServiceModel> GetServiceList()
        {
            using(jotunDBEntities db=new jotunDBEntities())
            {
                //return db.tblServices.OrderBy(s => s.Name).Where(s => s.IsActive == true)
                    return db.tblProducts.OrderBy(s=>s.ProductName).Where(s=>s.Status==1 && s.Service==true)
                    .Select(s => new ServiceModel()
                    {
                        ServiceId=s.Id,
                        ServiceTypeId=s.CategoryId,
                        Name=s.ProductName,
                        Price=s.PriceInStock,
                        Description=s.Description,
                    }).ToList();
            }
        }
        public static ServiceModel GetServiceItem(string id)
        {
            using(jotunDBEntities db=new jotunDBEntities())
            {
                return db.tblProducts.Where(s => s.Id == id).Select(s => new
                ServiceModel()
                {
                    ServiceId = s.Id,
                    ServiceTypeId = s.CategoryId,
                    Name = s.ProductName,
                    Price = s.PriceInStock,
                    Description = s.Description,
                }).FirstOrDefault();
            }
        }
    }
}