using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class CourseVM
    {
        [Required(ErrorMessage = "Please select Duration Mode")]
        public int? DurationModeId { get; set; }
        public SelectList DurationModeList { get; set; }
        public Course Course { get; set; }
    }
}