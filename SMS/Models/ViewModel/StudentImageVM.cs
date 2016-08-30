using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class StudentImageVM
    {
        public class StudentImageDataTable
        {
            public int RegId { get; set; }
            public string Date { get; set; }
            public string StudentRegNo { get; set; }
            public string StudentName { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public string Status { get; set; }
            public string Urgen_Waiting
            {
                get 
                {
                    if (StudentRegistration.IsPhotoVerified == false)
                    {
                        if (StudentRegistration.StudentFeedbacks.FirstOrDefault() != null)
                        {
                            var totalCourseCount = StudentRegistration.StudentFeedbacks.Count();
                            var totalFeedbackCount = StudentRegistration.StudentFeedbacks.Where(f => f.IsFeedbackGiven == true).Count();
                            if (totalCourseCount == totalFeedbackCount)
                            {
                                if (StudentRegistration.IsPhotoVerified == false)
                                {
                                    return "URGENT";
                                }
                            }
                            return "WAITING";
                        }
                        else
                        {
                            return "WAITING";
                        }
                    }
                    else
                    {
                        return "VERIFIED";
                    }
                    
                   
                                        
                }
            }

        }

        public StudentRegistration StudentRegistration { get; set; }

        [Required(ErrorMessage="Please upload file.")]
        public HttpPostedFileBase PhotoNewUrl { get; set; }

        public int StudentRegId { get; set; }

        public int PhotoModeID { get; set; }
        public SelectList PhotoModeList { get; set; }
        public string MonthName { get; set; }
        public SelectList MonthList { get; set; }

        public SelectList FinancialYearList { get; set; }

    }
}