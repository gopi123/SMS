using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CourseFullEditApprovalController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /CourseFullEditApproval/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CourseFullEditApproval(int courseFullEditID)
        {
            try
            {
                StudentRegistration_CourseFullEdit _dbCourseFullEdit = _db.StudentRegistration_CourseFullEdit.Find(courseFullEditID);

                CourseFullEditApprovalVM _mdlFullEditApproval = new CourseFullEditApprovalVM();
                _mdlFullEditApproval.ApprovedBy = Convert.ToInt32(Session["LoggedUserId"]);
                _mdlFullEditApproval.CourseCombination_NewValue = string.Join(",", _dbCourseFullEdit.StudentRegistrationCourse_CourseFullEdit
                                                                                .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                                .Select(mcd => mcd.Course.Name)));
                _mdlFullEditApproval.CourseCombination_OldValue = string.Join(",", _dbCourseFullEdit.StudentRegistration
                                                                                .StudentRegistrationCourses
                                                                                .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                                .Select(mcd => mcd.Course.Name)));
                _mdlFullEditApproval.CourseFee_NewValue = _dbCourseFullEdit.StudentReceipt_CourseFullEdit.Sum(r => r.Fee.Value);
                _mdlFullEditApproval.CourseFee_OldValue = _dbCourseFullEdit.StudentRegistration.StudentReceipts.Sum(r => r.Fee.Value);
                _mdlFullEditApproval.Discount_NewValue = _dbCourseFullEdit.Discount.Value;
                _mdlFullEditApproval.Discount_OldValue = _dbCourseFullEdit.StudentRegistration.Discount.Value;
                _mdlFullEditApproval.Email = _dbCourseFullEdit.StudentRegistration.StudentWalkInn.EmailId;
                _mdlFullEditApproval.IsCourseCombination_CrossChecked = false;
                _mdlFullEditApproval.IsCourseFees_CrossChecked = false;
                _mdlFullEditApproval.IsCustomer_CrossChecked = false;
                _mdlFullEditApproval.IsDiscountDetails_CrossChecked = false;
                _mdlFullEditApproval.IsReason_CrossChecked = false;
                _mdlFullEditApproval.MobileNo = _dbCourseFullEdit.StudentRegistration.StudentWalkInn.MobileNo;
                _mdlFullEditApproval.PhotoUrl = _dbCourseFullEdit.StudentRegistration.PhotoUrl;
                _mdlFullEditApproval.RegistrationNumber = _dbCourseFullEdit.StudentRegistration.RegistrationNumber;
                _mdlFullEditApproval.CourseFullEditReason = _dbCourseFullEdit.Reason;
                _mdlFullEditApproval.StudentName = _dbCourseFullEdit.StudentRegistration.StudentWalkInn.CandidateName;

                
              
                                                        

                return View(_mdlFullEditApproval);
                


            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
