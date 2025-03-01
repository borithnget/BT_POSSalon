using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using jotun.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using jotun.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Web.Security;

namespace jotun.Functions
{
    public class CommonFunctions
    {

        public static Boolean isAdminUser(string userId)
        {

            var role = getRoleNameByUserId(userId);
            if (role.Length > 0)
            {
                if (role.Contains(EnumHelpers.UserRole.Administrator.ToString()) || role.Contains(EnumHelpers.UserRole.SuperAdmin.ToString()))
                {
                    return true;

                }
                else return false;
            }

            else return false;
        }

        public static string getRoleNameByUserId(string userId)
        {
            var userName = "";
            string role = string.Empty;
            if (userId != null)
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var userProfiles = db.AspNetUsers.FirstOrDefault(userProfile => userProfile.Id == userId);
                    if (userProfiles != null)
                    {
                        userName = userProfiles.UserName;
                    }
                }
            }
            foreach (var r in Roles.GetRolesForUser(userName))
            {
                role += " " + r;
            }
            return role;
        }

        public static IEnumerable<SelectListItem> get_customer_location_by_customer_id(string customer_id)
        {

            // var uid = HttpContext.Current.User.Identity.GetUserId();
            List<SelectListItem> P = new List<SelectListItem>();
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var getprovince = db.tblCustomer_location.Where(x => x.customer_id == customer_id && x.status == true);
            foreach (var a in getprovince)
                {
                    P.Add(new SelectListItem
                    {
                        Value = a.id.ToString(),
                        Text = a.location
                    });

                }
            }
            return P;

        }

        public void stock_adjustment(string product_id, decimal amount, string unit_type_id)
        {
            try
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var product = db.tblProducts.Where(x => x.Id == product_id).FirstOrDefault();
                    if (product != null)
                    {
                        var stock_qty = product.QuantityInStock==null?0:product.QuantityInStock;
                        var stock_qty_retail = product.QuantityInStockRetail==null?0:product.QuantityInStockRetail;
                        var unit_qty = db.tblUnits.Where(u => u.Id == product.Unitid).FirstOrDefault().Quantity;

                        //var unit_qty = (from prou in db.tblProductByUnits
                        //                join unit in db.tblUnits on prou.UnitTypeID equals unit.Id
                        //                where string.Compare(prou.Id, product.Unitid) == 0
                        //                select unit.Quantity).FirstOrDefault();

                        Double new_stock = 0;
                        Double new_stock_retial = 0;
                        if (product.Unitid == unit_type_id)
                        {

                            new_stock = Convert.ToDouble((float)stock_qty - (float)amount);
                            new_stock_retial = Convert.ToDouble((float)new_stock * (float)unit_qty);

                        }
                        else
                        {
                            new_stock_retial = Convert.ToDouble((float)stock_qty_retail - (float)amount);
                            new_stock = Convert.ToDouble((float)new_stock_retial / (float)unit_qty);
                           
                        }
                        product.QuantityInStock = new_stock;
                        product.QuantityInStockRetail = new_stock_retial;
                        db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void stock_purchase(string product_id, decimal amount, string unit_type_id)
        {
            try
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var product = db.tblProducts.Where(x => x.Id == product_id).FirstOrDefault();
                    if (product != null)
                    {
                        var stock_qty = product.QuantityInStock;
                        var stock_qty_retail = product.QuantityInStockRetail;
                        var unit_qty = db.tblUnits.Where(u => u.Id == product.Unitid).FirstOrDefault().Quantity;
                        Double new_stock = 0;
                        Double new_stock_retial = 0;
                        if (product.Unitid == unit_type_id)
                        {
                            new_stock = Convert.ToDouble((float)stock_qty + (float)amount);
                            new_stock_retial = Convert.ToDouble((float)new_stock * (float)unit_qty);

                        }
                        else
                        {
                            new_stock_retial = Convert.ToDouble((float)stock_qty_retail + (float)amount);
                            new_stock = Convert.ToDouble((float)new_stock_retial / (float)unit_qty);

                        }
                        product.QuantityInStock = new_stock;
                        product.QuantityInStockRetail = new_stock_retial;
                        db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void revert_stock_pruchase(string purchase_detail_id)
        {
            try
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var sale_detail = db.tblPurchaseBySupplierDetails.Where(s => s.Id == purchase_detail_id).FirstOrDefault();
                    if (sale_detail != null)
                    {
                        var product = db.tblProducts.Where(x => x.Id == sale_detail.ProductId).FirstOrDefault();

                        if (product != null)
                        {
                            var stock_qty = product.QuantityInStock;
                            var stock_qty_retail = product.QuantityInStockRetail;
                            var unit_qty = db.tblUnits.Where(u => u.Id == product.Unitid).FirstOrDefault().Quantity;
                            if (product.Unitid == sale_detail.UnitTypeId)
                            {
                                product.QuantityInStock = stock_qty + (long)sale_detail.Quantity;
                                product.QuantityInStockRetail = stock_qty_retail + ((long)sale_detail.Quantity * unit_qty);

                            }
                            else
                            {
                                product.QuantityInStock = (stock_qty_retail + (long)sale_detail.Quantity) / unit_qty;
                                product.QuantityInStockRetail = stock_qty_retail + ((long)sale_detail.Quantity * unit_qty);
                            }
                            db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void revert_stock_balance(string sale_detail_id)
        {
            try
            {
                using (jotunDBEntities db = new jotunDBEntities())
                {
                    var sale_detail = db.tblSalesDetails.Where(s => s.Id == sale_detail_id).FirstOrDefault();
                    if (sale_detail != null)
                    {
                        var product = db.tblProducts.Where(x => x.Id == sale_detail.ProductId).FirstOrDefault();

                        if (product != null)
                        {
                            var stock_qty = product.QuantityInStock;
                            var stock_qty_retail = product.QuantityInStockRetail;
                            var unit_qty = db.tblUnits.Where(u => u.Id == product.Unitid).FirstOrDefault().Quantity;
                            if (product.Unitid == sale_detail.UnitTypeId)
                            {
                                product.QuantityInStock = stock_qty + (long)sale_detail.Quantity;
                                product.QuantityInStockRetail = stock_qty_retail + ((long)sale_detail.Quantity * unit_qty);

                            }
                            else
                            {
                                product.QuantityInStock = (stock_qty_retail + (long)sale_detail.Quantity) / unit_qty;
                                //product.QuantityInStockRetail = stock_qty_retail + ((long)sale_detail.Quantity * unit_qty);
                                product.QuantityInStockRetail = stock_qty_retail + ((long)sale_detail.Quantity);
                            }
                            db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void convert_All_unit_to_int()
        {
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var pro = db.tblProducts.ToList();
                foreach (var p in pro)
                {
                    if (p.Unitid != null && !String.IsNullOrEmpty(p.Unitid))
                    {
                        float quant = float.Parse(p.Unitid);
                        p.Unitid = Convert.ToInt32(quant).ToString();
                    }
                    else
                    {
                        p.Unitid = "1";
                    }
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public static string get_unit_id (string unit_name_eng)
        {
            string result = "";
            using (jotunDBEntities db = new jotunDBEntities())
            {
                var pro = db.tblUnits.Where(x=>x.UnitNameEng == unit_name_eng).FirstOrDefault();

                if(pro !=null)
                {
                    result = pro.Id;
                }
                
            }
            return result;
        }
        public static DateTime ToLocalTime(DateTime utcDate)
        {
            var localTimeZoneId = "SE Asia Standard Time";
            //var localTimeZoneId = "Pacific Standard Time";
            //var localTimeZoneId = "Tokyo Standard Time";
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);
            var localTime = TimeZoneInfo.ConvertTime(utcDate, localTimeZone);
            return localTime;
        }

        public static void UpdateCustomerPointbyNumberofOrder(string saleId,string customerId)
        {
            try
            {
                jotunDBEntities db = new jotunDBEntities();

                tblSale sale = db.tblSales.Find(saleId);
                var pointAmount =Convert.ToInt32(sale.SaleAmount);

                tblCustomer customer = db.tblCustomers.Find(customerId);
                customer.TotalPoint = customer.TotalPoint + 1;
                customer.TotalPointAmount = customer.TotalPointAmount + pointAmount;
                db.SaveChanges();

                tblCustomerPointHistory history = new tblCustomerPointHistory();
                history.SaleId = saleId;
                history.CustomerId = customerId;
                history.CreatedDate = DateTime.Now;
                history.PointQTY = 1;
                history.PointAmount = pointAmount;
                db.tblCustomerPointHistories.Add(history);
                db.SaveChanges();

            }catch(Exception ex)
            {

            }
        }

    }

}