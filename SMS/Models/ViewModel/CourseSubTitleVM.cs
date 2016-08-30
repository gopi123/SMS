using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class CourseSubTitleVM
    {
        [Required(ErrorMessage="Please select Course type")]
        public int? CourseTypeId { get; set; }
        public SelectList CourseTypeName { get; set; }

        [Required(ErrorMessage = "Please select Course series")]
        public int? CourseSeriesTypeId { get; set; }
        public SelectList CourseSeriesTypeName { get; set; }

        public CourseSubTitle CourseSubTitle { get; set; }


    }
}