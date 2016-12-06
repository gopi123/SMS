using SMS.Models.CustomValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CourseFullEditApprovalVM
    {
        
        public string RegistrationNumber { get; set; }
        public string StudentName { get; set; }
        public string  Email { get; set; }
        public string MobileNo { get; set; }
        public string PhotoUrl { get; set; }
        public int ApprovedBy { get; set; }
        public string CourseFullEditReason { get; set; }

        public decimal CourseFee_OldValue { get; set; }
        public decimal CourseFee_NewValue { get; set; }
        public int Discount_OldValue { get; set; }
        public int Discount_NewValue { get;set;}
        public string CourseCombination_OldValue { get; set; }
        public string CourseCombination_NewValue { get; set; }


        [Range(typeof(bool), "true", "true", ErrorMessage = "The field must be checked inorder to continue")]
        public bool IsCourseCombination_CrossChecked { get; set; }

        [Range(typeof(bool),"true","true",ErrorMessage = "The field must be checked inorder to continue")]
        public bool IsDiscountDetails_CrossChecked { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "The field must be checked inorder to continue")]
        public bool IsCourseFees_CrossChecked { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "The field must be checked inorder to continue")]
        public bool IsCustomer_CrossChecked { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "The field must be checked inorder to continue")]
        public bool IsReason_CrossChecked { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "The field must be checked inorder to continue")]
        public string RejectedReason { get; set; }
        
    }
}