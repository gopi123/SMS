using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMS.Models.ViewModel;
using SMS.Models;
using System.Transactions;
using System.Data;
using System.Data.Entity;


namespace SMS.Controllers
{
    public class MultiCourseController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();

        public int multiCourseId { get; set; }
        public JsonResult GetDataTable()
        {
            try
            {
                var _dTableMultiCourse = _db.MultiCourses
                                        .Where(x => x.Status == true)
                                        .AsEnumerable()
                                        .OrderByDescending(x => x.Id)
                                        .Select(x => new
                                        {
                                            SlNo = "",
                                            CourseCode = x.CourseCode,
                                            Courses = x.MultiCourseDetails
                                                     .Select(m => m.Course.Name)
                                                     .Aggregate((m, n) => m + "," + n),
                                            Duration = x.Duration,
                                            SingleFee = x.SingleFee,
                                            InstalmentFee = x.InstallmentFee,
                                            Id = x.Id
                                        });
                return Json(new { data = _dTableMultiCourse }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        //For loading the dropdown ddlCourseTitle
        public JsonResult FillCourseTitle(int courseTypeId = 0, int courseSeriesId = 0)
        {
            try
            {
                var _courseTitleList = _db.CourseSubTitles
                                        .Where(x => x.CourseSeriesType.Id == courseSeriesId &&
                                               x.CourseType.Id == courseTypeId)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        }).ToList();
                return Json(_courseTitleList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        //For generating the coursecode
        public JsonResult GetCourseCode(int courseTypeId = 0, int courseSeriesId = 0)
        {
            try
            {
                string _finalCourseCode = "";
                //indicates the total length of the serialNo
                int _length = 3;
                //gets the max of serialno with the given combination
                int? _serialNo = _db.MultiCourses
                              .Where(x => x.CourseSubTitle.CourseTypeId == courseTypeId &&
                               x.CourseSubTitle.CourseSeriesTypeId == courseSeriesId)
                              .Max(x => x.SerialNo);

                _serialNo = _serialNo == null ? 0 : _serialNo;
                //accessed during submit event
                Session["SerialNo"] = _serialNo.ToString();
                //increments the serialno
                _serialNo = _serialNo + 1;
                //append zeroes to the serialno
                string _threeDigitSerialNo = _serialNo.ToString().PadLeft(_length, '0');
                //gets the preliminary value of the course code
                string _prelimCourseCode = GetPrelimCourseCode(courseTypeId, courseSeriesId);
                //final course code
                _finalCourseCode = _prelimCourseCode + _threeDigitSerialNo;

                return Json(_finalCourseCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }       

        private string GetCourseIdinAscendingOrder(string[] courseCombination)
        {
            List<int> _lstCourseIds = new List<int>();
            string _lstCourseValues = "";

            //Get CourseIds from the combination
            foreach (var _multiCourseId in courseCombination)
            {
                if (_multiCourseId.Length > 0)
                {
                    string[] _contents = _multiCourseId.Split(',');
                    int _courseId = Convert.ToInt32(_contents[0]);
                    _lstCourseIds.Add(_courseId);
                }
            }

            _lstCourseIds = _lstCourseIds.OrderBy(i => i).ToList();
            _lstCourseValues = string.Join(",", _lstCourseIds);
            return _lstCourseValues;
        }

        //For generating the prelim coursecode
        public string GetPrelimCourseCode(int typeId, int seriesId)
        {
            string _courseStartCode = "NS";
            if (typeId == 1)
            {
                _courseStartCode = _courseStartCode + "IT";
                switch (seriesId)
                {
                    case 1:
                        _courseStartCode = _courseStartCode + "F";
                        break;
                    case 2:
                        _courseStartCode = _courseStartCode + "D";
                        break;
                    case 3:
                        _courseStartCode = _courseStartCode + "P";
                        break;
                    case 4:
                        _courseStartCode = _courseStartCode + "MD";
                        break;
                }
            }
            else if (typeId == 2)
            {
                _courseStartCode = _courseStartCode + "N";
                switch (seriesId)
                {
                    case 1:
                        _courseStartCode = _courseStartCode + "F";
                        break;
                    case 2:
                        _courseStartCode = _courseStartCode + "D";
                        break;
                    case 3:
                        _courseStartCode = _courseStartCode + "P";
                        break;
                    case 4:
                        _courseStartCode = _courseStartCode + "MD";
                        break;
                }
            }
            else if (typeId == 3)
            {
                _courseStartCode = _courseStartCode + "S";
                switch (seriesId)
                {
                    case 1:
                        _courseStartCode = _courseStartCode + "F";
                        break;
                    case 2:
                        _courseStartCode = _courseStartCode + "D";
                        break;
                    case 3:
                        _courseStartCode = _courseStartCode + "P";
                        break;
                    case 4:
                        _courseStartCode = _courseStartCode + "MD";
                        break;
                }
            }
            else
            {
                _courseStartCode = _courseStartCode + "E";
                switch (seriesId)
                {
                    case 1:
                        _courseStartCode = _courseStartCode + "F";
                        break;
                    case 2:
                        _courseStartCode = _courseStartCode + "D";
                        break;
                    case 3:
                        _courseStartCode = _courseStartCode + "P";
                        break;
                    case 4:
                        _courseStartCode = _courseStartCode + "MD";
                        break;
                }

            }
            return _courseStartCode;
        }

        //
        // GET: /MultiCourse/
        public ActionResult Index()
        {
            return View();
        }

        // GET: /MultiCourse/Add
        public ActionResult Add()
        {
            try
            {
                var _multiCourseVM = new MultiCourseVM
                {
                    CourseSeriesList = new SelectList(_db.CourseSeriesTypes.ToList(), "Id", "Name"),
                    CourseSubTitleList = new SelectList("", "Id", "Name"),
                    MultipleCourseList = new SelectList(_db.Courses
                                                        .AsEnumerable()
                                                        .Select(x => new
                                                        {
                                                            Id = x.Id + "," + x.Duration + "," + x.SingleFee + "," + x.InstallmentFee,
                                                            Name = x.Name
                                                        }), "Id", "Name"),
                    CourseTypeList = new SelectList(_db.CourseTypes.ToList(), "Id", "Name")
                };
                return View(_multiCourseVM);
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }

        //POST: /Multicourse/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(MultiCourseVM mdlMulticourse)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string _courseIds = GetCourseIdinAscendingOrder(mdlMulticourse.MultipleCourseId);
                    int count = _db.MultiCourses
                                  .Where(x => x.CourseIds == _courseIds)
                                  .Count();
                    if (count > 0)
                    {
                        return Json(new { message = "warning" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            //insert into multicourse table
                            MultiCourse _multiCourse = new MultiCourse();
                            _multiCourse.CourseSubTitleId = mdlMulticourse.CourseSubTitleId;
                            _multiCourse.CourseCode = mdlMulticourse.MultiCourse.CourseCode;
                            _multiCourse.OldCourseCode = mdlMulticourse.MultiCourse.OldCourseCode != null ? mdlMulticourse.MultiCourse.OldCourseCode.ToUpper() : "";
                            _multiCourse.Duration = mdlMulticourse.MultiCourse.Duration;
                            _multiCourse.SingleFee = mdlMulticourse.MultiCourse.SingleFee;
                            _multiCourse.InstallmentFee = mdlMulticourse.MultiCourse.InstallmentFee;
                            _multiCourse.SerialNo = Convert.ToInt32(Session["SerialNo"]) + 1;
                            _multiCourse.Status = true;
                            _multiCourse.CourseIds = GetCourseIdinAscendingOrder(mdlMulticourse.MultipleCourseId);

                            _db.MultiCourses.Add(_multiCourse);
                            int i = _db.SaveChanges();

                            if (i > 0)
                            {
                                var _maxMultiCourseId = _db.MultiCourses.Max(x => x.Id);
                                string[] _multiCourseIds = mdlMulticourse.MultipleCourseId;
                                foreach (var _multiCourseId in _multiCourseIds)
                                {
                                    if (_multiCourseId.Length > 0)
                                    {
                                        string[] _contents = _multiCourseId.Split(',');
                                        int _courseId = Convert.ToInt32(_contents[0]);

                                        MultiCourseDetail _multiCourseDetail = new MultiCourseDetail();
                                        _multiCourseDetail.MultiCourseId = _maxMultiCourseId;
                                        _multiCourseDetail.CourseId = _courseId;

                                        _db.MultiCourseDetails.Add(_multiCourseDetail);
                                        _db.SaveChanges();
                                    }
                                }
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
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Course/Edit
        public ActionResult Edit(int multiCourseId = 0)
        {
            try
            {
                var _mdlMultiCourse = _db.MultiCourses
                                  .Where(x => x.Id == multiCourseId)
                                  .FirstOrDefault();

                var _courseTypeId = _db.MultiCourses
                                  .Where(x => x.Id == multiCourseId)
                                  .Select(m => m.CourseSubTitle.CourseType.Id)
                                  .FirstOrDefault();

                var _courseSeriesId = _db.MultiCourses
                                  .Where(x => x.Id == multiCourseId)
                                  .Select(m => m.CourseSubTitle.CourseSeriesType.Id)
                                  .FirstOrDefault();

                var _courseTitleId = _db.MultiCourses
                                  .Where(x => x.Id == multiCourseId)
                                  .Select(m => m.CourseSubTitle.Id)
                                  .FirstOrDefault();

                var _multipleCourseIds = _db.MultiCourseDetails
                                        .Where(x => x.MultiCourseId == multiCourseId)
                                        .AsEnumerable()
                                        .Select(m => m.Course.Id).ToList();

                string[] _courseIds = _multipleCourseIds.Select(x => x.ToString()).ToArray();


                var _multiCourseVM = new MultiCourseVM
                {
                    CourseTypeList = new SelectList(_db.CourseTypes.ToList(), "Id", "Name", _courseTypeId),                
                    CourseSeriesList = new SelectList(_db.CourseSeriesTypes.ToList(), "Id", "Name", _courseSeriesId),                   
                    CourseSubTitleList = new SelectList(_db.CourseSubTitles
                                                        .Where(x => x.CourseSeriesType.Id == _courseSeriesId &&
                                                               x.CourseType.Id == _courseTypeId).ToList(), "Id", "Name", _courseTitleId),
                    MultipleCourseList = new SelectList(_db.Courses
                                                        .AsEnumerable()
                                                        .Select(x => new
                                                        {
                                                            Id = x.Id ,
                                                            Name = x.Name
                                                        }), "Id", "Name", _courseIds),
                    MultiCourse = _mdlMultiCourse,
                    CourseSeriesId=_courseSeriesId,
                    CourseSubTitleId=_courseTitleId,
                    CourseTypeId=_courseTypeId,
                    MultipleCourseId=_courseIds
                };
                return PartialView("_Edit", _multiCourseVM);
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }

        // POST: /Course/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MultiCourseVM mdlMultiCourseVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _multiCourse = _db.MultiCourses
                                          .Where(x => x.Id == mdlMultiCourseVM.MultiCourse.Id)
                                          .FirstOrDefault();

                    if (_multiCourse != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            _multiCourse.OldCourseCode = mdlMultiCourseVM.MultiCourse.OldCourseCode != null ? mdlMultiCourseVM.MultiCourse.OldCourseCode.ToUpper() : "";
                            _multiCourse.CourseSubTitleId = mdlMultiCourseVM.CourseSubTitleId;

                            _db.Entry(_multiCourse).State = EntityState.Modified;
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
        public ActionResult Delete(int multiCourseId)
        {
            try
            {
                //get multicourse 
                var _multiCourse = _db.MultiCourses
                                .Where(x => x.Id == multiCourseId)
                                .FirstOrDefault();

                if (_multiCourse != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        ////Change the status from true to false
                        //_multiCourse.Status = false;
                        //_db.Entry(_multiCourse).State = EntityState.Modified;
                        //int i = _db.SaveChanges();
                        var _multiCourseDetail = _db.MultiCourseDetails
                                               .Where(x => x.MultiCourseId == multiCourseId).ToList();
                        foreach (var item in _multiCourseDetail)
                        {
                            _db.MultiCourseDetails.Remove(item);
                        }
                        _db.Entry(_multiCourse).State = EntityState.Deleted;
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
