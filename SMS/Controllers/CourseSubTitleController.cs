using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CourseSubTitleController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /CourseSubTitle/

        public ActionResult Index()
        {
            return View();
        }

        //GetDataTable for datatable
        public JsonResult GetDataTable(int Id = 0)
        {
            try
            {
                var _dTableCourseSubTitle = _db.CourseSubTitles
                                    .Where(x => Id == 0 || x.Id == Id)
                                    .AsEnumerable()
                                    .OrderByDescending(x => x.Id)
                                    .Select(x => new
                                    {
                                        SlNo = "",
                                        TitleName = x.Name,
                                        SeriesName = x.CourseSeriesType.Name,
                                        TypeName = x.CourseType.Name,
                                        Id = x.Id
                                    });
                return Json(new { data = _dTableCourseSubTitle }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /CourseSubTitle/Add
        public ActionResult Add()
        {
            try
            {
                var _courseSubTitleVM = new CourseSubTitleVM
                {
                    CourseSeriesTypeName = new SelectList(_db.CourseSeriesTypes.ToList(), "Id", "Name"),
                    CourseTypeName = new SelectList(_db.CourseTypes.ToList(), "Id", "Name")
                };
                return View(_courseSubTitleVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        //POST: /CourseSubTitle/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(CourseSubTitleVM _mdlCourseSubTitle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        CourseSubTitle _courseSubTitle = new CourseSubTitle();
                        _courseSubTitle.CourseSeriesTypeId = _mdlCourseSubTitle.CourseSeriesTypeId;
                        _courseSubTitle.CourseTypeId = _mdlCourseSubTitle.CourseTypeId;
                        _courseSubTitle.Name = _mdlCourseSubTitle.CourseSubTitle.Name.ToUpper();

                        _db.CourseSubTitles.Add(_courseSubTitle);
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

        //
        // GET: /CourseSubTitle/Edit
        public ActionResult Edit(int _courseSubTitleId = 0)
        {
            try
            {
                //get course series type id
                var _courseSeriesTypeId = _db.CourseSubTitles
                                            .Where(x => x.Id == _courseSubTitleId)
                                            .Select(x => x.CourseSeriesType.Id)
                                            .FirstOrDefault();

                //get course type id
                var _courseTypeId = _db.CourseSubTitles
                                            .Where(x => x.Id == _courseSubTitleId)
                                            .Select(x => x.CourseType.Id)
                                            .FirstOrDefault();

                var _editList = new CourseSubTitleVM
                { 

                    CourseSeriesTypeName=new SelectList(_db.CourseSeriesTypes
                                                        .ToList(), "Id", "Name", _courseSeriesTypeId),

                    CourseTypeName = new SelectList(_db.CourseTypes.ToList(), "Id", "Name", _courseTypeId),
                    
                    CourseSubTitle=_db.CourseSubTitles
                                    .Where(x=>x.Id==_courseSubTitleId)
                                    .FirstOrDefault()
                };
                
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
        public ActionResult Edit(CourseSubTitleVM _mdlCourseSubTitle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _courseSubTitle = _db.CourseSubTitles
                                          .Where(x => x.Id == _mdlCourseSubTitle.CourseSubTitle.Id)
                                          .FirstOrDefault();

                    if (_courseSubTitle != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            _courseSubTitle.CourseSeriesTypeId = _mdlCourseSubTitle.CourseSeriesTypeId;
                            _courseSubTitle.CourseTypeId = _mdlCourseSubTitle.CourseTypeId;
                            _courseSubTitle.Name = _mdlCourseSubTitle.CourseSubTitle.Name.ToUpper();

                            _db.Entry(_courseSubTitle).State = EntityState.Modified;
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

        // POST: /Course/Delete
        [HttpPost]
        public ActionResult Delete(int _courseSubTitleId)
        {
            try
            {
                var _courseSubTitle = _db.CourseSubTitles
                                             .Where(x => x.Id == _courseSubTitleId)
                                             .FirstOrDefault();
                if (_courseSubTitle != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        _db.Entry(_courseSubTitle).State = EntityState.Deleted;
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
