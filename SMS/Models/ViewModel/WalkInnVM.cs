using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Foolproof;

namespace SMS.Models.ViewModel
{
    public class WalkInnVM
    {
        public class WalkInnDataTable
        {
            public string WalkInnDate { get; set; }
            public string Name { get; set; }
            public string Center { get; set; }
            public string Mobile { get; set; }
            public string Email { get; set; }
            public string CareGiverMobileNo { get; set; }
            public string SalesPerson { get; set; }
            public string CourseRecommended { get; set; }
            public int Id { get; set; }
            public DateTime? ExpJoinDate { get; set; }
            public string Status { get; set; }
        }

        [Required(ErrorMessage = "Please select Qualification", AllowEmptyStrings = false)]
        public int? QlfnTypeId { get; set; }
        public SelectList QlfnTypeList { get; set; }

        [Required(ErrorMessage = "Please select Course", AllowEmptyStrings = false)]
        public int? QlfnMainId { get; set; }
        public SelectList QlfnMainList { get; set; }

        [Required(ErrorMessage = "Please select Stream", AllowEmptyStrings = false)]
        public int? QlfnSubId { get; set; }
        public SelectList QlfnSubList { get; set; }

        [Required(ErrorMessage = "Please select State", AllowEmptyStrings = false)]
        public int? StateId { get; set; }
        public SelectList StateList { get; set; }

        [Required(ErrorMessage = "Please select District", AllowEmptyStrings = false)]
        public int? DistrictId { get; set; }
        public SelectList DistrictList { get; set; }
       
        public int? DemoGivenEmpId { get; set; }
        public SelectList DemoGivenEmpList { get; set; }

        [Required(ErrorMessage = "This field is required", AllowEmptyStrings = false)]
        public string[] KnowHowID { get; set; }
        public SelectList KnowHowList { get; set; }

        [Required(ErrorMessage = "This field is required", AllowEmptyStrings = false)]
        public int? PlacementID { get; set; }
        public SelectList PlacementList { get; set; }

        [Required(ErrorMessage = "This field is required", AllowEmptyStrings = false)]
        public int? WhyNSID { get; set; }
        public SelectList WhyNSList { get; set; }      

        [Required(ErrorMessage = "Please select Course recommended", AllowEmptyStrings = false)]
        public string[] CourseID { get; set; }
        public SelectList CourseList { get; set; }
       
        [Required(ErrorMessage = "Please select status of the customer", AllowEmptyStrings = false)]
        public int? customerTypeID { get; set; }
        public SelectList customerTypeList { get; set; }

        [Required(ErrorMessage = "Please select CareTaker", AllowEmptyStrings = false)]
        public int? CareGiverID { get; set; }
        public SelectList CareGiverList { get; set; }

        [Required(ErrorMessage = "Please select WalkInn Center", AllowEmptyStrings = false)]
        public int? CenterID { get; set; }
        public SelectList CenterList { get; set; }

        public int? StudentCurrentYear { get; set; }
        public SelectList StudentCurrentYearList { get; set; }

        [Required(ErrorMessage = "Please select Employee", AllowEmptyStrings = false)]
        public int? CRO1ID { get; set; }
        public SelectList CRO1EmpList { get; set; }

        [Required(ErrorMessage = "Please select Employee", AllowEmptyStrings = false)]
        public int? CRO2ID { get; set; }
        public SelectList CRO2EmpList { get; set; }

        [Required(ErrorMessage = "Please enter MobileNo", AllowEmptyStrings = false)]
        [RegularExpression("^[0-9]{10,10}$", ErrorMessage = "Invalid MobileNo.Minimum 10 numbers required")]
        [Remote("IsMobileAlreadyExists", "WalkInn", AdditionalFields = "InitialMobile", ErrorMessage = "MobileNo already exists")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Please enter EmailId", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        [Remote("IsEamilAlreadyExists", "WalkInn", AdditionalFields = "InitialEmail", ErrorMessage = "EmailId already exists")]
        public string EmailId { get; set; }


        public bool Gender { get; set; }

        //public bool GuardianType { get; set; }

        public bool HasPrevExp { get; set; }

        public bool IsEquipmentDemoGiven { get; set; }

        public bool JoinStatus { get; set; }

        //There was a problem in setting Foolproofvalidation for multiple items.Should check after including mvcexpressive annotations   

        public Nullable<int> YearOfCompletion { get; set; }
      
        public string CollegeAddress { get; set; }
        
        public Nullable<int> ExpInYears { get; set; }
       
        public string IndustryType { get; set; }
       
        public string CompanyAddress { get; set; }
      
        public string Feedback { get; set; }

        public DateTime? JoinDate { get; set; }
        
        public int? BatchPrefferedID { get; set; }
        public SelectList BatchPrefferedList { get; set; }
       
        public string NotJoiningReason { get; set; }

        public EnumClass.CROCount CROCount { get; set; }

        public StudentWalkInn StudentWalkInn { get; set; }

        public List<StudentRelation> StudentRelation { get; set; }

        [Required(ErrorMessage = "Please enter Percentage", AllowEmptyStrings = false)]
        public int CRO1Percentage { get; set; }

        [Required(ErrorMessage = "Please enter Percentage", AllowEmptyStrings = false)]
        public int CRO2Percentage { get; set; }
        

       

    }
}