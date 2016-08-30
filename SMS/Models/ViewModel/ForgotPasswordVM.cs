using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class ForgotPasswordVM
    {
        [Required(ErrorMessage="Username is required",AllowEmptyStrings=false)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        public string Email { get; set; }

        [Required(ErrorMessage = "DOB is required", AllowEmptyStrings = false)]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "MobileNo is required", AllowEmptyStrings = false)]
        [RegularExpression("^[0-9]{10,10}$", ErrorMessage = "Invalid MobileNo.Please enter 10 digits.")]
        public string MobileNo { get; set; }
    }
}