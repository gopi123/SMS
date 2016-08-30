using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class MailTest_Feedback
    {
        [Required(ErrorMessage="Please select StudentID")]
        public string StudentID { get; set; }
    }
}