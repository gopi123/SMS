using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class EmployeeVM
    {
        [Required(ErrorMessage="Please select Center",AllowEmptyStrings=true)]
        public string[] CenterCodeId { get; set; }
        public SelectList CenterCodeList { get; set; }

        [Required(ErrorMessage = "Please select Designation", AllowEmptyStrings = true)]
        public int? DesignationId { get; set; }
        public SelectList DesignationList { get; set; }

        [Required(ErrorMessage = "Please select Department", AllowEmptyStrings = true)]
        public int? DepartmentId { get; set; }
        public SelectList DepartmentList { get; set; }

        [Required(ErrorMessage = "Please select Type", AllowEmptyStrings = true)]
        public int? QlfnTypeId { get; set; }
        public SelectList QlfnTypeList { get; set; }

        [Required(ErrorMessage = "Please select Stream", AllowEmptyStrings = true)]
        public int? QlfnMainId { get; set; }
        public SelectList QlfnMainList { get; set; }

        [Required(ErrorMessage = "Please select Course", AllowEmptyStrings = true)]
        public int? QlfnSubId { get; set; }
        public SelectList QlfnSubList { get; set; }

        [Required(ErrorMessage = "Please select State", AllowEmptyStrings = true)]
        public int? StateId { get; set; }
        public SelectList StateList { get; set; }

        [Required(ErrorMessage = "Please select District", AllowEmptyStrings = true)]
        public int? DistrictId { get; set; }
        public SelectList DistrictList { get; set; }

        [Required(ErrorMessage = "Please upload Photo", AllowEmptyStrings = false)]        
        public HttpPostedFileBase PhotoUrl { get; set; }

        [Required(ErrorMessage = "Please upload AddressProof", AllowEmptyStrings = false)]
        public HttpPostedFileBase AddressProofUrl { get; set; }

        [Required(ErrorMessage = "Please upload IdProof", AllowEmptyStrings = false)]
        public HttpPostedFileBase IdProofUrl { get; set; }
      
        public HttpPostedFileBase PhotoUrlEdit { get; set; }
      
        public HttpPostedFileBase AddressProofUrlEdit { get; set; }
     
        public HttpPostedFileBase IdProofUrlEdit { get; set; }

        [Required(ErrorMessage = "Please enter UserName", AllowEmptyStrings = false)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Minimum 6 characters required")]
        [Remote("CheckUsername", "Employee")]
        public string UserName { get; set; }

        public bool Gender { get; set; }

        public bool MaritalStatus { get; set; }

        public Employee Employee { get; set; }

        public User User { get; set; }

       
    }
   
}