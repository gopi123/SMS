using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class DiscountSettingVM
    {
        public class clsDiscountSettings
        {
            public int CentreCodeId { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public string Foundation { get; set; }
            public string Diploma { get; set; }
            public string MasterDiploma { get; set; }
            public string Professional { get; set; }
            public DateTime FromDate { get; set; }
        }

        public List<clsDiscountSettings> DiscountSettingsList { get; set; }      


        [Required(ErrorMessage = "Please select Role")]
        public int[] RoleId { get; set; }
        public SelectList RoleList { get; set; }

        [Required(ErrorMessage = "Please select Centre")]
        public string[] GroupName { get; set; }
        public SelectList GroupNameList { get; set; }

        [Required(ErrorMessage="Select Date")]
        public DateTime? DiscountFromDate { get; set; }

        public string[] CentreCode { get; set; }
        public SelectList CentreCodeList { get; set; }

    }
}