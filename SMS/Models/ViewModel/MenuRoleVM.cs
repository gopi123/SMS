using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class MenuRoleVM
    {
        [Required(ErrorMessage="Please select Role")]
        public int? RoleId { get; set; }
        public SelectList RoleList { get; set; }

        [Required(ErrorMessage = "Please select Designation")]
        public int? DesignationId { get; set; }
        public SelectList DesignationList { get; set; } 
        public List<MenuVM> MenuList { get; set; }
    }
}