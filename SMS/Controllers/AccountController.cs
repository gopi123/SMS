using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SMS.Controllers
{
    public class AccountController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /Account/

        public ActionResult Login()
        {
            Common _cmn = new Common();
            
            return View();

            //string name = _cmn.ComputePassword("admin123");
            //Session["LoggedUserId"] = 10;
            //Session["LoggedUserName"] = "test";
            //Session["LoggedUserRoleName"] = "test";
            //return RedirectToAction("Index", "DiscountSetting");
            //return View("Maintenance");
        }     




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM mdlLogin)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    Common _cmn = new Common();
                    var _passswordHash = _cmn.ComputePassword(mdlLogin.Password);
                    var _user = _db.Users
                                .Where(u => u.UserName == mdlLogin.Username
                                        && u.Password == _passswordHash
                                        && u.Employee.Status == true)
                                .FirstOrDefault();


                    if (_user != null)
                    {
                        Session["LoggedUserId"] = _user.EmployeeId;
                        Session["LoggedUserName"] = _user.UserName;
                        Session["LoggedUserRoleName"] = _user.Employee.Designation.Role.RoleName;


                        if (_user.Employee.IsEmailVerified == false)
                        {
                            return RedirectToAction("EmailNotVerified");
                        }
                        if (_user.Employee.IsMobileVerified == false)
                        {
                            return RedirectToAction("MobileNotVerified");
                        }
                        if (_user.IsFirstTimeLogin == true)
                        {
                            return RedirectToAction("PasswordChange");
                        }

                        FormsAuthentication.SetAuthCookie(mdlLogin.Username, true);
                        //return RedirectToAction("WalkInnReport", "Report");
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid username or password";
                        return View();
                    }
                }
                return View();

            }
            catch (Exception ex)
            {
                return View();
            }


        }

        #region ForgotPassword
        //Forgot Password
        public ActionResult ForgotPassword()
        {
            Common _cmn = new Common();
            var _passwordGenerator = _cmn.CreatePassword(6);
            if (_passwordGenerator != null)
            {
                var _passwordHash = _cmn.ComputePassword(_passwordGenerator);
                if (_passwordHash != null)
                {
                    var data = Encoding.ASCII.GetBytes(_passwordHash);
                    //var hashedPassword = ASCIIEncoding.GetString(data); 
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordVM mdlForgotPassword)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {

                        var _user = _db.Users.Where(u => u.UserName == mdlForgotPassword.Username &&
                                                    u.Employee.EmailId == mdlForgotPassword.Email.ToUpper() &&
                                                    u.Employee.DOB == mdlForgotPassword.DOB &&
                                                    u.Employee.MobileNo == mdlForgotPassword.MobileNo &&
                                                    u.Employee.Status == true)
                                            .FirstOrDefault();
                        if (_user != null)
                        {
                            //if email is not verified
                            if (_user.Employee.IsEmailVerified == false)
                            {
                                return Json(new { message = "EmailNotVerified" }, JsonRequestBehavior.AllowGet);
                            }
                            //if mobile is not verified
                            else if (_user.Employee.IsMobileVerified == false)
                            {
                                return Json(new { message = "MobileNotVerified" }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                #region updatePassword
                                Common _cmn = new Common();
                                //Create an unique 5 digit password
                                var _passwordGenerator = _cmn.CreatePassword(5);
                                if (_passwordGenerator != null)
                                {
                                    //Password Hashing is performed here
                                    var _passwordHash = _cmn.ComputePassword(_passwordGenerator);
                                    if (_passwordHash != null)
                                    {
                                        //saving password to user table
                                        _user.Password = _passwordHash;
                                        _user.IsFirstTimeLogin = true;
                                        _db.Entry(_user).State = EntityState.Modified;
                                        int i = _db.SaveChanges();
                                        if (i > 0)
                                        {
                                            //Populate body                                         
                                            string _body = PopulateBody_ForgotPassword(_user.Employee.Name, _passwordGenerator);
                                            //Sending Mail
                                            var isMailSend = _cmn.SendEmail(_user.Employee.EmailId, _body, "Password Reset");
                                            if (isMailSend)
                                            {
                                                _ts.Complete();
                                                return Json(new { message = "Success" }, JsonRequestBehavior.AllowGet);
                                            }


                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            return Json(new { message = "InvalidUser" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(new { message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return View();
            }

        }

        #endregion

        #region EmailUpdate Verification

        public ActionResult EmailNotVerified()
        {
            return View();
        }

        //Mailsending goes here
        public ActionResult EmailNotVerifiedPost()
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {

                    //gets the current email verification details
                    var _emailVerification = _db.EmailVerifications
                                            .AsEnumerable()
                                            .Where(m => m.TypeId == Int32.Parse(Session["LoggedUserId"].ToString()) && m.Type == "E")
                                            .FirstOrDefault();
                    //if emailverification exists
                    if (_emailVerification != null)
                    {
                        //creates new key
                        string _key = Guid.NewGuid().ToString();
                        //updates current emailverification details
                        _emailVerification.VerificationKey = _key;
                        _db.Entry(_emailVerification).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        //if successfull updation
                        if (i > 0)
                        {
                            Common _cmn = new Common();
                            //gets the current employee details
                            var _employee = _db.Employees
                                          .AsEnumerable()
                                          .Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                          .FirstOrDefault();
                            //if employee exists
                            if (_employee != null)
                            {
                                //Email sending
                                string _emailKey = "http://www.networkzsystem.com/sms/Account/EmailVerification?key=" + _key;
                                string _body = PopulateBody_EmailUpdate(_employee.Name, _emailKey);

                                var isMailSend = _cmn.SendEmail(_employee.EmailId, _body, "Email Verification");
                                if (isMailSend)
                                {
                                    _ts.Complete();
                                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                    }
                    return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EmailVerification(string key)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    //gets the current emailverification details
                    var _emailVerification = _db.EmailVerifications
                                            .Where(x => x.VerificationKey == key)
                                            .FirstOrDefault();
                    //if emailverification exists
                    if (_emailVerification != null)
                    {
                        //gets the current employee details
                        var _employee = _db.Employees
                                       .Where(e => e.Id == _emailVerification.TypeId)
                                       .FirstOrDefault();
                        //if employee exists
                        if (_employee != null)
                        {
                            //update the employee details
                            _employee.IsEmailVerified = true;
                            _db.Entry(_employee).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            //if successfull updation
                            if (i > 0)
                            {
                                _ts.Complete();
                                return View("EmailVerified");
                            }
                        }
                    }
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }



        #endregion

        #region MobileUpdate Verification

        public ActionResult MobileNotVerified()
        {
            return View();
        }

        //Mobile Verification code goes here       
        public ActionResult MobileNotVerifiedPost()
        {
            Common _cmn = new Common();
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    //Gets RandomNo
                    int _randomNo = _cmn.GenerateRandomNo();
                    int _empId = Convert.ToInt32(Session["LoggedUserId"]);
                    string _empMobileNo = _db.Employees.Where(e => e.Id == _empId)
                                                       .Select(e => e.MobileNo).FirstOrDefault();
                    if (_empMobileNo != null)
                    {
                        //Gets the last mobile verification details
                        var _mobVerification = _db.MobileVerifications.Where(x => x.Type == "E" && x.TypeId == _empId)
                                                                      .OrderByDescending(x => x.Id)
                                                                      .First();
                        //if mobile verification exists
                        if (_mobVerification != null)
                        {
                            _mobVerification.PinNo = _randomNo;
                            _db.Entry(_mobVerification).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            //if successfull updation
                            if (i > 0)
                            {
                                //Sends the 4 digitno to the employees mobileNo
                                string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _empMobileNo + "&msg=Enter this pin number " + _randomNo + " to register. By Entering this pin number I accept all communications from NetworkzSystems to this mobile number&route=T");
                                if (!_result.StartsWith("Invalid Username/password"))
                                {
                                    _ts.Complete();
                                    return Json(new { message = "Success" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    return Json(new { message = "Error" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { message = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MobileVerification()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MobileVerification(MobileVerification mdlMobileVerification)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    //gets the current mobile verification details
                    var _mobVerification = _db.MobileVerifications
                                        .AsEnumerable()
                                        .Where(m => m.PinNo == mdlMobileVerification.PinNo)
                                        .FirstOrDefault();
                    if (_mobVerification != null)
                    {
                        //gets the current employee details
                        var _employee = _db.Employees
                                      .AsEnumerable()
                                      .Where(e => e.Id == _mobVerification.TypeId)
                                      .FirstOrDefault();
                        //if employee exists
                        if (_employee != null)
                        {
                            //updating mobile verification status
                            _employee.IsMobileVerified = true;
                            _db.Entry(_employee).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {
                                //Checks whether the user is first time login
                                var _isFirstTimeLogin = _employee.Users.FirstOrDefault().IsFirstTimeLogin;
                                //if first time login sends password the users mobile       
                                if (_isFirstTimeLogin == true)
                                {
                                    var _user = _employee.Users.FirstOrDefault();
                                    Common _cmn = new Common();
                                    //Create an unique 5 digit password
                                    var _passwordGenerator = _cmn.CreatePassword(5);
                                    if (_passwordGenerator != null)
                                    {
                                        //Password Hashing is performed here
                                        var _passwordHash = _cmn.ComputePassword(_passwordGenerator);
                                        if (_passwordHash != null)
                                        {
                                            //saving password to user table
                                            _user.Password = _passwordHash;
                                            _db.Entry(_user).State = EntityState.Modified;
                                            int j = _db.SaveChanges();
                                            if (j > 0)
                                            {
                                                //Sends the password to the employees mobileNo
                                                string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _employee.MobileNo + "&msg=Registration successfull.Your password is  " + _passwordGenerator + " . Please login into the website using the given password.&route=T");
                                                if (!_result.StartsWith("Invalid Username/password"))
                                                {
                                                    _ts.Complete();
                                                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                                }
                                                else
                                                {
                                                    return Json(new { message = "error_mobile" }, JsonRequestBehavior.AllowGet);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    _ts.Complete();
                                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    return Json(new { message = "error_pinno" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MobileVerified()
        {
            return View();
        }

        #endregion

        #region UserVerification
        //This method is called when user clicks on the mail received during registraion process
        public ActionResult UserVerification(string key)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    //gets the current emailverification details
                    var _emailVerification = _db.EmailVerifications
                                        .Where(x => x.VerificationKey == key)
                                        .FirstOrDefault();
                    //if emailverification exists
                    if (_emailVerification != null)
                    {
                        //gets the current employee details
                        var _employee = _db.Employees
                                       .Where(e => e.Id == _emailVerification.TypeId)
                                       .FirstOrDefault();
                        //if employee exists
                        if (_employee != null)
                        {
                            if (_employee.IsEmailVerified == true && _employee.IsMobileVerified == true)
                            {
                                return RedirectToAction("DetailsVerified");
                            }
                            else
                            {
                                //update the employee details
                                _employee.IsEmailVerified = true;
                                _db.Entry(_employee).State = EntityState.Modified;
                                int i = _db.SaveChanges();
                                //if successfull updation 
                                if (i > 0)
                                {
                                    //If mobile is not verified
                                    if (_employee.IsMobileVerified == false)
                                    {
                                        Common _cmn = new Common();
                                        //Gets RandomNo
                                        int _randomNo = _cmn.GenerateRandomNo();
                                        //gets the employee mobileNo
                                        string _empMobileNo = _employee.MobileNo;
                                        if (_empMobileNo != null)
                                        {
                                            //Adds new MobileVerification details
                                            MobileVerification _mobVerification = new MobileVerification();
                                            _mobVerification.PinNo = _randomNo;
                                            _mobVerification.Type = "E";
                                            _mobVerification.TypeId = _employee.Id;
                                            _db.MobileVerifications.Add(_mobVerification);

                                            int j = _db.SaveChanges();

                                            if (j > 0)
                                            {
                                                #region code for setting IsFirstTimeLogin in users table
                                                //gets the current user details
                                                var _user = _db.Users.Where(x => x.EmployeeId == _employee.Id).FirstOrDefault();
                                                _user.IsFirstTimeLogin = true;
                                                _db.Entry(_user).State = EntityState.Modified;
                                                int k = _db.SaveChanges();

                                                #endregion
                                                if (k > 0)
                                                {
                                                    //Sends the 4 digitno to the employees mobileNo
                                                    string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _empMobileNo + "&msg=Enter this pin number " + _randomNo + " to register. By Entering this pin number I accept all communications from NetworkzSystems to this mobile number&route=T");
                                                    if (!_result.StartsWith("Invalid Username/password"))
                                                    {
                                                        _ts.Complete();
                                                        return RedirectToAction("MobileVerification");

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return RedirectToAction("EmailSuccessfullVerification");
                                    }
                                }

                            }

                        }
                    }

                }
                return View("Error");
            }
            catch (Exception ex)
            {
                return View("Error");
            }


        }


        #endregion

        #region PasswordChange Region

        public ActionResult PasswordChange()
        {
            try
            {
                var _user = _db.Users.AsEnumerable()
                          .Where(u => u.EmployeeId == Int32.Parse(Session["LoggedUserId"].ToString()))
                          .FirstOrDefault();
                if (_user != null)
                {
                    var _userVM = new UserVM
                    {
                        Id = _user.Id,
                        UserName = _user.UserName
                    };
                    return View(_userVM);
                }

                return View("Error");
            }
            catch (Exception ex)
            {
                return View("Error");
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PasswordChange(UserVM mdlUserVM)
        {
            try
            {
                //get current user
                var _user = _db.Users.AsEnumerable()
                          .Where(u => u.Id == mdlUserVM.Id)
                          .FirstOrDefault();
                //if user exists
                if (_user != null)
                {
                    Common _cmn = new Common();
                    //md5 hashing
                    var _currentPassword = _cmn.ComputePassword(mdlUserVM.CurrentPassword);
                    //if current password equals the database password
                    if (_user.Password == _currentPassword)
                    {
                        _user.Password = _cmn.ComputePassword(mdlUserVM.NewPassword);
                        _user.IsFirstTimeLogin = false;

                        _db.Entry(_user).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            return Json(new { message = "success", JsonRequestBehavior.AllowGet });
                        }
                    }
                    //else
                    else
                    {
                        return Json(new { message = "error_password", JsonRequestBehavior.AllowGet });
                    }
                }

                return Json(new { message = "error", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                return Json(new { message = "exceprtion", JsonRequestBehavior.AllowGet });
            }


        }

        #endregion

        public ActionResult EmailSuccessfullVerification()
        {
            return View("EmailSuccessfullVerification");
        }

        public ActionResult DetailsVerified()
        {
            return View();
        }

        private string PopulateBody_ForgotPassword(string employeeName, string updatedPassword)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/ForgotPassword.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{EmployeeName}", employeeName);
            body = body.Replace("{UpdatedPassword}", updatedPassword);
            body = body.Replace("{ActivationLink}", "http://www.networkzsystems.com/sms");
            return body;
        }

        private string PopulateBody_EmailUpdate(string employeeName, string link)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/EmailUpdateTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{EmployeeName}", employeeName);
            body = body.Replace("{ActivationLink}", link);
            return body;
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");

        }

        public ActionResult StudentVerification(string key)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    //gets the current emailverification details
                    var _emailVerification = _db.EmailVerifications
                                            .OrderByDescending(e => e.VerificationKey == key)
                                            .First();

                    if (_emailVerification != null)
                    {
                        //gets the current student registration details
                        StudentRegistration _student = _db.StudentRegistrations
                                                       .Where(r => r.Id == _emailVerification.TypeId)
                                                       .First();


                        if (_student != null)
                        {
                            //if student hasn't verified the email yet
                            if (_student.IsEmailVerified == false)
                            {
                                _student.IsEmailVerified = true;
                                _db.Entry(_student).State = EntityState.Modified;
                                int i = _db.SaveChanges();
                                if (i > 0)
                                {
                                    _ts.Complete();
                                    return RedirectToAction("StudentEmailSuccessfullVerification");
                                }
                            }
                            //if the user clicks the student verification link twice
                            else
                            {
                                return RedirectToAction("StudentEmailAlreadyVerified");
                            }
                        }
                    }
                }
                return View("Error");
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult StudentEmailSuccessfullVerification()
        {
            return View("StudentEmailSuccessfullVerification");
        }

        public ActionResult StudentEmailAlreadyVerified()
        {
            return View("StudentEmailAlreadyVerified");
        }

        public ActionResult Testing()
        {
            return View("Maintenance");
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }



    }
}
