using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Data.Entity;

namespace SMS.Controllers
{
    public class EmployeeController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();

        // GET: /Employee/

        public ActionResult Index()
        {          
            return View();
        }

        //GET: /Datatable/
        public JsonResult GetDataTable()
        {
            try
            {
                Common _cmn = new Common();
               //Get current rank of the employee
                int _currEmpRank = _db.Employees
                                  .AsEnumerable()  
                                  .Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                  .Select(e => e.Designation.Role.Rank).FirstOrDefault().Value;

                //Gets all the centerCodeIds allotted to an employee
                List<int> _centerCodeIds = _cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"]))
                                        .Select(x => x.Id).ToList();


                //Get all employees having the above centercodids
                List<int> _employeeIds = _db.EmployeeCenters
                                        .Where(x => _centerCodeIds.Any(cc => x.CenterCodeId == cc))
                                        .Select(x => x.Employee.Id).Distinct().ToList();
                                        

               

                //Get all employees of lower rank compared to logged user 
                var _dTableEmployee = _db.Employees
                                        .Where(x => x.Status == true && x.Designation.Role.Rank > _currEmpRank && _employeeIds.Contains(x.Id) )
                                        .AsEnumerable()
                                        .OrderByDescending(x => x.Id)
                                        .Select(x => new
                                        {
                                            SlNo = "",
                                            Name = x.Name,
                                            CenterCode = x.EmployeeCenters
                                                     .Select(ec => ec.CenterCode.CentreCode)
                                                     .Aggregate((m, n) => m + "," + n),
                                            Designation = x.Designation.DesignationName,
                                            EmailId = x.OfficialEmailId,
                                            Mobile = x.OfficialMobileNo != null ? x.OfficialMobileNo : x.MobileNo,
                                            Id = x.Id
                                        });
                return Json(new { data = _dTableEmployee }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET:/ Add/
        public ActionResult Add()
        {
            try
            {
                Common _cmn = new Common();
                int _empId = Convert.ToInt32(Session["LoggedUserId"].ToString());
                var _mdlEmployee = new EmployeeVM
                {
                    CenterCodeList = new SelectList(_cmn.GetCenterEmpwise(_empId), "Id", "CentreCode"),
                    DepartmentList = new SelectList(_cmn.GetRolesEmpwise(_empId), "Id", "RoleName"),
                    QlfnTypeList = new SelectList(_db.QlfnTypes.ToList(), "Id", "Name"),
                    StateList = new SelectList(_db.States.ToList(), "StateId", "StateName"),
                    DesignationList = new SelectList(""),
                    DistrictList = new SelectList(""),
                    QlfnMainList = new SelectList(""),
                    QlfnSubList = new SelectList(""),                
                    QlfnSubId = 0
                };
                return View(_mdlEmployee);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }

        }

        // GET: /District/       
        public JsonResult GetDistrict(int stateId)
        {
            try
            {
                var _districtList = _db.Districts
                                        .Where(x => x.StateId == stateId)
                                        .Select(x => new
                                        {
                                            Id = x.DistrictId,
                                            Name = x.DistrictName
                                        }).ToList();
                return Json(_districtList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Designation/       
        public JsonResult GetDesignation(int departmentId)
        {
            try
            {
                var _designationList = _db.Designations
                                        .Where(x => x.RoleId == departmentId)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.DesignationName
                                        }).ToList();
                return Json(_designationList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Course/       
        public JsonResult GetCourse(int qlfnTypeId)
        {
            try
            {
                var _designationList = _db.QlfnMains
                                        .Where(x => x.QlfnTypeId == qlfnTypeId)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        }).ToList();
                return Json(_designationList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Stream/       
        public JsonResult GetStream(int qlfnMainId)
        {
            try
            {
                var _designationList = _db.QlfnSubs
                                        .Where(x => x.QlfnMainId == qlfnMainId)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        }).ToList();
                return Json(_designationList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Check UserName/
        public JsonResult CheckUsername(string UserName)
        {
            try
            {
                var _employeeCount = _db.Users
                              .Where(e => e.UserName == UserName)
                              .Select(e => e.Id).FirstOrDefault();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(EmployeeVM mdlEmployee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {

                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                        string _photoPath = "~/UploadImages/Employee/Photo";
                        string _addressProofPath = "~/UploadImages/Employee/AddressProof";
                        string _idProofPath = "~/UploadImages/Employee/IdProof";
                        string _extension = string.Empty;
                        string _photoFileName = string.Empty;
                        string _idFileName = string.Empty;
                        string _addressFileName = string.Empty;
                        string _photoFullPath = string.Empty;
                        string _idFullPath = string.Empty;
                        string _addressFullPath = string.Empty;
                        int _employeeId = 0;

                        var _employeeCount = _db.Employees.Count();
                        if (_employeeCount > 0)
                        {
                            _employeeId = _db.Employees.Max(e => e.Id);
                        }
                        _employeeId = _employeeId + 1;



                        //Gets the Uploaded image
                        HttpPostedFileBase _photoFile = mdlEmployee.PhotoUrl;
                        //Gets the extension of the image
                        _extension = Path.GetExtension(_photoFile.FileName);
                        //Gets the new file name ( empId + extension )
                        _photoFileName = "Emp" + _employeeId + _extension;
                        //Gets the path to save the image
                        _photoFullPath = Path.Combine(Server.MapPath(_photoPath), _photoFileName);


                        //Gets the Uploaded image
                        HttpPostedFileBase _idFile = mdlEmployee.IdProofUrl;
                        //Gets the extension of the image
                        _extension = Path.GetExtension(_idFile.FileName);
                        //Gets the new file name ( empId + extension )
                        _idFileName = "Emp" + _employeeId + _extension;
                        //Gets the path to save the image
                        _idFullPath = Path.Combine(Server.MapPath(_idProofPath), _idFileName);


                        //Gets the Uploaded image
                        HttpPostedFileBase _addressFile = mdlEmployee.AddressProofUrl;
                        //Gets the extension of the image
                        _extension = Path.GetExtension(_addressFile.FileName);
                        //Gets the new file name ( empId + extension )
                        _addressFileName = "Emp" + _employeeId + _extension;
                        //Gets the path to save the image
                        _addressFullPath = Path.Combine(Server.MapPath(_addressProofPath), _addressFileName);



                        //Employee Details
                        Employee _employee = new Employee();
                        _employee.Address = mdlEmployee.Employee.Address.ToUpper();
                        _employee.AddressProofUrl = _addressProofPath + "/" + _addressFileName;
                        _employee.BloodGroup = mdlEmployee.Employee.BloodGroup.ToUpper();
                        _employee.CenterCodeIds = string.Join(",", mdlEmployee.CenterCodeId);
                        _employee.DateOfJoin = mdlEmployee.Employee.DateOfJoin;
                        _employee.DesignationId = mdlEmployee.DesignationId;
                        _employee.DistrictId = mdlEmployee.DistrictId;
                        _employee.DOB = mdlEmployee.Employee.DOB;
                        _employee.EmailId = mdlEmployee.Employee.EmailId.ToUpper();
                        _employee.Gender = mdlEmployee.Gender == true ? (int)EnumClass.Gender.Female : (int)EnumClass.Gender.Male;
                        _employee.IdProofUrl = _idProofPath + "/" + _idFileName;
                        _employee.MaritalStatus = mdlEmployee.MaritalStatus == true ? (int)EnumClass.MaritalStatus.MARRIED : (int)EnumClass.MaritalStatus.UNMARRIED;
                        _employee.MobileNo = mdlEmployee.Employee.MobileNo;
                        _employee.Name = mdlEmployee.Employee.Name.ToUpper();
                        _employee.OfficialEmailId = mdlEmployee.Employee.OfficialEmailId != null ? mdlEmployee.Employee.OfficialEmailId.ToUpper() : null;
                        _employee.OfficialMobileNo = mdlEmployee.Employee.OfficialMobileNo != null ? mdlEmployee.Employee.OfficialMobileNo : null;
                        _employee.PhotoUrl = _photoPath + "/" + _photoFileName;
                        _employee.Pincode = mdlEmployee.Employee.Pincode;
                        _employee.QlfnMainId = mdlEmployee.QlfnMainId;
                        _employee.QlfnSubId = mdlEmployee.QlfnSubId;
                        _employee.QlfnTypeId = mdlEmployee.QlfnTypeId;
                        _employee.IsEmailVerified = false;
                        _employee.IsMobileVerified = false;
                        _employee.Status = true;

                        //Saving Employee Details
                        _db.Employees.Add(_employee);
                        int i = _db.SaveChanges();

                        if (i > 0)
                        {
                            //Saving Employee Center Details
                            EmployeeCenter _empCenter = new EmployeeCenter();
                            foreach (var item in mdlEmployee.CenterCodeId)
                            {
                                _employeeId = _db.Employees.Max(e => e.Id);
                                _empCenter.CenterCodeId = Convert.ToInt32(item);
                                _empCenter.EmployeeId = _employeeId;
                                _db.EmployeeCenters.Add(_empCenter);
                                _db.SaveChanges();
                            }

                            //Saving User Details                           
                            User _user = new User();
                            _user.Attempts = 0;
                            _user.EmployeeId = _employeeId;
                            _user.UserName = mdlEmployee.UserName;
                            _db.Users.Add(_user);
                            int j = _db.SaveChanges();
                            if (j > 0)
                            {
                                //Saving EmailVerificationDetails
                                EmailVerification _emailVerificaiton = new EmailVerification();
                                string _key = Guid.NewGuid().ToString();
                                _emailVerificaiton.Type = "E";
                                _emailVerificaiton.VerificationKey = _key;
                                _emailVerificaiton.TypeId = _employeeId;
                                _db.EmailVerifications.Add(_emailVerificaiton);
                                int l = _db.SaveChanges();
                                if (l > 0)
                                {
                                    //Mail sending
                                    Common _cmn = new Common();
                                    //Loading body of email
                                    string _emailKey = "http://www.networkzsystems.com/sms/Account/UserVerification?key=" + _key;
                                    string _body = PopulateBody(mdlEmployee.UserName, _employee.Name, _emailKey);                                   
                                    //Email sending
                                    var isMailSend = _cmn.SendEmail(_employee.EmailId, _body, "Employee Verification");
                                    if (isMailSend)
                                    {
                                        //Saving Transaction details                                       
                                        int k = _cmn.AddTransactions(actionName, controllerName, "");
                                        if (k > 0)
                                        {
                                            //image save
                                            _photoFile.SaveAs(_photoFullPath);
                                            _idFile.SaveAs(_idFullPath);
                                            _addressFile.SaveAs(_addressFullPath);
                                            _ts.Complete();
                                            return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                        }
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
                return Json(new { message = "exception"+ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string PopulateBody(string userName, string employeeName, string key)
        {
            string _body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/EmailTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }
            _body = _body.Replace("{EmployeeName}", employeeName);
            _body = _body.Replace("{UserName}", userName);
            _body = _body.Replace("{ActivationLink}", key);
            return _body;
        }

        private string PopulateBodyEdit(string employeeName, string key)
        {
            string _body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/EmailUpdateTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }
            _body = _body.Replace("{EmployeeName}", employeeName);

            _body = _body.Replace("{ActivationLink}", key);
            return _body;
        }

        //GET: /CenterCode/Edit
        public ActionResult Edit(int employeeId)
        {
            try
            {

                var _mdlEmployee = _db.Employees.Where(x => x.Id == employeeId).FirstOrDefault();

                var _mdlEmployeeVM = new EmployeeVM
                {
                    CenterCodeId = _mdlEmployee.CenterCodeIds.Split(','),
                    CenterCodeList = new SelectList(_db.CenterCodes.ToList(), "Id", "CentreCode"),

                    DepartmentId = _mdlEmployee.Designation.RoleId,
                    DepartmentList = new SelectList(_db.Roles.ToList(), "Id", "RoleName"),

                    DesignationId = _mdlEmployee.DesignationId,
                    DesignationList = new SelectList(_db.Designations
                                                    .Where(d => d.RoleId == _mdlEmployee.Designation.RoleId)
                                                    .ToList(), "Id", "DesignationName"),

                    Employee = _mdlEmployee,

                    Gender = _mdlEmployee.Gender == (int)EnumClass.Gender.Female ? true : false,

                    MaritalStatus = _mdlEmployee.MaritalStatus == (int)EnumClass.MaritalStatus.MARRIED ? true : false,

                    StateId = _mdlEmployee.District.StateId,
                    StateList = new SelectList(_db.States.ToList(), "StateId", "StateName"),

                    DistrictId = _mdlEmployee.DistrictId,
                    DistrictList = new SelectList(_db.Districts
                                                .Where(x => x.StateId == _mdlEmployee.District.StateId)
                                                .ToList(), "DistrictId", "DistrictName"),


                    QlfnTypeId = _mdlEmployee.QlfnTypeId,
                    QlfnTypeList = new SelectList(_db.QlfnTypes.ToList(), "Id", "Name"),

                    QlfnMainId = _mdlEmployee.QlfnMainId,
                    QlfnMainList = new SelectList(_db.QlfnMains
                                                .Where(m => m.QlfnTypeId == _mdlEmployee.QlfnTypeId)
                                                .ToList(), "Id", "Name"),

                    QlfnSubId = _mdlEmployee.QlfnSubId,
                    QlfnSubList = new SelectList(_db.QlfnSubs
                                                .Where(s => s.QlfnMainId == _mdlEmployee.QlfnMainId)
                                                .ToList(), "Id", "Name"),

                    User = _db.Users
                                .Where(u => u.EmployeeId == employeeId)
                                .FirstOrDefault()



                };
                return View(_mdlEmployeeVM);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(EmployeeVM mdlEmployee)
        {
            try
            {
                ModelState.Remove("UserName");
                ModelState.Remove("PhotoUrl");
                ModelState.Remove("AddressProofUrl");
                ModelState.Remove("IdProofUrl");
                if (ModelState.IsValid)
                {
                    var _employee = _db.Employees
                                   .Where(x => x.Id == mdlEmployee.Employee.Id)
                                   .FirstOrDefault();
                    if (_employee != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {

                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            string _photoPath = "~/UploadImages/Employee/Photo";
                            string _addressProofPath = "~/UploadImages/Employee/AddressProof";
                            string _idProofPath = "~/UploadImages/Employee/IdProof";
                            string _extension = "";
                            string _photoFileName = "";
                            string _idFileName = "";
                            string _addressFileName = "";
                            string _photoFullPath = "";
                            string _idFullPath = "";
                            string _addressFullPath = "";
                            string _key = "";
                            int _employeeId = mdlEmployee.Employee.Id;
                            HttpPostedFileBase _photoFile = null;
                            HttpPostedFileBase _idFile = null;
                            HttpPostedFileBase _addressFile = null;
                            Common _cmn = new Common();

                            //Gets the Uploaded image
                            if (mdlEmployee.PhotoUrlEdit != null)
                            {
                                _photoFile = mdlEmployee.PhotoUrlEdit;
                                //Gets the extension of the image
                                _extension = Path.GetExtension(_photoFile.FileName);
                                //Gets the new file name ( empId + extension )
                                _photoFileName = "Emp" + _employeeId + _extension;
                                //Gets the path to save the image
                                _photoFullPath = Path.Combine(Server.MapPath(_photoPath), _photoFileName);
                            }


                            if (mdlEmployee.IdProofUrlEdit != null)
                            {
                                //Gets the Uploaded image
                                _idFile = mdlEmployee.IdProofUrlEdit;
                                //Gets the extension of the image
                                _extension = Path.GetExtension(_idFile.FileName);
                                //Gets the new file name ( empId + extension )
                                _idFileName = "Emp" + _employeeId + _extension;
                                //Gets the path to save the image
                                _idFullPath = Path.Combine(Server.MapPath(_idProofPath), _idFileName);
                            }


                            if (mdlEmployee.AddressProofUrlEdit != null)
                            {
                                //Gets the Uploaded image
                                _addressFile = mdlEmployee.AddressProofUrlEdit;
                                //Gets the extension of the image
                                _extension = Path.GetExtension(_addressFile.FileName);
                                //Gets the new file name ( empId + extension )
                                _addressFileName = "Emp" + _employeeId + _extension;
                                //Gets the path to save the image
                                _addressFullPath = Path.Combine(Server.MapPath(_addressProofPath), _addressFileName);
                            }

                            //Check if employee has changed the EmailId
                            if (mdlEmployee.Employee.EmailId.ToUpper() != _employee.EmailId)
                            {
                                #region updating employee verification table

                                _employee.IsEmailVerified = false;

                                //Getting existing EmailVerification Details
                                var _emailVerificaiton = _db.EmailVerifications
                                                        .Where(x => x.TypeId == mdlEmployee.Employee.Id && x.Type == "E")
                                                        .FirstOrDefault();
                                _key = Guid.NewGuid().ToString();
                                _emailVerificaiton.VerificationKey = _key;

                                //Saving modified EmailVerification details
                                _db.Entry(_emailVerificaiton).State = EntityState.Modified;
                                _db.SaveChanges();

                                #endregion

                                #region emailSending

                                //Loading body of email
                                string _emailKey = "http://www.networkzsystems.com/sms/Account/UserVerification?key=" + _key;
                                string _body = PopulateBodyEdit(mdlEmployee.Employee.Name, _emailKey);
                                //Email sending
                                var isMailSend = _cmn.SendEmail(mdlEmployee.Employee.EmailId, _body, "Email Verification");
                                //if email sending is failed
                                if (!isMailSend)
                                {
                                    return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
                                }

                                #endregion

                            }
                            //Check if employee has changed the MobileNo
                            if (mdlEmployee.Employee.MobileNo != _employee.MobileNo)
                            {
                                _employee.IsMobileVerified = false;
                            }

                            _employee.Address = mdlEmployee.Employee.Address.ToUpper();
                            //Check if new addressproof has been added
                            _employee.AddressProofUrl = mdlEmployee.AddressProofUrlEdit != null ? _addressProofPath + "/" + _addressFileName : _employee.AddressProofUrl;
                            _employee.BloodGroup = mdlEmployee.Employee.BloodGroup.ToUpper();
                            _employee.CenterCodeIds = string.Join(",", mdlEmployee.CenterCodeId);
                            _employee.DateOfJoin = mdlEmployee.Employee.DateOfJoin;
                            _employee.DesignationId = mdlEmployee.DesignationId;
                            _employee.DistrictId = mdlEmployee.DistrictId;
                            _employee.DOB = mdlEmployee.Employee.DOB;
                            _employee.EmailId = mdlEmployee.Employee.EmailId.ToUpper();
                            _employee.Gender = mdlEmployee.Gender == true ? (int)EnumClass.Gender.Female : (int)EnumClass.Gender.Male;
                            //Check if new Idproof has been added
                            _employee.IdProofUrl = mdlEmployee.IdProofUrlEdit != null ? _idProofPath + "/" + _idFileName : _employee.IdProofUrl;
                            _employee.MaritalStatus = mdlEmployee.MaritalStatus == true ? (int)EnumClass.MaritalStatus.MARRIED : (int)EnumClass.MaritalStatus.UNMARRIED;
                            _employee.MobileNo = mdlEmployee.Employee.MobileNo;
                            _employee.Name = mdlEmployee.Employee.Name.ToUpper();
                            _employee.OfficialEmailId = mdlEmployee.Employee.OfficialEmailId != null ? mdlEmployee.Employee.OfficialEmailId.ToUpper() : null;
                            _employee.OfficialMobileNo = mdlEmployee.Employee.OfficialMobileNo != null ? mdlEmployee.Employee.OfficialMobileNo : null;
                            //Check if new Photo has been added
                            _employee.PhotoUrl = mdlEmployee.PhotoUrlEdit != null ? _photoPath + "/" + _photoFileName : _employee.PhotoUrl;
                            _employee.Pincode = mdlEmployee.Employee.Pincode;
                            _employee.QlfnMainId = mdlEmployee.QlfnMainId;
                            _employee.QlfnSubId = mdlEmployee.QlfnSubId;
                            _employee.QlfnTypeId = mdlEmployee.QlfnTypeId;
                            _employee.Status = true;

                            //Saving Employee Details
                            _db.Entry(_employee).State = EntityState.Modified;
                            int i = _db.SaveChanges();

                            if (i > 0)
                            {
                                #region updating employeecenters

                                //Remove existing center
                                foreach (var _existingCenter in _db.EmployeeCenters
                                                                .Where(e => e.EmployeeId == mdlEmployee.Employee.Id)
                                                                .ToList())
                                {
                                    _db.EmployeeCenters.Remove(_existingCenter);
                                }

                                //Saving Employee Center Details
                                EmployeeCenter _empCenter = new EmployeeCenter();
                                foreach (var item in mdlEmployee.CenterCodeId)
                                {
                                    _empCenter.CenterCodeId = Convert.ToInt32(item);
                                    _empCenter.EmployeeId = _employeeId;
                                    _db.EmployeeCenters.Add(_empCenter);
                                    _db.SaveChanges();
                                }

                                #endregion

                                int k = _cmn.AddTransactions(actionName, controllerName, "");
                                if (k > 0)
                                {
                                    #region saving image
                                    //image save
                                    if (_photoFile != null)
                                    {
                                        _photoFile.SaveAs(_photoFullPath);
                                    }
                                    if (_idFile != null)
                                    {
                                        _idFile.SaveAs(_idFullPath);
                                    }
                                    if (_addressFile != null)
                                    {
                                        _addressFile.SaveAs(_addressFullPath);
                                    }
                                    #endregion

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

        [HttpPost]
        public ActionResult Delete(int employeeId)
        {
            try
            {
                //get multicourse 
                var _mdlEmployee = _db.Employees
                                    .Where(x => x.Id == employeeId)
                                    .FirstOrDefault();

                if (_mdlEmployee != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        //Change the status from true to false
                        _mdlEmployee.Status = false;

                        _db.Entry(_mdlEmployee).State = EntityState.Modified;
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

        public ActionResult EmployeeEnable()
        {
            return View();
        }

        public JsonResult Get_EmployeeDisableList()
        {
            try
            {
                Common _cmn = new Common();
               //Get current rank of the employee
                int _currEmpRank = _db.Employees
                                  .AsEnumerable()  
                                  .Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                  .Select(e => e.Designation.Role.Rank).FirstOrDefault().Value;

                //Gets all the centerCodeIds allotted to an employee
                List<int> _centerCodeIds = _cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"]))
                                        .Select(x => x.Id).ToList();


                //Get all employees having the above centercodids
                List<int> _employeeIds = _db.EmployeeCenters
                                        .Where(x => _centerCodeIds.Any(cc => x.CenterCodeId == cc))
                                        .Select(x => x.Employee.Id).Distinct().ToList();
                                        

               

                //Get all employees of lower rank compared to logged user 
                var _dTableEmployee = _db.Employees
                                        .Where(x => x.Status == false && x.Designation.Role.Rank > _currEmpRank && _employeeIds.Contains(x.Id) )
                                        .AsEnumerable()
                                        .OrderByDescending(x => x.Id)
                                        .Select(x => new
                                        {
                                            SlNo = "",
                                            Name = x.Name,
                                            CenterCode = x.EmployeeCenters
                                                     .Select(ec => ec.CenterCode.CentreCode)
                                                     .Aggregate((m, n) => m + "," + n),
                                            Designation = x.Designation.DesignationName,
                                            EmailId = x.OfficialEmailId,
                                            Mobile = x.OfficialMobileNo != null ? x.OfficialMobileNo : x.MobileNo,
                                            Id = x.Id
                                        });
                return Json(new { data = _dTableEmployee }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EnableEmployee(int empId)
        {
            try
            {
                Common _cmn=new Common();
                 string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                var _dbEmployee = _db.Employees
                                .Where(e => e.Id == empId)
                                .FirstOrDefault();
                if (_dbEmployee != null)
                {
                    using(TransactionScope _ts=new TransactionScope())
                    {
                        _dbEmployee.Status = true;
                        _db.Entry(_dbEmployee).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            int l = _cmn.AddTransactions(actionName, controllerName, "");
                            if (l > 0)
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
                return Json(new { message = "exception" }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
