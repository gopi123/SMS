using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CourseInterchangeController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();

        #region Class Definitions
        public class CourseDetails
        {
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public string SoftwareUsed { get; set; }
            public int Duration { get; set; }
            public decimal Fee { get; set; }
            public string CourseIds { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public int CourseFee_Updated
            {
                get
                {
                    int _newFee = 0;



                    if (StudentRegistration != null)
                    {
                        decimal _discount = StudentRegistration.Discount.Value;
                        decimal _existingCourseFee = StudentRegistration.TotalCourseFee.Value;
                        decimal _newCourseFee = Fee;
                        decimal _currST = currST;
                        FeeCalculation _clsFee = CalculateCourseFee(StudentRegistration, Fee, currST);
                        _newFee = _clsFee.CourseFee;
                        STAmt_Updated = _clsFee.STAmount;
                        Discount_Updated = _clsFee.DiscountPercentage;

                    }
                    else
                    {
                        _newFee = 0;
                        STAmt_Updated = 0;
                        Discount_Updated = 0;
                    }


                    return _newFee;
                }
            }
            public int STAmt_Updated { get; set; }
            public int Discount_Updated { get; set; }
            public decimal currST { get; set; }
        }

        public class FeeCalculation
        {
            public int CourseFee { get; set; }
            public int STAmount { get; set; }
            public int DiscountPercentage { get; set; }
        }

        public class CourseInterchangeDataTable
        {
            public string InterchangeDate
            {
                get
                {
                    return StudentRegHistory.TransactionDate.Value.ToString("dd/MM/yyyy");
                }
            }
            public string DoneBy
            {
                get
                {
                    return StudentRegHistory.Employee.Name;
                }
            }
            public string RegNo { get; set; }
            public string SalesPerson { get; set; }
            public string ExistingSoftwareUsed
            {
                get
                {
                    return string.Join(",", StudentRegHistory.StudentRegistrationCourse_History
                                    .SelectMany(rh => rh.MultiCourse.MultiCourseDetails
                                    .Select(rhmc => rhmc.Course.Name)));
                }
            }
            public string NewSoftwareUsed { get; set; }
            public int ExistingFee
            {
                get
                {
                    return StudentRegHistory.StudentReceipt_History.Sum(s => s.Fee.Value);
                }
            }
            public int NewFee { get; set; }
            public string StudentName { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public StudentRegistration_History StudentRegHistory
            {
                get
                {
                    StudentRegistration_History _studRegHistory = new StudentRegistration_History();
                    _studRegHistory = StudentRegistration.StudentRegistration_History.OrderByDescending(r => r.Id).FirstOrDefault();
                    return _studRegHistory;
                }
            }
        }

        public class SuccessMessage
        {
            public int RegistrationId { get; set; }
            public string Status { get; set; }

        }

        #endregion

        public ActionResult Index()
        {
            try
            {
                Common _cmn = new Common();

                var _finYearList = _cmn.FinancialYearList().ToList()
                                    .OrderByDescending(x => x.ToString())
                                    .Select(x => new
                                    {
                                        Id = x.ToString(),
                                        Name = x.ToString()
                                    });

                var _monthList = from EnumClass.Month c in Enum.GetValues(typeof(EnumClass.Month))
                                 select new { Id = c.ToString(), Name = c.ToString() };

                var _indexVM = new IndexVM
                {
                    FinancialYearList = new SelectList(_finYearList, "Id", "Name"),
                    MonthList = new SelectList(_monthList, "Id", "Name"),
                    MonthName = "ALL"
                };
                return View(_indexVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public JsonResult GetDataTable(string finYear, string month)
        {
            try
            {

                Common _cmn = new Common();
                List<CourseInterchangeDataTable> _dTableReg = new List<CourseInterchangeDataTable>();
                DateTime _startFinDate = new DateTime();
                DateTime _endFinDate = new DateTime();

                var arrFinYear = finYear.Split('-');
                var _startFinYear = Convert.ToInt32(arrFinYear[0]);
                var _endFinYear = Convert.ToInt32(arrFinYear[1]);
                var _currentRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));
                int monthID = _cmn.GetMonthID(month);

                if (monthID == (int)EnumClass.SelectAll.ALL)
                {
                    _startFinDate = new DateTime(_startFinYear, (int)EnumClass.FinYearMonth.STARTMONTH, 1).Date;
                    _endFinDate = new DateTime(_endFinYear, (int)EnumClass.FinYearMonth.ENDMONTH, 31).Date;
                }
                else
                {
                    //Eg:FinYear=2016-2017
                    //if month is greater than 4 then 2016 is taken else 2017 is taken
                    if (monthID >= 4)
                    {
                        _startFinDate = new DateTime(_startFinYear, monthID, 1);
                        _endFinDate = new DateTime(_startFinYear, monthID, DateTime.DaysInMonth(_startFinYear, monthID));
                    }
                    else
                    {
                        _startFinDate = new DateTime(_endFinYear, monthID, 1);
                        _endFinDate = new DateTime(_endFinYear, monthID, DateTime.DaysInMonth(_startFinYear, monthID));
                    }

                }

                //Gets all the centerCodeIds allotted to an employee
                List<int> _centerCodeIds = _cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"]))
                                            .Select(x => x.Id).ToList();
                List<StudentRegistration> _lstStudReg = new List<StudentRegistration>();
                _lstStudReg = _db.StudentRegistrations
                            .Where(r => r.StudentRegistration_History.Any() &&
                                    _centerCodeIds.Contains(r.StudentWalkInn.CenterCode.Id))
                            .ToList();

                _dTableReg = _lstStudReg
                                .AsEnumerable()
                                .Where(r => (r.StudentRegistration_History.OrderByDescending(rh => rh.Id).FirstOrDefault().TransactionDate.Value.Date >= _startFinDate
                                            && r.StudentRegistration_History.OrderByDescending(rh => rh.Id).FirstOrDefault().TransactionDate.Value.Date <= _endFinDate))
                                .OrderByDescending(r => r.Id)
                                .Select(rd => new CourseInterchangeDataTable
                                {
                                    StudentRegistration = rd,
                                    NewFee = rd.StudentReceipts.Sum(s => s.Fee.Value),
                                    NewSoftwareUsed = string.Join(",", rd.StudentRegistrationCourses
                                                         .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                         .Select(mc => mc.Course.Name))),
                                    RegNo = rd.RegistrationNumber,
                                    SalesPerson = rd.StudentWalkInn.CROCount == 1 ? rd.StudentWalkInn.Employee1.Name :
                                                  rd.StudentWalkInn.Employee1.Name + "," + rd.StudentWalkInn.Employee2.Name,
                                    StudentName = rd.StudentWalkInn.CandidateName

                                }).ToList();

                var _regList = _dTableReg
                               .Select(x => new
                               {
                                   InterchangeDate = x.InterchangeDate,
                                   DoneBy = x.DoneBy.ToUpper(),
                                   RegNo = x.RegNo,
                                   SalesPerson = x.SalesPerson.ToUpper(),
                                   ExistingSoftwareUsed = x.ExistingSoftwareUsed.ToUpper(),
                                   NewSoftwareUsed = x.NewSoftwareUsed.ToUpper(),
                                   ExistingFee = x.ExistingFee,
                                   NewFee = x.NewFee,
                                   StudentName = x.StudentName.ToUpper()
                               }).ToList();



                return Json(new { data = _regList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CourseInterchangeSearch()
        {
            return View();
        }

        public ActionResult GetStudentInfo(string regNo)
        {
            try
            {
                StudentInfoVM _certificateVM = new StudentInfoVM();
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.RegistrationNumber == regNo)
                            .FirstOrDefault();
                if (_dbRegn != null)
                {
                    _certificateVM = new StudentInfoVM
                    {
                        RegistrationId = _dbRegn.Id,
                        RegistrationNo = _dbRegn.RegistrationNumber,
                        SalesPerson = _dbRegn.StudentWalkInn.CROCount == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                                      _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name,
                        SoftwareUsed = string.Join(",", _dbRegn.StudentRegistrationCourses
                                      .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                      .Select(mcd => mcd.Course.Name))),
                        StudentName = _dbRegn.StudentWalkInn.CandidateName,
                        PhotoUrl = _dbRegn.PhotoUrl,
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        CourseCount = _dbRegn.StudentRegistrationCourses
                                    .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                    .Select(mcd => mcd.Course)
                                    .Count(),
                        PendingPaymentCount = _dbRegn.StudentReceipts
                                    .Where(r => r.Status == false)
                                    .Count(),
                        PendingFeedbackCount = _dbRegn.StudentFeedbacks
                                            .Where(f => f.IsFeedbackGiven == false)
                                            .Count(),

                        PaidPaymentCount = _dbRegn.StudentReceipts
                                        .Where(r => r.Status == true)
                                        .Count()
                    };
                }
                return PartialView("_GetStudentInfo", _certificateVM);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult CourseInterchange(int regId)
        {
            try
            {
                Common _cmn = new Common();
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regId)
                            .FirstOrDefault();

                if (_dbRegn != null)
                {
                    int loggedEmpId = Convert.ToInt32(Session["LoggedUserId"]);
                    var _employee = _db.Employees
                                   .Where(e => e.Id == loggedEmpId)
                                   .FirstOrDefault();

                    //Mask email if loggedemployee is not the cro of current student
                    var _studentEmail = _dbRegn.StudentWalkInn.EmailId;
                    if (_employee.Designation.Role.Id == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (_dbRegn.StudentWalkInn.CRO1ID != loggedEmpId || _dbRegn.StudentWalkInn.CRO2ID != loggedEmpId)
                        {
                            _studentEmail = _cmn.MaskString(_studentEmail, "email");
                        }
                    }

                    //Mask mobile if loggedemployee is not the cro of current student
                    var _studentMobile = _dbRegn.StudentWalkInn.MobileNo;
                    if (_employee.Designation.Role.Id == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (_dbRegn.StudentWalkInn.CRO1ID != loggedEmpId || _dbRegn.StudentWalkInn.CRO2ID != loggedEmpId)
                        {
                            _studentMobile = _cmn.MaskString(_studentMobile, "mobile");
                        }
                    }

                    var _mgrMobileNo = string.Join(",", _db.EmployeeCenters
                                                        .Where(ec => ec.CenterCode.Id == _dbRegn.StudentWalkInn.CenterCode.Id
                                                                     && ec.Employee.Designation.Id == (int)EnumClass.Designation.MANAGERSALES)
                                                        .Select(e => e.Employee.MobileNo).ToList());



                    //Courses whose feedback has been submitted cannot be downgraded
                    var _feedbackCourseIdList = _dbRegn.StudentFeedbacks
                                                .Where(f => f.IsFeedbackGiven == true)
                                                .Select(c => new
                                                {
                                                    Id = c.Course.Id,
                                                    Name = c.Course.Name
                                                }).ToList();

                    var _paidCount = _dbRegn.StudentReceipts.Where(r => r.Status == true).Count();
                    var _installmentNo = from EnumClass.InstallmentNo b in Enum.GetValues(typeof(EnumClass.InstallmentNo))
                                         where ((int)b > _paidCount)
                                         select new { Id = (int)b, Name = (int)b };
                    var _roundUpList = from EnumClass.RoundUpList r in Enum.GetValues(typeof(EnumClass.RoundUpList))
                                       select new { Id = (int)r, Name = r.ToString().Replace('_', ' ') };
                    var _taxPercentage = _db.ServiceTaxes
                                   .AsEnumerable()
                                   .Where(t => t.FromDate <= Common.LocalDateTime())
                                   .OrderByDescending(t => t.FromDate)
                                   .First().Percentage;

                    var _courseList = _db.Courses
                                   .Select(c => new
                                   {
                                       Id = c.Id,
                                       Name = c.Name
                                   }).ToList();

                    int _totalCourseInterchangeFee = Convert.ToInt32(EnumClass.COURSEINTERCHANGEFEE.TOTAL_COURSEINTERCHANGE_FEE);
                    int _courseInterchangeAmount = Convert.ToInt32((_totalCourseInterchangeFee) / ((100 + _taxPercentage) / 100));
                    int _courseInterchange_ST_Amount = _totalCourseInterchangeFee - _courseInterchangeAmount;

                    var _mdlCourseInterchange = new CourseInterchangeVM
                    {
                        StudentID = _dbRegn.RegistrationNumber,
                        Email = _studentEmail,
                        MobileNo = _studentMobile,
                        Name = _dbRegn.StudentWalkInn.CandidateName,
                        StudentRegistration = _dbRegn,
                        Curr_MultiCourseCode = string.Join(",", _dbRegn.StudentRegistrationCourses
                                            .Select(src => src.MultiCourse.CourseCode)),
                        Curr_CourseTitle = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                    .Select(src => src.MultiCourse.CourseSubTitle.Name)),
                        Curr_SoftwareDetails = string.Join(Environment.NewLine, _dbRegn.StudentRegistrationCourses
                                             .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                             .Select(mcd => mcd.Course.Name)),
                        CourseFeedbackList = new SelectList(_feedbackCourseIdList, "Id", "Name"),
                        CourseFeedbackId = _feedbackCourseIdList.Select(f => f.Id).ToArray(),
                        InstallmentID = _dbRegn.NoOfInstallment.Value,
                        InstallmentType = (_paidCount < 2 && _dbRegn.FeeMode == (int)EnumClass.InstallmentType.SINGLE)
                                            ? EnumClass.InstallmentType.SINGLE : EnumClass.InstallmentType.INSTALLMENT,
                        InstallmentList = new SelectList(_installmentNo, "Id", "Name"),
                        MultiCourseList = new SelectList("", "Id", "CourseCode"),
                        RoundUpList = new SelectList(_roundUpList, "Id", "Name"),
                        RoundUpId = (int)EnumClass.RoundUpList.ROUND_UP,
                        ST = Convert.ToDouble(_taxPercentage),
                        CentreCode = _dbRegn.StudentWalkInn.CenterCode.CentreCode,
                        PrevCourseId = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                      .Select(rc => rc.MultiCourse.Id)),
                        //CourseIds = string.Join(",", _notFeedbackCourseIds.ToList()),
                        StudentReceipt = _dbRegn.StudentReceipts.ToList(),
                        StudentMobileNo = _dbRegn.StudentWalkInn.MobileNo,
                        CroName = _dbRegn.StudentWalkInn.CROCount.Value == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                                _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name,
                        MgrMobileNo = _mgrMobileNo,
                        Feedback_CourseIds = string.Join(",", _feedbackCourseIdList.Select(f => f.Id).ToList()),//used for validating purpose
                        Curr_Discount = _dbRegn.Discount.Value,
                        CourseInterchangeFee = _courseInterchangeAmount,
                        CourseInterchangeST = _courseInterchange_ST_Amount,
                        CourseList = new SelectList(_courseList, "Id", "Name")
                    };

                    return View(_mdlCourseInterchange);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult CourseInterchange(CourseInterchangeVM mdlCourseInterchange)
        {
            StudentRegistration _dbRegn = new StudentRegistration();
            StudentRegistration_History _dbRegn_History = new StudentRegistration_History();
            List<int> _newFeedbackCourseIds = new List<int>();
            SuccessMessage _successMsg = new SuccessMessage();
            int _totalCourseFee = 0;
            int _totalST = 0;
            int _totalAmount = 0;
            try
            {
                if (ModelState.IsValid)
                {

                    _dbRegn = _db.StudentRegistrations
                         .Where(r => r.Id == mdlCourseInterchange.StudentRegistration.Id)
                         .FirstOrDefault();
                    if (_dbRegn != null)
                    {
                        #region Taking Backup of existing studentregistration details

                        //Back up of student registration
                        _dbRegn_History.Discount = _dbRegn.Discount;
                        _dbRegn_History.FeeMode = _dbRegn.FeeMode;
                        _dbRegn_History.HistoryType = (int)EnumClass.STUDENT_BACKUP_TYPE.COURSE_INTERCHANGE;
                        _dbRegn_History.STPercentage = _dbRegn.STPercentage;
                        _dbRegn_History.TotalAmount = _dbRegn.TotalAmount;
                        _dbRegn_History.TotalCourseFee = _dbRegn.TotalCourseFee;
                        _dbRegn_History.TotalDuration = _dbRegn.TotalDuration;
                        _dbRegn_History.TotalSTAmount = _dbRegn.TotalSTAmount;
                        _dbRegn_History.TransactionDate = Common.LocalDateTime();
                        _dbRegn_History.CROID = Convert.ToInt32(Session["LoggedUserId"]);
                        _dbRegn_History.IsModifiedByEmployee = true;
                        _dbRegn_History.AdditionalCourseFee = mdlCourseInterchange.CourseInterchangeFee;
                        _dbRegn_History.AdditionalSTAmount = mdlCourseInterchange.CourseInterchangeST;

                        //Back up of student registration courses
                        foreach (var item in _dbRegn.StudentRegistrationCourses)
                        {
                            StudentRegistrationCourse_History _dbRegn_Course_History = new StudentRegistrationCourse_History();
                            _dbRegn_Course_History.MultiCourseID = item.MultiCourseID;
                            _dbRegn_History.StudentRegistrationCourse_History.Add(_dbRegn_Course_History);
                        }

                        //Backup of student student receipt
                        foreach (var item in _dbRegn.StudentReceipts)
                        {
                            StudentReceipt_History _dbReceipt_History = new StudentReceipt_History();
                            _dbReceipt_History.BankName = item.BankName;
                            _dbReceipt_History.BranchName = item.BranchName;
                            _dbReceipt_History.ChDate = item.ChDate;
                            _dbReceipt_History.ChNo = item.ChNo;
                            _dbReceipt_History.CROID = item.CROID;
                            _dbReceipt_History.DueDate = item.DueDate;
                            _dbReceipt_History.Fee = item.Fee;
                            _dbReceipt_History.ModeOfPayment = item.ModeOfPayment;
                            _dbReceipt_History.PDCSlip = item.PDCSlip;
                            _dbReceipt_History.ST = item.ST;
                            _dbReceipt_History.Status = item.Status;
                            _dbReceipt_History.STPercentage = item.STPercentage;
                            _dbReceipt_History.StudentReceiptNo = item.StudentReceiptNo;
                            _dbReceipt_History.Total = item.Total;


                            _dbRegn_History.StudentReceipt_History.Add(_dbReceipt_History);
                        }

                        _dbRegn.StudentRegistration_History.Add(_dbRegn_History);
                        #endregion
                        //Removing existing course details
                        foreach (var _existingCourse in _dbRegn.StudentRegistrationCourses.ToList())
                        {
                            _db.StudentRegistrationCourses.Remove(_existingCourse);
                        }

                        //Remove existing receipts
                        foreach (var _existingReceipt in _dbRegn.StudentReceipts.ToList())
                        {
                            _db.StudentReceipts.Remove(_existingReceipt);
                        }


                        //saving student course details
                        foreach (var courseId in mdlCourseInterchange.MultiCourseId)
                        {
                            StudentRegistrationCourse _studCourse = new StudentRegistrationCourse();
                            _studCourse.MultiCourseID = Convert.ToInt32(courseId);
                            _dbRegn.StudentRegistrationCourses.Add(_studCourse);
                        }

                        //adding new receipt
                        foreach (var _receipt in mdlCourseInterchange.StudentReceipt)
                        {
                            //adding coursefee and totalst details
                            _totalCourseFee = _totalCourseFee + _receipt.Fee.Value;
                            _totalST = _totalST + _receipt.ST.Value;

                            StudentReceipt _studReceipt = new StudentReceipt();
                            _studReceipt.DueDate = _receipt.DueDate;
                            _studReceipt.Fee = _receipt.Fee;
                            _studReceipt.ST = _receipt.ST;
                            _studReceipt.Status = _receipt.Status;
                            _studReceipt.Total = _receipt.Total;
                            _studReceipt.STPercentage = _receipt.STPercentage;
                            _studReceipt.StudentReceiptNo = _receipt.StudentReceiptNo;
                            _studReceipt.CROID = _receipt.CROID;
                            _studReceipt.ModeOfPayment = _receipt.ModeOfPayment;
                            _dbRegn.StudentReceipts.Add(_studReceipt);
                        }

                        _totalAmount = _totalCourseFee + _totalST;
                        _dbRegn.STPercentage = mdlCourseInterchange.ST;
                        _dbRegn.TotalSTAmount = _totalST;
                        _dbRegn.TotalCourseFee = _totalCourseFee;
                        _dbRegn.TotalAmount = _totalAmount;
                        _dbRegn.FeeMode = mdlCourseInterchange.InstallmentType == EnumClass.InstallmentType.SINGLE ? (int)EnumClass.InstallmentType.SINGLE : (int)EnumClass.InstallmentType.INSTALLMENT;
                        _dbRegn.NoOfInstallment = _dbRegn.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? 0 : mdlCourseInterchange.InstallmentID;
                        _dbRegn.MultiCourseIDs = string.Join(",", mdlCourseInterchange.MultiCourseId
                                                                       .Where(mc => !string.IsNullOrEmpty(mc))
                                                                       .Select(mc => Int32.Parse(mc))
                                                                       .OrderBy(mc => mc)
                                                                       .ToList());
                        _dbRegn.Discount = mdlCourseInterchange.Curr_Discount;

                        //removing non feedback courses 
                        foreach (var _existingFeedback in _dbRegn.StudentFeedbacks.Where(f => f.IsFeedbackGiven == false).ToList())
                        {
                            _db.StudentFeedbacks.Remove(_existingFeedback);
                        }

                        //adding newly added course in studentfeedback
                        string[] _arrCourseId = mdlCourseInterchange.NewCourseIds
                                            .Split(',')
                                            .Where(x => !string.IsNullOrEmpty(x))
                                            .OrderBy(x => x.ToString()).ToArray();
                        foreach (var _newCourseId in _arrCourseId)
                        {
                            StudentFeedback _studentFeedback = _dbRegn.StudentFeedbacks
                                                            .Where(f => f.Course.Id == Convert.ToInt32(_newCourseId))
                                                            .FirstOrDefault();
                            //adding new feedback
                            if (_studentFeedback == null)
                            {
                                _newFeedbackCourseIds.Add(Convert.ToInt32(_newCourseId));
                            }
                        }

                        //adding new coursedids to feedback
                        foreach (var _newFeedbackCourseId in _newFeedbackCourseIds)
                        {
                            StudentFeedback _newFeedback = new StudentFeedback();
                            _newFeedback.CourseId = Convert.ToInt32(_newFeedbackCourseId);
                            _dbRegn.StudentFeedbacks.Add(_newFeedback);
                        }

                        //updating student feedback
                        var m = 0;
                        var count = _dbRegn.StudentFeedbacks.Count();
                        int _totalCourseAmt = 0;
                        foreach (var feedback in _dbRegn.StudentFeedbacks)
                        {
                            // if not is last feedback then normal procedure is followed
                            if (++m != count)
                            {
                                //Gets each course in feedback
                                var _course = _db.Courses
                                             .Where(c => c.Id == feedback.CourseId).FirstOrDefault();

                                var _currDiscount = _dbRegn_History.Discount.Value;//getting the old discount value
                                var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                                var _currCourseAmt = mdlCourseInterchange.InstallmentType == EnumClass.InstallmentType.SINGLE ? _course.SingleFee : _course.InstallmentFee;
                                var _currTotalAmt = Convert.ToInt32(_currCourseAmt * _currDiscPercentage);
                                _totalCourseAmt = Convert.ToInt32(_totalCourseAmt + _currTotalAmt);

                                feedback.TotalCourseAmount = _currTotalAmt;
                            }
                            //if last item courseamount in feedback=TotalCourseFee-sum(individual course amount)
                            else
                            {
                                feedback.TotalCourseAmount = _totalCourseFee - _totalCourseAmt;
                            }
                        }

                        _db.Entry(_dbRegn).State = EntityState.Modified;
                        int j = _db.SaveChanges();
                        if (j > 0)
                        {
                            _successMsg.RegistrationId = _dbRegn.Id;
                            _successMsg.Status = "success";

                            return Json(_successMsg, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                _successMsg.Status = "error";
                return Json(_successMsg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _successMsg.Status = ex.Message;
                return Json(_successMsg, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCourseDetails(int[] multiCourseId, string feeMode, int regId)
        {

            List<CourseDetails> _courseDetails = new List<CourseDetails>();
            CourseDetails _objCourseDetails = new CourseDetails();
            try
            {

                var _dbRegn = _db.StudentRegistrations
                             .Where(r => r.Id == regId)
                             .FirstOrDefault();


                List<MultiCourse> _lstMultiCourse = new List<MultiCourse>();
                _lstMultiCourse = _db.MultiCourses
                                .Where(mcd => multiCourseId.Contains(mcd.Id))
                                .ToList();
                if (_lstMultiCourse.Count > 0)
                {
                    _objCourseDetails.CourseCode = string.Join(",", _lstMultiCourse.Select(x => x.CourseCode).ToList());
                    _objCourseDetails.CourseTitle = string.Join(",", _lstMultiCourse.Select(x => x.CourseSubTitle.Name.ToUpper()).ToList());
                    _objCourseDetails.Duration = _lstMultiCourse.Sum(x => x.Duration.Value);
                    _objCourseDetails.Fee = feeMode == "SINGLE" ? _lstMultiCourse.Sum(s => s.SingleFee.Value) : _lstMultiCourse.Sum(s => s.InstallmentFee.Value);
                    _objCourseDetails.SoftwareUsed = string.Join(Environment.NewLine, _lstMultiCourse.SelectMany(x => x.MultiCourseDetails
                                                                    .Select(y => y.Course.Name).ToList()));
                    _objCourseDetails.CourseIds = string.Join(",", _lstMultiCourse.SelectMany(x => x.MultiCourseDetails
                                                                   .Select(y => y.Course.Id).ToList()));
                    _objCourseDetails.StudentRegistration = _dbRegn;
                    _objCourseDetails.currST = _db.ServiceTaxes
                                               .AsEnumerable()
                                               .Where(t => t.FromDate <= Common.LocalDateTime())
                                               .OrderByDescending(t => t.FromDate)
                                               .First().Percentage.Value;
                }
                else
                {
                    _objCourseDetails.CourseCode = "";
                    _objCourseDetails.CourseTitle = "";
                    _objCourseDetails.Duration = 0;
                    _objCourseDetails.Fee = 0;
                    _objCourseDetails.SoftwareUsed = "";
                    _objCourseDetails.CourseIds = "";
                    _objCourseDetails.StudentRegistration = null;
                }

                _courseDetails.Add(_objCourseDetails);


                var _courseUpdatedDetails = _courseDetails
                                         .Select(mc => new
                                         {
                                             CourseCode = mc.CourseCode,
                                             CourseTitle = mc.CourseTitle,
                                             Duration = mc.Duration,
                                             CourseFee = mc.CourseFee_Updated,
                                             STAmount = mc.STAmt_Updated,
                                             DiscountPercentage = mc.Discount_Updated,
                                             SoftwareUsed = mc.SoftwareUsed,
                                             CourseIds = mc.CourseIds
                                         }).ToList();

                return Json(_courseUpdatedDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        //send pinno to the student
        public JsonResult MobileVerification(string studMobileNo, int prevCourseFee, string prevCourseCode, int currCourseFee, string currCourseCode, string studentID, string studentName)
        {
            int pinNo = 0;
            try
            {
                Common _cmn = new Common();
                //Gets RandomNo
                int _randomNo = _cmn.GenerateRandomNo();

                string _message = "Course Change \n" +
                                   studentID + "\n" +
                                   studentName.ToUpper() + "\n" +
                                   "Old: " + prevCourseCode + "," + prevCourseFee + " plus ST" + "\n" +
                                   "New: " + currCourseCode + "," + currCourseFee + " plus ST" + "\n" +
                                   "Agree: " + _randomNo;
                //Sends the 4 digitno to the sutdent mobileNo
                string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + studMobileNo + "&msg=" + _message + " &route=T");
                if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                {
                    pinNo = _randomNo;
                    return Json(pinNo, JsonRequestBehavior.AllowGet);

                }
                return Json(pinNo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(pinNo, JsonRequestBehavior.AllowGet);
            }
        }

        public static FeeCalculation CalculateCourseFee(StudentRegistration studReg, decimal newCourseFee, decimal currST)
        {
            FeeCalculation _clsFeeCalc = new FeeCalculation();
            try
            {

                decimal discount = studReg.Discount.Value;
                decimal existingCourseFee = studReg.StudentReceipts.Sum(r => r.Fee).Value;
                decimal existingSTAmount = studReg.StudentReceipts.Sum(r => r.ST).Value;
                decimal stAmt = 0;
                decimal currTotalAmt = 0;
                decimal discountAmt = 0;
                decimal newTotalAmt = 0;
                decimal newCourseFee_AftrDiscount = 0;
                decimal newSTAmt_AftrDiscount = 0;

                //TotalAmount Calculation           

                stAmt = Math.Round((newCourseFee) * (currST / 100));
                currTotalAmt = newCourseFee + stAmt;

                //Gets the new discount amount
                discountAmt = Math.Round((currTotalAmt) * (discount / 100));
                //Gets the new Total Amt
                newTotalAmt = currTotalAmt - discountAmt;

                //Applying Roundup
                newTotalAmt = Math.Ceiling((decimal)newTotalAmt / 10) * 10;

                //calculating coursefee from new total amt
                newCourseFee_AftrDiscount = Math.Round((newTotalAmt) / ((100 + currST) / 100));


                if (newCourseFee_AftrDiscount < existingCourseFee)
                {
                    discount = (100 - ((existingCourseFee * 100) / newCourseFee));
                    newCourseFee_AftrDiscount = existingCourseFee;
                    newSTAmt_AftrDiscount = existingSTAmount;
                }
                else
                {
                    //Gets the new st amt
                    newSTAmt_AftrDiscount = Convert.ToInt32(newTotalAmt - newCourseFee_AftrDiscount);
                }



                _clsFeeCalc.CourseFee = Convert.ToInt32(newCourseFee_AftrDiscount);
                _clsFeeCalc.DiscountPercentage = Convert.ToInt32(discount);
                _clsFeeCalc.STAmount = Convert.ToInt32(newSTAmt_AftrDiscount);
            }
            catch (Exception ex)
            {
                _clsFeeCalc.CourseFee = 0;
                _clsFeeCalc.DiscountPercentage = 0;
                _clsFeeCalc.STAmount = 0;
            }

            return _clsFeeCalc;
        }

        #region Mail Sending

        //sending email to student regarding new course details
        public JsonResult SendMail(int studentRegID)
        {
            try
            {
                StudentRegistration _studReg = _db.StudentRegistrations
                                        .Where(r => r.Id == studentRegID)
                                        .FirstOrDefault();
                string studEmailId = _studReg.StudentWalkInn.EmailId;

                Common _cmn = new Common();
                string _body = PopulateBody(_studReg);
                //Email sending
                bool isMailSend = _cmn.SendEmailTesting(studEmailId, _body, "Student Course Interchange");
                if (isMailSend)
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("error", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
           

        }

        //sending email to student regarding new course details
        private string PopulateBody(StudentRegistration studReg)
        {
            string _body = string.Empty;
            string _studName = studReg.StudentWalkInn.CandidateName.ToUpper();
            string _studID = studReg.RegistrationNumber;
            string _courseCode = string.Join(",", studReg.StudentRegistrationCourses.Select(src => src.MultiCourse.CourseCode));
            int _courseFee = studReg.StudentReceipts.Sum(r => r.Fee.Value);
            string _softwareUsed = string.Join(",", studReg.StudentRegistrationCourses.SelectMany(src => src.MultiCourse.MultiCourseDetails
                                                                    .Select(sr => sr.Course.Name)));
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/CourseUpgradationDetails_Template.html")))
            {
                _body = reader.ReadToEnd();
            }

            _body = _body.Replace("{EmailHeader}", "Course Interchange");
            _body = _body.Replace("{EmailCaption}", "You are receiving this email because you have opted for course interchange");
            _body = _body.Replace("{StudentName}", _studName);
            _body = _body.Replace("{StudentID}", _studID);
            _body = _body.Replace("{CourseList}", _courseCode);
            _body = _body.Replace("{CourseFee}", _courseFee.ToString());
            _body = _body.Replace("{CourseName}", _softwareUsed);


            return _body;
        }
        #endregion
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
