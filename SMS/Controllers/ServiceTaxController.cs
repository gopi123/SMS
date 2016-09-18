using SMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using DotNetIntegrationKit;

namespace SMS.Controllers
{
    public class ServiceTaxController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /ServiceTax/
        public ActionResult Index()
        {            
            return View();
        }

        //LoadGridView
        public JsonResult GetDataTable()
        {
            try
            {
                var _dTableServiceTax = _db.ServiceTaxes
                                    .OrderByDescending(x => x.FromDate)
                                    .ToList();
                return Json(new { data = _dTableServiceTax }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        // GET: /ServiceTax/Add
        public ActionResult Add()
        {
            return View();
        }

        // POST: /ServiceTax/Add
        [HttpPost]
        public JsonResult Add(ServiceTax _mdlServiceTax)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Check whether tax already exists in service tax table
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        ServiceTax _serviceTax = new ServiceTax();
                        _serviceTax.Percentage = _mdlServiceTax.Percentage;
                        _serviceTax.FromDate = _mdlServiceTax.FromDate;
                        _serviceTax.TransactionTime = Common.LocalDateTime();

                        _db.ServiceTaxes.Add(_serviceTax);
                        int i = _db.SaveChanges();
                        //insertion into transactiondetails table
                        if (i > 0)
                        {
                            Common _cmn = new Common();
                            int k = _cmn.AddTransactions(actionName, controllerName, "");
                            if (k > 0)
                            {
                                _ts.Complete();
                                return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { message = "exception" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /ServiceTax/Edit
        public ActionResult Edit(int serviceTaxId = 0)
        {
            try
            {
                var _serviceTaxList = _db.ServiceTaxes
                               .Where(x => x.Id == serviceTaxId)
                               .FirstOrDefault();
                return PartialView("_Edit", _serviceTaxList);
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }


        // POST: /ServiceTax/Edit
        [HttpPost]
        public JsonResult Edit(ServiceTax _mdlServiceTax)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _serviceTaxList = _db.ServiceTaxes
                                    .Where(s => s.Id == _mdlServiceTax.Id)
                                    .FirstOrDefault();
                    if (_serviceTaxList != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            _serviceTaxList.Percentage = _mdlServiceTax.Percentage;
                            _serviceTaxList.FromDate = _mdlServiceTax.FromDate;
                            _serviceTaxList.TransactionTime = Common.LocalDateTime();

                            _db.Entry(_serviceTaxList).State = EntityState.Modified;
                            int i=_db.SaveChanges();
                            //insertion into transactiondetails table
                            if (i > 0)
                            {
                                Common _cmn = new Common();
                                int k = _cmn.AddTransactions(actionName, controllerName, "");
                                if (k > 0)
                                {
                                    _ts.Complete();
                                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                }
                            }    

                            return Json(new { message = "success", JsonRequestBehavior.AllowGet });
                        }
                    }                   
                }
                return Json(new { message = "error", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, JsonRequestBehavior.AllowGet });
            }
        }

        // DELETE: /ServiceTax/Delete
        [HttpPost]
        public ActionResult Delete(int serviceTaxId)
        {
            try
            {

                var _mdlServiceTax = _db.ServiceTaxes
                                    .Where(x => x.Id == serviceTaxId)
                                    .FirstOrDefault();

                if (_mdlServiceTax != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        _db.Entry(_mdlServiceTax).State = EntityState.Deleted;
                        int i = _db.SaveChanges();

                        if (i > 0)
                        {
                            Common _cmn = new Common();
                            int k = _cmn.AddTransactions(actionName, controllerName, "");
                            if (k > 0)
                            {
                                _ts.Complete();
                                return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
