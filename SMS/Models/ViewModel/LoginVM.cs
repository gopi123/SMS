using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage="Username is required",AllowEmptyStrings=false)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required", AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}