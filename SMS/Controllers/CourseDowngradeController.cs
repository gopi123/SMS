using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CourseDowngradeController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();

        public class CourseDetails
        {
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public string SoftwareUsed { get; set; }
            public int Duration { get; set; }
            public decimal Fee { get; set; }
            public string CourseIds { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public int Fee_Updated
            {
                get
                {
                    int _newFee = 0;
                    var _paidFee = StudentRegistration.StudentReceipts
                                .Where(r => r.Status == true)
                                .Sum(r => r.Fee);
                    if (_paidFee > Fee)
                    {
                        _newFee = _paidFee.Value;
                    }
                    else
                    {
                        _newFee = Convert.ToInt32(Fee);
                    }

                    return _newFee;
                }
            }
        }
        //
        // GET: /CourseDowngrade/     

        public ActionResult CourseDowngrade(int regId)
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
                    List<int> _feedbackCourseIdList = _dbRegn.StudentFeedbacks
                                           .Where(f => f.IsFeedbackGiven == true)
                                           .Select(f => f.Course.Id).ToList();

                    List<int> _courseIds = _dbRegn.StudentRegistrationCourses
                                        .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                        .Select(mcd => mcd.Course.Id).ToList();

                    List<int> _notFeedbackCourseIds = _courseIds.Where(c => !_feedbackCourseIdList.Contains(c))
                                                    .ToList();

                    var _notFeedbackCourseList = _db.Courses
                                               .Where(c => _notFeedbackCourseIds.Contains(c.Id))
                                               .Select(c => new
                                               {
                                                   Id = c.Id,
                                                   Name = c.Name
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

                    var _mdlCourseDwongrade = new CourseDowngradeVM
                    {

                        Email = _studentEmail,
                        MobileNo = _studentMobile,
                        Name = _dbRegn.StudentWalkInn.CandidateName,
                        StudentRegistration = _dbRegn,
                        Curr_MultiCourseCode = string.Join(",", _dbRegn.StudentRegistrationCourses
                                            .Select(src => src.MultiCourse.CourseCode)),

                        Curr_CourseTitle = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                    .Select(src => src.MultiCourse.CourseSubTitle.Name)),
                        Curr_SoftwareDetails = string.Join(",", _dbRegn.StudentRegistrationCourses
                                             .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                             .Select(mcd => mcd.Course.Name)),
                        CourseDowngradeList = new SelectList(_notFeedbackCourseList, "Id", "Name"),
                        InstallmentID = _dbRegn.NoOfInstallment.Value,
                        InstallmentType = _dbRegn.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? EnumClass.InstallmentType.SINGLE : EnumClass.InstallmentType.INSTALLMENT,
                        InstallmentList = new SelectList(_installmentNo, "Id", "Name"),
                        MultiCourseList = new SelectList("", "Id", "CourseCode"),
                        RoundUpList = new SelectList(_roundUpList, "Id", "Name"),
                        RoundUpId = (int)EnumClass.RoundUpList.ROUND_UP,
                        ST = Convert.ToDouble(_taxPercentage),
                        CentreCode = _dbRegn.StudentWalkInn.CenterCode.CentreCode,
                        PrevCourseId = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                      .Select(rc => rc.MultiCourse.Id)),
                        CourseIds = string.Join(",", _notFeedbackCourseIds.ToList()),
                        StudentReceiptList = _dbRegn.StudentReceipts.ToList(),
                        StudentMobileNo = _dbRegn.StudentWalkInn.MobileNo,
                        CroName = _dbRegn.StudentWalkInn.CROCount.Value == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                                _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name,
                        MgrMobileNo = _mgrMobileNo
                    };

                    return View(_mdlCourseDwongrade);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public JsonResult CourseDowngrade_Post(CourseDowngradeVM mdlCourseDowngrade)
        {
            if (mdlCourseDowngrade.InstallmentType == EnumClass.InstallmentType.SINGLE)
            {
                ModelState.Remove("InstallmentID");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        int _totalCourseFee = 0;
                        int _totalST = 0;
                        int _totalAmount = 0;
                        Common _cmn = new Common();

                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        var _dbRegistration = _db.StudentRegistrations
                                           .Where(r => r.Id == mdlCourseDowngrade.StudentRegistration.Id)
                                           .SingleOrDefault();
                        //Remove existing receipts
                        foreach (var _existingReceipt in _dbRegistration.StudentReceipts.ToList())
                        {
                            _db.StudentReceipts.Remove(_existingReceipt);
                        }

                        //saving receipt details                            
                        foreach (var _receipt in mdlCourseDowngrade.StudentReceiptList)
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
                            _dbRegistration.StudentReceipts.Add(_studReceipt);
                        }

                        _totalAmount = _totalCourseFee + _totalST;
                        _dbRegistration.STPercentage = mdlCourseDowngrade.ST;
                        _dbRegistration.TotalSTAmount = _totalST;
                        _dbRegistration.TotalCourseFee = _totalCourseFee;
                        _dbRegistration.TotalAmount = _totalAmount;
                        _dbRegistration.FeeMode = mdlCourseDowngrade.InstallmentType == EnumClass.InstallmentType.SINGLE ? (int)EnumClass.InstallmentType.SINGLE : (int)EnumClass.InstallmentType.INSTALLMENT;
                        _dbRegistration.NoOfInstallment = _dbRegistration.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? 0 : mdlCourseDowngrade.InstallmentID;
                        _dbRegistration.MultiCourseIDs = string.Join(",", mdlCourseDowngrade.MultiCourseId
                                                                       .Where(mc => !string.IsNullOrEmpty(mc))
                                                                       .Select(mc => Int32.Parse(mc))
                                                                       .OrderBy(mc => mc)
                                                                       .ToList());

                        //removing studentregistrationcourses
                        foreach (var _existingCourses in _dbRegistration.StudentRegistrationCourses.ToList())
                        {
                            _db.StudentRegistrationCourses.Remove(_existingCourses);
                        }

                        //saving student course details
                        foreach (var courseId in mdlCourseDowngrade.MultiCourseId)
                        {
                            StudentRegistrationCourse _studCourse = new StudentRegistrationCourse();
                            _studCourse.MultiCourseID = Convert.ToInt32(courseId);
                            _dbRegistration.StudentRegistrationCourses.Add(_studCourse);
                        }

                        //removing the course selected
                        foreach (var _existingFeedBack in _dbRegistration.StudentFeedbacks
                                                         .Where(f => f.CourseId == mdlCourseDowngrade.CourseDowngradeId)
                                                         .ToList())
                        {
                            //_db.StudentFeedbacks.Remove(_existingFeedBack);
                            _dbRegistration.StudentFeedbacks.Remove(_existingFeedBack);
                        }

                        //updating student feedback
                        var m = 0;
                        var count = _dbRegistration.StudentFeedbacks.Count();
                        int _totalCourseAmt = 0;
                        foreach (var feedback in _dbRegistration.StudentFeedbacks)
                        {
                            // if not is last feedback then normal procedure is followed
                            if (++m != count)
                            {
                                //Gets each course in feedback
                                var _course = _db.Courses
                                             .AsEnumerable()
                                             .Where(c => c.Id == feedback.Course.Id).FirstOrDefault();

                                var _currDiscount = 0;
                                var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                                var _currCourseAmt = mdlCourseDowngrade.InstallmentType == EnumClass.InstallmentType.SINGLE ? _course.SingleFee : _course.InstallmentFee;
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


                        _db.Entry(_dbRegistration).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            int l = _cmn.AddTransactions(actionName, controllerName, "");
                            if (l > 0)
                            {
                                _ts.Complete();
                            }
                            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        /// <summary>
        ///  This function is called on selecting degrade course
        ///  This function returns course code details
        /// </summary>
        /// <param name="courseIds"></param>
        /// <returns></returns>
        public JsonResult GetCourseCode(int downgradeCourseId, int regId)
        {
            string _courseCode = string.Empty;
            try
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regId)
                            .FirstOrDefault();

                //Gets the courseid list of a particular student
                List<int> _courseIdList = _dbRegn.StudentRegistrationCourses
                                          .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                          .Select(mcd => mcd.Course.Id).ToList();

                //Gets the feedback course ids
                List<int> _feedbackCourseIds = _dbRegn.StudentFeedbacks
                                                .Where(f => f.IsFeedbackGiven == true)
                                                .Select(c => c.Course.Id).ToList();

                //Removes the feedback course from the courseIdList
                List<int> _courseIdList_afterRemoval_feedbackCourseIds = _courseIdList.Where(c => !_feedbackCourseIds.Any(f => f == c))
                                                               .ToList();

                //Gets the courseid list after course downgrade
                List<int> _afterDowngradeCourseIdList = _courseIdList_afterRemoval_feedbackCourseIds
                                                    .Where(c => downgradeCourseId != c)
                                                    .OrderBy(c => c)
                                                    .ToList();


                var _courseCodeDetails = _db.MultiCourseDetails
                                         .Where(mcd => _afterDowngradeCourseIdList.Contains(mcd.Course.Id))
                                         .Select(mc => new
                                         {
                                             Id = mc.MultiCourse.Id,
                                             Name = mc.MultiCourse.CourseCode
                                         })
                                         .Distinct()
                                         .ToList();

                if (_courseCodeDetails != null)
                {
                    return Json(_courseCodeDetails, JsonRequestBehavior.AllowGet);
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCourseDetails(int multiCourseId, string feeMode, int regId)
        {
            List<CourseDetails> _courseDetails = new List<CourseDetails>();
            try
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regId)
                            .FirstOrDefault();
                //Single Fee
                if (feeMode == "SINGLE")
                {
                    _courseDetails = _db.MultiCourses
                                   .Where(mc => mc.Id == multiCourseId)
                                   .AsEnumerable()
                                   .Select(mc => new CourseDetails
                                   {
                                       CourseCode = mc.CourseCode,
                                       CourseTitle = mc.CourseSubTitle.Name,
                                       Duration = mc.Duration.Value,
                                       Fee = mc.SingleFee.Value,
                                       SoftwareUsed = mc.MultiCourseDetails
                                                        .Select(m => m.Course.Name)
                                                        .Aggregate((m, n) => m + "," + n),
                                       CourseIds = string.Join(",", mc.MultiCourseDetails
                                                                 .Select(m => m.Course.Id)),
                                       StudentRegistration = _dbRegn
                                   }).ToList();
                }
                //Installment Fee
                else
                {
                    _courseDetails = _db.MultiCourses
                                   .Where(mc => mc.Id == multiCourseId)
                                   .AsEnumerable()
                                   .Select(mc => new CourseDetails
                                   {
                                       CourseCode = mc.CourseCode,
                                       CourseTitle = mc.CourseSubTitle.Name,
                                       Duration = mc.Duration.Value,
                                       Fee = mc.InstallmentFee.Value,
                                       SoftwareUsed = mc.MultiCourseDetails
                                                        .Select(c => c.Course.Name)
                                                        .Aggregate((m, n) => m + "," + n),
                                       CourseIds = string.Join(",", mc.MultiCourseDetails
                                                                .Select(m => m.Course.Id)),
                                       StudentRegistration = _dbRegn
                                   }).ToList();
                }

                var _courseUpdatedDetails = _courseDetails
                                           .Select(mc => new
                                           {
                                               CourseCode = mc.CourseCode,
                                               CourseTitle = mc.CourseTitle,
                                               Duration = mc.Duration,
                                               Fee_Updated = mc.Fee_Updated,
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

        public JsonResult MobileVerification(string studMobileNo, int prevCourseFee, string prevCourseCode, int currCourseFee, string currCourseCode)
        {
            int pinNo = 0;
            try
            {
                Common _cmn = new Common();
                //Gets RandomNo
                int _randomNo = _cmn.GenerateRandomNo();

                string _message = "Pin(" + _randomNo + "), I agree to downgrade from " + prevCourseCode.ToUpper() + "(Rs." + prevCourseFee + " plus ST) to "
                        + currCourseCode.ToUpper() + "(Rs." + currCourseFee + "plus ST). Any excess paid will not be refunded.";
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

        public JsonResult MobileVerification_Mgr(string mgrMobileNo, int prevCourseFee, string prevCourseCode, int currCourseFee, string currCourseCode, string studentName, string croName)
        {
            int pinNo = 0;
            try
            {
                Common _cmn = new Common();
                //Gets RandomNo
                int _randomNo = _cmn.GenerateRandomNo();

                string _message = "Student-" + studentName + ", CRO-" + croName + ". Pin(" + _randomNo + "), I agree to downgrade from " + prevCourseCode.ToUpper() + "(Rs." + prevCourseFee + " plus ST) to "
                        + currCourseCode.ToUpper() + "(Rs." + currCourseFee + "plus ST). Any excess paid will not be refunded.";
                //Sends the 4 digitno to the sutdent mobileNo
                string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + mgrMobileNo + "&msg=" + _message + " &route=T");
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

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
