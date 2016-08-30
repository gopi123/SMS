using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Runtime.Serialization;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Objects;
using System.Data;
using System.IO;
using Microsoft.Reporting.WebForms;
using System.ComponentModel;

namespace SMS.Controllers
{
    public class CourseUpgradeController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /CourseUpgrade/

        #region ClassDefinition
        public class ResponseStatus
        {
            public string Status { get; set; }
            public string Data { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class CentreCodeDetails
        {
            public int CentreId { get; set; }
            public string CentreCode { get; set; }
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
        public class SuccessMessage
        {
            public int RegistrationId { get; set; }
            public string Status { get; set; }
            public string RegistrationNo { get; set; }
            public string PdfName { get; set; }
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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CourseUpgrade(int regId)
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
                    //Gets the logged employee
                    var _cro1EmpList = GetCROEmployeeList();
                    //Gets the employee except the logged employee
                    var _cro2EmpList = GetCROEmployeeList()
                                     .Where(e => e.Id != loggedEmpId)
                                     .ToList();
                    var _installmentNo = from EnumClass.InstallmentNo b in Enum.GetValues(typeof(EnumClass.InstallmentNo))
                                         select new { Id = (int)b, Name = (int)b };
                    var _roundUpList = from EnumClass.RoundUpList r in Enum.GetValues(typeof(EnumClass.RoundUpList))
                                       select new { Id = (int)r, Name = r.ToString().Replace('_', ' ') };
                    var _taxPercentage = _db.ServiceTaxes
                                   .AsEnumerable()
                                   .Where(t => t.FromDate <= Common.LocalDateTime())
                                   .OrderByDescending(t => t.FromDate)
                                   .First().Percentage;

                    var _mdlCourseUpgrade = new CourseUpgradeVM
                    {
                        CRO1EmpList = new SelectList(_cro1EmpList, "Id", "Name"),
                        CRO1ID = loggedEmpId,
                        CRO1Percentage = 100,
                        CRO2EmpList = new SelectList(_cro2EmpList, "Id", "Name"),
                        Email = _studentEmail,
                        MobileNo = _studentMobile,
                        Name = _dbRegn.StudentWalkInn.CandidateName,
                        StudentRegistration = _dbRegn,
                        CROCount = EnumClass.CROCount.ONE,
                        DefaultStudentMobNo = _dbRegn.StudentWalkInn.MobileNo,
                        InstallmentType = EnumClass.InstallmentType.SINGLE,
                        InstallmentList = new SelectList(_installmentNo, "Id", "Name"),
                        MultiCourseList = new SelectList("", "Id", "CourseCode"),
                        RoundUpList = new SelectList(_roundUpList, "Id", "Name"),
                        RoundUpId = (int)EnumClass.RoundUpList.ROUND_UP,
                        ST = Convert.ToDouble(_taxPercentage),
                        DefaultDiscountPercentage = (int)EnumClass.DiscountPercentage.DISCOUNT,
                        PrevStudentWalkinnId = _dbRegn.StudentWalkInn.Id,
                        DefaultEmailId = _dbRegn.StudentWalkInn.EmailId,
                        CentreCode = _dbRegn.StudentWalkInn.CenterCode.CentreCode,
                        PrevCourseId=string.Join(",",_dbRegn.StudentRegistrationCourses
                                                      .Select(rc=>rc.MultiCourse.Id))
                    };

                    return View(_mdlCourseUpgrade);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult CourseUpgrade(CourseUpgradeVM mdlCourseUpgradeVM)
        {
            SuccessMessage _successMsg = new SuccessMessage();
            try
            {
                if (mdlCourseUpgradeVM.CROCount == EnumClass.CROCount.ONE)
                {
                    ModelState.Remove("CRO2ID");
                }
                if (mdlCourseUpgradeVM.InstallmentType == EnumClass.InstallmentType.SINGLE)
                {
                    ModelState.Remove("InstallmentID");
                }
                if (ModelState.IsValid)
                {

                    int _walkInnId = Add_WalkInn(mdlCourseUpgradeVM);
                    //int _walkInnId = 3997;
                    if (_walkInnId != 0)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string _extension = "";
                            string _imgFileName = "";
                            string _imgPath = "";
                            string _imgSavePath = "";
                            string _studentRegNo = "";
                            string _photoUrl = "";
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                            Common _cmn = new Common();
                            int _totalAmount = 0;
                            int _totalST = 0;
                            int _totalCourseFee = 0;
                            StudentRegistration _dbRegistration = new StudentRegistration();

                            _studentRegNo = GetStudentRegistrationNo(_walkInnId);
                            //if studentregistration number is null
                            if (_studentRegNo == "")
                            {                              
                                _successMsg.Status = "StudentRegistrationNo is null";
                                return Json(_successMsg, JsonRequestBehavior.AllowGet);
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

                                //if new photo is uploaded
                                if (mdlCourseUpgradeVM.PhotoUrl != null)
                                {
                                    _photoFile = mdlCourseUpgradeVM.PhotoUrl;
                                    _extension = Path.GetExtension(_photoFile.FileName);
                                    _imgFileName = _studentRegNo + _extension;
                                    _imgPath = "~/UploadImages/Student";
                                    _imgSavePath = Path.Combine(Server.MapPath(_imgPath), _imgFileName);
                                    _photoUrl = _imgPath + "/" + _imgFileName;
                                }
                                //if photo is uploaded on pervious walkinn and is not rejected 
                                //copy the uploaded image
                                else
                                {
                                    _dbRegistration = _db.StudentRegistrations
                                                            .Where(r => r.StudentWalkInn.Id == mdlCourseUpgradeVM.PrevStudentWalkinnId)
                                                            .FirstOrDefault();
                                    if (_dbRegistration != null)
                                    {
                                        var _isPhotoUploaded = _dbRegistration.IsPhotoUploaded.Value;
                                        var _isPhotoRejected = _dbRegistration.IsPhotoRejected.Value;
                                        var _oldPhotoUrl = _dbRegistration.PhotoUrl;

                                        //if photo is uploaded and its not rejected
                                        if (_isPhotoUploaded && !_isPhotoRejected)
                                        {
                                            _extension = Path.GetExtension(Path.Combine(Server.MapPath(_oldPhotoUrl)));
                                            _imgFileName = _studentRegNo + _extension;
                                            _imgPath = "~/UploadImages/Student";
                                            _imgSavePath = Path.Combine(Server.MapPath(_imgPath), _imgFileName);
                                            _photoUrl = _imgPath + "/" + _imgFileName;

                                            FileInfo _fileInfo = new FileInfo(Path.Combine(Server.MapPath(_oldPhotoUrl)));
                                            _fileInfo.CopyTo(_imgSavePath);
                                        }
                                        else
                                        {
                                            _photoUrl = "~/UploadImages/Student/NoImageSelected.png";
                                        }
                                    }
                                }

                                StudentRegistration _studRegn = new StudentRegistration();
                                _studRegn.Discount = mdlCourseUpgradeVM.Discount;
                                _studRegn.TotalDuration = mdlCourseUpgradeVM.Duration;
                                _studRegn.FeeMode = mdlCourseUpgradeVM.InstallmentType == EnumClass.InstallmentType.SINGLE ? (int)EnumClass.InstallmentType.SINGLE : (int)EnumClass.InstallmentType.INSTALLMENT;
                                _studRegn.IsEmailVerified = false;
                                _studRegn.MultiCourseIDs = string.Join(",", mdlCourseUpgradeVM.MultiCourseId);
                                _studRegn.NoOfInstallment = mdlCourseUpgradeVM.InstallmentID;
                                _studRegn.PhotoUrl = _photoUrl;
                                _studRegn.RegistrationVenueID = (int)EnumClass.RegistrationVenue.CENTRE;
                                _studRegn.RegistrationNumber = _studentRegNo;
                                _studRegn.STPercentage = mdlCourseUpgradeVM.ST;
                                _studRegn.TransactionDate = Common.LocalDateTime();
                                _studRegn.StudentWalkInnID = _walkInnId;
                                _studRegn.IsEmailVerified = false;

                                //if photo is verified in previous walkinn then photo upload is not possible
                                _studRegn.IsPhotoVerified = _dbRegistration.IsPhotoVerified;
                                _studRegn.PhotoVerifiedDate = _dbRegistration.IsPhotoVerified.Value ? Common.LocalDateTime() : (DateTime?)null;

                                if (mdlCourseUpgradeVM.PhotoUrl != null)
                                {
                                    _studRegn.IsPhotoUploaded = true;
                                    _studRegn.PhotoUploadedDate = Common.LocalDateTime();
                                }
                                else
                                {
                                    //if photo is uploaded on previous walkinn and is not rejected
                                    if (_dbRegistration.IsPhotoUploaded.Value && !_dbRegistration.IsPhotoRejected.Value)
                                    {
                                        _studRegn.IsPhotoUploaded = true;
                                        _studRegn.PhotoUploadedDate = Common.LocalDateTime();
                                    }
                                    else
                                    {
                                        _studRegn.IsPhotoUploaded = false;
                                        _studRegn.PhotoUploadedDate = (DateTime?)null;
                                    }
                                }
                                _studRegn.RoundUpID = mdlCourseUpgradeVM.RoundUpId;
                                _studRegn.IsCertificateIssued = false;
                                _studRegn.IsPhotoRejected = false;                               

                                //saving student course details
                                foreach (var courseId in mdlCourseUpgradeVM.MultiCourseId)
                                {
                                    StudentRegistrationCourse _studCourse = new StudentRegistrationCourse();
                                    _studCourse.MultiCourseID = Convert.ToInt32(courseId);
                                    _studRegn.StudentRegistrationCourses.Add(_studCourse);
                                }
                               

                                for (int q = 0; q < mdlCourseUpgradeVM.StudentReceipt.Count; q++)
                                {
                                    _totalCourseFee = _totalCourseFee + mdlCourseUpgradeVM.StudentReceipt[q].Fee.Value;
                                    _totalST = _totalST + mdlCourseUpgradeVM.StudentReceipt[q].ST.Value;


                                    StudentReceipt _studReceipt = new StudentReceipt();
                                    _studReceipt.DueDate = mdlCourseUpgradeVM.StudentReceipt[q].DueDate;
                                    _studReceipt.Fee = mdlCourseUpgradeVM.StudentReceipt[q].Fee;
                                    _studReceipt.ST = mdlCourseUpgradeVM.StudentReceipt[q].ST;
                                    _studReceipt.Status = q == 0 ? true : false;//first receipt will be paid
                                    _studReceipt.Total = mdlCourseUpgradeVM.StudentReceipt[q].Total;
                                    _studReceipt.STPercentage = mdlCourseUpgradeVM.ST;
                                    _studReceipt.CROID = q == 0 ? Convert.ToInt32(Session["LoggedUserId"].ToString()) : (int?)null;
                                    _studReceipt.ModeOfPayment = q == 0 ? (int)EnumClass.PaymentMode.CASH : (int?)null;
                                    _studRegn.StudentReceipts.Add(_studReceipt);

                                }

                                _totalAmount = _totalCourseFee + _totalST;
                                _studRegn.TotalSTAmount = _totalST;
                                _studRegn.TotalCourseFee = _totalCourseFee;
                                _studRegn.TotalAmount = _totalAmount;

                                //saving studentfeedback details
                                string[] arrCourseId = mdlCourseUpgradeVM.CourseIds
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

                                        var _currDiscount = mdlCourseUpgradeVM.Discount;
                                        var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                                        var _currCourseAmt = mdlCourseUpgradeVM.InstallmentType == EnumClass.InstallmentType.SINGLE ? _course.SingleFee : _course.InstallmentFee;
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
                                    var _studWalkinn = _db.StudentWalkInns.Where(w => w.Id == _walkInnId).FirstOrDefault();
                                    var _croName = _studWalkinn.CROCount == (int)EnumClass.CROCount.ONE ? _studWalkinn.Employee1.Name :
                                        _studWalkinn.Employee1.Name + "," + _studWalkinn.Employee2.Name;
                                    var _centreId = _studWalkinn.CenterCode.Id;
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
                                    else
                                    {
                                        _successMsg.Status = "error_updation_studentserialno";
                                    }
                                }
                                else
                                {
                                    _successMsg.Status = "error_saving_registration";
                                }
                            }
                        }
                    }
                    else
                    {                        
                        _successMsg.Status = "Error while adding walkinn";
                    }
                }
            }
            catch (Exception ex)
            {             
                _successMsg.Status = ex.Message;
            }
            return Json(_successMsg, JsonRequestBehavior.AllowGet);
        }

