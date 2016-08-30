using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;

namespace SMS.Controllers
{
    public class CourseController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /Course/

        public ActionResult Index()
        {
            return View();
        }

        //Load DataTable
        public JsonResult GetDataTable(int Id = 0)
        {
            try
            {
                var _dTableCourse = _db.Courses
                                    .Where(x => Id == 0 || x.Id == Id)
                                    .AsEnumerable()
                                    .OrderByDescending(x=>x.Id)
                                    .Select(x => new
                                    {
                                        SlNo = "",
                                        Name = x.Name,
                                        Duration = string.Format("{0} - {1}", x.Duration, "Hrs"),
                                        SingleFee = x.SingleFee,
                                        InstalmentFee = x.InstallmentFee,
                                        Id = x.Id
                                    });
                return Json(new { data = _dTableCourse }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Course/Add
        public ActionResult Add()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        //POST: /Course/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(Course _mdlCourse)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        Course _course = new Course();
                        _course.Duration = _mdlCourse.Duration;
                        _course.InstallmentFee = _mdlCourse.InstallmentFee;
                        _course.Name = _mdlCourse.Name.ToUpper();
                        _course.SingleFee = _mdlCourse.SingleFee;

                        _db.Courses.Add(_course);
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

        // GET: /Course/Edit
        public ActionResult Edit(int courseId = 0)
        {
            try
            {
                var _editList = _db.Courses
                              .Where(c => c.Id == courseId)
                              .FirstOrDefault();
                return PartialView("_Edit", _editList);
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }

        // POST: /Course/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Course _mdlCourse)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _course = _db.Courses
                                .Where(c => c.Id == _mdlCourse.Id)
                                .FirstOrDefault();
                    if (_course != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            _course.Duration = _mdlCourse.Duration;
                            _course.InstallmentFee = _mdlCourse.InstallmentFee;
                            _course.Name = _mdlCourse.Name.ToUpper();
                            _course.SingleFee = _mdlCourse.SingleFee;

                            _db.Entry(_course).State = EntityState.Modified;
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
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult Delete(int courseId)
        {
            try
            {

                var _course = _db.Courses
                                    .Where(c => c.Id == courseId)
                                    .FirstOrDefault();
                if (_course != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        _db.Entry(_course).State = EntityState.Deleted;
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
