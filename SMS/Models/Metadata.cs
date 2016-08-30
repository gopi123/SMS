using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
//using ExpressiveAnnotations;
//using ExpressiveAnnotations.Attributes;
using System.Web.Mvc;


namespace SMS.Models
{
    public class ServiceTaxMetadata
    {
        [Required(ErrorMessage = "Please enter amount", AllowEmptyStrings = false)]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        public Nullable<decimal> Percentage { get; set; }

        [Required(ErrorMessage = "Please enter date", AllowEmptyStrings = false)]
        public Nullable<System.DateTime> FromDate { get; set; }
    }

    public class CourseMetadata
    {
        [Required(ErrorMessage = "Please enter Course name", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter Course duration", AllowEmptyStrings = false)]
        public Nullable<int> Duration { get; set; }

        [Required(ErrorMessage = "Please enter Course fee", AllowEmptyStrings = false)]
        public Nullable<decimal> SingleFee { get; set; }

        [Required(ErrorMessage = "Installment fee is not given", AllowEmptyStrings = false)]
        public Nullable<decimal> InstallmentFee { get; set; }

    }

    public class CourseSubTitleMetadata
    {
        [Required(ErrorMessage = "Please enter Course Title", AllowEmptyStrings = false)]
        public string Name { get; set; }
    }

    public class CenterCodeMetaData
    {
        [Required(ErrorMessage = "Please enter CenterCode", AllowEmptyStrings = false)]
        public string CentreCode { get; set; }

        [Required(ErrorMessage = "Please enter Address", AllowEmptyStrings = false)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter Pincode", AllowEmptyStrings = false)]
        [RegularExpression("^[0-9]{6,6}$", ErrorMessage = "Invalid Pincode")]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "Please enter Phoneno", AllowEmptyStrings = false)]
        public string PhoneNo { get; set; }


    }
    public class CenterCodePartnerDetailsMetaData
    {
        [Required(ErrorMessage = "PartnerName", AllowEmptyStrings = false)]
        public string PartnerName { get; set; }

        [Required(ErrorMessage = "ContactNumber", AllowEmptyStrings = false)]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "EmailId", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        public string EmailId { get; set; }

        [Required(ErrorMessage = "AltContactNumber", AllowEmptyStrings = false)]
        public string AltContactNumber { get; set; }

        [Required(ErrorMessage = "AltEmailId", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        public string AltEmailId { get; set; }


    }

    public class EmployeeMetaData
    {
        [Required(ErrorMessage = "Please enter EmployeeName", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter DateOfBirth", AllowEmptyStrings = false)]
        public System.DateTime DOB { get; set; }

        [Required(ErrorMessage = "Please enter Address", AllowEmptyStrings = false)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter Pincode", AllowEmptyStrings = false)]
        [RegularExpression("^[0-9]{6,6}$", ErrorMessage = "Invalid Pincode.Minimum 6 characters required.")]
        public string Pincode { get; set; }

        [Required(ErrorMessage = "Please enter Bloodgroup", AllowEmptyStrings = false)]
        public string BloodGroup { get; set; }

        [Required(ErrorMessage = "Please enter MobileNo", AllowEmptyStrings = false)]
        [RegularExpression("^[0-9]{10,10}$", ErrorMessage = "Invalid MobileNo.Minimum 10 numbers required")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Please enter EmailId", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        public string EmailId { get; set; }

        [Required(ErrorMessage = "Please enter Date Of Joining", AllowEmptyStrings = false)]
        public System.DateTime DateOfJoin { get; set; }

        [RegularExpression("^[0-9]{10,10}$", ErrorMessage = "Invalid MobileNo.Minimum 10 numbers required")]
        public string OfficialMobileNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        public string OfficialEmailId { get; set; }


    }

    public class MobileVerificationMetaData
    {
        [Required(ErrorMessage = "Please enter PinNo", AllowEmptyStrings = false)]
        public Nullable<int> PinNo { get; set; }
    }

    public class StudentWalkInnMetaData
    {
        [Required(ErrorMessage="Please enter Name",AllowEmptyStrings=false)]
        public string CandidateName { get; set; }

        [Required(ErrorMessage = "Please enter DOB", AllowEmptyStrings = false)]
        public Nullable<System.DateTime> DOB { get; set; }

        [Required(ErrorMessage = "Please enter Address", AllowEmptyStrings = false)]
        public string Address { get; set; }  

        [Required(ErrorMessage = "Please enter Location", AllowEmptyStrings = false)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Please enter ContactNo", AllowEmptyStrings = false)]
        public string GuardianContactNo { get; set; }
    }

    public class StudentReceiptMetaData
    {
        [Required(ErrorMessage="Enter Total",AllowEmptyStrings=false)]
        public int Total { get; set; }

         [Required(ErrorMessage = "Enter DueDate", AllowEmptyStrings = false)]
        public DateTime DueDate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
         public int? StudentReceiptNo { get; set; }
        
    }



}