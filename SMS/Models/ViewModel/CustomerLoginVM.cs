using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace SMS.Models.ViewModel
{
    public class CustomerLoginVM
    {
        [Required(ErrorMessage = "Enter Username", AllowEmptyStrings = true)]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Must be at least 4 characters long.")] 
        [System.Web.Mvc.Remote("CheckUsername", "Customer")]
        public string UserName { get; set; }

        [Required(ErrorMessage="Enter Password",AllowEmptyStrings=true)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[$@$!%*#?&])[A-Za-z\d$@$!%*#?&]{6,}$",
            ErrorMessage="Minimum 6 characters atleast one Alphabet(a-z,A-Z), one Number(0-9) and one Special Character(@,#..). Example:user@123")]
        public string Password { get; set; }

        [Required(ErrorMessage="Enter ConfirmPassword",AllowEmptyStrings=false)]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Enter StudentID", AllowEmptyStrings = true)]
        public string StudentID { get; set; }

        [Required(ErrorMessage = "Enter DOB", AllowEmptyStrings = true)]
        public string DOB { get; set; }

        [Required(ErrorMessage = "Enter EmailID", AllowEmptyStrings = true)]
        [EmailAddress(ErrorMessage = "Invalid EmailId")]
        public string EmailID { get; set; }

        [Required(ErrorMessage = "Enter MobileNo", AllowEmptyStrings = true)]
        [RegularExpression(@"^((\+91-?)|0)?[0-9]{10}$", ErrorMessage = "Invalid MobileNo")]
        public string MobileNo { get; set; }
        public string StudentName { get; set; }
        public int StudentLoginID { get; set; }        
        public string ReturnPinNo { get; set; }

        [Required(ErrorMessage = "Enter PinNo")]
        [RegularExpression(@"^((\+91-?)|0)?[0-9]{4}$", ErrorMessage = "PinNo must contain only 4 numbers")]
        [Compare("ReturnPinNo",ErrorMessage="Invalid PinNo")]
        public string PinNo { get; set; }

        [Required(ErrorMessage="Enter Username")]
        public string LoginUserName { get; set; }

        [Required(ErrorMessage = "Enter Username")]
        public string LoginPassword { get; set; }

    }
}