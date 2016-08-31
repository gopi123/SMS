
using Microsoft.Reporting.WebForms;
using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class RegistrationController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();

        #region ClassDefinition
        public class CourseList
        {
            public int id { get; set; }
            public string text { get; set; }
        }

        //used to get coursedetails on dropdwon item select
        public class CourseDetails
        {
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public string SoftwareUsed { get; set; }
            public int Duration { get; set; }
            public decimal Fee { get; set; }
            public string CourseIds { get; set; }
        }

        //used for student serial number creation
        public class SerialNoDetails
        {
            public int SerialNo { get; set; }
            public string CenterCode { get; set; }
        }

        public class CentreCodeDetails
        {
            public int CentreId { get; set; }
            public string CentreCode { get; set; }
        }

        public class SuccessMessage
        {
            public int RegistrationId { get; set; }
            public string Status { get; set; }
            public string RegistrationNo { get; set; }
            public string PdfName { get; set; }
        }

        public class SkippingMonth
        {
            public int Fee { get; set; }
            public int ST { get; set; }
            public int Amount { get; set; }
            public string DueDate { get; set; }
            public string RecNo { get; set; }
            public bool Status { get; set; }

        }

        public class clsReceipt
        {
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public string CourseTitle { get; set; }
            public string Duration { get; set; }
            public string PaymentType { get; set; }
            public string PaymentMode { get; set; }
            public string CroName { get; set; }
            public string StudentMobileNo { get; set; }
            public string StudentEmail { get; set; }
            public string CentreCode { get; set; }
            public string STRegnNo { get; set; }
            public string CentreCodeAddress { get; set; }
            public decimal PaymentFeeDetails { get; set; }
            public decimal PaymentSTAmount { get; set; }
            public double PaymentSTPercentage { get; set; }
            public decimal PaymentAmountDetails { get; set; }
            public DateTime PaymentDateDetails { get; set; }
            public string ReceiptNoDetails { get; set; }
            public string CentreCodePhoneNo { get; set; }
            public List<StudentReceipt> StudentReceipt { get; set; }
            public DateTime ReceiptDate
            {
                get
                {
                    DateTime _receiptDate = StudentReceipt.Where(r => r.Status == true)
                                          .Last().DueDate.Value.Date;
                    return _receiptDate;
                }
            }
            public int ReceiptNo
            {
                get
                {
                    int _receiptNo = 0;
                    _receiptNo = StudentReceipt.Where(r => r.Status == true)
                                .Last().StudentReceiptNo.Value;
                    return _receiptNo;
                }
            }
            public int CourseFee
            {
                get
                {
                    int _courseFee = 0;
                    _courseFee = StudentReceipt.Where(r => r.Status == true)
                               .Last().Fee.Value;
                    return _courseFee;
                }
            }
            public decimal ServiceTax
            {
                get
                {
                    decimal _serviceTax = 0;
                    _serviceTax = StudentReceipt.Where(r => r.Status == true)
                               .Last().ST.Value;
                    return _serviceTax;
                }
            }
            public decimal TotalAmount
            {
                get
                {
                    decimal _totalAmount = 0;
                    _totalAmount = StudentReceipt.Where(r => r.Status == true)
                               .Last().Total.Value;
                    return _totalAmount;
                }
            }
            public string TotalAmountInWords
            {
                get
                {
                    Common _cmn = new Common();
                    string _strTotalAmount = string.Empty;
                    int _totalAmount = StudentReceipt.Where(r => r.Status == true)
                                        .Last().Total.Value;
                    _strTotalAmount = _cmn.NumbersToWords(_totalAmount);
                    return _strTotalAmount;

                }
            }
            public decimal BalancePayable
            {
                get
                {
                    decimal _balanceAmt = 0;
                    int _count = 0;
                    _count = StudentReceipt.Where(r => r.Status == false).Count();
                    if (_count > 0)
                    {
                        _balanceAmt = StudentReceipt.Where(r => r.Status == false)
                                    .Sum(r => r.Fee.Value);
                    }
                    else
                    {
                        _balanceAmt = 0;
                    }
                    return _balanceAmt;
                }
            }
            public string CrossCheckedBy
            {
                get
                {
                    string _empName = string.Empty;
                    _empName = StudentReceipt.Where(r => r.Status == true)
                                .Last().Employee.Name;
                    return _empName;
                }
            }
            public string CourseCode { get; set; }


        }

        public class clsReceipt_Print
        {
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public string STRegNo { get; set; }
            public string RecNo { get; set; }
            public string RecDate { get; set; }
            public string CourseFee { get; set; }
            public string STPercentage { get; set; }
            public string STAmount { get; set; }
            public string TotalAmount { get; set; }
        }

        #endregion


        // GET: /Registration/
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
                    MonthName = Common.LocalDateTime().ToString("MMMM")
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
                List<RegistraionVM.RegDataTable> _dTableReg = new List<RegistraionVM.RegDataTable>();
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

                _dTableReg = _db.StudentRegistrations
                                .AsEnumerable()
                                .Where(r => _centerCodeIds.Contains(r.StudentWalkInn.CenterCode.Id)
                                            && (r.TransactionDate.Value.Date >= _startFinDate && r.TransactionDate.Value.Date <= _endFinDate))
                                .Select(r => new RegistraionVM.RegDataTable
                                {
                                    Centre = r.RegistrationNumber,
                                    CourseFee = r.TotalCourseFee.Value,
                                    Discount = r.Discount.Value,
                                    CurrEmpId = Int32.Parse(Session["LoggedUserId"].ToString()),
                                    WalkInn = r.StudentWalkInn,
                                    Receipt = r.StudentReceipts.Where(rc => rc.Status == false).FirstOrDefault(),
                                    RegDate = r.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                    RegistrationID = r.Id,
                                    SalesPerson = r.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ?
                                                                            r.StudentWalkInn.Employee1.Name :
                                                                            r.StudentWalkInn.Employee1.Name + "," + r.StudentWalkInn.Employee2.Name,
                                    StudentName = r.StudentWalkInn.CandidateName,
                                    SoftwareUsed = string.Join(",", r.StudentRegistrationCourses
                                                         .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                         .Select(mc => mc.Course.Name))),
                                    IsSalesIndividual = _currentRole == (int)EnumClass.Role.SALESINDIVIDUAL ? true : false

                                })
                                .OrderByDescending(r => r.RegistrationID)
                                .ToList();

                var _regList = _dTableReg
                                .Select(x => new
                                {
                                    RegDate = x.RegDate,
                                    Centre = x.Centre,
                                    SalesPerson = x.SalesPerson,
                                    StudentName = x.StudentName,
                                    MobileNo = x.MobileNo,
                                    SoftwareUsed = x.SoftwareUsed,
                                    Discount = x.Discount,
                                    CourseFee = x.CourseFee,
                                    NextDueDate = x.NextDueDate,
                                    NextDueAmount = x.NextDueAmount,
                                    RegistrationID = x.RegistrationID
                                }).ToList();


                return Json(new { data = _regList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Add: /Registration/
        public ActionResult Add(int walkInnId)
        {
            try
            {


                var _walkInnDetails = _db.StudentWalkInns
                                    .Where(s => s.Id == walkInnId).FirstOrDefault();

                var _taxPercentage = _db.ServiceTaxes
                                    .AsEnumerable()
                                    .Where(t => t.FromDate <= Common.LocalDateTime())
                                    .OrderByDescending(t => t.FromDate)
                                    .First().Percentage;

                if (_walkInnDetails != null)
                {
                    var _installmentNo = from EnumClass.InstallmentNo b in Enum.GetValues(typeof(EnumClass.InstallmentNo))
                                         select new { Id = (int)b, Name = (int)b };

                    var _roundUpList = from EnumClass.RoundUpList r in Enum.GetValues(typeof(EnumClass.RoundUpList))
                                       select new { Id = (int)r, Name = r.ToString().Replace('_', ' ') };

                    var _courseList = _db.Courses
                                    .Select(c => new
                                    {
                                        Id = c.Id,
                                        Name = c.Name
                                    }).ToList();

                    var _studentReg = new RegistraionVM
                    {
                        RegistrationVenue = EnumClass.RegistrationVenue.CENTRE,
                        Name = _walkInnDetails.CandidateName,
                        MobileNo = _walkInnDetails.MobileNo,
                        Email = _walkInnDetails.EmailId,
                        MultiCourseList = new SelectList("", "Id", "CourseCode"),
                        InstallmentType = EnumClass.InstallmentType.SINGLE,
                        InstallmentList = new SelectList(_installmentNo, "Id", "Name"),
                        ST = Convert.ToDouble(_taxPercentage),
                        RoundUpList = new SelectList(_roundUpList, "Id", "Name"),
                        RoundUpId = (int)EnumClass.RoundUpList.ROUND_UP,
                        WalkInnID = _walkInnDetails.Id,
                        DefaultDiscountPercentage = (int)EnumClass.DiscountPercentage.DISCOUNT,
                        CentreCode = _walkInnDetails.CenterCode.CentreCode,
                        CROName = _walkInnDetails.CROCount == (int)EnumClass.CROCount.ONE ? _walkInnDetails.Employee1.Name : _walkInnDetails.Employee1.Name + "," + _walkInnDetails.Employee2.Name,
                        CentreId = _walkInnDetails.CenterCode.Id,
                        CourseList = new SelectList(_courseList, "Id", "Name")

                    };

                    return View(_studentReg);
                }
                return RedirectToAction("Index", "WalkInn");
            }
            catch (Exception ex)
            {
                return View("Error");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(RegistraionVM mdlRegistration)
        {
            try
            {

                if (mdlRegistration.InstallmentType == EnumClass.InstallmentType.SINGLE)
                {
                    ModelState.Remove("InstallmentID");
                }
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string _extension = "";
                        string _imgFileName = "";
                        string _imgPath = "";
                        string _imgSavePath = "";
                        string _studentRegNo = "";
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                        Common _cmn = new Common();
                        SuccessMessage _successMsg = new SuccessMessage();
                        int _totalAmount = 0;
                        int _totalST = 0;
                        int _totalCourseFee = 0;

                        _studentRegNo = GetStudentRegistrationNo(mdlRegistration.WalkInnID);
                        //if studentregistration number is null
                        if (_studentRegNo == "")
                        {
                            return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            HttpPostedFileBase _photoFile = null;
                            var _studentRegId = _db.StudentRegistrations.Count();
                            if (_studentRegId > 0)
                            {
                                _studentRegId = _db.StudentRegistrations.Max(r => r.Id);
                            }
                            _studentRegId = _studentRegId + 1;


                            if (mdlRegistration.PhotoUrl != null)
                            {
                                _photoFile = mdlRegistration.PhotoUrl;
                                _extension = Path.GetExtension(_photoFile.FileName);
                                _imgFileName = _studentRegNo + _extension;
                                _imgPath = "~/UploadImages/Student";
                                _imgSavePath = Path.Combine(Server.MapPath(_imgPath), _imgFileName);
                            }

                            StudentRegistration _studRegn = new StudentRegistration();
                            _studRegn.Discount = mdlRegistration.Discount;
                            _studRegn.TotalDuration = mdlRegistration.Duration;
                            _studRegn.FeeMode = mdlRegistration.InstallmentType == EnumClass.InstallmentType.SINGLE ? (int)EnumClass.InstallmentType.SINGLE : (int)EnumClass.InstallmentType.INSTALLMENT;
                            _studRegn.IsEmailVerified = false;
                            _studRegn.MultiCourseIDs = string.Join(",", mdlRegistration.MultiCourseId);
                            _studRegn.NoOfInstallment = mdlRegistration.InstallmentID;
                            _studRegn.PhotoUrl = mdlRegistration.PhotoUrl != null ? _imgPath + "/" + _imgFileName : "~/UploadImages/Student/NoImageSelected.png";
                            _studRegn.RegistrationVenueID = mdlRegistration.RegistrationVenue == EnumClass.RegistrationVenue.CENTRE ? (int)EnumClass.RegistrationVenue.CENTRE : (int)EnumClass.RegistrationVenue.AT_SITE;
                            _studRegn.RegistrationNumber = _studentRegNo;
                            _studRegn.STPercentage = mdlRegistration.ST;
                            _studRegn.TransactionDate = Common.LocalDateTime();
                            _studRegn.StudentWalkInnID = mdlRegistration.WalkInnID;
                            _studRegn.IsEmailVerified = false;
                            _studRegn.IsPhotoVerified = false;
                            _studRegn.IsPhotoUploaded = mdlRegistration.PhotoUrl != null ? true : false;
                            _studRegn.RoundUpID = mdlRegistration.RoundUpId;
                            _studRegn.PhotoUploadedDate = mdlRegistration.PhotoUrl != null ? Common.LocalDateTime() : (DateTime?)null;
                            _studRegn.IsCertificateIssued = false;
                            _studRegn.IsPhotoRejected = false;

                            //saving student course details
                            foreach (var courseId in mdlRegistration.MultiCourseId)
                            {
                                StudentRegistrationCourse _studCourse = new StudentRegistrationCourse();
                                _studCourse.MultiCourseID = Convert.ToInt32(courseId);
                                _studRegn.StudentRegistrationCourses.Add(_studCourse);
                            }


                            for (int q = 0; q < mdlRegistration.StudentReceipt.Count; q++)
                            {
                                _totalCourseFee = _totalCourseFee + mdlRegistration.StudentReceipt[q].Fee.Value;
                                _totalST = _totalST + mdlRegistration.StudentReceipt[q].ST.Value;


                                StudentReceipt _studReceipt = new StudentReceipt();
                                _studReceipt.DueDate = mdlRegistration.StudentReceipt[q].DueDate;
                                _studReceipt.Fee = mdlRegistration.StudentReceipt[q].Fee;
                                _studReceipt.ST = mdlRegistration.StudentReceipt[q].ST;
                                _studReceipt.Status = q == 0 ? true : false;//first receipt will be paid
                                _studReceipt.Total = mdlRegistration.StudentReceipt[q].Total;
                                _studReceipt.STPercentage = mdlRegistration.ST;
                                _studReceipt.CROID = q == 0 ? Convert.ToInt32(Session["LoggedUserId"].ToString()) : (int?)null;
                                _studReceipt.ModeOfPayment = q == 0 ? (int)EnumClass.PaymentMode.CASH : (int?)null;
                                _studRegn.StudentReceipts.Add(_studReceipt);

                            }

                            _totalAmount = _totalCourseFee + _totalST;
                            _studRegn.TotalSTAmount = _totalST;
                            _studRegn.TotalCourseFee = _totalCourseFee;
                            _studRegn.TotalAmount = _totalAmount;

                            //saving studentfeedback details
                            string[] arrCourseId = mdlRegistration.CourseIds
                                                    .Split(',')
                                                    .Where(x => !string.IsNullOrEmpty(x))
                                                    .OrderBy(x => x.ToString()).ToArray();
                            var isLastItem = false;
                            int _totalCourseAmt = 0;
                            for (int j = 0; j < arrCourseId.Length; j++)
                            {
                                //checking for the last item
                                if (j == arrCourseId.Length - 1)
                                {
                                    isLastItem = true;
                                }
                                //if not last item normal procedure
                                if (!isLastItem)
                                {
                                    var _course = _db.Courses
                                         .AsEnumerable()
                                         .Where(c => c.Id == Int32.Parse(arrCourseId[j])).FirstOrDefault();

                                    var _currDiscount = mdlRegistration.Discount;
                                    var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                                    var _currCourseAmt = mdlRegistration.InstallmentType == EnumClass.InstallmentType.SINGLE ? _course.SingleFee : _course.InstallmentFee;
                                    var _currTotalAmt = Convert.ToInt32(_currCourseAmt * _currDiscPercentage);
                                    _totalCourseAmt = Convert.ToInt32(_totalCourseAmt + _currTotalAmt);


                                    StudentFeedback _studFeedback = new StudentFeedback();
                                    _studFeedback.CourseId = Convert.ToInt32(arrCourseId[j]);
                                    _studFeedback.TotalCourseAmount = _currTotalAmt;
                                    _studFeedback.IsFeedbackGiven = false;
                                    _studFeedback.IsProjectUploaded = false;
                                    _studRegn.StudentFeedbacks.Add(_studFeedback);
                                }
                                //if last item courseamount in feedback=TotalCourseFee-sum(individual course amount)
                                else
                                {
                                    StudentFeedback _studFeedback = new StudentFeedback();
                                    _studFeedback.CourseId = Convert.ToInt32(arrCourseId[j]);
                                    _studFeedback.TotalCourseAmount = _totalCourseFee - _totalCourseAmt;
                                    _studFeedback.IsFeedbackGiven = false;
                                    _studFeedback.IsProjectUploaded = false;
                                    _studRegn.StudentFeedbacks.Add(_studFeedback);
                                }
                            }

                            _db.StudentRegistrations.Add(_studRegn);
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {
                                //updating walkinn status
                                var _studWalkinn = _db.StudentWalkInns.Where(w => w.Id == mdlRegistration.WalkInnID).FirstOrDefault();
                                _studWalkinn.Status = EnumClass.WalkinnStatus.REGISTERED.ToString();
                                _db.Entry(_studWalkinn).State = EntityState.Modified;
                                int j = _db.SaveChanges();
                                if (j > 0)
                                {
                                    //updating student serialnumber table
                                    var _studentSerialNo = _db.StudentSerialNoes
                                                         .Where(s => s.CenterCodeID == _studWalkinn.CenterCodeId).FirstOrDefault();
                                    _studentSerialNo.SerialNo = _studentSerialNo.SerialNo + 1;
                                    _studentSerialNo.SerialNoDate = Common.LocalDateTime();
                                    _db.Entry(_studentSerialNo).State = EntityState.Modified;
                                    int k = _db.SaveChanges();
                                    if (k > 0)
                                    {
                                        int l = _cmn.AddTransactions(actionName, controllerName, "");
                                        if (l > 0)
                                        {
                                            if (_photoFile != null)
                                            {
                                                _photoFile.SaveAs(_imgSavePath);
                                            }
                                            _ts.Complete();
                                            _successMsg.RegistrationId = _studRegn.Id;
                                            _successMsg.Status = "success";
                                        }
                                        return Json(_successMsg, JsonRequestBehavior.AllowGet);
                                    }
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

        //GET:/Get MultiCourseList on dropdown search
        public JsonResult GetCourseList(string term, string value)
        {
            try
            {

                List<CourseList> _courseList = new List<CourseList>();
                try
                {
                    //For first time addition
                    if (value == "")
                    {
                        //Gets all the multicourse details
                        _courseList = _db.MultiCourses
                                      .Where(x => x.CourseCode.StartsWith(term))
                                      .Select(x => new CourseList
                                      {
                                          id = x.Id,
                                          text = x.CourseCode
                                      }).ToList();
                    }
                    //For second time addition
                    else
                    {
                        //Converts the string to integer list
                        List<int> _selectedMCIds = value.Split(',').Select(int.Parse).ToList();

                        //Gets all the childids the selected multicourseids 
                        List<int> _selectedCourseIds = _db.MultiCourseDetails
                                                   .Where(mc => _selectedMCIds.Contains(mc.MultiCourseId.Value))
                                                   .Select(m => m.CourseId.Value).Distinct().ToList();

                        //Gets the multicourseids of selectedcourseids
                        List<int> _allSelectedMCIds = _db.MultiCourseDetails
                                                    .Where(mc => _selectedCourseIds.Contains(mc.CourseId.Value))
                                                    .Select(m => m.MultiCourseId.Value).Distinct().ToList();

                        //Gets all the multicourses which are not in allselectedmcids
                        _courseList = _db.MultiCourses
                                     .Where(x => x.CourseCode.StartsWith(term) && !_allSelectedMCIds.Contains(x.Id))
                                     .Select(x => new CourseList
                                     {
                                         id = x.Id,
                                         text = x.CourseCode
                                     }).ToList();

                    }
                    return Json(_courseList, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        //Get MultiCourseDetails on dropdown change
        public JsonResult GetCourseDetails(int multiCourseId, string feeMode)
        {
            List<CourseDetails> _courseDetails = new List<CourseDetails>();
            try
            {
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
                                                                 .Select(m => m.Course.Id))
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
                                                                .Select(m => m.Course.Id))

                                   }).ToList();
                }
                return Json(_courseDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        //PinNo sending to students mobile
        public JsonResult MobileVerification(string studMobileNo)
        {
            int pinNo = 0;
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    Common _cmn = new Common();
                    //Gets RandomNo
                    int _randomNo = _cmn.GenerateRandomNo();
                    //Adds new MobileVerification details
                    MobileVerification _mobVerification = new MobileVerification();
                    _mobVerification.PinNo = _randomNo;
                    _mobVerification.Type = "S";
                    _mobVerification.TypeId = 0;
                    _mobVerification.MobileNo = studMobileNo;

                    _db.MobileVerifications.Add(_mobVerification);

                    int i = _db.SaveChanges();
                    if (i > 0)
                    {

                        //Sends the 4 digitno to the sutdent mobileNo
                        string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + studMobileNo + "&msg=OTP for registration process in NetworkzSystems is:" + _randomNo + ". By sharing this pin I accept all communications from  NetworkzSystems to this mobile number&route=T");
                        if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                        {
                            _ts.Complete();
                            pinNo = _randomNo;
                            return Json(pinNo, JsonRequestBehavior.AllowGet);

                        }
                    }
                }
                return Json(pinNo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(pinNo, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetStudentRegistrationNo(int walkinnID)
        {
            string _regNo = "";
            try
            {
                int _length = 3;
                int _serialNo = 0;
                string _threeDigitSlNo = "";
                string _centerCode = "";
                Common _cmn = new Common();
                var _startDate = _cmn.GetFinancialYear().StartDate;
                var _endDate = _cmn.GetFinancialYear().EndDate;
                StudentSerialNo _studSerialNo = new StudentSerialNo();


                //gets centerId from walkinnid
                var _centerDetails = _db.StudentWalkInns
                                      .Where(w => w.Id == walkinnID)
                                      .Select(w => new CentreCodeDetails
                                      {
                                          CentreCode = w.CenterCode.CentreCode,
                                          CentreId = w.CenterCode.Id
                                      }).FirstOrDefault();



                //Gets the last serialno of that center
                _studSerialNo = _db.StudentSerialNoes
                                      .Where(n => n.CenterCode.Id == _centerDetails.CentreId &&
                                            EntityFunctions.TruncateTime(n.SerialNoDate.Value) >= _startDate.Date &&
                                            EntityFunctions.TruncateTime(n.SerialNoDate.Value) <= _endDate.Date)
                                      .FirstOrDefault();
                if (_studSerialNo != null)
                {
                    _serialNo = _studSerialNo.SerialNo.Value + 1;
                }
                else
                {
                    _serialNo = 1;

                    _studSerialNo = _db.StudentSerialNoes
                                        .Where(s => (s.CenterCodeID.Value == _centerDetails.CentreId))
                                        .FirstOrDefault();
                    _studSerialNo.SerialNo = 0;
                    _db.Entry(_studSerialNo).State = EntityState.Modified;
                    _db.SaveChanges();
                }


                //gets the three digit serialnumber
                _threeDigitSlNo = _serialNo.ToString().PadLeft(_length, '0');
                //generates the centercodewith first five character
                _centerCode = _centerDetails.CentreCode.Substring(0, 5);
                //gets the integer of month
                string sMonth = Common.LocalDateTime().ToString("MM");
                //gets the last two digits of year
                string sYear = Common.LocalDateTime().ToString("yy");

                _regNo = _centerCode + sMonth + sYear + _threeDigitSlNo;
                return _regNo;


            }
            catch (Exception ex)
            {
                return _regNo;
            }
        }

        //sends discount verification sms to concerned center manager where student has join
        public JsonResult DiscountVerification(string studentName, int discountPercentage, string centreCode)
        {
            int _randomNo = 0;
            try
            {
                Common _cmn = new Common();
                //get the student centre from walkinn
                int _studentCenterId = _db.CenterCodes
                                       .Where(cc => cc.CentreCode == centreCode)
                                       .Select(cc => cc.Id).FirstOrDefault();

                //get the employee who availed discount for student
                string _empName = _db.Employees
                                .AsEnumerable()
                                .Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                .Select(e => e.Name).FirstOrDefault();

                //get the centremanager from centre
                var _employee = _cmn.GetCentreManager(_studentCenterId);

                //if no employee has been assigned to a particular centre
                if (_employee == null)
                {
                    return Json("employee_error", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //gets the centermanager mobileno
                    var _empMobileNo = _employee.OfficialMobileNo;

                    //Gets RandomNo
                    _randomNo = _cmn.GenerateRandomNo();

                    //message for employee
                    var _msg = "Dear " + _employee.Name.ToUpper() + ", Discount of " + discountPercentage + "% has been set for student " + studentName.ToUpper() + " of " + centreCode + " by " + _empName + "." +
                              "By sharing this pin " + _randomNo + " I agree to allow the student to register with the above said discount percentage.";

                    //Sends the 4 digitno to the employee mobileNo
                    string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _empMobileNo + "&msg=" + _msg + "&route=T");
                    if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                    {
                        return Json(_randomNo, JsonRequestBehavior.AllowGet);

                    }
                }
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }

        }

        //send verification mail to regtistered student
        public bool SendMail(int studRegId, string studentRegNo, string studName, string courseList, int courseFee, string studEmailId, string courseName)
        {

            //Saving EmailVerificationDetails
            EmailVerification _emailVerificaiton = new EmailVerification();
            string _key = Guid.NewGuid().ToString();
            _emailVerificaiton.Type = "S";
            _emailVerificaiton.VerificationKey = _key;
            _emailVerificaiton.TypeId = studRegId;
            _db.EmailVerifications.Add(_emailVerificaiton);
            int i = _db.SaveChanges();
            if (i > 0)
            {
                Common _cmn = new Common();
                string _body = PopulateBody(studName, studentRegNo, courseList, courseFee, _key, courseName);
                //Email sending
                var isMailSend = _cmn.SendEmail(studEmailId, _body, "Student Registration");
                return isMailSend;
            }
            return false;


        }

        private string PopulateBody(string studName, string studentRegID, string courseList, int courseFee, string key, string courseName)
        {
            string _body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/StudentRegistrationEmailTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{StudentID}", studentRegID);
            _body = _body.Replace("{CourseList}", courseList);
            _body = _body.Replace("{CourseFee}", courseFee.ToString());
            _body = _body.Replace("{CourseName}", courseName);
            _body = _body.Replace("{ActivationLink}", "http://www.networkzsystems.com/sms/Account/StudentVerification?key=" + key);
            return _body;
        }

        //send confirmation sms to regtistered student
        public bool SendSMS(string studName, string studRegID, string courseList, int courseFee, string mobileNo)
        {
            bool result = false;
            //sending message to student
            var _message = "Dear " + studName + ".Your StudentId is " + studRegID + ",CourseCode is " + courseList + " and Fees Rs" + courseFee + "(plus ST). "
                        + "Your Registration date is" + Common.LocalDateTime().Date.ToString("dd/MM/yyyy");
            Common _cmn = new Common();
            string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + mobileNo + "&msg=" + _message + "&route=T");
            if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
            {
                result = true;
                return result;
            }
            return result;
        }

        public bool SendSMSOffical(string croName, string studRegID, string studName, int discount, string courseList, int courseFee, int centreID)
        {
            bool result = false;
            Common _cmn = new Common();
            //Gets the designationids of employees to whom sms has to be send
            List<int> _desgnList = GetEmpDesignationList(centreID);
            //Gets the mobile no of all the employees
            List<string> _lstOfficalMobNos = _cmn.GetEmployeeDesignationWise(_desgnList)
                                            .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                            .Distinct()
                                            .ToList();
            //converts the list string to comma seperated string
            string _officialMobNos = string.Join(",", _lstOfficalMobNos);
            //sending message to student
            var _message = "Congrats " + croName.ToUpper() + " for the registration of " + studRegID + "(" + studName.ToUpper() + ") on " + Common.LocalDateTime().Date.ToString("dd/MM/yyy") + ", CourseCode - " + courseList + " and Fees - Rs" + courseFee + "(excluding ST). "
                        + "with a discount of " + discount + "%";

            string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _officialMobNos + "&msg=" + _message + "&route=T");
            if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
            {
                result = true;
                return result;
            }
            return result;
        }

        //Gets the designationids of employees to whom sms has to be send
        public List<int> GetEmpDesignationList(int centreID)
        {
            List<int> _designationList = new List<int>();
            //gets the designation of current employee of a particular center(center manager)
            int _currentCMdesignationList = _db.CenterCodes
                                            .Where(c => c.Id == centreID && c.Status == true)
                                            .Select(e => e.Employee.Designation.Id).FirstOrDefault();
            //Adding current offical employee
            _designationList.Add(_currentCMdesignationList);

            //Adding ED+Manager+SalesIndividual
            _designationList.Add((int)EnumClass.Designation.ED);
            _designationList.Add((int)EnumClass.Designation.MANAGERSALES);
            _designationList.Add((int)EnumClass.Designation.ASSISTANTMANAGER);
            _designationList.Add((int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER);

            return _designationList;
        }

        public ActionResult Edit(int RegId, bool redirect)
        {
            try
            {
                Common _cmn = new Common();
                bool _showData = true;

                //Calculating ST Change
                if (CalculateSTChange(RegId))
                {
                    //Gets the studentregistration details
                    var _studentRegistration = _db.StudentRegistrations
                                                .Where(r => r.Id == RegId)
                                                .FirstOrDefault();



                    //Gets the current role of employee
                    var _currentRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));

                    //if concerned cro is the logged user show mobile otherwise not
                    if (_currentRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (_studentRegistration.StudentWalkInn.CRO1ID == Convert.ToInt32(Session["LoggedUserId"].ToString())
                            || _studentRegistration.StudentWalkInn.CRO2ID == Convert.ToInt32(Session["LoggedUserId"].ToString()))
                        {
                            _showData = true;
                        }
                        else
                        {
                            _showData = false;
                        }
                    }


                    if (_studentRegistration != null)
                    {
                        //Gets the student course title
                        var _studentCourseTitle = String.Join(",", _studentRegistration.StudentRegistrationCourses
                                                       .Select(s => s.MultiCourse.CourseSubTitle.Name));

                        //Gets the croname
                        var _croName = _studentRegistration.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ?
                                                                                        _studentRegistration.StudentWalkInn.Employee1.Name :
                                                                                        _studentRegistration.StudentWalkInn.Employee1.Name + "," + _studentRegistration.StudentWalkInn.Employee2.Name;
                        //Gets the multicourse code
                        var _multiCourseCode = String.Join(",", _studentRegistration.StudentRegistrationCourses.
                                                              Select(m => m.MultiCourse.CourseCode));

                        //Gets the installment list
                        var _installmentList = from EnumClass.InstallmentNo b in Enum.GetValues(typeof(EnumClass.InstallmentNo))
                                               select new { Id = (int)b, Name = (int)b };

                        //Gets the multicourseId for displaying in dropdownlist
                        var _multiCourseId = _studentRegistration.StudentRegistrationCourses.Select(s => s.MultiCourseID.Value.ToString()).ToArray();


                        //Gets the multicourseList
                        var _multiCourseList = new SelectList(_studentRegistration.StudentRegistrationCourses.Select(x => x.MultiCourse).ToList(), "Id", "CourseCode");

                        //Gets the roundup list
                        var _roundUpList = from EnumClass.RoundUpList r in Enum.GetValues(typeof(EnumClass.RoundUpList))
                                           select new { Id = (int)r, Name = r.ToString().Replace('_', ' ') };

                        //Gets the softwareused
                        var _softwareUsed = string.Join(",", _studentRegistration.StudentRegistrationCourses
                                                             .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                             .Select(mc => mc.Course.Name)));
                        ////Student MultiCourseId
                        //var _multiCourseId = String.Join(",", _studentRegistration.StudentRegistrationCourses
                        //                                    .Select(rc => rc.MultiCourse.Id));

                        //Tax percentage
                        var _taxPercentage = _db.ServiceTaxes
                                            .AsEnumerable()
                                            .Where(t => t.FromDate <= Common.LocalDateTime())
                                            .OrderByDescending(t => t.FromDate)
                                            .First().Percentage;


                        var _studentRegVM = new RegistraionVM
                        {
                            CentreCode = _studentRegistration.StudentWalkInn.CenterCode.CentreCode,
                            CentreId = _studentRegistration.StudentWalkInn.CenterCode.Id,
                            CourseFee = _studentRegistration.TotalCourseFee.Value,
                            CourseTitle = _studentCourseTitle,
                            CROName = _croName,
                            DefaultDiscountPercentage = (int)EnumClass.DiscountPercentage.DISCOUNT,
                            Discount = _studentRegistration.Discount.Value,
                            Duration = _studentRegistration.TotalDuration.Value,
                            Email = _showData == true ? _studentRegistration.StudentWalkInn.EmailId : _cmn.MaskString(_studentRegistration.StudentWalkInn.EmailId, "email"),
                            InstallmentID = _studentRegistration.NoOfInstallment.Value,
                            InstallmentList = new SelectList(_installmentList, "Id", "Name"),
                            InstallmentType = _studentRegistration.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? EnumClass.InstallmentType.SINGLE : EnumClass.InstallmentType.INSTALLMENT,
                            MobileNo = _showData == true ? _studentRegistration.StudentWalkInn.MobileNo : _cmn.MaskString(_studentRegistration.StudentWalkInn.MobileNo, "mobile"),
                            MultiCourseCode = _multiCourseCode,
                            MultiCourseId = _multiCourseId,
                            MultiCourseList = _multiCourseList,
                            Name = _studentRegistration.StudentWalkInn.CandidateName,
                            RegistrationVenue = _studentRegistration.RegistrationVenueID == (int)EnumClass.RegistrationVenue.CENTRE ? EnumClass.RegistrationVenue.CENTRE : EnumClass.RegistrationVenue.AT_SITE,
                            RoundUpId = _studentRegistration.RoundUpID.Value,
                            RoundUpList = new SelectList(_roundUpList, "Id", "Name"),
                            SoftwareDetails = _softwareUsed,
                            ST = Convert.ToDouble(_taxPercentage),
                            STAmount = _studentRegistration.TotalSTAmount.Value,
                            StudentReceipt = _studentRegistration.StudentReceipts.ToList(),
                            StudentRegistration = _studentRegistration,
                            RegistrationNumber = _studentRegistration.RegistrationNumber,
                            StudentRegistrationID = _studentRegistration.Id,
                            TotalFee = _studentRegistration.TotalAmount.Value,
                            WalkInnID = _studentRegistration.StudentWalkInn.Id,
                            FeeModeType = _studentRegistration.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? EnumClass.InstallmentType.SINGLE.ToString() : EnumClass.InstallmentType.INSTALLMENT.ToString(),
                            InstallmentCount = _studentRegistration.NoOfInstallment.Value,
                            FeeMode = _studentRegistration.FeeMode.Value


                        };

                        ViewBag.StepsChanged = redirect;


                        return View(_studentRegVM);
                    }
                }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                return View("Error");
            }




        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(RegistraionVM mdlRegistration)
        {
            try
            {
                ModelState.Remove("MultiCourseId");
                ModelState.Remove("PinNo");
                if (mdlRegistration.InstallmentType == EnumClass.InstallmentType.SINGLE)
                {
                    ModelState.Remove("InstallmentID");
                }
                if (ModelState.IsValid)
                {
                    var _dbRegistration = _db.StudentRegistrations
                                     .Where(r => r.Id == mdlRegistration.StudentRegistrationID)
                                     .SingleOrDefault();
                    if (_dbRegistration != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string _extension = "";
                            string _imgFileName = "";
                            string _imgPath = "";
                            string _imgSavePath = "";
                            HttpPostedFileBase _photoFile = null;
                            int _totalCourseFee = 0;
                            int _totalST = 0;
                            int _totalAmount = 0;

                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                            Common _cmn = new Common();

                            //uploaded photo
                            if (mdlRegistration.PhotoUrl != null)
                            {
                                //gets the file
                                _photoFile = mdlRegistration.PhotoUrl;
                                //gets the extension of the file
                                _extension = Path.GetExtension(_photoFile.FileName);
                                //gets the image file name
                                _imgFileName = mdlRegistration.RegistrationNumber + _extension;
                                //gets the image path
                                _imgPath = "~/UploadImages/Student";
                                //Saving path along with file name
                                _imgSavePath = Path.Combine(Server.MapPath(_imgPath), _imgFileName);
                                //setting is image verified to false
                                _dbRegistration.IsPhotoVerified = false;
                                //setting photourl
                                _dbRegistration.PhotoUrl = _imgPath + "/" + _imgFileName;
                                _dbRegistration.IsPhotoUploaded = true;
                                _dbRegistration.IsPhotoVerified = false;
                                _dbRegistration.PhotoUploadedDate = Common.LocalDateTime();
                                _dbRegistration.IsPhotoRejected = false;
                                _dbRegistration.PhotoRejectedReason = null;
                                _dbRegistration.PhotoRejectedDate = null;
                            }

                            if (Chk_SkippingAmountDetails(mdlRegistration.StudentRegistrationID, mdlRegistration.StudentReceipt))
                            {
                                if (!SendSkippingAmountMail(mdlRegistration.StudentReceipt, mdlRegistration.StudentRegistrationID))
                                {
                                    return Json("error_skippingamount_email", JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (!Chk_PaymentScheduleDetails(mdlRegistration.StudentRegistrationID, mdlRegistration.StudentReceipt))
                            {
                                return Json("error_paymentschedule_email", JsonRequestBehavior.AllowGet);
                            }

                            //Remove existing receipts
                            foreach (var _existingReceipt in _db.StudentReceipts
                                                          .Where(e => e.StudentRegistrationID == mdlRegistration.StudentRegistrationID)
                                                          .ToList())
                            {
                                _db.StudentReceipts.Remove(_existingReceipt);
                            }

                            //saving receipt details                            
                            foreach (var _receipt in mdlRegistration.StudentReceipt)
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
                            _dbRegistration.STPercentage = mdlRegistration.ST;
                            _dbRegistration.TotalSTAmount = _totalST;
                            _dbRegistration.TotalCourseFee = _totalCourseFee;
                            _dbRegistration.TotalAmount = _totalAmount;
                            _dbRegistration.FeeMode = mdlRegistration.InstallmentType == EnumClass.InstallmentType.SINGLE ? (int)EnumClass.InstallmentType.SINGLE : (int)EnumClass.InstallmentType.INSTALLMENT;
                            _dbRegistration.NoOfInstallment = _dbRegistration.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? 0 : mdlRegistration.InstallmentID;

                            //if feemode has changed(from single to installment || installment to single)
                            if (_dbRegistration.FeeMode != mdlRegistration.FeeMode)
                            {

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

                                        var _currDiscount = mdlRegistration.Discount;
                                        var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                                        var _currCourseAmt = mdlRegistration.InstallmentType == EnumClass.InstallmentType.SINGLE ? _course.SingleFee : _course.InstallmentFee;
                                        var _currTotalAmt = Convert.ToInt32(_currCourseAmt * _currDiscPercentage);
                                        _totalCourseAmt = Convert.ToInt32(_totalCourseAmt + _currTotalAmt);

                                        feedback.TotalCourseAmount = _currTotalAmt;
                                    }
                                    //if last item courseamount in feedback=TotalCourseFee-sum(individual course amount)
                                    else
                                    {
                                        feedback.TotalCourseAmount = _totalCourseFee - _totalCourseAmt;
                                    }
                                    _dbRegistration.StudentFeedbacks.Add(feedback);
                                }
                            }



                            if (_dbRegistration.StudentWalkInn.CandidateName != mdlRegistration.Name)
                            {
                                _dbRegistration.StudentWalkInn.CandidateName = mdlRegistration.Name;
                            }

                            if (_dbRegistration.StudentWalkInn.MobileNo != mdlRegistration.MobileNo && _cmn.MaskString(_dbRegistration.StudentWalkInn.MobileNo, "mobile") != mdlRegistration.MobileNo)
                            {
                                _dbRegistration.StudentWalkInn.MobileNo = mdlRegistration.MobileNo;
                                var isSMSSend = SendSMS(mdlRegistration.Name.ToUpper(), mdlRegistration.RegistrationNumber, mdlRegistration.MultiCourseCode, mdlRegistration.CourseFee, mdlRegistration.MobileNo);
                                if (!isSMSSend)
                                {
                                    return Json("error_mobile", JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (_dbRegistration.StudentWalkInn.EmailId != mdlRegistration.Email && _cmn.MaskString(_dbRegistration.StudentWalkInn.EmailId, "email") != mdlRegistration.Email)
                            {
                                _dbRegistration.StudentWalkInn.EmailId = mdlRegistration.Email;
                                var _courseName = string.Join(",", _dbRegistration.StudentRegistrationCourses
                                                                    .SelectMany(src => src.MultiCourse.MultiCourseDetails
                                                                    .Select(mcd => mcd.Course.Name)));
                                //Mail Sending
                                var isMailSend = SendMail(mdlRegistration.StudentRegistrationID, mdlRegistration.RegistrationNumber, mdlRegistration.Name.ToUpper(), mdlRegistration.MultiCourseCode, mdlRegistration.CourseFee, mdlRegistration.Email, _courseName);
                                if (!isMailSend)
                                {
                                    return Json("error_email", JsonRequestBehavior.AllowGet);
                                }
                            }




                            _db.Entry(_dbRegistration).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {
                                int l = _cmn.AddTransactions(actionName, controllerName, "");
                                if (l > 0)
                                {
                                    if (_photoFile != null)
                                    {
                                        _photoFile.SaveAs(_imgSavePath);
                                    }
                                    _ts.Complete();

                                }
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

        //Resending email to the students registered emailId incase if the student doesnt received the intital email while registration
        [HttpPost]
        public JsonResult ValidateEmailOnEdit(int studId, string studRegId, string studName, string courseList, int courseFee, string studEmailId)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    var _studRegistration = _db.StudentRegistrations
                                            .Where(r => r.Id == studId)
                                            .FirstOrDefault();
                    if (_studRegistration != null)
                    {
                        var courseName = string.Join(",", _studRegistration.StudentRegistrationCourses
                                                        .SelectMany(src => src.MultiCourse.MultiCourseDetails
                                                        .Select(mcd => mcd.Course.Name)));
                        var _result = SendMail(studId, studRegId, studName, courseList, courseFee, studEmailId, courseName);

                        if (_result)
                        {
                            _ts.Complete();
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json("mail_error", JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }

        //Returns fee details of course.Called on radiobutton feemode change
        public ActionResult GetCourseFee(string multiCourseId, string feeModeType)
        {
            decimal _courseFee = 0;
            try
            {
                List<int> _lstMultiCourseId = multiCourseId.Split(',').Select(int.Parse).ToList();
                if (feeModeType == EnumClass.InstallmentType.SINGLE.ToString())
                {
                    _courseFee = _db.MultiCourses
                                .AsEnumerable()
                                .Where(mc => _lstMultiCourseId.Contains(mc.Id))
                                .Sum(mc => mc.SingleFee.Value);

                }
                else
                {
                    _courseFee = _db.MultiCourses
                                .AsEnumerable()
                                .Where(mc => _lstMultiCourseId.Contains(mc.Id))
                                .Sum(mc => mc.InstallmentFee.Value);
                }
                return Json(Convert.ToInt32(_courseFee), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(Convert.ToInt32(_courseFee), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Check EmailId/
        public JsonResult IsEmailAlreadyExists(string Email, string InitialEmail)
        {
            try
            {
                //For  addition
                if (InitialEmail == "")
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.EmailId == Email);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                //For Edition
                else if (InitialEmail != Email)
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.EmailId == Email);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Check MobileNo/
        public JsonResult IsMobileAlreadyExists(string MobileNo, string InitialMobile)
        {
            try
            {
                //For  addition
                if (InitialMobile == "")
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.MobileNo == MobileNo);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                //For Edition
                else if (InitialMobile != MobileNo)
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.MobileNo == MobileNo);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public bool CalculateSTChange(int registrationId)
        {
            try
            {
                //Get current student registration details
                var _dbRegistration = _db.StudentRegistrations
                                    .Where(r => r.Id == registrationId)
                                    .FirstOrDefault();
                if (_dbRegistration != null)
                {
                    //Get current tax percentage
                    var _currTaxPercentage = _db.ServiceTaxes
                                           .AsEnumerable()
                                           .Where(t => t.FromDate <= Common.LocalDateTime())
                                           .OrderByDescending(t => t.FromDate)
                                           .First().Percentage;
                    //if ST has been modified
                    if (_dbRegistration.STPercentage != Convert.ToDouble(_currTaxPercentage))
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            //Get payment not completed receipt
                            var _nonPaidReceipt = _dbRegistration.StudentReceipts
                                                          .Where(r => r.Status == false)
                                                          .ToList();
                            //if student has pending payment update the amount with current st
                            if (_nonPaidReceipt.Count > 0)
                            {
                                //Modifying receipt details
                                foreach (var _receipt in _nonPaidReceipt)
                                {
                                    var _currCourseFee = _receipt.Fee;
                                    var _currSTPercent = _currTaxPercentage;
                                    var _currSTAmount = Convert.ToInt32(_currCourseFee * (_currSTPercent / 100));
                                    var _currTotalAmount = _currCourseFee + _currSTAmount;
                                    _receipt.STPercentage = Convert.ToDouble(_currSTPercent);
                                    _receipt.ST = _currSTAmount;
                                    _receipt.Total = _currTotalAmount;
                                    _dbRegistration.StudentReceipts.Add(_receipt);
                                }

                                //Modifying registration details
                                _dbRegistration.STPercentage = Convert.ToDouble(_currTaxPercentage);
                                _dbRegistration.TotalAmount = _dbRegistration.StudentReceipts
                                                             .Sum(r => r.Total);
                                _dbRegistration.TotalSTAmount = _dbRegistration.StudentReceipts
                                                              .Sum(r => r.ST);

                                _db.Entry(_dbRegistration).State = EntityState.Modified;
                                int i = _db.SaveChanges();
                                if (i > 0)
                                {
                                    _ts.Complete();
                                    return true;
                                }

                            }
                        }

                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool Chk_PaymentScheduleDetails(int regId, List<StudentReceipt> lstStudentReceipt)
        {
            try
            {
                List<SkippingMonth> _lstSkippingMonth = new List<SkippingMonth>();
                List<StudentReceipt> _lstReceipt = new List<StudentReceipt>();
                List<string> _lstEmailId = new List<string>();
                string _status = string.Empty;
                Common _cmn = new Common();

                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regId)
                            .FirstOrDefault();

                _lstEmailId.Add(_dbRegn.StudentWalkInn.EmailId);


                var _croName = _dbRegn.StudentWalkInn.CROCount == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                                _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name;

                //if studentreceipt details are not in the database then its first time registration
                if (lstStudentReceipt == null)
                {
                    _lstReceipt = _dbRegn.StudentReceipts.ToList();
                    _status = "add";
                }
                else
                {
                    _lstReceipt = lstStudentReceipt;
                    _status = "edit";
                }


                for (int i = 0; i < _lstReceipt.Count; i++)
                {
                    SkippingMonth _skippingMonth = new SkippingMonth();
                    _skippingMonth.Amount = _lstReceipt[i].Total.Value;
                    _skippingMonth.DueDate = _lstReceipt[i].DueDate.Value.ToString("dd/MM/yyyy");
                    _skippingMonth.Fee = _lstReceipt[i].Fee.Value;
                    _skippingMonth.ST = _lstReceipt[i].ST.Value;
                    _skippingMonth.RecNo = _lstReceipt[i].Status == true ? Common.GetReceiptNo(_lstReceipt[i].StudentReceiptNo) : "";

                    //Skipping month calculation
                    DateTime _currReceipt_DueDate = _lstReceipt[i].DueDate.Value;
                    //DateTime _currReceipt_MaxDueDate = _startReceiptDate.AddMonths(i);
                    DateTime _currReceipt_MaxDueDate = i > 0 ? _lstReceipt[i - 1].DueDate.Value.AddMonths(1) : _lstReceipt[i].DueDate.Value;
                    DateTime _currReceipt_maxAllowedDate = new DateTime(_currReceipt_MaxDueDate.Year, _currReceipt_MaxDueDate.Month,
                                                        DateTime.DaysInMonth(_currReceipt_MaxDueDate.Year, _currReceipt_MaxDueDate.Month));

                    if (_currReceipt_DueDate > _currReceipt_maxAllowedDate)
                    {
                        _skippingMonth.Status = false;
                    }
                    else
                    {
                        _skippingMonth.Status = true;
                    }

                    _lstSkippingMonth.Add(_skippingMonth);
                }
                var _skippingMonthCount = _lstSkippingMonth
                                        .Where(sm => sm.Status == false && sm.RecNo == null)
                                        .Count();
                if (_skippingMonthCount > 0)
                {
                    var _cro1EmailId = _dbRegn.StudentWalkInn.Employee1.OfficialEmailId;
                    var _cro2EmailId = _dbRegn.StudentWalkInn.CROCount == 2 ? _dbRegn.StudentWalkInn.Employee2.OfficialEmailId : null;
                    var _salesManagerEmailId = _cmn.GetManager(_dbRegn.StudentWalkInn.CenterCode.Id).OfficialEmailId;
                    var _centreManagerEmailId = _cmn.GetCentreManager(_dbRegn.StudentWalkInn.CenterCode.Id).OfficialEmailId;
                    var _edEamilId = "ed@networkzsystems.com";

                    _lstEmailId.Add(_cro1EmailId);
                    _lstEmailId.Add(_cro2EmailId);
                    _lstEmailId.Add(_salesManagerEmailId);
                    _lstEmailId.Add(_centreManagerEmailId);
                    _lstEmailId.Add(_edEamilId);
                }
                var _strEmail = String.Join(",", _lstEmailId.Where(x => x != null).ToList());

                return SendSkippingMonthMail(_lstSkippingMonth, _dbRegn.RegistrationNumber, _dbRegn.StudentWalkInn.CandidateName, _croName, _strEmail, _status);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Chk_SkippingAmountDetails(int regId, List<StudentReceipt> lstStudentReceipt)
        {
            try
            {
                List<string> _lstEmailId = new List<string>();
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regId)
                            .FirstOrDefault();

                var _croName = _dbRegn.StudentWalkInn.CROCount == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                              _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name;

                var _currReceiptList = lstStudentReceipt.ToList();
                var _prevReceiptList = _dbRegn.StudentReceipts.ToList();

                var _groupBy_currReceiptList = _currReceiptList
                                                 .GroupBy(g => g.DueDate.Value.ToString("yyyy.MM"))
                                                 .Select(cr => new
                                                 {
                                                     Amount = cr.Sum(g => g.Total)
                                                 }).ToList();

                var _groupBy_prevReceiptList = _prevReceiptList
                                               .GroupBy(g => g.DueDate.Value.ToString("yyyy.MM"))
                                               .Select(cr => new
                                               {
                                                   Amount = cr.Sum(g => g.Total)
                                               }).ToList();

                if (_groupBy_currReceiptList.Count > _groupBy_prevReceiptList.Count)
                {
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendSkippingAmountMail(List<StudentReceipt> currReceiptList, int regID)
        {
            try
            {

                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regID)
                            .FirstOrDefault();

                if (_dbRegn != null)
                {
                    string _body = string.Empty;
                    var tdString = "";
                    var tdPrevString = "";
                    List<string> _lstEmailId = new List<string>();
                    Common _cmn = new Common();

                    var prevReceiptList = _dbRegn.StudentReceipts.ToList();
                    var studID = _dbRegn.RegistrationNumber;
                    var studName = _dbRegn.StudentWalkInn.CandidateName;
                    var croName = _dbRegn.StudentWalkInn.CROCount == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                                  _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name;
                    var _cro1EmailId = _dbRegn.StudentWalkInn.Employee1.OfficialEmailId;
                    var _cro2EmailId = _dbRegn.StudentWalkInn.CROCount == 2 ? _dbRegn.StudentWalkInn.Employee2.OfficialEmailId : null;
                    var _salesManagerEmailId = _cmn.GetManager(_dbRegn.StudentWalkInn.CenterCode.Id).OfficialEmailId;
                    var _centreManagerEmailId = _cmn.GetCentreManager(_dbRegn.StudentWalkInn.CenterCode.Id).OfficialEmailId;
                    var _edEamilId = "ed@networkzsystems.com";

                    _lstEmailId.Add(_cro1EmailId);
                    _lstEmailId.Add(_cro2EmailId);
                    _lstEmailId.Add(_salesManagerEmailId);
                    _lstEmailId.Add(_centreManagerEmailId);
                    _lstEmailId.Add(_edEamilId);


                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/SkippingAmountPaymentTemplate.html")))
                    {
                        _body = reader.ReadToEnd();
                    }

                    foreach (var item in currReceiptList)
                    {
                        tdString = tdString + "<tr><td>" + item.Fee + "</td>";
                        tdString = tdString + "<td>" + item.ST + "</td>";
                        tdString = tdString + "<td>" + item.Total + "</td>";
                        tdString = tdString + "<td style='text-align:center'>" + item.DueDate.Value.ToString("dd/MM/yyyy") + "</td>";
                        tdString = tdString + "<td style='text-align:center'>" + Common.GetReceiptNo(item.StudentReceiptNo) + "</td></tr>";

                    }
                    foreach (var item in prevReceiptList)
                    {
                        tdPrevString = tdPrevString + "<tr><td>" + item.Fee + "</td>";
                        tdPrevString = tdPrevString + "<td>" + item.ST + "</td>";
                        tdPrevString = tdPrevString + "<td>" + item.Total + "</td>";
                        tdPrevString = tdPrevString + "<td style='text-align:center'>" + item.DueDate.Value.ToString("dd/MM/yyyy") + "</td>";
                        tdPrevString = tdPrevString + "<td style='text-align:center'>" + Common.GetReceiptNo(item.StudentReceiptNo) + "</td></tr>";

                    }

                    _body = _body.Replace("{StudentID}", studID);
                    _body = _body.Replace("{StudentName}", studName);
                    _body = _body.Replace("{CroName}", croName);
                    _body = _body.Replace("{SkippingCurrPaymentContent}", tdString);
                    _body = _body.Replace("{SkippingPrevPaymentContent}", tdPrevString);

                    //Email sending
                    var isMailSend = _cmn.SendEmail(string.Join(",", _lstEmailId.Where(e => !String.IsNullOrEmpty(e))), _body, "Skipping Amount Payment Schedule");
                    return isMailSend;
                }

                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendSkippingMonthMail(List<SkippingMonth> lstSkippingMoth, string studID, string studName, string croName, string emailList, string mode)
        {
            string _body = string.Empty;
            var tdString = "";

            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/SkippingMonthPaymentTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }

            foreach (var item in lstSkippingMoth)
            {
                if (item.Status == true)
                {
                    tdString = tdString + "<tr><td>" + item.Fee + "</td>";
                    tdString = tdString + "<td>" + item.ST + "</td>";
                    tdString = tdString + "<td>" + item.Amount + "</td>";
                    tdString = tdString + "<td style='text-align:center'>" + item.DueDate + "</td>";
                    tdString = tdString + "<td style='text-align:center'>" + item.RecNo + "</td></tr>";
                }
                else
                {
                    tdString = tdString + "<tr style='color:red'><td>" + item.Fee + "</td>";
                    tdString = tdString + "<td>" + item.ST + "</td>";
                    tdString = tdString + "<td>" + item.Amount + "</td>";
                    tdString = tdString + "<td style='text-align:center'>" + item.DueDate + "</td>";
                    tdString = tdString + "<td style='text-align:center'>" + item.RecNo + "</td></tr>";
                }

            }
            _body = _body.Replace("{StudentID}", studID);
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{CroName}", croName);
            _body = _body.Replace("{SkippingMonthContent}", tdString);
            if (mode == "add")
            {
                _body = _body.Replace("{EmailHeader}", "Payment Schedule Details");
                _body = _body.Replace("{EmailHeader_Content}", "You are receiving this mail because you have registered for a course in NetworkzSystems");
            }
            else
            {
                _body = _body.Replace("{EmailHeader}", "Modified Payment Schedule Details");
                _body = _body.Replace("{EmailHeader_Content}", "You are receiving this mail because  payment details has been modified");
            }


            Common _cmn = new Common();
            //Email sending
            var isMailSend = _cmn.SendEmail(emailList, _body, "Payment Schedule");
            return isMailSend;
        }

        public ActionResult CourseCodeSearch(List<int> courseId)
        {
            try
            {
                var test = _db.MultiCourseDetails
                                  .GroupBy(x => x.MultiCourseId)
                                  .Select(x => new
                                        {
                                            MultiCourseId = x.Key,
                                            SubCourses = x.Select(y => y.Course.Id),
                                            MultiCourses = x.Select(y => y.MultiCourse).FirstOrDefault()
                                        }).ToList();



                var courseList = _db.MultiCourseDetails
                                  .GroupBy(x => x.MultiCourseId)
                                  .Select(x => new
                                        {
                                            MultiCourseId = x.Key,
                                            SubCourses = x.Select(y => y.Course.Id),
                                            MultiCourses = x.Select(y => y.MultiCourse).FirstOrDefault()
                                        })
                                        .Where(x => courseId.All(y => x.SubCourses.Contains(y)))
                                        .AsEnumerable()
                                        .Select(x => new RegistraionVM.clsCourseCodeSearch
                                        {
                                            CourseCode = x.MultiCourses.CourseCode,
                                            CourseCombination = String.Join(", ", x.MultiCourses.MultiCourseDetails
                                                                                .Select(mcd => mcd.Course.Name)),
                                            InstallmentFee = x.MultiCourses.InstallmentFee.Value,
                                            SingleFee = x.MultiCourses.SingleFee.Value
                                        }).Take(5).ToList();


                return PartialView("_CourseCodeSearch", courseList);
            }
            catch (Exception ex)
            {
                return PartialView(null);
            }
        }


        //This function sends the receipt,email and sms
        public JsonResult Generate_Receipt_Email_SMS(int studentRegId)
        {
            try
            {
                var _dbRegn = _db.StudentRegistrations
                        .Where(r => r.Id == studentRegId).FirstOrDefault();
                SuccessMessage _successMsg = new SuccessMessage();
                var _student_Name = _dbRegn.StudentWalkInn.CandidateName.ToUpper();
                var _student_RegistrationNo = _dbRegn.RegistrationNumber;
                var _student_MultiCourseCode = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                            .Select(src => src.MultiCourse.CourseCode));
                var _student_CourseFee = _dbRegn.TotalCourseFee.Value;
                var _student_MobNo = _dbRegn.StudentWalkInn.MobileNo;
                var _stu_CroName = _dbRegn.StudentWalkInn.CROCount == 1 ? _dbRegn.StudentWalkInn.Employee1.Name.ToUpper() :
                                _dbRegn.StudentWalkInn.Employee1.Name.ToUpper() + "," + _dbRegn.StudentWalkInn.Employee2.Name.ToUpper();
                var _stu_Discount = _dbRegn.Discount.Value;
                var _stu_CentreId = _dbRegn.StudentWalkInn.CenterCode.Id;
                var _stu_EamilId = _dbRegn.StudentWalkInn.EmailId;
                var _stu_SoftwareUsed = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                        .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                                        .Select(mcd => mcd.Course.Name));

                var _isReceiptForEmail_Generated = GenerateReceipts(studentRegId);
                var _isReceiptForPrint_Generated = GenerateReceiptForPrint(studentRegId);
                var _isMailSend = SendMail(studentRegId, _student_RegistrationNo, _student_Name, _student_MultiCourseCode, _student_CourseFee, _stu_EamilId, _stu_SoftwareUsed);
                var _isMailSend_PaymentSchedule = Chk_PaymentScheduleDetails(studentRegId, null);
                var _isSMSSend = SendSMS(_student_Name, _student_RegistrationNo, _student_MultiCourseCode, _student_CourseFee, _student_MobNo);
                var _isOfficalSMSSend = SendSMSOffical(_stu_CroName, _student_RegistrationNo, _student_Name, _stu_Discount, _student_MultiCourseCode, _student_CourseFee, _stu_CentreId);
                var _isReceiptSMSend=SendReceiptSMS(studentRegId);
                
                if (_isReceiptForEmail_Generated)
                {
                    if (_isReceiptForPrint_Generated)
                    {
                        if (_isMailSend)
                        {
                            if (_isMailSend_PaymentSchedule)
                            {
                                if (_isSMSSend)
                                {
                                    if (_isOfficalSMSSend)
                                    {
                                        _successMsg.RegistrationId = _dbRegn.Id;
                                        _successMsg.RegistrationNo = _dbRegn.RegistrationNumber;
                                        _successMsg.PdfName = _dbRegn.RegistrationNumber + "_" +
                                            Common.GetReceiptNo(_dbRegn.StudentReceipts.Where(r => r.Status == true).FirstOrDefault().StudentReceiptNo);
                                        _successMsg.Status = "success";
                                        return Json(_successMsg, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        _successMsg.Status = "error_officialsms";
                                        return Json(_successMsg, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    _successMsg.Status = "error_studentsms";
                                    return Json(_successMsg, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                _successMsg.Status = "error_email_paymentschedule";
                                return Json(_successMsg.Status, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            _successMsg.Status = "error_email_student";
                            return Json(_successMsg.Status, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        _successMsg.Status = "error_receipt_print";
                        return Json(_successMsg.Status, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    _successMsg.Status = "error_receipt";
                    return Json(_successMsg.Status, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public bool GenerateReceipts(int studentRegID)
        {
            try
            {
                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                DataTable _dTable = new DataTable();
                _dTable = GetReceiptDetails(studentRegID);

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = Server.MapPath("~/Report/Receipt.rdlc"); ; //This is your rdlc name.   

                viewer.LocalReport.EnableExternalImages = true;
                string imagePath = new Uri(Server.MapPath("~/Styles/ReceiptImages/EReceipt-logo.jpg")).AbsoluteUri;
                string nsLogoPath = new Uri(Server.MapPath("~/Styles/ReceiptImages/NSlogo.jpg")).AbsoluteUri;
                ReportParameter parameter1 = new ReportParameter("ReceiptLogoPath", imagePath);
                ReportParameter parameter2 = new ReportParameter("NSLogoPath", nsLogoPath);
                viewer.LocalReport.SetParameters(parameter1);
                viewer.LocalReport.SetParameters(parameter2);

                ReportDataSource datasource = new ReportDataSource("dtReceipt", _dTable);
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(datasource);

                //FileName form=>StudentID_ReceiptNo
                var _fileName = Server.MapPath("~/Receipt/" + _dTable.Rows[0]["StudentID"].ToString() + "_"
                              + _dTable.Rows[0]["ReceiptNo"].ToString());

                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                using (FileStream stream = new FileStream(_fileName + ".pdf", FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool GenerateReceiptForPrint(int studentRegId)
        {
            try
            {
                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                DataTable _dTable = new DataTable();
                _dTable = GetReceiptTable(studentRegId);

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = Server.MapPath("~/Report/Receipt_Print.rdlc"); ; //This is your rdlc name.   

                viewer.LocalReport.EnableExternalImages = true;
                string nsLogoPath = new Uri(Server.MapPath("~/Styles/ReceiptImages/NSlogo.jpg")).AbsoluteUri;
                ReportParameter parameter2 = new ReportParameter("NSLogoPath", nsLogoPath);
                viewer.LocalReport.SetParameters(parameter2);

                ReportDataSource datasource = new ReportDataSource("dtReceipt_Print", _dTable);
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(datasource);

                //FileName form=>StudentID_ReceiptNo
                var _fileName = Server.MapPath("~/Receipt_Print/" + _dTable.Rows[0]["StudentID"].ToString() + "_"
                              + _dTable.Rows[0]["RecNo"].ToString());

                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                using (FileStream stream = new FileStream(_fileName + ".pdf", FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public DataTable GetReceiptDetails(int studentRegId)
        {
            List<clsReceipt> _clsReceipt = new List<clsReceipt>();
            DataTable _dtReceipt = new DataTable();
            Common _cmn = new Common();
            try
            {
                var _test = _db.StudentReceipts
                           .Where(r => r.StudentRegistration.Id == studentRegId)
                           .ToList();
                _clsReceipt = _db.StudentReceipts
                                 .Where(r => r.StudentRegistration.Id == studentRegId)
                                 .AsEnumerable()
                                 .Select(r => new clsReceipt()
                                 {
                                     CentreCode = r.StudentRegistration.StudentWalkInn.CenterCode.CentreCode,
                                     CentreCodeAddress = r.StudentRegistration.StudentWalkInn.CenterCode.Address.ToUpper(),
                                     CentreCodePhoneNo = r.StudentRegistration.StudentWalkInn.CenterCode.PhoneNo,
                                     CourseTitle = string.Join(",", r.StudentRegistration.StudentRegistrationCourses
                                                                 .Select(sc => sc.MultiCourse.CourseSubTitle.Name)),
                                     CroName = r.StudentRegistration.StudentWalkInn.CROCount == 1 ? r.StudentRegistration.StudentWalkInn.Employee1.Name :
                                             r.StudentRegistration.StudentWalkInn.Employee1.Name + "," + r.StudentRegistration.StudentWalkInn.Employee2.Name,
                                     Duration = r.StudentRegistration.TotalDuration.ToString(),
                                     PaymentFeeDetails = r.Fee.Value,
                                     PaymentSTAmount = r.ST.Value,
                                     PaymentSTPercentage = r.STPercentage.Value,
                                     PaymentAmountDetails = r.Total.Value,
                                     PaymentDateDetails = r.DueDate.Value.Date,
                                     PaymentMode = r.StudentRegistration.FeeMode.Value == (int)EnumClass.InstallmentType.SINGLE ?
                                                   EnumClass.InstallmentType.SINGLE.ToString() : EnumClass.InstallmentType.INSTALLMENT.ToString(),
                                     PaymentType = "CASH",
                                     ReceiptNoDetails = r.StudentReceiptNo.ToString(),
                                     STRegnNo = r.StudentRegistration.StudentWalkInn.CenterCode.STRegNo,
                                     StudentEmail = _cmn.MaskString(r.StudentRegistration.StudentWalkInn.EmailId, "email"),
                                     StudentMobileNo = _cmn.MaskString(r.StudentRegistration.StudentWalkInn.MobileNo, "mobile"),
                                     StudentID = r.StudentRegistration.RegistrationNumber,
                                     StudentName = r.StudentRegistration.StudentWalkInn.CandidateName.ToUpper(),
                                     StudentReceipt = r.StudentRegistration.StudentReceipts.ToList(),
                                     CourseCode = string.Join(",", r.StudentRegistration.StudentRegistrationCourses
                                                                 .Select(sc => sc.MultiCourse.CourseCode))
                                 }).ToList();
                var _clsReceipts = _clsReceipt
                                     .Select(r => new
                                     {
                                         ReceiptDate = r.ReceiptDate,
                                         ReceiptNo = Common.GetReceiptNo(r.ReceiptNo),
                                         StudentID = r.StudentID,
                                         StudentName = r.StudentName,
                                         CourseTitle = r.CourseTitle,
                                         Duration = r.Duration,
                                         PaymentType = r.PaymentType,
                                         PaymentMode = r.PaymentMode,
                                         CourseFee = r.CourseFee,
                                         ServiceTax = r.ServiceTax,
                                         TotalAmount = r.TotalAmount,
                                         TotalAmountInWords = r.TotalAmountInWords.ToString().ToUpper(),
                                         BalancePayable = r.BalancePayable,
                                         CroName = r.CroName,
                                         CrossCheckedBy = r.CrossCheckedBy,
                                         StudentMobileNo = r.StudentMobileNo,
                                         StudentEmail = r.StudentEmail,
                                         CentreCode = r.CentreCode,
                                         STRegnNo = r.STRegnNo,
                                         CentreCodeAddress = r.CentreCodeAddress.Replace(",", ", "),
                                         CentreCodePhoneNo = r.CentreCodePhoneNo,
                                         PaymentAmountDetails = r.PaymentAmountDetails,
                                         PaymentDateDetails = r.PaymentDateDetails,
                                         ReceiptNoDetails = r.ReceiptNoDetails,
                                         CourseCode = r.CourseCode,
                                         PaymentFeeDetails = r.PaymentFeeDetails,
                                         PaymentSTAmount = r.PaymentSTAmount,
                                         PaymentSTPercentage = r.PaymentSTPercentage

                                     }).ToList();

                _dtReceipt = ToDataTable(_clsReceipts);
            }
            catch (Exception ex)
            {
                _dtReceipt = null;
            }
            return _dtReceipt;
        }

        public DataTable GetReceiptTable(int studentRegId)
        {
            List<clsReceipt_Print> _clsReceipt_Print = new List<clsReceipt_Print>();
            DataTable _dtReceipt = new DataTable();
            Common _cmn = new Common();
            try
            {
                using (var _db = new dbSMSNSEntities())
                {

                    var _receiptId = _db.StudentReceipts
                                    .Where(r => r.StudentRegistration.Id == studentRegId && r.Status == true)
                                    .OrderByDescending(r=>r.Id)
                                    .FirstOrDefault().Id;

                    _clsReceipt_Print = _db.StudentReceipts
                                      .Where(r => r.Id == _receiptId)
                                      .AsEnumerable()
                                      .Select(r => new clsReceipt_Print
                                      {
                                          CourseFee = _cmn.ConvertNumberToComma(r.Fee.Value),
                                          RecDate = r.DueDate.Value.ToString("dd/MM/yyyy"),
                                          RecNo = Common.GetReceiptNo(r.StudentReceiptNo),
                                          STAmount = _cmn.ConvertNumberToComma(r.ST.Value),
                                          STPercentage = r.STPercentage.ToString(),
                                          STRegNo = r.StudentRegistration.StudentWalkInn.CenterCode.STRegNo,
                                          StudentID = r.StudentRegistration.RegistrationNumber,
                                          StudentName = r.StudentRegistration.StudentWalkInn.CandidateName,
                                          TotalAmount = _cmn.ConvertNumberToComma(r.Total.Value)
                                      }).ToList();


                    _dtReceipt = ToDataTable(_clsReceipt_Print);

                }
            }
            catch (Exception ex)
            {
                _dtReceipt = null;
            }
            return _dtReceipt;
        }

        public DataTable ToDataTable<T>(IList<T> data)// T is any generic type
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public bool SendReceiptSMS(int studRegId)
        {
            try
            {

                Common _cmn = new Common();
                bool result = false;
                var _studRegistraion = _db.StudentRegistrations
                                    .Where(r => r.Id == studRegId).FirstOrDefault();

                var _latestReceiptDetails = _studRegistraion.StudentReceipts
                                          .Last(r => r.Status == true);


                if (_studRegistraion != null)
                {
                    string _cro1MobNo = string.Empty;
                    string _cro2MobNo = string.Empty;
                    string _feePaidCroMobNo = string.Empty;
                    int _croCount = 0;
                    var _receiptNo = Common.GetReceiptNo(_latestReceiptDetails.StudentReceiptNo);
                    var _receiptDate = _latestReceiptDetails.DueDate.Value.ToString("dd/MM/yyyy");
                    var _studRegNo = _studRegistraion.RegistrationNumber;
                    var _studName = _studRegistraion.StudentWalkInn.CandidateName;
                    var _croName = _studRegistraion.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ? _studRegistraion.StudentWalkInn.Employee1.Name
                                                                                                      : _studRegistraion.StudentWalkInn.Employee1.Name + ',' + _studRegistraion.StudentWalkInn.Employee2.Name;
                    var _courseCode = string.Join(",", _studRegistraion.StudentRegistrationCourses
                                                    .Select(x => x.MultiCourse.CourseCode));
                    var _currPaid = _latestReceiptDetails.Total.Value;
                    var _sumPaid = _studRegistraion.StudentReceipts
                                 .Where(r => r.Status == true).Sum(r => r.Total);
                    var _balanceamount = _studRegistraion.TotalAmount - _sumPaid;
                    var _nextDueDetails = _studRegistraion.StudentReceipts
                                           .Where(r => r.Status == false)
                                           .FirstOrDefault();
                    var _nextDueDate = _nextDueDetails != null ? _nextDueDetails.DueDate.Value.ToString("dd/MM/yyyy") : "FULL PAID";



                    //Gets the designationids of employees to whom sms has to be send
                    List<int> _desgnList = GetEmpDesignationList(_studRegistraion.StudentWalkInn.CenterCodeId.Value);
                    //Gets the mobile no of all the employees
                    List<string> _lstMobNos = _cmn.GetEmployeeDesignationWise(_desgnList)
                                                    .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo).ToList();

                    _croCount = _studRegistraion.StudentWalkInn.CROCount.Value;

                    //if cro count is one
                    if (_croCount == (int)EnumClass.CROCount.ONE)
                    {
                        //if fee paid cro and walkinn cro are same
                        if (_latestReceiptDetails.CROID == _studRegistraion.StudentWalkInn.CRO1ID)
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                        }
                        //adding fee paid cro mobileno        
                        else
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                            _feePaidCroMobNo = _latestReceiptDetails.Employee.OfficialMobileNo != null ? _latestReceiptDetails.Employee.OfficialMobileNo :
                                                                                                   _latestReceiptDetails.Employee.MobileNo;
                        }
                    }
                    //if cro count is two
                    else
                    {
                        //adding cro1 and cro2 mobileno
                        if ((_latestReceiptDetails.CROID == _studRegistraion.StudentWalkInn.CRO1ID) || (_latestReceiptDetails.CROID == _studRegistraion.StudentWalkInn.CRO2ID))
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                            _cro2MobNo = _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee2.MobileNo;
                        }
                        //adding cro1+cro2+fee paid cro mobileno 
                        else
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo :
                                                                                                       _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                            _cro2MobNo = _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee2.MobileNo;
                            _feePaidCroMobNo = _latestReceiptDetails.Employee.OfficialMobileNo != null ? _latestReceiptDetails.Employee.OfficialMobileNo :
                                                                                                   _latestReceiptDetails.Employee.MobileNo;
                        }
                    }

                    //Adding student + guardian mobile no
                    string _stuMobNo = _studRegistraion.StudentWalkInn.MobileNo;
                    //string _guardianMobNo = _studRegistraion.StudentWalkInn.GuardianContactNo;

                    _lstMobNos.Add(_cro1MobNo);
                    _lstMobNos.Add(_cro2MobNo);
                    _lstMobNos.Add(_feePaidCroMobNo);
                    _lstMobNos.Add(_stuMobNo);
                    //_lstMobNos.Add(_guardianMobNo);

                    string _mobNos = string.Join(",", _lstMobNos.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList());

                    //sending message to student
                    var _message = "Rec.No:" + _receiptNo + ", " + _receiptDate + ", " + _studRegNo + ", " + _studName + ", Cro:" + _croName + ", " + _courseCode + ", Paid:Rs." + _currPaid + " including ST, BAL:Rs." +
                                    _balanceamount + ", NextDue:" + _nextDueDate;

                    string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _mobNos + "&msg=" + _message + "&route=T");
                    if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                    {
                        result = true;

                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
