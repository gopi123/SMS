using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class StudentInfoVM
    {
        public string RegistrationNo { get; set; }
        public string StudentName { get; set; }
        public string SalesPerson { get; set; }
        public string SoftwareUsed { get; set; }
        public int RegistrationId { get; set; }
        public string PhotoUrl { get; set; }
        public string ControllerName { get; set; }
        public int PendingPaymentCount { get; set; }
        public int PendingFeedbackCount { get; set; }
        public int CourseCount { get; set; }
        public int PaidPaymentCount { get; set; }
        public int CourseInterchangeCount { get; set; }
        public int CourseFullEditCount { get; set; }
        public StudentRegistration StudentRegistration { get; set; }
        public int Feedback_Payment_Count
        {
            get
            {
                int _returnAmount=0;
                int _totalFeePaid = StudentRegistration.StudentReceipts.Where(r => r.Status == true)
                                  .Sum(r => r.Total.Value);
                int _totalFeedbackAmount = StudentRegistration.StudentFeedbacks.Where(r => r.IsFeedbackGiven == true)
                                  .Sum(r => r.TotalCourseAmount.Value);
                if (_totalFeedbackAmount > _totalFeePaid)
                {
                    _returnAmount = _totalFeedbackAmount - _totalFeePaid;
                }
                else
                {
                    _returnAmount = 0;
                }

                return _returnAmount;
            }
        }

    }
}