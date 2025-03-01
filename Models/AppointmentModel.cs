using jotun.Entities;
using Microsoft.Owin.BuilderProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Models
{
    public class AppointmentModel
    {
        public string AppointmentId { get; set; }
        public Nullable<System.DateTime> AppointmentDate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerContact { get; set; }
        public string StatTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<System.TimeSpan> EndTimeExpected { get; set; }
        public Nullable<decimal> PriceExpected { get; set; }
        public Nullable<decimal> PriceFull { get; set; }
        public Nullable<decimal> DiscountAmount { get; set; }
        public Nullable<decimal> DiscountPercentage { get; set; }
        public Nullable<decimal> PriceFinal { get; set; }
        public Nullable<bool> Canceled { get; set; }
        public string CanceledReason { get; set; }
        public string AppointmentStatus { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public string[] service_id { get; set; }
        public string[] service_price { get; set; }
        public List<ServiceBookedModel> serviceBookeds { get; set; }
        
        public AppointmentModel()
        {
            serviceBookeds = new List<ServiceBookedModel>();
        }
        public static AppointmentModel GetAppointmentModel(string id)
        {
            AppointmentModel model = new AppointmentModel();
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                model = (from app in db.tblAppointments

                         where string.Compare(app.AppointmentId, id) == 0
                         select new AppointmentModel()
                         {
                                AppointmentId=app.AppointmentId,
                                AppointmentDate=app.AppointmentDate,
                                CustomerId=app.CustomerId,
                                CustomerName=app.CustomerName,
                                CustomerContact=app.CustomerContact,
                                StatTime= app.StatTime.ToString(),
                                EndTime=app.EndTime,
                                EndTimeExpected=app.EndTimeExpected,
                                PriceExpected=app.PriceExpected,
                                PriceFull=app.PriceFull,
                                DiscountAmount=app.DiscountAmount,
                                PriceFinal=app.PriceFinal,
                                AppointmentStatus=app.AppointmentStatus
                         }).FirstOrDefault();

                model.serviceBookeds = ServiceBookedModel.GetServiceBookedByAppointmentId(model.AppointmentId);
            }catch(Exception ex)
            {

            }
            return model;
        }

        
    }
    
    public class ServiceBookedModel
    {
        public string Id { get; set; }
        public string AppointmentId { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public Nullable<decimal> Price { get; set; }

        public static List<ServiceBookedModel> GetServiceBookedByAppointmentId(string appointmentId)
        {
            List<ServiceBookedModel> models = new List<ServiceBookedModel>();
            try
            {
                jotunDBEntities db = new jotunDBEntities();
                models = (from sb in db.tblServiceBookeds
                          join prod in db.tblProducts on sb.ServiceId equals prod.Id
                          where string.Compare(sb.AppointmentId, appointmentId) == 0
                          select new ServiceBookedModel()
                          {
                              Id=sb.Id,
                              AppointmentId=sb.AppointmentId,
                              ServiceId=sb.ServiceId,
                              Price=sb.Price,
                              ServiceName=prod.ProductName
                          }).ToList();
            }catch(Exception ex)
            {

            }
            return models;
        }
    }
}