using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class ReportController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        public class PopulateSelectList
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        //
        // GET: /Report/

        #region WalkInnReport

        public ActionResult WalkInnReport()
        {
            try
            {
                Common _cmn = new Common();

                var _finYearList = GetFinancialYearList();
                var _centreList = GetCentreList();
                string[] _currFinancialYear = _cmn.FinancialYearList().ToList()
                                         .OrderByDescending(x => x.ToString())
                                         .First().Split('-');
                DateTime _startDate = new DateTime(Convert.ToInt32(_currFinancialYear[0]), 4, 1);
                DateTime _endDate = new DateTime(Convert.ToInt32(_currFinancialYear[1]), 3, 31);

                var _employeeList = GetEmployeeList((int)EnumClass.SelectAll.ALL, EnumClass.SelectAll.ALL.ToString(), _startDate, _endDate);
                var _categoryList = from EnumClass.WalkinnStatus b in Enum.GetValues(typeof(EnumClass.WalkinnStatus))
                                    select new { Id = b.ToString(), Name = b.ToString() };

                //Adding all if list count is greater than 1
                if (_centreList.Count > 1)
                {
                    _centreList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
                if (_employeeList.Count > 1)
                {
                    _employeeList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }

                //Getting current month date
                var currDate = Common.LocalDateTime();
                var _fromDate = new DateTime(currDate.Year, currDate.Month, 1);
                var _toDate = new DateTime(currDate.Year, currDate.Month, DateTime.DaysInMonth(currDate.Year, currDate.Month));

                var _indexVM = new ReportVM
                {
                    FinYearList = new SelectList(_finYearList, "Id", "Name"),
                    FinYearId = _finYearList.First().Id,
                    CentreList = new SelectList(_centreList, "Id", "Name"),
                    CentreId = _centreList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_centreList.First().Id),
                    EmpList = new SelectList(_employeeList, "Id", "Name"),
                    EmpId = _employeeList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_employeeList.First().Id),
                    FromDate = _fromDate.Date,
                    ToDate = _toDate.Date,
                    CategoryTypeList = new SelectList(_categoryList, "Id", "Name"),
                    CategoryType = _categoryList.Last().Id
                };

                return View(_indexVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WalkInnReport(ReportVM rptVM)
        {
            Session["CentreId"] = rptVM.CentreId;
            Session["CategoryType"] = rptVM.CategoryType;
            Session["EmpId"] = rptVM.EmpId;
            Session["FromDate"] = rptVM.FromDate;
            Session["ToDate"] = rptVM.ToDate;
            Session["FinYearId"] = rptVM.FinYearId;

            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WalkInnGenerateReport()
        {
            return View();
        }

        //Get Employees on centre change
        public JsonResult GetEmployee_On_CentreChange(int centreId, string categoryType, string financialYear)
        {
            string[] _financialYear = financialYear.Split('-');
            DateTime _startDate = new DateTime(Convert.ToInt32(_financialYear[0]), 4, 1);
            DateTime _endDate = new DateTime(Convert.ToInt32(_financialYear[1]), 3, 31);
            var _empList = GetEmployeeList(centreId, categoryType, _startDate, _endDate);
            if (_empList != null)
            {
                if (_empList.Count > 1)
                {
                    _empList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
            }
            return Json(_empList, JsonRequestBehavior.AllowGet);
        }
        public List<PopulateSelectList> GetEmployeeList(int centreId, string empCategoryId, DateTime startDate, DateTime endDate)
        {
            Common_Report _cmnReport = new Common_Report();
            var _empList = _cmnReport.GetEmployeeList_Walkinn(centreId, empCategoryId, Convert.ToInt32(Session["LoggedUserId"]), startDate, endDate)
                            .Select(e => new PopulateSelectList
                            {
                                Id = e.Id.ToString(),
                                Name = e.Name
                            }).ToList();
            return _empList;
        }

        #endregion

        #region CollectionReport

        public ActionResult CollectionReport()
        {
            try
            {
                Common _cmn = new Common();

                var _finYearList = GetFinancialYearList();
                var _centreList = GetCentreList();
                string[] _currFinancialYear = _cmn.FinancialYearList().ToList()
                                         .OrderByDescending(x => x.ToString())
                                         .First().Split('-');
                //DateTime _startDate = new DateTime(Convert.ToInt32(_currFinancialYear[0]), 4, 1);
                DateTime _startDate = new DateTime(2016, 5, 25);
                DateTime _endDate = new DateTime(Convert.ToInt32(_currFinancialYear[1]), 3, 31);

                var _employeeList = GetEmployeeList_Collection((int)EnumClass.SelectAll.ALL, EnumClass.WalkinnStatus.REGISTERED.ToString(), _startDate, _endDate);

                //Adding all if list count is greater than 1
                if (_centreList.Count > 1)
                {
                    _centreList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
                if (_employeeList.Count > 1)
                {
                    _employeeList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }

                //Getting current month date
                var currDate = Common.LocalDateTime();
                var _fromDate = new DateTime(currDate.Year, currDate.Month, 1);
                var _toDate = new DateTime(currDate.Year, currDate.Month, DateTime.DaysInMonth(currDate.Year, currDate.Month));

                var _indexVM = new ReportVM
                {
                    FinYearList = new SelectList(_finYearList, "Id", "Name"),
                    FinYearId = _finYearList.First().Id,
                    CentreList = new SelectList(_centreList, "Id", "Name"),
                    CentreId = _centreList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_centreList.First().Id),
                    EmpList = new SelectList(_employeeList, "Id", "Name"),
                    EmpId = _employeeList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_employeeList.First().Id),
                    FromDate = _fromDate.Date,
                    ToDate = _toDate.Date
                };

                return View(_indexVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CollectionReport(ReportVM rptVM)
        {
            Session["CentreId"] = rptVM.CentreId;
            Session["CategoryType"] = EnumClass.WalkinnStatus.REGISTERED.ToString();
            Session["EmpId"] = rptVM.EmpId;
            Session["FromDate"] = rptVM.FromDate;
            Session["ToDate"] = rptVM.ToDate;
            Session["FinYearId"] = rptVM.FinYearId;

            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CollectionGenerateReport()
        {
            return View();
        }

        public JsonResult GetEmployee_On_CentreChange_Collection(int centreId, string categoryType, string financialYear)
        {
            string[] _financialYear = financialYear.Split('-');
            DateTime _startDate = new DateTime(Convert.ToInt32(_financialYear[0]), 4, 1);
            DateTime _endDate = new DateTime(Convert.ToInt32(_financialYear[1]), 3, 31);
            var _empList = GetEmployeeList_Collection(centreId, categoryType, _startDate, _endDate);
            if (_empList != null)
            {
                if (_empList.Count > 1)
                {
                    _empList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
            }
            return Json(_empList, JsonRequestBehavior.AllowGet);
        }
        public List<PopulateSelectList> GetEmployeeList_Collection(int centreId, string empCategoryId, DateTime startDate, DateTime endDate)
        {
            Common_Report _cmnReport = new Common_Report();
            var _empList = _cmnReport.GetEmployeeList_Collection(centreId, Convert.ToInt32(Session["LoggedUserId"]), startDate, endDate
                            , EnumClass.ReportMode.COLLECTIONREPORT.ToString())
                            .Select(e => new PopulateSelectList
                            {
                                Id = e.Id.ToString(),
                                Name = e.Name
                            }).ToList();
            return _empList;
        }
        #endregion

        #region PendingReport
        public ActionResult PendingReport()
        {
            try
            {
                Common _cmn = new Common();

                var _finYearList = GetFinancialYearList();
                var _centreList = GetCentreList();
                string[] _currFinancialYear = _cmn.FinancialYearList().ToList()
                                                .OrderByDescending(x => x.ToString())
                                                .First().Split('-');
                DateTime _startDate = new DateTime(Convert.ToInt32(_currFinancialYear[0]), 4, 1);
                DateTime _endDate = new DateTime(Convert.ToInt32(_currFinancialYear[1]), 3, 31);

                
                var _employeeList = GetEmployeeList_Pending((int)EnumClass.SelectAll.ALL, EnumClass.WalkinnStatus.REGISTERED.ToString(), _startDate, _endDate);

                //Adding all if list count is greater than 1
                if (_centreList.Count > 1)
                {
                    _centreList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
                if (_employeeList.Count > 1)
                {
                    _employeeList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }

                //Getting current month date
                var currDate = Common.LocalDateTime();
                var _fromDate = new DateTime(currDate.Year, currDate.Month, 1);
                var _toDate = new DateTime(currDate.Year, currDate.Month, DateTime.DaysInMonth(currDate.Year, currDate.Month));

                var _indexVM = new ReportVM
                {
                    FinYearList = new SelectList(_finYearList, "Id", "Name"),
                    FinYearId = _finYearList.First().Id,
                    CentreList = new SelectList(_centreList, "Id", "Name"),
                    CentreId = _centreList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_centreList.First().Id),
                    EmpList = new SelectList(_employeeList, "Id", "Name"),
                    EmpId = _employeeList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_employeeList.First().Id),
                    FromDate = _fromDate.Date,
                    ToDate = _toDate.Date
                };

                return View(_indexVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public JsonResult GetEmployee_On_CentreChange_Pending(int centreId, string categoryType, string financialYear)
        {
            string[] _financialYear = financialYear.Split('-');
            DateTime _startDate = new DateTime(Convert.ToInt32(_financialYear[0]), 4, 1);
            DateTime _endDate = new DateTime(Convert.ToInt32(_financialYear[1]), 3, 31);
            var _empList = GetEmployeeList_Pending(centreId, categoryType, _startDate, _endDate);
            if (_empList != null)
            {
                if (_empList.Count > 1)
                {
                    _empList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
            }
            return Json(_empList, JsonRequestBehavior.AllowGet);
        }
        public List<PopulateSelectList> GetEmployeeList_Pending(int centreId, string empCategoryId, DateTime startDate, DateTime endDate)
        {
            Common_Report _cmnReport = new Common_Report();
            var _empList = _cmnReport.GetEmployeeList_Collection(centreId, Convert.ToInt32(Session["LoggedUserId"]), startDate, endDate
                            , EnumClass.ReportMode.PENDINGREPORT.ToString())
                            .Select(e => new PopulateSelectList
                            {
                                Id = e.Id.ToString(),
                                Name = e.Name
                            }).ToList();
            return _empList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PendingReport(ReportVM rptVM)
        {
            Session["CentreId"] = rptVM.CentreId;
            Session["CategoryType"] = EnumClass.WalkinnStatus.REGISTERED.ToString();
            Session["EmpId"] = rptVM.EmpId;
            Session["FromDate"] = rptVM.FromDate;
            Session["ToDate"] = rptVM.ToDate;
            Session["FinYearId"] = rptVM.FinYearId;

            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PendingGenerateReport()
        {
            return View();
        }
        #endregion

        #region RegistrationReport

        public ActionResult RegistrationReport()
        {
            Common _cmn = new Common();

            var _finYearList = GetFinancialYearList();
            var _centreList = GetCentreList();
            var _courseCategoryList = GetCourseCategory();
            var _courseList = GetCourse_Categorywise((int)EnumClass.SelectAll.ALL);
            string[] _currFinancialYear = _cmn.FinancialYearList().ToList()
                                            .OrderByDescending(x => x.ToString())
                                            .First().Split('-');
            DateTime _startDate = new DateTime(Convert.ToInt32(_currFinancialYear[0]), 4, 1);
            DateTime _endDate = new DateTime(Convert.ToInt32(_currFinancialYear[1]), 3, 31);


            var _employeeList = GetEmployeeList_Registration((int)EnumClass.SelectAll.ALL, _startDate, _endDate);

            //Adding all if list count is greater than 1
            if (_centreList.Count > 1)
            {
                _centreList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
            }
            if (_employeeList.Count > 1)
            {
                _employeeList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
            }
            if (_courseCategoryList.Count > 1)
            {
                _courseCategoryList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
            }
            if (_courseList.Count > 1)
            {
                _courseList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
            }

            //Getting current month date
            var currDate = Common.LocalDateTime();
            var _fromDate = new DateTime(currDate.Year, currDate.Month, 1);
            var _toDate = new DateTime(currDate.Year, currDate.Month, DateTime.DaysInMonth(currDate.Year, currDate.Month));

            var _indexVM = new ReportVM
            {
                FinYearList = new SelectList(_finYearList, "Id", "Name"),
                FinYearId = _finYearList.First().Id,
                CentreList = new SelectList(_centreList, "Id", "Name"),
                CentreId = _centreList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_centreList.First().Id),
                EmpList = new SelectList(_employeeList, "Id", "Name"),
                EmpId = _employeeList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_employeeList.First().Id),
                FromDate = _fromDate.Date,
                ToDate = _toDate.Date,
                CourseCategoryList=new SelectList(_courseCategoryList,"Id","Name"),
                CourseCategoryId=_courseCategoryList.Count>1?(int)EnumClass.SelectAll.ALL:Convert.ToInt32(_courseCategoryList.First().Id),
                CourseList = new SelectList(_courseList, "Id", "Name"),
                CourseId = _courseList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_courseList.First().Id)
            };

            return View(_indexVM);
        }

        public List<PopulateSelectList> GetEmployeeList_Registration(int centreId, DateTime startDate, DateTime endDate)
        {
            Common_Report _cmnReport = new Common_Report();
            var _empList = _cmnReport.GetEmployeeList_Registration(centreId, Convert.ToInt32(Session["LoggedUserId"]), startDate, endDate)
                            .Select(e => new PopulateSelectList
                            {
                                Id = e.Id.ToString(),
                                Name = e.Name
                            }).ToList();
            return _empList;
        }

        public JsonResult GetEmployee_On_CentreChange_Registration(int centreId, string financialYear)
        {
            string[] _financialYear = financialYear.Split('-');
            DateTime _startDate = new DateTime(Convert.ToInt32(_financialYear[0]), 4, 1);
            DateTime _endDate = new DateTime(Convert.ToInt32(_financialYear[1]), 3, 31);
            var _empList = GetEmployeeList_Registration(centreId, _startDate, _endDate);
            if (_empList != null)
            {
                if (_empList.Count > 1)
                {
                    _empList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
            }
            return Json(_empList, JsonRequestBehavior.AllowGet);
        }

        public List<PopulateSelectList> GetCourseCategory()
        {
            List<PopulateSelectList> _courseCategory = new List<PopulateSelectList>();
            try
            {
                _courseCategory = _db.CourseSeriesTypes
                                .AsEnumerable()
                                .Select(c => new PopulateSelectList
                                {
                                    Id = c.Id.ToString(),
                                    Name = c.Name
                                }).ToList();
            }
            catch (Exception ex)
            {
                _courseCategory = null;
            }
             
            return _courseCategory;
        }

        public List<PopulateSelectList> GetCourse_Categorywise(int categoryId)
        {
            List<PopulateSelectList> _lstCourse = new List<PopulateSelectList>();
            try
            {

                List<int> _lstCourseID = _db.MultiCourseDetails
                                          .Where(mc => categoryId == (int)EnumClass.SelectAll.ALL || mc.MultiCourse.CourseSubTitle.CourseSeriesType.Id == categoryId)
                                          .Select(mc => mc.Course.Id).Distinct().ToList();
                _lstCourse = _db.Courses
                            .Where(c => _lstCourseID.Contains(c.Id))
                            .AsEnumerable()
                            .Select(c => new PopulateSelectList
                            {
                                Id = c.Id.ToString(),
                                Name = c.Name
                            }).ToList();
                
            }
            catch (Exception ex)
            {
                _lstCourse = null;
            }
            return _lstCourse;
        }

        public JsonResult GetCourse_On_CategoryChange_Registration(int categoryId)
        {
            
            try
            {
                var _lstCourse = GetCourse_Categorywise(categoryId);
                if (_lstCourse != null)
                {
                    if (_lstCourse.Count > 1)
                    {
                        _lstCourse.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                    }
                }
                return Json(_lstCourse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrationReport(ReportVM rptVM)
        {
            Session["CentreId"] = rptVM.CentreId;           
            Session["EmpId"] = rptVM.EmpId;
            Session["FromDate"] = rptVM.FromDate;
            Session["ToDate"] = rptVM.ToDate;
            Session["FinYearId"] = rptVM.FinYearId;
            Session["CourseCategoryId"] = rptVM.CourseCategoryId;
            Session["CourseId"] = rptVM.CourseId;

            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RegistrationGenerateReport()
        {
            return View();
        }
        #endregion

        #region Reference Report

        public ActionResult ReferenceReport()
        {
            try
            {
                Common _cmn = new Common();

                var _finYearList = GetFinancialYearList();
                var _centreList = GetCentreList();
                string[] _currFinancialYear = _cmn.FinancialYearList().ToList()
                                         .OrderByDescending(x => x.ToString())
                                         .First().Split('-');
                DateTime _startDate = new DateTime(Convert.ToInt32(_currFinancialYear[0]), 4, 1);
                DateTime _endDate = new DateTime(Convert.ToInt32(_currFinancialYear[1]), 3, 31);

                var _employeeList = GetEmployeeList((int)EnumClass.SelectAll.ALL, EnumClass.SelectAll.ALL.ToString(), _startDate, _endDate);               

                //Adding all if list count is greater than 1
                if (_centreList.Count > 1)
                {
                    _centreList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }
                if (_employeeList.Count > 1)
                {
                    _employeeList.Insert(0, new PopulateSelectList { Id = "-1", Name = "ALL" });
                }

                //Getting current month date
                var currDate = Common.LocalDateTime();
                var _fromDate = new DateTime(currDate.Year, currDate.Month, 1);
                var _toDate = new DateTime(currDate.Year, currDate.Month, DateTime.DaysInMonth(currDate.Year, currDate.Month));

                var _indexVM = new ReportVM
                {
                    FinYearList = new SelectList(_finYearList, "Id", "Name"),
                    FinYearId = _finYearList.First().Id,
                    CentreList = new SelectList(_centreList, "Id", "Name"),
                    CentreId = _centreList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_centreList.First().Id),
                    EmpList = new SelectList(_employeeList, "Id", "Name"),
                    EmpId = _employeeList.Count > 1 ? (int)EnumClass.SelectAll.ALL : Convert.ToInt32(_employeeList.First().Id),
                    FromDate = _fromDate.Date,
                    ToDate = _toDate.Date,                  
                };

                return View(_indexVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReferenceReport(ReportVM rptVM)
        {
            Session["CentreId"] = rptVM.CentreId;           
            Session["EmpId"] = rptVM.EmpId;
            Session["FromDate"] = rptVM.FromDate;
            Session["ToDate"] = rptVM.ToDate;
            Session["FinYearId"] = rptVM.FinYearId;

            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReferenceGenerateReport()
        {
            return View();
        }
        #endregion


        #region Common
        public List<PopulateSelectList> GetFinancialYearList()
        {
            List<PopulateSelectList> _finYearList = new List<PopulateSelectList>();
            try
            {
                Common _cmn = new Common();

                _finYearList = _cmn.FinancialYearList().ToList()
                                    .OrderByDescending(x => x.ToString())
                                    .Select(x => new PopulateSelectList
                                    {
                                        Id = x.ToString(),
                                        Name = x.ToString()
                                    }).ToList();
                //_finYearList.Insert(_finYearList.Count, new PopulateSelectList { Id = "-1", Name = "All" });

                return _finYearList;
            }
            catch (Exception ex)
            {
                return _finYearList = null;
            }
        }
        public List<PopulateSelectList> GetCentreList()
        {
            List<PopulateSelectList> _centreList = new List<PopulateSelectList>();
            try
            {
                Common _cmn = new Common();
                var _empId = Convert.ToInt32(Session["LoggedUserId"]);
                var _employee = _db.Employees
                                .Where(e => e.Id == _empId)
                                .FirstOrDefault();


                _centreList = _cmn.GetCenterEmpwise(_empId)
                            .Select(c => new PopulateSelectList
                            {
                                Id = c.Id.ToString(),
                                Name = c.CentreCode.ToString()
                            }).ToList();

                return _centreList;
            }
            catch (Exception ex)
            {
                return _centreList = null;
            }
        }


        #endregion




        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
