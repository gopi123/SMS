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
    public class CenterCodeController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();

        //
        // GET: /CenterCode/
        public ActionResult Index()
        {
            return View();
        }

        //GET DataTable
        public JsonResult GetDataTable()
        {
            try
            {
                var _dTableCenterCode = _db.CenterCodes
                                        .Where(x => x.Status == true)
                                        .AsEnumerable()
                                        .OrderByDescending(x => x.Id)
                                        .Select(x => new
                                        {
                                            SlNo = "",
                                            CenterCode = x.CentreCode,
                                            CenterAddress = x.Address + "," + x.District.DistrictName.ToUpper() + "," + x.State.StateName.ToUpper(),
                                            PhoneNo = x.PhoneNo,
                                            EmployeeName = x.EmployeeId != null ? x.Employee.Name : "NOT ASSIGNED",
                                            Id = x.Id
                                        });
                return Json(new { data = _dTableCenterCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        //
        // GET: /CenterCode/Add
        public ActionResult Add()
        {
            var _centerCodeVM = new CenterCodeVM
            {
                BranchList = new SelectList(_db.Branches.ToList(), "Id", "Name"),
                FirmList = new SelectList(_db.Firms.ToList(), "Id", "Name"),
                StateList = new SelectList(_db.States.ToList(), "StateId", "StateName"),
                DistrictList = new SelectList(""),
                EmployeeList = new SelectList(_db.Employees.ToList(), "Id", "Name"),
                STCompulsory=true

            };
            return View(_centerCodeVM);
        }

        //
        // GET: /District
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

        //
        // POST: /CenterCode/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(CenterCodeVM mdlCenterCode)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                        Common _cmn = new Common();

                        CenterCode _centerCode = new CenterCode();
                        _centerCode.Address = mdlCenterCode.CenterCode.Address.ToUpper();
                        _centerCode.BranchTypeID = mdlCenterCode.BranchID;
                        _centerCode.CentreCode = mdlCenterCode.CenterCode.CentreCode.ToUpper();
                        _centerCode.DistrictId = mdlCenterCode.DistrictId;
                        _centerCode.EmployeeId = mdlCenterCode.EmployeeID;
                        _centerCode.FirmTypeID = mdlCenterCode.FirmID;
                        _centerCode.PhoneNo = mdlCenterCode.CenterCode.PhoneNo;
                        _centerCode.Pincode = mdlCenterCode.CenterCode.Pincode;
                        _centerCode.StateId = mdlCenterCode.StateId;
                        _centerCode.Status = mdlCenterCode.CenterCode.Status;
                        _centerCode.STCompulsory = mdlCenterCode.STCompulsory;
                        _centerCode.STRegNo = mdlCenterCode.STRegNo != null ? mdlCenterCode.STRegNo.ToUpper() : "";

                        foreach (var item in mdlCenterCode.CenterCodePartnerDetail) 
                        {
                            _centerCode.CenterCodePartnerDetails.Add(item);
                        }    
                        

                        _db.CenterCodes.Add(_centerCode);
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            //Saving Employee Centre Details
                            var _employee = _db.Employees
                                    .Where(e => e.Designation.Id == (int)EnumClass.Designation.ED)
                                    .FirstOrDefault();
                            EmployeeCenter _empCenter = new EmployeeCenter();
                            _empCenter.CenterCodeId = _centerCode.Id;
                            _empCenter.EmployeeId = _employee.Id;

                            _db.EmployeeCenters.Add(_empCenter);
                            int j = _db.SaveChanges();
                            if (j > 0)
                            {
                                //Saving StudentSerialNo Details
                                StudentSerialNo _studentSerialNo = new StudentSerialNo();
                                _studentSerialNo.CenterCode = _centerCode;
                                _studentSerialNo.SerialNo = 1;
                                _studentSerialNo.SerialNoDate = Common.LocalDateTime();

                                _db.StudentSerialNoes.Add(_studentSerialNo);
                                int k = _db.SaveChanges();
                                if (k > 0)
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

                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }


        //GET: /CenterCode/Edit
        public ActionResult Edit(int centerCodeId)
        {
            try
            {
                var _mdlCenterCode=_db.CenterCodes.Where(x=>x.Id==centerCodeId).FirstOrDefault();

                var _mdlCenterCodeVM = new CenterCodeVM
                {
                    BranchID = _mdlCenterCode.BranchTypeID,
                    BranchList = new SelectList(_db.Branches.ToList(), "Id", "Name"),
                    CenterCode = _mdlCenterCode,
                    CenterCodePartnerDetail = _mdlCenterCode.CenterCodePartnerDetails.ToList(),
                    DistrictId = _mdlCenterCode.DistrictId,
                    DistrictList = new SelectList(_db.Districts.ToList(), "DistrictId", "DistrictName"),
                    EmployeeID = _mdlCenterCode.EmployeeId,
                    EmployeeList = new SelectList(_db.Employees.ToList(), "Id", "Name"),
                    FirmID = _mdlCenterCode.FirmTypeID,
                    FirmList = new SelectList(_db.Firms.ToList(), "Id", "Name"),
                    StateId = _mdlCenterCode.StateId,
                    StateList = new SelectList(_db.States.ToList(), "StateId", "StateName"),
                    STCompulsory = _mdlCenterCode.STCompulsory,
                    STRegNo = _mdlCenterCode.STRegNo

                };
                return View(_mdlCenterCodeVM);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        //POST: /CenterCode/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(CenterCodeVM mdlCenterCode)
        {
            try
            {
               
                if (ModelState.IsValid)
                {
                    var _centerCode = _db.CenterCodes
                               .Where(x => x.Id == mdlCenterCode.CenterCode.Id)
                               .FirstOrDefault();
                    if (_centerCode != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                            _centerCode.Address = mdlCenterCode.CenterCode.Address.ToUpper();
                            _centerCode.BranchTypeID = mdlCenterCode.BranchID;
                            _centerCode.CentreCode = mdlCenterCode.CenterCode.CentreCode.ToUpper();
                            _centerCode.DistrictId = mdlCenterCode.DistrictId;
                            _centerCode.EmployeeId = mdlCenterCode.EmployeeID;
                            _centerCode.FirmTypeID = mdlCenterCode.FirmID;
                            _centerCode.PhoneNo = mdlCenterCode.CenterCode.PhoneNo;
                            _centerCode.Pincode = mdlCenterCode.CenterCode.Pincode;
                            _centerCode.StateId = mdlCenterCode.StateId;
                            _centerCode.Status = mdlCenterCode.CenterCode.Status;
                            _centerCode.STCompulsory = mdlCenterCode.STCompulsory;
                            _centerCode.STRegNo = mdlCenterCode.STRegNo != null ? mdlCenterCode.STRegNo.ToUpper() : "";

                            foreach (var _currPartnerDetail in mdlCenterCode.CenterCodePartnerDetail)
                            {
                                var _existingPartnerDetail = _centerCode.CenterCodePartnerDetails
                                                           .Where(ex => ex.Id == _currPartnerDetail.Id && ex.Id!=0)
                                                           .FirstOrDefault();
                                if (_existingPartnerDetail != null)
                                {
                                    //existing child                                    
                                    _existingPartnerDetail.AltContactNumber = _currPartnerDetail.AltContactNumber;
                                    _existingPartnerDetail.AltEmailId = _currPartnerDetail.AltEmailId;                                 
                                    _existingPartnerDetail.ContactNumber = _currPartnerDetail.ContactNumber;
                                    _existingPartnerDetail.EmailId = _currPartnerDetail.EmailId;
                                    _existingPartnerDetail.PartnerName = _currPartnerDetail.PartnerName;

                                    _db.Entry(_existingPartnerDetail).State = EntityState.Modified;
                                  
                                }
                                else
                                {
                                    //new child
                                    _centerCode.CenterCodePartnerDetails.Add(_currPartnerDetail);

                                }
                            }

                            foreach (var _existingPartnerDetail in _centerCode.CenterCodePartnerDetails.ToList())
                            {
                                //removing deleted child
                                var _currParentDetail = mdlCenterCode.CenterCodePartnerDetail
                                                        .Where(x => x.Id == _existingPartnerDetail.Id);
                                if (_currParentDetail == null || _currParentDetail.Count() == 0)
                                {
                                    _centerCode.CenterCodePartnerDetails.Remove(_existingPartnerDetail);
                                    _db.CenterCodePartnerDetails.Remove(_existingPartnerDetail);
                                }
                            }                           
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
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Delete(int centreCodeId)
        {
            try
            {
                //get multicourse 
                var _mdlCenterCode = _db.CenterCodes
                                    .Where(x => x.Id == centreCodeId)
                                    .FirstOrDefault();

                if (_mdlCenterCode != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        //Change the status from true to false
                        _mdlCenterCode.Status = false;

                        _db.Entry(_mdlCenterCode).State = EntityState.Modified;
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
