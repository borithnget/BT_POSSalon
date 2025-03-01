using jotun.Entities;
using jotun.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jotun.Controllers
{
    public class InvoiceController : Controller
    {
        // GET: Invoice
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }




        public JsonResult GetSaleInvoice(InvoiceViewModels model)
        {
            List<InvoiceViewModels> models = new List<InvoiceViewModels>();
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var invoice = db.tblInvoices.OrderBy(x => x.Id).ToList();
                foreach (var item in invoice)
                {
                    if (item.Status == 1)
                    {
                        var sale = (from p1 in db.tblSales
                                        where p1.Id == item.SaleId
                                        select p1).FirstOrDefault();
                        var date = item.UpdatedDate;
                        if (date == null) { date = item.CreatedDate; }
                        else { date = item.UpdatedDate; }
                        models.Add(new InvoiceViewModels()
                        {
                            Id = item.Id,
                            SaleId = sale.Id,
                            OutstandingInvoiceAmount = sale.Id,
                            CreatedDate = Convert.ToString(item.CreatedDate),


                        });
                    }
                }
            }
            var jsonResult = Json(new { data = models }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }
    }
}