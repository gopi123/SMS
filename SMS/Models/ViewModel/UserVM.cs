using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Foolproof;

namespace SMS.Models.ViewModel
{
    public class UserVM
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter Current Password", AllowEmptyStrings = false)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Please enter New Password", AllowEmptyStrings = false)]
        [RegularExpression("^.{5,20}$", ErrorMessage = "Only 5-20 characters allowed.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please Confirm Password", AllowEmptyStrings = false)]        
        [EqualTo("NewPassword",ErrorMessage="Password Mismatch")]
        public string NewPassword2 { get; set; }
    }
}