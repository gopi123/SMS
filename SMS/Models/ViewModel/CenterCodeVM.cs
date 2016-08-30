using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Foolproof;

namespace SMS.Models.ViewModel
{
    public class CenterCodeVM
    {
        [Required(ErrorMessage = "Please select State", AllowEmptyStrings = false)]
        public int? StateId { get; set; }
        public SelectList StateList { get; set; }

        [Required(ErrorMessage = "Please select District", AllowEmptyStrings = false)]
        public int? DistrictId { get; set; }
        public SelectList DistrictList { get; set; }

       
        public int? EmployeeID { get; set; }
        public SelectList EmployeeList { get; set; }

        [Required(ErrorMessage = "Please select BranchType", AllowEmptyStrings = false)]
        public int? BranchID { get; set; }
        public SelectList BranchList { get; set; }

        [Required(ErrorMessage = "Please select FirmType", AllowEmptyStrings = false)]
        public int? FirmID { get; set; }
        public SelectList FirmList { get; set; }

        public bool STCompulsory { get; set; }

        [RequiredIf("STCompulsory", true, ErrorMessage = "Please enter Registration No")]
        public string STRegNo { get; set; }       

        public CenterCode CenterCode { get; set; }
        public List<CenterCodePartnerDetail> CenterCodePartnerDetail { get; set; }


      
    }
}