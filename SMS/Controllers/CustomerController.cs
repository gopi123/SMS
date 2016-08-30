
using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CustomerController : Controller
    {
        public class AmountDetails
        {
            public int PaidAmount { get; set; }
            public int CourseAmount { get; set; }
        }
        public class NegativeFeedbackDetails
        {
            public string QuestionNo { get; set; }
            public string Question { get; set; }
            public string Rating { get; set; }
            public string Reason { get; set; }
        }

        public class StudentLoginDetails
        {
            public string Status { get; set; }
            public string StudentEmail { get; set; }
        }
        dbSMSNSEntities _db = new dbSMSNSEntities();

        #region LoginSection
        public ActionResult Login()
        {
            return View();
        }

        //searching using registrationnumber during Login
        public JsonResult LoginSearch(string regNo, DateTime DOB, string mobNo, string email)
        {
            StudentLoginDetails _clsStudentLogin = new StudentLoginDetails();
            try
            {
                var _dbReg = _db.StudentRegistrations
                            .Where(r => r.RegistrationNumber == regNo)
                            .FirstOrDefault();
                //if registration existis
                if (_dbReg != null)
                {
                    if (_dbReg.IsEmailVerified == true)
                    {
                        Common _cmn = new Common();

                        if (DOB.Date != _dbReg.StudentWalkInn.DOB.Value.Date)
                        {
                            _clsStudentLogin.Status = "dob_not_right";
                        }
                        else if (mobNo != _dbReg.StudentWalkInn.MobileNo)
                        {
                            _clsStudentLogin.Status = "mobileno_not_right";
                        }
                        else if (email.ToLower() != _dbReg.StudentWalkInn.EmailId.ToLower())
                        {
                            _clsStudentLogin.Status = "emailid_not_right";
                        }
                        else if (_dbReg.StudentLogins.Count == 0)
                        {

                            using (TransactionScope _ts = new TransactionScope())
                            {
                                var _verificationId = _cmn.ComputePassword(_dbReg.RegistrationNumber);
                                StudentLogin _studLogin = new StudentLogin();
                                _studLogin.VerificationID = _verificationId;
                                _studLogin.StudentRegistrationId = _dbReg.Id;
                                _db.StudentLogins.Add(_studLogin);
                                int i = _db.SaveChanges();
                                if (i > 0)
                                {

                                    if (SendLoginVerificationLink(_dbReg.RegistrationNumber, _dbReg.StudentWalkInn.CandidateName.ToUpper(),
                                        _dbReg.StudentWalkInn.EmailId, _verificationId))
                                    {
                                        _ts.Complete();
                                        _clsStudentLogin.StudentEmail = _cmn.MaskString(_dbReg.StudentWalkInn.EmailId, "email");
                                        _clsStudentLogin.Status = "success";
                                    }
                                    else
                                    {
                                        _clsStudentLogin.Status = "email_sending_error";
                                    }
                                }
                            }
                        }
                        else
                        {
                            //if students havent received the email then send once again
                            if (_dbReg.StudentLogins.FirstOrDefault().Username == null)
                            {
                                if (SendLoginVerificationLink(_dbReg.RegistrationNumber, _dbReg.StudentWalkInn.CandidateName.ToUpper(),
                                            _dbReg.StudentWalkInn.EmailId, _dbReg.StudentLogins.FirstOrDefault().VerificationID))
                                {
                                    _clsStudentLogin.StudentEmail = _cmn.MaskString(_dbReg.StudentWalkInn.EmailId, "email");
                                    _clsStudentLogin.Status = "success";
                                }
                                else
                                {
                                    _clsStudentLogin.Status = "email_sending_error";
                                }
                            }
                            else
                            {
                                _clsStudentLogin.Status = "student_already_registered";
                            }

                        }
                    }
                    else
                    {
                        _clsStudentLogin.Status = "email_not_verified";
                    }
                }
                else
                {
                    _clsStudentLogin.Status = "studentid_not_present";
                }
                return Json(_clsStudentLogin, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _clsStudentLogin.Status = ex.Message;
                return Json(_clsStudentLogin, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Login(string LoginUserName, string LoginPassword)
        {
            try
            {
                Common _cmn = new Common();
                var _password = _cmn.ComputePassword(LoginPassword);

                var _dbLogin = _db.StudentLogins
                            .Where(s => s.Username == LoginUserName && s.Password == _password)
                            .FirstOrDefault();
                if (_dbLogin != null)
                {
                    Session["CustomerRegNo"] = _dbLogin.StudentRegistration.RegistrationNumber;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid username or password";
                    return View();
                }
            }

            catch (Exception ex)
            {
                return View();
            }
        }

        public ActionResult Index()
        {
            var regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;
            if (regNo != null)
            {
                var _dbRegn = _db.StudentRegistrations
                               .Where(r => r.RegistrationNumber == regNo)
                               .FirstOrDefault();

                var _custDashboard = new CustomerDashboardVM
                {
                    Address = _dbRegn.StudentWalkInn.Address,
                    Email = _dbRegn.StudentWalkInn.EmailId,
                    PhoneNo = _dbRegn.StudentWalkInn.MobileNo,
                    PhotoUrl = _dbRegn.PhotoUrl,
                    RegistrationNumber = _dbRegn.RegistrationNumber,
                    StudentName = _dbRegn.StudentWalkInn.CandidateName,
                    Duration = _dbRegn.TotalDuration.Value,
                    MultiCourse = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                .Select(mc => mc.MultiCourse.CourseSubTitle.Name)),
                    TotalAmount = _dbRegn.TotalAmount.Value
                };

                ViewBag.LoggedCustomerName = _dbRegn.StudentWalkInn.CandidateName;
                Session["LoggedCustomerId"] = _dbRegn.Id;
                return View(_custDashboard);
            }
            else
            {
                return RedirectToAction("Login");
            }


        }


        public ActionResult SendValidationEmail(string regNo)
        {
            try
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.RegistrationNumber == regNo)
                            .FirstOrDefault();

                if (_dbRegn != null)
                {
                    Common _cmn = new Common();
                    var _studEmail = _dbRegn.StudentWalkInn.EmailId;
                    var _courseList = string.Join(",", _dbRegn.StudentRegistrationCourses.Select(x => x.MultiCourse.CourseCode));
                    var _courseName = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                    .SelectMany(src => src.MultiCourse.MultiCourseDetails
                                                    .Select(mcd => mcd.Course.Name)));
                    if (SendVerificationMail(_dbRegn.Id, _dbRegn.RegistrationNumber, _dbRegn.StudentWalkInn.CandidateName, _courseList, _dbRegn.TotalCourseFee.Value, _studEmail, _courseName))
                    {
                        return Json(new { message = "success~" + _cmn.MaskString(_studEmail, "email") }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { message = "error_regno~error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message + "~exception" }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool SendVerificationMail(int studId, string studentRegID, string studName, string courseList, int courseFee, string _studEmailId, string courseName)
        {
            //Saving EmailVerificationDetails
            EmailVerification _emailVerificaiton = new EmailVerification();
            string _key = Guid.NewGuid().ToString();
            _emailVerificaiton.Type = "S";
            _emailVerificaiton.VerificationKey = _key;
            _emailVerificaiton.TypeId = studId;
            _db.EmailVerifications.Add(_emailVerificaiton);
            int i = _db.SaveChanges();
            if (i > 0)
            {
                Common _cmn = new Common();
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
                _body = _body.Replace("{ActivationLink}", "http://www.networkzsystems.com/sms/Account/StudentVerification?key=" + _key);
                //Email sending
                var isMailSend = _cmn.SendEmail(_studEmailId, _body, "Student Registration");
                return isMailSend;
            }
            return false;


        }

        public bool SendLoginVerificationLink(string studID, string studName, string studEmail, string verificationId)
        {
            try
            {
                Common _cmn = new Common();
                string _body = string.Empty;

                using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/StudentLogin_VerificationTemplate.html")))
                {
                    _body = reader.ReadToEnd();
                }

                _body = _body.Replace("{StudentID}", studID);
                _body = _body.Replace("{StudentName}", studName);
                _body = _body.Replace("{ActivationLink}", "http://networkzsystems.com/sms/customer/register?verificationid=" + verificationId);

                //Email sending
                var isMailSend = _cmn.SendEmail(studEmail, _body, "Student Login");
                return isMailSend;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //This function is called on click of the login of mail send
        public ActionResult Register(string verificationid)
        {
            try
            {
                var _dbLogin = _db.StudentLogins
                            .Where(l => l.VerificationID == verificationid)
                            .FirstOrDefault();
                CustomerLoginVM _cutomerLoginVM = new CustomerLoginVM();
                if (_dbLogin != null)
                {
                    _cutomerLoginVM.EmailID = _dbLogin.StudentRegistration.StudentWalkInn.EmailId.ToUpper();
                    _cutomerLoginVM.DOB = _dbLogin.StudentRegistration.StudentWalkInn.DOB.Value.ToString("dd/MM/yyyy");
                    _cutomerLoginVM.MobileNo = _dbLogin.StudentRegistration.StudentWalkInn.MobileNo;
                    _cutomerLoginVM.StudentLoginID = _dbLogin.Id;
                    _cutomerLoginVM.StudentID = _dbLogin.StudentRegistration.RegistrationNumber;
                    _cutomerLoginVM.StudentName = _dbLogin.StudentRegistration.StudentWalkInn.CandidateName;
                }
                else
                {
                    _cutomerLoginVM.StudentID = "0";
                }
                return View(_cutomerLoginVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult Register(int studentLoginId, string userName, string password)
        {
            try
            {
                Common _cmn = new Common();
                var _dbLogin = _db.StudentLogins
                            .Where(s => s.Id == studentLoginId)
                            .FirstOrDefault();
                if (_dbLogin != null)
                {
                    if (_dbLogin.Username == null)
                    {
                        _dbLogin.Username = userName;
                        _dbLogin.Password = _cmn.ComputePassword(password);
                        _dbLogin.PasswordSetDate = Common.LocalDateTime();

                        _db.Entry(_dbLogin).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("error", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("student_already_registered", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Customer");
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Check UserName/
        public JsonResult CheckUsername(string UserName)
        {
            try
            {
                var _employeeCount = _db.StudentLogins
                              .Where(s => s.Username == UserName)
                              .Select(s => s.Id).FirstOrDefault();
                if (_employeeCount > 0)
                {
                    return Json("Sorry this name already exists", JsonRequestBehavior.AllowGet);
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        //This function sends the username to the students emailid
        public JsonResult ForgotUserName(string mobNo, string emailId, string studentID)
        {
            StudentLoginDetails _clsStudentLogin = new StudentLoginDetails();
            try
            {

                var _dbStudentLogin = _db.StudentLogins
                                .Where(w => w.StudentRegistration.StudentWalkInn.MobileNo == mobNo &&
                                    w.StudentRegistration.StudentWalkInn.EmailId == emailId && w.StudentRegistration.RegistrationNumber == studentID)
                                .FirstOrDefault();
                if (_dbStudentLogin != null)
                {
                    Common _cmn = new Common();
                    string _body = string.Empty;
                    var _email = _dbStudentLogin.StudentRegistration.StudentWalkInn.EmailId;

                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/StudentLogin_ForgotUsername.html")))
                    {
                        _body = reader.ReadToEnd();
                    }

                    _body = _body.Replace("{Username}", _dbStudentLogin.Username);
                    _body = _body.Replace("{StudentName}", _dbStudentLogin.StudentRegistration.StudentWalkInn.CandidateName.ToUpper());


                    //Email sending
                    var isMailSend = _cmn.SendEmail(_email, _body, "Student Login");
                    if (isMailSend)
                    {
                        _clsStudentLogin.Status = "success";
                        _clsStudentLogin.StudentEmail = _cmn.MaskString(_email, "email");
                    }
                    else
                    {
                        _clsStudentLogin.Status = "email_error";
                    }
                }
                else
                {
                    _clsStudentLogin.Status = "student_not_present";
                }

                return Json(_clsStudentLogin, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _clsStudentLogin.Status = ex.Message;
                return Json(_clsStudentLogin, JsonRequestBehavior.AllowGet);
            }
        }

        //This function sends password link to the students emailid
        public JsonResult Send_ForgotPasswordLink(string mobNo, string emailId, string studentID)
        {
            StudentLoginDetails _clsStudentLogin = new StudentLoginDetails();
            try
            {

                var _dbStudentLogin = _db.StudentLogins
                                .Where(w => w.StudentRegistration.StudentWalkInn.MobileNo == mobNo &&
                                    w.StudentRegistration.StudentWalkInn.EmailId.ToLower() == emailId.ToLower() &&
                                    w.StudentRegistration.RegistrationNumber == studentID)
                                .FirstOrDefault();
                if (_dbStudentLogin != null)
                {
                    Common _cmn = new Common();
                    string _body = string.Empty;
                    var _email = _dbStudentLogin.StudentRegistration.StudentWalkInn.EmailId;
                    var _verificationid = _dbStudentLogin.VerificationID;

                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/StudentLogin_ForgotPassword.html")))
                    {
                        _body = reader.ReadToEnd();
                    }

                    _body = _body.Replace("{ActivationLink}", "http://networkzsystems.com/sms/customer/forgotpassword?verificationid=" + _verificationid);
                    _body = _body.Replace("{StudentName}", _dbStudentLogin.StudentRegistration.StudentWalkInn.CandidateName.ToUpper());


                    //Email sending
                    var isMailSend = _cmn.SendEmail(_email, _body, "Student Login");
                    if (isMailSend)
                    {
                        _clsStudentLogin.Status = "success";
                        _clsStudentLogin.StudentEmail = _cmn.MaskString(_email, "email");
                    }
                    else
                    {
                        _clsStudentLogin.Status = "email_error";
                    }
                }
                else
                {
                    _clsStudentLogin.Status = "student_not_present";
                }

                return Json(_clsStudentLogin, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _clsStudentLogin.Status = ex.Message;
                return Json(_clsStudentLogin, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ForgotPassword(string verificationId)
        {
            try
            {
                var _dbLogin = _db.StudentLogins
                      .Where(l => l.VerificationID == verificationId)
                      .FirstOrDefault();
                CustomerLoginVM _custLoginVM = new CustomerLoginVM();
                Common _cmn = new Common();
                if (_dbLogin != null)
                {
                    _custLoginVM.StudentID = _dbLogin.StudentRegistration.RegistrationNumber;
                    _custLoginVM.StudentLoginID = _dbLogin.Id;
                    _custLoginVM.StudentName = _dbLogin.StudentRegistration.StudentWalkInn.CandidateName;
                    _custLoginVM.ReturnPinNo = _cmn.GenerateRandomNo().ToString();
                    _custLoginVM.PinNo = null;
                    var _mobNo = _dbLogin.StudentRegistration.StudentWalkInn.MobileNo;


                    //sending pinno to student
                    var _message = "Your 4 digit pinno to change the password is " + _custLoginVM.ReturnPinNo;
                    string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _mobNo + "&msg=" + _message + "&route=T");
                    if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                    {
                        return View(_custLoginVM);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Customer");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }

        }

        [HttpPost]
        public ActionResult ForgotPassword(int studentLoginId, string password)
        {
            StudentLoginDetails _clsStudentLoginDetails = new StudentLoginDetails();
            try
            {
                var _dbLogin = _db.StudentLogins
                              .Where(l => l.Id == studentLoginId)
                              .FirstOrDefault();
                Common _cmn = new Common();

                if (_dbLogin != null)
                {
                    //User cannot enter old password
                    var _newPassword = _cmn.ComputePassword(password);
                    if (_newPassword != _dbLogin.Password)
                    {
                        _dbLogin.Password = _newPassword;
                        _db.Entry(_dbLogin).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            _clsStudentLoginDetails.Status = "success";
                        }
                        else
                        {
                            _clsStudentLoginDetails.Status = "db_error";
                        }
                    }
                    else
                    {
                        _clsStudentLoginDetails.Status = "password_already_exist";
                    }

                }
                else
                {
                    _clsStudentLoginDetails.Status = "student_not_exist";
                }
                return Json(_clsStudentLoginDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _clsStudentLoginDetails.Status = ex.Message;
                return Json(_clsStudentLoginDetails, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region E-WorkbookSection
        //Ebook Get
        public ActionResult Ebook()
        {
            var _regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;
            if (_regNo != null)
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.RegistrationNumber == _regNo)
                            .FirstOrDefault();
                if (_dbRegn != null)
                {
                    var _eBookVM = new CustomerIndexVM
                    {
                        RegistraionNumber = _dbRegn.RegistrationNumber,
                        StudentName = _dbRegn.StudentWalkInn.CandidateName,
                        StudentFeedback = _dbRegn.StudentFeedbacks.ToList()
                    };
                    return View(_eBookVM);
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //Ebook Download
        public ActionResult EbookDownload(int courseId)
        {
            try
            {
                var _course = _db.Courses
                            .Where(c => c.Id == courseId)
                            .FirstOrDefault();
                if (_course != null)
                {
                    string filePath = Server.MapPath(_course.CourseDownloadUrl);
                    Response.ContentType = "application/pdf";
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
                    Response.WriteFile(filePath);
                    Response.End();
                    return RedirectToAction("Ebook");
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        #endregion

        #region ProjectUpload Section

        public ActionResult ProjectUploadList()
        {
            var _regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;

            if (_regNo != null)
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.RegistrationNumber == _regNo)
                            .FirstOrDefault();

                if (_dbRegn != null)
                {
                    //gets the totalfee
                    int _totalFee = _dbRegn.TotalAmount.Value;
                    //gets the paid fee
                    int _paidFee = _dbRegn.StudentReceipts
                                 .Where(r => r.Status == true)
                                 .Sum(r => r.Total.Value);
                    //gets the remaining fee to be paid
                    int _remainingFee = _totalFee - _paidFee;

                    var _dtCustomerList = _db.StudentFeedbacks
                                       .Where(f => f.StudentRegistration.RegistrationNumber == _regNo)
                                       .AsEnumerable()
                                       .Select(f => new CustomerIndexVM.dtCustomer
                                       {
                                           Id = f.Id,
                                           Course = f.Course.Name,
                                           IsProjectUploaded = f.IsProjectUploaded,
                                           IsTrainerVerified = f.IsTrainerVerified,
                                           StudentProjectApproval = f.StudentProjectApprovals.FirstOrDefault(),
                                           IsFeedBackGiven = f.IsFeedbackGiven,
                                           IsPhotoRejected = f.StudentRegistration.IsPhotoRejected.Value,
                                           StudentFeedback = f.StudentRegistration.StudentFeedbacks.ToList()

                                       }).ToList();


                    var _eProjectUploadVM = new CustomerIndexVM
                    {
                        dtCustomerList = _dtCustomerList,
                        RemainingFee = _remainingFee
                    };
                    return View(_eProjectUploadVM);
                }
                else
                {
                    return View("Error");
                }
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult ProjectUpload(int feedBackId)
        {
            try
            {
                var _dbFeedback = _db.StudentFeedbacks
                                .Where(f => f.Id == feedBackId)
                                .FirstOrDefault();
                if (_dbFeedback != null)
                {
                    var _mdlProjectUploadVM = new CustomerProjectUploadVM()
                    {
                        Course = _dbFeedback.Course,
                        FeedbackId = _dbFeedback.Id,
                        StudentEmailId = _dbFeedback.StudentRegistration.StudentWalkInn.EmailId,
                        CROName = _dbFeedback.StudentRegistration.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ? _dbFeedback.StudentRegistration.StudentWalkInn.Employee1.Name :
                                                                                        _dbFeedback.StudentRegistration.StudentWalkInn.Employee1.Name + ',' + _dbFeedback.StudentRegistration.StudentWalkInn.Employee2.Name,
                        CenterId = _dbFeedback.StudentRegistration.StudentWalkInn.CenterCode.Id
                    };

                    return View(_mdlProjectUploadVM);
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectUpload(CustomerProjectUploadVM mdlProjectUpload)
        {
            try
            {
                var _dbFeedback = _db.StudentFeedbacks
                                .Where(f => f.Id == mdlProjectUpload.FeedbackId)
                                .FirstOrDefault();

                Common _cmn = new Common();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                if (_dbFeedback != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        //saving project
                        var _projectFile = mdlProjectUpload.ProjectUpload;
                        var _extension = Path.GetExtension(_projectFile.FileName);
                        var _projectFileName = _dbFeedback.StudentRegistration.RegistrationNumber + "_" + Regex.Replace(_dbFeedback.Course.Name, @"\s+", "") + _extension;
                        var _projectPath = "~/UploadProjects";
                        var _projectSavePath = Path.Combine(Server.MapPath(_projectPath), _projectFileName);
                        _projectFile.SaveAs(_projectSavePath);

                        _dbFeedback.IsProjectUploaded = true;
                        _dbFeedback.ProjectUploadDate = Common.LocalDateTime();
                        _dbFeedback.ProjectUploadURL = _projectPath + "/" + _projectFileName;
                        _dbFeedback.ProjectTitle = mdlProjectUpload.ProjectTitle;
                        _dbFeedback.IsTrainerVerified = false;
                        _dbFeedback.IsLeaderVerified = false;

                        _db.Entry(_dbFeedback).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            AmountDetails _clsAmountDetails = IsFeesCompleted_Feedback_ProjectUpload(mdlProjectUpload.FeedbackId, "projectupload");

                            //If paid amount is less that course amount send email to CM, Manager and student
                            if (_clsAmountDetails.PaidAmount < _clsAmountDetails.CourseAmount)
                            {

                                List<string> _emailList = new List<string>();

                                string studEmailId = mdlProjectUpload.StudentEmailId;
                                string centerMgrEmailId = _cmn.GetCentreManager(mdlProjectUpload.CenterId).OfficialEmailId;
                                string mgrEmailId = _cmn.GetManager(mdlProjectUpload.CenterId).OfficialEmailId;

                                _emailList.Add(studEmailId);
                                _emailList.Add(centerMgrEmailId);
                                _emailList.Add(mgrEmailId);

                                var _studentName = _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName;
                                var _studentRegNo = _dbFeedback.StudentRegistration.RegistrationNumber;
                                var _croName = mdlProjectUpload.CROName;
                                var _courseList = String.Join(",", _dbFeedback.StudentRegistration
                                                                    .StudentRegistrationCourses
                                                                    .Select(c => c.MultiCourse.CourseSubTitle.Name));
                                var _feedBackCourseName = String.Join(",", _dbFeedback.StudentRegistration
                                                                .StudentFeedbacks
                                                                .Where(f => f.IsProjectUploaded == true)
                                                                .Select(c => c.Course.Name));
                                var _notFeedBackCourseName = String.Join(",", _dbFeedback.StudentRegistration
                                                               .StudentFeedbacks
                                                               .Where(f => f.IsProjectUploaded == false)
                                                               .Select(c => c.Course.Name));
                                var _paymentType = _dbFeedback.StudentRegistration.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? "SINGLE" : "INSTALLMENT";
                                _notFeedBackCourseName = _notFeedBackCourseName == "" ? "All Project Uploaded" : _notFeedBackCourseName;
                                var _currentCourseName = _dbFeedback.Course.Name;
                                var _totalCourseAmt = _clsAmountDetails.CourseAmount;
                                var _totalPaid = _clsAmountDetails.PaidAmount;
                                var _outstandingAmt = _totalCourseAmt - _totalPaid;

                                var isMailSend = SendMail(_studentRegNo, _studentName, _croName, _totalPaid, _courseList, _feedBackCourseName, _notFeedBackCourseName, _totalCourseAmt, _outstandingAmt, "Project Upload", _emailList, _currentCourseName, _paymentType);

                                if (!isMailSend)
                                {
                                    return Json(new { message = "email_error" }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            _ts.Complete();
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


        #endregion

        #region E-FeedBack Section

        //EFeedback List
        public ActionResult EfeedBackList()
        {
            try
            {
                var _regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;
                if (_regNo != null)
                {
                    var _dbRegn = _db.StudentRegistrations
                                .Where(r => r.RegistrationNumber == _regNo)
                                .FirstOrDefault();
                    if (_dbRegn != null)
                    {
                        var _eFeedbackVM = new CustomerIndexVM
                        {
                            StudentFeedback = _dbRegn.StudentFeedbacks.ToList(),
                            isPhotoUploaded = _dbRegn.IsPhotoUploaded.Value,
                            isPhotoRejected = _dbRegn.IsPhotoRejected.Value
                        };
                        return View(_eFeedbackVM);
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                else
                {
                    return RedirectToAction("Login");
                }

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult EfeedBack(int feedBackID)
        {
            try
            {
                Common _cmn = new Common();
                var _dbFeedback = _db.StudentFeedbacks
                                .Where(f => f.Id == feedBackID)
                                .FirstOrDefault();

                var _instructorIdList = _cmn.GetInstructorList();

                var _instructorList = _db.Employees
                                    .Where(e => _instructorIdList.Contains(e.Designation.Id))
                                    .ToList();

                var _joinedCourseIdList = _dbFeedback
                                          .StudentRegistration
                                          .StudentFeedbacks.Select(f => f.Course.Id).ToList();

                var _notJoinedCourseId = _db.Courses
                                        .Where(c => !_joinedCourseIdList.Contains(c.Id))
                                        .ToList();
                if (_dbFeedback != null)
                {
                    var _feedBackVM = new CustomerFeedbackVM()
                    {
                        CourseName = _dbFeedback.Course.Name,
                        Duration = _dbFeedback.Course.Duration.Value,
                        InstuctorList = new SelectList(_instructorList, "Id", "Name"),
                        FeedBackId = _dbFeedback.Id,
                        StudentFeedback = _dbFeedback,
                        DefaultInstructorPhoto = "~/UploadImages/Student/NoImageSelected.png",
                        PreferredCourseList = new SelectList(_notJoinedCourseId, "Id", "Name"),
                        FutureCourseJoinStatus = EnumClass.FutureCourseJoinStatus.YES,
                        StudentEmailId = _dbFeedback.StudentRegistration.StudentWalkInn.EmailId,
                        CROName = _dbFeedback.StudentRegistration.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ? _dbFeedback.StudentRegistration.StudentWalkInn.Employee1.Name :
                                                                                        _dbFeedback.StudentRegistration.StudentWalkInn.Employee1.Name + ',' + _dbFeedback.StudentRegistration.StudentWalkInn.Employee2.Name,
                        CenterId = _dbFeedback.StudentRegistration.StudentWalkInn.CenterCode.Id

                    };
                    return View(_feedBackVM);
                }
                else
                {
                    return RedirectToAction("EfeedBackList");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EfeedBack(CustomerFeedbackVM mdlCustomerFeedback)
        {
            try
            {
                if (mdlCustomerFeedback.StudentFeedback.CiricullumRating > 2)
                {
                    ModelState.Remove("CiricullumDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.CourseRecommendationRating > 2)
                {
                    ModelState.Remove("CourseRecommendationDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.CustomerCareRating > 2)
                {
                    ModelState.Remove("CustomerCareDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.EquipmentRating > 2)
                {
                    ModelState.Remove("EquipmentDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.InstructorClassStartTimeRating > 2)
                {
                    ModelState.Remove("InstructorClassStartTimeDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.InstructorExplanationRating > 2)
                {
                    ModelState.Remove("InstructorExplanationDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.InstructorPreparationRating > 2)
                {
                    ModelState.Remove("InstructorPreparationDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.InstructorProjectSupportRating > 2)
                {
                    ModelState.Remove("InstructorProjectSupportDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.LearningEnvironmentRating > 2)
                {
                    ModelState.Remove("LearningEnvironmentDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.OverallExperienceRating > 2)
                {
                    ModelState.Remove("OverallExperienceDislikeReason");
                }
                if (mdlCustomerFeedback.StudentFeedback.LearningEnvironmentRating > 2)
                {
                    ModelState.Remove("LearningEnvironmentDislikeReason");
                }
                if (mdlCustomerFeedback.FutureCourseJoinStatus == EnumClass.FutureCourseJoinStatus.NO)
                {
                    ModelState.Remove("PreferredCourseID");
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                        var _isNegativeFeedbackGiven = false;
                        List<NegativeFeedbackDetails> _negFeedbackDetails = new List<NegativeFeedbackDetails>();
                        var q = 0;

                        var _dbFeedback = _db.StudentFeedbacks
                                        .Where(f => f.Id == mdlCustomerFeedback.FeedBackId)
                                        .FirstOrDefault();
                        if (_dbFeedback != null)
                        {
                            _dbFeedback.InstructorID = mdlCustomerFeedback.InstructorID;
                            _dbFeedback.IsFeedbackGiven = true;
                            _dbFeedback.FeedbackDate = Common.LocalDateTime();
                            _dbFeedback.CourseStartDate = mdlCustomerFeedback.CourseStartDate;
                            _dbFeedback.CourseEndDate = mdlCustomerFeedback.CourseEndDate;
                            _dbFeedback.IsHardCopyRequired = mdlCustomerFeedback.IsHardCopyRequired;
                            _dbFeedback.LearningEnvironmentRating = mdlCustomerFeedback.StudentFeedback.LearningEnvironmentRating;
                            if (mdlCustomerFeedback.StudentFeedback.LearningEnvironmentRating <= 2)
                            {
                                _dbFeedback.LearningEnvironmentDislikeReason = mdlCustomerFeedback.LearningEnvironmentDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = " Course was given in an environment conducive for learning";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.LearningEnvironmentRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.LearningEnvironmentDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);
                            }

                            _dbFeedback.EquipmentRating = mdlCustomerFeedback.StudentFeedback.EquipmentRating;
                            if (mdlCustomerFeedback.StudentFeedback.EquipmentRating <= 2)
                            {
                                _dbFeedback.EquipmentDislikeReason = mdlCustomerFeedback.EquipmentDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = " Computers and equipment were in good working condition";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.EquipmentRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.EquipmentDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);
                            }

                            _dbFeedback.CiricullumRating = mdlCustomerFeedback.StudentFeedback.CiricullumRating;
                            if (mdlCustomerFeedback.StudentFeedback.CiricullumRating <= 2)
                            {
                                _dbFeedback.CiricullumDislikeReason = mdlCustomerFeedback.CiricullumDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = " Course and ciricullum matched with my expectation";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.CiricullumRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.CiricullumDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);
                            }

                            _dbFeedback.InstructorClassStartTimeRating = mdlCustomerFeedback.StudentFeedback.InstructorClassStartTimeRating;
                            if (mdlCustomerFeedback.StudentFeedback.InstructorClassStartTimeRating <= 2)
                            {
                                _dbFeedback.InstructorClassStartTimeDislikeReason = mdlCustomerFeedback.InstructorClassStartTimeDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = " Instructor has started class ontime";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.InstructorClassStartTimeRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.InstructorClassStartTimeDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }

                            _dbFeedback.InstructorPreparationRating = mdlCustomerFeedback.StudentFeedback.InstructorPreparationRating;
                            if (mdlCustomerFeedback.StudentFeedback.InstructorPreparationRating <= 2)
                            {
                                _dbFeedback.InstructorPreparationDislikeReason = mdlCustomerFeedback.InstructorPreparationDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = "Instructor was well prepared";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.InstructorPreparationRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.InstructorPreparationDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }

                            _dbFeedback.InstructorExplanationRating = mdlCustomerFeedback.StudentFeedback.InstructorExplanationRating;
                            if (mdlCustomerFeedback.StudentFeedback.InstructorExplanationRating <= 2)
                            {
                                _dbFeedback.InstructorExplanationDislikeReason = mdlCustomerFeedback.InstructorExplanationDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = " Instructor explained the course in an easy way to understand";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.InstructorExplanationRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.InstructorExplanationDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }



                            _dbFeedback.InstructorProjectSupportRating = mdlCustomerFeedback.StudentFeedback.InstructorProjectSupportRating;
                            if (mdlCustomerFeedback.StudentFeedback.InstructorProjectSupportRating <= 2)
                            {
                                _dbFeedback.InstructorProjectSupportDislikeReason = mdlCustomerFeedback.InstructorProjectSupportDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = "Instructor was helpful to support Lab,Project and ProjectLab";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.InstructorProjectSupportRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.InstructorProjectSupportDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }

                            _dbFeedback.OverallExperienceRating = mdlCustomerFeedback.StudentFeedback.OverallExperienceRating;
                            if (mdlCustomerFeedback.StudentFeedback.OverallExperienceRating <= 2)
                            {
                                _dbFeedback.OverallExperienceDislikeReason = mdlCustomerFeedback.OverallExperienceDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = "Overall experience";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.OverallExperienceRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.OverallExperienceDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }

                            _dbFeedback.CustomerCareRating = mdlCustomerFeedback.StudentFeedback.CustomerCareRating;
                            if (mdlCustomerFeedback.StudentFeedback.CustomerCareRating <= 2)
                            {
                                _dbFeedback.CustomerCareDislikeReason = mdlCustomerFeedback.CustomerCareDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = "Care and friendliness of our customer care team";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.CustomerCareRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.CustomerCareDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }

                            _dbFeedback.CourseRecommendationRating = mdlCustomerFeedback.StudentFeedback.CourseRecommendationRating;
                            if (mdlCustomerFeedback.StudentFeedback.CourseRecommendationRating <= 2)
                            {
                                _dbFeedback.CourseRecommendationDislikeReason = mdlCustomerFeedback.CourseRecommendationDislikeReason.ToUpper();
                                _isNegativeFeedbackGiven = true;
                                q = q + 1;
                                NegativeFeedbackDetails _negFeedback = new NegativeFeedbackDetails();
                                _negFeedback.QuestionNo = q.ToString();
                                _negFeedback.Question = "I will recommend this course to others";
                                _negFeedback.Rating = mdlCustomerFeedback.StudentFeedback.CourseRecommendationRating.ToString();
                                _negFeedback.Reason = mdlCustomerFeedback.CourseRecommendationDislikeReason;
                                _negFeedbackDetails.Add(_negFeedback);

                            }


                            _dbFeedback.IsCourseInterestedInFuture = mdlCustomerFeedback.FutureCourseJoinStatus == EnumClass.FutureCourseJoinStatus.YES ? true : false;
                            _dbFeedback.PreferredCourseIds = mdlCustomerFeedback.FutureCourseJoinStatus == EnumClass.FutureCourseJoinStatus.YES ?
                                                            string.Join(",", mdlCustomerFeedback.PreferredCourseID) : null;
                            if (mdlCustomerFeedback.FutureCourseJoinStatus == EnumClass.FutureCourseJoinStatus.YES)
                            {
                                _dbFeedback.PreferredCourseName = mdlCustomerFeedback.StudentFeedback.PreferredCourseName != null ? mdlCustomerFeedback.StudentFeedback.PreferredCourseName.ToUpper() : null;
                            }
                            _db.Entry(_dbFeedback).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {

                                AmountDetails _clsAmountDetails = IsFeesCompleted_Feedback_ProjectUpload(mdlCustomerFeedback.FeedBackId, "feedback");
                                if ((_clsAmountDetails.PaidAmount < _clsAmountDetails.CourseAmount) || (_isNegativeFeedbackGiven == true))
                                {
                                    Common _cmn = new Common();
                                    List<string> _emailList = new List<string>();

                                    //Getting centermangaer and manager emailid
                                    string centerMgrEmailId = _cmn.GetCentreManager(mdlCustomerFeedback.CenterId).OfficialEmailId;
                                    string mgrEmailId = _cmn.GetManager(mdlCustomerFeedback.CenterId).OfficialEmailId;
                                    string edEmailId = "ed@networkzsystems.com";

                                    //Adding email to emaillist
                                    _emailList.Add(centerMgrEmailId);
                                    _emailList.Add(mgrEmailId);
                                    _emailList.Add(edEmailId);

                                    //if any negative feedback given then email to cm and manager
                                    if (_isNegativeFeedbackGiven == true)
                                    {

                                        var _studentMobileNo = _dbFeedback.StudentRegistration.StudentWalkInn.MobileNo;
                                        var _croName = mdlCustomerFeedback.CROName;
                                        var _instructorName = _dbFeedback.Employee.Name;

                                        //sending feedback email
                                        var isFeebackMailSend = SendFeedbackMail(_negFeedbackDetails, _dbFeedback.Course.Name, _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName,
                                                                _dbFeedback.StudentRegistration.RegistrationNumber, _emailList, _studentMobileNo, _croName, _instructorName);

                                        if (!isFeebackMailSend)
                                        {
                                            return Json(new { message = "email_error" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }


                                    //If paid amount is less that course amount send email to CM, Manager and student
                                    if (_clsAmountDetails.PaidAmount < _clsAmountDetails.CourseAmount)
                                    {
                                        string studEmailId = mdlCustomerFeedback.StudentEmailId;
                                        _emailList.Add(studEmailId);


                                        var _studentName = _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName;
                                        var _studentRegNo = _dbFeedback.StudentRegistration.RegistrationNumber;
                                        var _croName = mdlCustomerFeedback.CROName;
                                        var _courseList = String.Join(",", _dbFeedback.StudentRegistration
                                                                            .StudentRegistrationCourses
                                                                            .Select(c => c.MultiCourse.CourseSubTitle.Name));
                                        var _feedBackCourseName = String.Join(",", _dbFeedback.StudentRegistration
                                                                        .StudentFeedbacks
                                                                        .Where(f => f.IsFeedbackGiven == true)
                                                                        .Select(c => c.Course.Name));
                                        var _notFeedBackCourseName = String.Join(",", _dbFeedback.StudentRegistration
                                                                       .StudentFeedbacks
                                                                       .Where(f => f.IsFeedbackGiven == false)
                                                                       .Select(c => c.Course.Name));
                                        _notFeedBackCourseName = _notFeedBackCourseName == "" ? "All Feedback Submitted" : _notFeedBackCourseName;
                                        var _paymentType = _dbFeedback.StudentRegistration.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? "SINGLE" : "INSTALLMENT";

                                        var _currentCourseName = _dbFeedback.Course.Name;
                                        var _totalCourseAmt = _clsAmountDetails.CourseAmount;
                                        var _totalPaid = _clsAmountDetails.PaidAmount;
                                        var _outstandingAmt = _totalCourseAmt - _totalPaid;

                                        var isMailSend = SendMail(_studentRegNo, _studentName, _croName, _totalPaid, _courseList, _feedBackCourseName, _notFeedBackCourseName, _totalCourseAmt, _outstandingAmt, "Feedback", _emailList, _currentCourseName, _paymentType);

                                        if (!isMailSend)
                                        {
                                            return Json(new { message = "email_error" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }

                                }
                                _ts.Complete();
                                return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }

                }

                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                //foreach (var entityValidationErrors in ex.EntityValidationErrors)
                //{
                //    foreach (var validationError in entityValidationErrors.ValidationErrors)
                //    {
                //        Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                //    }
                //}
                return Json(ex.Message);
            }
        }

        public bool SendFeedbackMail(List<NegativeFeedbackDetails> negFeedbackDetails, string currCourseName, string studName, string studID, List<string> emailList,
                                            string studentMobileNo, string croName, string instructorName)
        {
            string _body = string.Empty;
            var tdString = "";

            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/Feedback.html")))
            {
                _body = reader.ReadToEnd();
            }

            foreach (var item in negFeedbackDetails)
            {
                tdString = tdString + "<tr><td>" + item.QuestionNo + "</td>";
                tdString = tdString + "<td>" + item.Question + "</td>";
                tdString = tdString + "<td style='text-align:center'>" + item.Rating + "</td>";
                tdString = tdString + "<td>" + item.Reason + "</td></tr>";
            }
            _body = _body.Replace("{StudentID}", studID);
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{CurrentCourseName}", currCourseName);
            _body = _body.Replace("{StudentMobileNo}", studentMobileNo);
            _body = _body.Replace("{CroName}", croName);
            _body = _body.Replace("{InstructorName}", instructorName);
            _body = _body.Replace("{FeedbackContent}", tdString);

            Common _cmn = new Common();
            //Email sending
            var isMailSend = _cmn.SendEmail(string.Join(",", emailList), _body, "Student Feedback");
            return isMailSend;
        }

        public ActionResult GetFacultyPhoto(int empId)
        {
            try
            {
                var _photoUrl = _db.Employees
                              .Where(e => e.Id == empId)
                              .FirstOrDefault().PhotoUrl;

                return Json(new { message = _photoUrl }, JsonRequestBehavior.AllowGet);

            }

            catch (Exception ex)
            {
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }




        #endregion

        #region PhotoUpload
        public ActionResult PhotoUpload()
        {
            try
            {
                var _regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;
                if (_regNo != null)
                {
                    var _dbRegn = _db.StudentRegistrations
                                .Where(r => r.RegistrationNumber == _regNo)
                                .FirstOrDefault();
                    if (_dbRegn != null)
                    {
                        var _photoUploadVM = new CustomerPhotoUploadVM
                        {
                            StudentRegId = _dbRegn.Id,
                            PhotoUrl = _dbRegn.PhotoUrl,
                            IsPhotoVerified = _dbRegn.IsPhotoVerified.Value,
                            IsPhotoRejected = _dbRegn.IsPhotoRejected.Value

                        };
                        return View(_photoUploadVM);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PhotoUpload(CustomerPhotoUploadVM mdlPhotoUploadVM)
        {
            try
            {
                var _dbRegn = _db.StudentRegistrations
                        .Where(r => r.Id == mdlPhotoUploadVM.StudentRegId)
                        .FirstOrDefault();
                if (_dbRegn != null)
                {
                    if (mdlPhotoUploadVM.PhotoNewUrl != null)
                    {
                        var _photoFile = mdlPhotoUploadVM.PhotoNewUrl;
                        var _extension = Path.GetExtension(_photoFile.FileName);
                        var _imgFileName = _dbRegn.RegistrationNumber + _extension;
                        var _imgPath = "~/UploadImages/Student";
                        var _imgSavePath = Path.Combine(Server.MapPath(_imgPath), _imgFileName);
                        _photoFile.SaveAs(_imgSavePath);

                        _dbRegn.PhotoUrl = _imgPath + "/" + _imgFileName;
                        _dbRegn.IsPhotoUploaded = true;
                        _dbRegn.PhotoUploadedDate = Common.LocalDateTime();
                        _dbRegn.IsPhotoRejected = false;
                        _dbRegn.PhotoRejectedReason = null;
                        _dbRegn.PhotoRejectedDate = null;

                        _db.Entry(_dbRegn).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("exception", JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region ReceiptSection
        public ActionResult ReceiptList()
        {
            try
            {
                var _regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;
                //var _regNo = "NSTAJ0316093";
                List<CustomerReceiptVM> _receiptList = new List<CustomerReceiptVM>();
                if (_regNo != null)
                {
                    var _dbReceiptsList = _db.StudentReceipts
                                        .Where(r => r.StudentRegistration.RegistrationNumber == _regNo)
                                        .ToList();
                    if (_dbReceiptsList != null)
                    {
                        _receiptList = _dbReceiptsList
                                            .Select(r => new CustomerReceiptVM
                                            {
                                                DueDate = r.DueDate.Value.ToString("dd/MM/yyyy"),
                                                Fee = r.Fee.Value,
                                                ReceiptId = r.Id,
                                                ReceiptNo = r.StudentReceiptNo != null ? Common.GetReceiptNo(r.StudentReceiptNo.Value) : "--",
                                                ST = r.ST.Value,
                                                STPercentage = r.STPercentage.Value,
                                                Total = r.Total.Value,
                                                Status = r.Status.Value

                                            }).ToList();
                        return View(_receiptList);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public void ReceiptDownload(int receiptId)
        {
            try
            {
                Common _cmn = new Common();
                byte[] _content = _cmn.MergeReceiptPdf(receiptId);
                Response.Clear();
                MemoryStream ms = new MemoryStream(_content);
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Receipt.pdf");
                Response.Buffer = true;
                ms.WriteTo(Response.OutputStream);
                Response.End();
            }
            catch
            {

            }

        }

        //Checks whether pdf exists or not
        [HttpPost]
        public JsonResult ReceiptPdfCheck(int receiptId)
        {
            try
            {
                var _dbReceipt = _db.StudentReceipts
                                .Where(r => r.Id == receiptId)
                                .FirstOrDefault();
                if (_dbReceipt != null)
                {
                    var _fileName = "Receipt/" + _dbReceipt.StudentRegistration.RegistrationNumber + "_" + Common.GetReceiptNo(_dbReceipt.StudentReceiptNo.Value) + ".pdf";
                    var _filePath = Path.Combine(HttpRuntime.AppDomainAppPath, _fileName);
                    if (System.IO.File.Exists(_filePath))
                    {
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("receipt_not_found", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("db_error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region CertificateStatus Section
        public ActionResult CertificateStatus()
        {
            try
            {
                var _regNo = Session["CustomerRegNo"] != null ? Session["CustomerRegNo"].ToString() : null;
                List<CustomerCertificateStatusVM> _feedbackList = new List<CustomerCertificateStatusVM>();
                if (_regNo != null)
                {

                    _feedbackList = _db.StudentFeedbacks
                                    .Where(f => f.StudentRegistration.RegistrationNumber == _regNo)
                                    .AsEnumerable()
                                    .Select(f => new CustomerCertificateStatusVM
                                            {
                                                ListStudentFeedback = f.StudentRegistration.StudentFeedbacks.ToList(),
                                                StudentFeedback = f,
                                                StudentReceipt = f.StudentRegistration.StudentReceipts.ToList(),
                                                StudentRegistration = f.StudentRegistration,
                                                CourseName = f.Course.Name
                                            }).ToList();
                    return View(_feedbackList);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }
        #endregion

        public bool SendSMS(string studName, int pinNo, string mobileNo)
        {
            bool result = false;
            //sending message to student
            var _message = "Dear " + studName + ".You can now login to Networkz Systems using the following PinNo - " + pinNo;
            Common _cmn = new Common();
            string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + mobileNo + "&msg=" + _message + "&route=T");
            if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
            {
                result = true;
                return result;
            }
            return result;
        }

        //send verification mail to regtistered student
        public bool SendMail(string studId, string studName, string croName, int totalPaid, string courseList, string courseName,
                            string futureCourseName, int totalCourseAmt, int outstandingAmt, string action, List<string> _emailList, string currCourseName, string paymentType)
        {
            string _body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/WarningEmail.html")))
            {
                _body = reader.ReadToEnd();
            }
            _body = _body.Replace("{CurrentCourseName}", currCourseName);
            _body = _body.Replace("{StudentID}", studId);
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{CROName}", croName);
            _body = _body.Replace("{CourseList}", courseList);
            _body = _body.Replace("{CourseName}", courseName);
            _body = _body.Replace("{FutureCourseName}", futureCourseName);
            _body = _body.Replace("{PaymentType}", paymentType);
            _body = _body.Replace("{TotalPaid}", totalPaid.ToString("#,##0"));
            _body = _body.Replace("{TotalCourseAmount}", totalCourseAmt.ToString("#,##0"));
            _body = _body.Replace("{OutstandingAmount}", outstandingAmt.ToString("#,##0"));
            _body = _body.Replace("{Action}", action);

            Common _cmn = new Common();
            //Email sending
            //var isMailSend = _cmn.SendEmail(string.Join(",", _emailList), _body, "Payment Reminder");
            var isMailSend = _cmn.SendEmail(string.Join(",", _emailList), _body, "Payment Reminder");
            return isMailSend;
        }



        //Checks if user completed the current feedback and projectupload course amount
        public AmountDetails IsFeesCompleted_Feedback_ProjectUpload(int feedBackId, string action)
        {
            AmountDetails _clsAmt = new AmountDetails();
            var _totalFeebackAmount = 0;

            var _dbStudentFeedback = _db.StudentFeedbacks
                                    .Where(r => r.Id == feedBackId)
                                    .FirstOrDefault();

            var _dbStudentRegistration = _db.StudentFeedbacks
                                        .Where(r => r.Id == feedBackId)
                                        .FirstOrDefault().StudentRegistration;

            //Total amount paid by the student(excluding ST)
            var _paidAmount = _dbStudentRegistration.StudentReceipts
                              .Where(r => r.Status == true)
                              .Sum(r => r.Fee);

            var _feeMode = _dbStudentRegistration.FeeMode;
            //if installment type is single
            if (_feeMode == (int)EnumClass.InstallmentType.SINGLE)
            {
                _totalFeebackAmount = _dbStudentRegistration.TotalCourseFee.Value;
            }
            else
            {
                if (action == "projectupload")
                {
                    //adding all feedback amount where projectuploaded is true
                    _totalFeebackAmount = _dbStudentRegistration.StudentFeedbacks
                                            .Where(r => r.IsProjectUploaded == true)
                                            .Sum(r => r.TotalCourseAmount.Value);
                }
                else
                {
                    //adding all feedback amount where feedback is true
                    _totalFeebackAmount = _dbStudentRegistration.StudentFeedbacks
                                            .Where(r => r.IsFeedbackGiven == true)
                                            .Sum(r => r.TotalCourseAmount.Value);
                }

            }



            _clsAmt.CourseAmount = _totalFeebackAmount;
            _clsAmt.PaidAmount = _paidAmount.Value;
            return _clsAmt;
        }

        public ActionResult LogOut()
        {
            Session["CustomerRegNo"] = null;
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
