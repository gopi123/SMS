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
    }
}