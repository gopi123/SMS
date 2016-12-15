using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CertificateController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();




        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetStudentInfo(string regNo)
        {
            try
            {
                List<StudentInfoVM> _lstStudentInfo = new List<StudentInfoVM>();
                List<StudentRegistration> _dbRegnList = _db.StudentRegistrations
                                                    .Where(r => r.RegistrationNumber == regNo || r.StudentWalkInn.EmailId == regNo || r.StudentWalkInn.MobileNo == regNo)
                                                    .ToList();
                if (_dbRegnList != null)
                {
                    foreach (StudentRegistration _dbRegn in _dbRegnList)
                    {
                        StudentInfoVM _mdlStudentInfo = new StudentInfoVM
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
                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString()
                        };
                    }
                }
                return PartialView("_GetStudentInfo", _lstStudentInfo);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult CertificateStatus(int regID)
        {
            try
            {
                List<CustomerCertificateStatusVM> _feedbackList = new List<CustomerCertificateStatusVM>();
                var _studRegn = _db.StudentRegistrations
                                .Where(r => r.Id == regID)
                                .FirstOrDefault();
                if (_studRegn != null)
                {
                    var _regNo = _studRegn.RegistrationNumber;

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
                }
                return PartialView("_CertificateStatus", _feedbackList);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }

        }

        public void CertificateVerification(string regNo)
        {
            try
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.RegistrationNumber == regNo)
                            .FirstOrDefault();
                string FilePath = string.Empty;

                if (_dbRegn != null)
                {
                    var _isCertificateIssued = _dbRegn.IsCertificateIssued.Value;
                    if (_isCertificateIssued)
                    {
                        FilePath = Server.MapPath("~/Certificates/" + _dbRegn.RegistrationNumber + ".pdf");

                    }
                    else
                    {
                        FilePath = Server.MapPath("~/Certificates/CertificateNotProcessed.pdf");
                    }
                }
                else
                {
                    FilePath = Server.MapPath("~/Certificates/StudentIDNotExist.pdf");
                }
                WebClient User = new WebClient();
                Byte[] FileBuffer = User.DownloadData(FilePath);
                if (FileBuffer != null)
                {
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-length", FileBuffer.Length.ToString());
                    Response.BinaryWrite(FileBuffer);
                    Response.End();
                }



            }
            catch (Exception ex)
            {
                RedirectToAction("CertificateDontExist");
            }
        }

        public ActionResult CertificateDontExist()
        {
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