        public List<Employee> GetCROEmployeeList()
        {
            Common _cmn = new Common();

            int _loggedUserId = Convert.ToInt32(Session["LoggedUserId"]);

            //Get all EmployeeIds allotted to a particular center       
            List<int> _empIds = _cmn.GetEmployeeCenterWise(_loggedUserId)
                                .Where(e => e.Designation.Role.Id == (int)EnumClass.Role.SALESINDIVIDUAL
                                            || e.Designation.Role.Id == (int)EnumClass.Role.MANAGER)
                                .Select(e => e.Id).ToList();
            _empIds.Add(_loggedUserId);

            //Get all employees with EmployeeId filtering and except loggeduserid
            List<Employee> _croEmpList = _db.Employees
                                            .Where(e => _empIds.Contains(e.Id) && e.Status == true)
                                            .Distinct()
                                            .ToList();

            return _croEmpList;
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
                        CourseCount=_dbRegn.StudentRegistrationCourses
                                    .SelectMany(src=>src.MultiCourse.MultiCourseDetails)
                                    .Select(mcd=>mcd.Course)
                                    .Count(),
                        PendingPaymentCount=_dbRegn.StudentReceipts
                                    .Where(r=>r.Status==false)
                                    .Count(),
                        PendingFeedbackCount=_dbRegn.StudentFeedbacks
                                            .Where(f=>f.IsFeedbackGiven==false)
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

        public JsonResult MobileVerification(string studMobileNo)
        {
            int pinNo = 0;
            ResponseStatus _clsResponse = new ResponseStatus();
            try
            {
                Common _cmn = new Common();
                var _dbWalkInn = _db.StudentWalkInns
                                .Where(r => r.MobileNo == studMobileNo)
                                .FirstOrDefault();

                if (_dbWalkInn != null)
                {
                    _clsResponse.Status = "exist";
                    _clsResponse.Data = _dbWalkInn.CandidateName.ToUpper();

                }
                else
                {
                    //Gets RandomNo
                    int _randomNo = _cmn.GenerateRandomNo();

                    //Sends the 4 digitno to the sutdent mobileNo
                    string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + studMobileNo + "&msg=OTP for registration process in NetworkzSystems is:" + _randomNo + ". By sharing this pin I accept all communications from  NetworkzSystems to this mobile number&route=T");
                    if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                    {
                        _clsResponse.Status = "success";
                        _clsResponse.Data = _randomNo.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                _clsResponse.Status = "error";
                _clsResponse.ErrorMessage = ex.Message;
            }
            return Json(_clsResponse, JsonRequestBehavior.AllowGet);
        }

        public int Add_WalkInn(CourseUpgradeVM _courseUpgradeVM)
        {
            int _returnWalkInnID = 0;
            try
            {

                var _dbWalkInn = _db.StudentWalkInns
                                 .AsNoTracking()
                                 .Include(w => w.StudentWalkInnCourses)
                                 .Where(w => w.Id == _courseUpgradeVM.PrevStudentWalkinnId)
                                 .FirstOrDefault();

                if (_dbWalkInn != null)
                {
                    _dbWalkInn.EmailId = _courseUpgradeVM.DefaultEmailId;
                    _dbWalkInn.MobileNo = _courseUpgradeVM.DefaultStudentMobNo;
                    _dbWalkInn.CROCount = _courseUpgradeVM.CROCount == EnumClass.CROCount.ONE ? (int)EnumClass.CROCount.ONE :
                                       (int)EnumClass.CROCount.TWO;
                    _dbWalkInn.CRO1ID = _courseUpgradeVM.CRO1ID;
                    _dbWalkInn.CRO1Percentage = _courseUpgradeVM.CRO1Percentage;
                    if (_dbWalkInn.CROCount == (int)EnumClass.CROCount.TWO)
                    {
                        _dbWalkInn.CRO2ID = _courseUpgradeVM.CRO2ID;
                        _dbWalkInn.CRO2Percentage = _courseUpgradeVM.CRO2Percentage;
                    }
                    _dbWalkInn.TransactionDate = Common.LocalDateTime();
                    _db.StudentWalkInns.Add(_dbWalkInn);
                    int i = _db.SaveChanges();
                    if (i > 0)
                    {
                        _returnWalkInnID = _dbWalkInn.Id;

                    }
                }
                return _returnWalkInnID;
            }
            catch (Exception ex)
            {
                return _returnWalkInnID;
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


        //send confirmation sms to regtistered student
        public bool SendSMS(string studName, string studRegID, string courseList, int courseFee, string mobileNo)
        {
            bool result = false;
            //sending message to student
            var _message = "Dear " + studName + ".Your StudentId is " + studRegID + ",CourseCode is " + courseList + " and Fees Rs" + courseFee + "(excluding ST). "
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
                                            .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo).ToList();
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


        public bool SendMail_PaymentScheduleDetails(int regId, List<StudentReceipt> lstStudentReceipt)
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

                //Gets the starting receipt date
                DateTime _startReceiptDate = _lstReceipt.First().DueDate.Value;

                for (int i = 0; i < _lstReceipt.Count; i++)
                {
                    SkippingMonth _skippingMonth = new SkippingMonth();
                    _skippingMonth.Amount = _lstReceipt[i].Total.Value;
                    _skippingMonth.DueDate = _lstReceipt[i].DueDate.Value.ToString("dd/MM/yyyy");
                    _skippingMonth.Fee = _lstReceipt[i].Fee.Value;
                    _skippingMonth.ST = _lstReceipt[i].ST.Value;
                    _skippingMonth.RecNo = _lstReceipt[i].Status == true ? Common.GetReceiptNo(_lstReceipt[i].StudentReceiptNo.Value) : "";

                    DateTime _currReceipt_DueDate = _lstReceipt[i].DueDate.Value;
                    DateTime _currReceipt_MaxDueDate = _startReceiptDate.AddMonths(i);
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
                _body = _body.Replace("{EmailHeader_Content}", "You are receiving this mail because your payment details has been modified");
            }


            Common _cmn = new Common();
            //Email sending
            var isMailSend = _cmn.SendEmail(emailList, _body, "Payment Schedule");
            return isMailSend;
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
                var _isMailSend_PaymentSchedule = SendMail_PaymentScheduleDetails(studentRegId, null);
                var _isSMSSend = SendSMS(_student_Name, _student_RegistrationNo, _student_MultiCourseCode, _student_CourseFee, _student_MobNo);
                var _isOfficalSMSSend = SendSMSOffical(_stu_CroName, _student_RegistrationNo, _student_Name, _stu_Discount, _student_MultiCourseCode, _student_CourseFee, _stu_CentreId);
                var _isReceiptSMSend = SendReceiptSMS(studentRegId);

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
                                            Common.GetReceiptNo(_dbRegn.StudentReceipts.Where(r => r.Status == true).FirstOrDefault().StudentReceiptNo.Value);
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
                                    .OrderByDescending(r => r.Id)
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
                    var _receiptNo = Common.GetReceiptNo(_latestReceiptDetails.StudentReceiptNo.Value);
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
                    //string _stuMobNo = _studRegistraion.StudentWalkInn.MobileNo;
                    //string _guardianMobNo = _studRegistraion.StudentWalkInn.GuardianContactNo;

                    _lstMobNos.Add(_cro1MobNo);
                    _lstMobNos.Add(_cro2MobNo);
                    _lstMobNos.Add(_feePaidCroMobNo);
                    //_lstMobNos.Add(_stuMobNo);
                    //_lstMobNos.Add(_guardianMobNo);

                    string _mobNos = string.Join(",", _lstMobNos.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList());

                    //sending message to student
                    var _message = "Demo - Rec.No:" + _receiptNo + ", " + _receiptDate + ", " + _studRegNo + ", " + _studName + ", Cro:" + _croName + ", " + _courseCode + ", Paid:Rs." + _currPaid + " including ST, BAL:Rs." +
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
