using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CustomerCertificateStatusVM
    {
        public bool IsPhotoUploaded
        {
            get
            {
                if (StudentRegistration.IsPhotoUploaded.Value == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool IsPhotoRejected
        {
            get
            {
                if (StudentRegistration.IsPhotoRejected == false)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public bool IsPhotoVerified
        {
            get
            {
                if (StudentRegistration.IsPhotoVerified.Value)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsFeedbackSubmitted
        {
            get
            {
                if (StudentFeedback.IsFeedbackGiven.Value == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsProjectUploaded
        {
            get
            {
                if (StudentFeedback.IsProjectUploaded == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
        public bool IsTrainerVerified
        {
            get
            {
                if (StudentFeedback.IsTrainerVerified == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsLeaderVerified
        {
            get
            {
                if (StudentFeedback.IsLeaderVerified == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsTrainerApproved
        {
            get
            {
                if (StudentFeedback.IsTrainerVerified == true && StudentFeedback.StudentProjectApprovals.FirstOrDefault().IsTrainerApproved == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsLeaderApproved
        {
            get
            {
                if (StudentFeedback.IsLeaderVerified == true && StudentFeedback.StudentProjectApprovals.FirstOrDefault().IsLeaderApproved == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsFeePaid
        {
            get
            {
                int _totalPaidFee = StudentReceipt
                                .Where(r => r.Status == true)
                                .Sum(r => r.Total.Value);
                int _totalFee = StudentRegistration.TotalAmount.Value;

                if (_totalPaidFee == _totalFee)
                {
                    return true;
                }
                else{
                    return false;
                }
                //int _totalPaid_CourseFee = 0;
                //int _curr_CourseFee = 0;
                //_totalPaid_CourseFee = StudentReceipt
                //                      .Where(r => r.Status == true)
                //                      .Sum(r => r.Fee.Value);
                //_curr_CourseFee = ListStudentFeedback
                //                .Where(f => f.Id <= StudentFeedback.Id)
                //                .Sum(f => f.TotalCourseAmount.Value);
                //if (_curr_CourseFee <= _totalPaid_CourseFee)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
        }
        public bool IsCertificateIssued
        {
            get
            {
                if (StudentRegistration.IsCertificateIssued == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public List<StudentFeedback> ListStudentFeedback { get; set; }
        public List<StudentReceipt> StudentReceipt { get; set; }
        public StudentRegistration StudentRegistration { get; set; }
        public StudentFeedback StudentFeedback { get; set; }
        public string CourseName { get; set; }


    }
}