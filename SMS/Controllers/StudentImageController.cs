
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
    public class StudentImageController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /StudentImage/

        public ActionResult Index()
        {
            Common _cmn = new Common();

            var _finYearList = _cmn.FinancialYearList().ToList()
                                .OrderByDescending(x => x.ToString())
                                .Select(x => new
                                {
                                    Id = x.ToString(),
                                    Name = x.ToString()
                                });

            var _photoModeList = from EnumClass.PhotoMode r in Enum.GetValues(typeof(EnumClass.PhotoMode))
                                 select new { Id = (int)r, Name = r.ToString() };

            var _monthList = from EnumClass.Month c in Enum.GetValues(typeof(EnumClass.Month))
                             select new { Id = c.ToString(), Name = c.ToString() };


            var _mdlStudentImage = new StudentImageVM
            {
                FinancialYearList = new SelectList(_finYearList, "Id", "Name"),
                PhotoModeList = new SelectList(_photoModeList, "Id", "Name"),               
                MonthList = new SelectList(_monthList, "Id", "Name"),
                MonthName = Common.LocalDateTime().ToString("MMMM")
            };
            return View(_mdlStudentImage);
        }

        public JsonResult GetDataTable(int photoMode, string finYear, string month)
        {
            try
            {
                Common _cmn = new Common();
                List<StudentImageVM.StudentImageDataTable> _regList = new List<StudentImageVM.StudentImageDataTable>();

                if (photoMode == (int)EnumClass.PhotoMode.NEW)
                {
                    _regList = _db.StudentRegistrations
                               .AsEnumerable()
                               .Where(r => r.IsPhotoUploaded == true 
                                    && r.IsPhotoRejected==false 
                                    && r.IsPhotoVerified == false)
                               .OrderBy(r => r.Id)
                               .Select(r => new StudentImageVM.StudentImageDataTable
                               {
                                   Date = r.PhotoUploadedDate.Value.Date.ToString("dd/MM/yyyy"),
                                   StudentName = r.StudentWalkInn.CandidateName,
                                   StudentRegNo = r.RegistrationNumber,
                                   RegId = r.Id,
                                   StudentRegistration = r
                               })
                               .ToList();
                }
                else
                {

                    DateTime _startFinDate = new DateTime();
                    DateTime _endFinDate = new DateTime();

                    var arrFinYear = finYear.Split('-');
                    var _startFinYear = Convert.ToInt32(arrFinYear[0]);
                    var _endFinYear = Convert.ToInt32(arrFinYear[1]);
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


                    _regList = _db.StudentRegistrations
                              .AsEnumerable()
                              .Where(r => r.IsPhotoUploaded == true 
                                           && r.IsPhotoVerified == true
                                           && (r.TransactionDate.Value.Date >= _startFinDate && r.TransactionDate.Value.Date <= _endFinDate))
                              .OrderBy(r => r.Id)
                              .Select(r => new StudentImageVM.StudentImageDataTable
                              {
                                  Date = r.PhotoUploadedDate.Value.Date.ToString("dd/MM/yyyy"),
                                  StudentName = r.StudentWalkInn.CandidateName,
                                  StudentRegNo = r.RegistrationNumber,
                                  RegId = r.Id,
                                  StudentRegistration = r
                              })
                              .ToList();
                }



                var _regImageList = _regList
                                 .Select(r => new
                                 {
                                     Date = r.Date,
                                     StudentName = r.StudentName,
                                     StudentRegNo = r.StudentRegNo,
                                     RegId = r.RegId,
                                     Status = r.Urgen_Waiting
                                 })
                                 .OrderBy(r => r.Status)
                                 .ToList();


                return Json(new { data = _regImageList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Add(int RegId)
        {
            try
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == RegId)
                            .FirstOrDefault();
                if (_dbRegn != null)
                {
                    var _mdlStudentImageVM = new StudentImageVM
                    {
                        StudentRegistration = _dbRegn,
                        StudentRegId = RegId
                    };
                    return View(_mdlStudentImageVM);
                }
                return View("Index");

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(StudentImageVM mdlStudentImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var _dbRegn = _db.StudentRegistrations
                                .Where(r => r.Id == mdlStudentImage.StudentRegId)
                                .FirstOrDefault();
                    if (_dbRegn != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            if (mdlStudentImage.PhotoNewUrl != null)
                            {
                                var _photoFile = mdlStudentImage.PhotoNewUrl;
                                var _extension = Path.GetExtension(_photoFile.FileName);
                                var _imgFileName = _dbRegn.RegistrationNumber + _extension;
                                var _imgPath = "~/UploadImages/Student";
                                var _imgSavePath = Path.Combine(Server.MapPath(_imgPath), _imgFileName);
                                _photoFile.SaveAs(_imgSavePath);

                                _dbRegn.PhotoUrl = _imgPath + "/" + _imgFileName;
                            }
                            _dbRegn.IsPhotoVerified = true;
                            _dbRegn.PhotoVerifiedDate = Common.LocalDateTime();


                            _db.Entry(_dbRegn).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {
                                _ts.Complete();
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult PhotoReject(string rejectedReason, int studentRegistrationId)
        {
            try
            {
                List<string> _lstEmailId = new List<string>();
                string _studentEmail = string.Empty;
                string _cro1Email = string.Empty;
                string _cro2Email = string.Empty;
                string _salesManagerEmail = string.Empty;
                string _centreManagerEmail = string.Empty;
                string _edEmail = string.Empty;
                string _emailIds = string.Empty;
                Common _cmn = new Common();

                var _studentRegistration = _db.StudentRegistrations
                                            .Where(r => r.Id == studentRegistrationId)
                                            .FirstOrDefault();
                if (_studentRegistration != null)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        _studentRegistration.IsPhotoRejected = true;
                        _studentRegistration.PhotoRejectedReason = rejectedReason.ToUpper();
                        _studentRegistration.PhotoRejectedDate = Common.LocalDateTime();

                        _db.Entry(_studentRegistration).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            _studentEmail = _studentRegistration.StudentWalkInn.EmailId;
                            _cro1Email = _studentRegistration.StudentWalkInn.Employee1.OfficialEmailId;
                            if (_studentRegistration.StudentWalkInn.CROCount > 1)
                            {
                                _cro2Email = _studentRegistration.StudentWalkInn.Employee2.OfficialEmailId;
                            }
                            _salesManagerEmail = _cmn.GetManager(_studentRegistration.StudentWalkInn.CenterCode.Id)
                                                .OfficialEmailId;
                            _centreManagerEmail = _cmn.GetCentreManager(_studentRegistration.StudentWalkInn.CenterCode.Id)
                                                .OfficialEmailId;
                            _edEmail = "ed@networkzsystems.com";

                            _lstEmailId.Add(_studentEmail);
                            _lstEmailId.Add(_cro1Email);
                            _lstEmailId.Add(_cro2Email);
                            _lstEmailId.Add(_salesManagerEmail);
                            _lstEmailId.Add(_centreManagerEmail);
                            _lstEmailId.Add(_edEmail);

                            _emailIds = string.Join(",", _lstEmailId.Where(x => !string.IsNullOrEmpty(x)).ToList());

                            if (SendMail(_studentRegistration.StudentWalkInn.CandidateName, rejectedReason, _emailIds))
                            {
                                _ts.Complete();
                                return Json("success", JsonRequestBehavior.AllowGet);
                            }
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

        public bool SendMail(string studentName, string rejectedReason, string _emailId)
        {
            Common _cmn = new Common();
            string _body = PopulateBody(studentName, rejectedReason.ToUpper());
            //Email sending
            var isMailSend = _cmn.SendEmail(_emailId, _body, "Photo Rejection");
            return isMailSend;


        }

        private string PopulateBody(string studName, string rejectedReason)
        {
            string _body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/PhotoRejectTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{RejectedReason}", rejectedReason);
            return _body;
        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
