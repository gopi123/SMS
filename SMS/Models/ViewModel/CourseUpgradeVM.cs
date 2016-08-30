using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class CourseUpgradeVM
    {
        public string Name { get; set; }

        [Required(ErrorMessage="Please enter EmailId",AllowEmptyStrings=false)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter MobileNo", AllowEmptyStrings = false)]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Please select Employee", AllowEmptyStrings = false)]
        public int? CRO1ID { get; set; }
        public SelectList CRO1EmpList { get; set; }

        [Required(ErrorMessage = "Please select Employee", AllowEmptyStrings = false)]
        public int? CRO2ID { get; set; }
        public SelectList CRO2EmpList { get; set; }
        public StudentRegistration StudentRegistration { get; set; }
        public HttpPostedFileBase PhotoUrl { get; set; }

        [Required(ErrorMessage = "Please enter Percentage", AllowEmptyStrings = false)]
        public int CRO1Percentage { get; set; }

        [Required(ErrorMessage = "Please enter Percentage", AllowEmptyStrings = false)]
        public int CRO2Percentage { get; set; }

        public EnumClass.CROCount CROCount { get; set; }
        public string DefaultStudentMobNo { get; set; }

        [Required(ErrorMessage = "Installment")]
        public EnumClass.InstallmentType InstallmentType { get; set; }

        [Required(ErrorMessage = "Please select no of installment", AllowEmptyStrings = false)]
        public int? InstallmentID { get; set; }
        public SelectList InstallmentList { get; set; }

        [Required(ErrorMessage = "Please select MultiCourse", AllowEmptyStrings = false)]
        public string[] MultiCourseId { get; set; }
        public SelectList MultiCourseList { get; set; }
        public string CourseIds { get; set; }
        public string MultiCourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string SoftwareDetails { get; set; }
        public int Duration { get; set; }

        [Range(0, 100, ErrorMessage = "Enter number between 0 to 100")]
        public int Discount { get; set; }
        public int DefaultDiscountPercentage { get; set; }
        public int CourseFee { get; set; }
        public double ST { get; set; }
        public int STAmount { get; set; }
        public int TotalFee { get; set; }
        public int RoundUpId { get; set; }
        public SelectList RoundUpList { get; set; }       
        public int PrevStudentWalkinnId { get; set; }
        public List<StudentReceipt> StudentReceipt { get; set; }
        public string DefaultEmailId { get; set; }
        public string CentreCode { get; set; }
        public string PrevCourseId { get; set; }



    }
}