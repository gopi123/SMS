using SMS.Models.CustomValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class GroupVM
    {
        public class GroupCentreCodeList
        {
            public string SlNo { get; set; }
            public string CentreCodeName { get; set; }
            public string GroupName { get; set; }
            public string GroupCreatedDate { get; set; }
        }

        [Required(ErrorMessage="Please enter GroupName")]
        [Remote("CheckGroupName", "Group", ErrorMessage = "Sorry group name already exists", AdditionalFields = "InitialGroupName")]
        [RegularExpression(@"[^\s]+", ErrorMessage = "Invalid Group Name.Spaces are not allowed for Group Name")]
        public string GroupName { get; set; }

        [RequiredArrayAttribute(ErrorMessage="Please select atleast one Centre Code")]
        public int[] CentreCodeId { get; set; }
        public SelectList CentreCodeList { get; set; }
        public DateTime TransactionDate { get; set; }
        public string InitialGroupName { get; set; }

    }
}