using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class MultiCourseVM
    {
        [Required(ErrorMessage="Please select Course title",AllowEmptyStrings=false)]
        public int? CourseSubTitleId { get; set; }
        public SelectList CourseSubTitleList { get; set; }

        [Required(ErrorMessage = "Please select Course type", AllowEmptyStrings = false)]
        public int? CourseTypeId { get; set; }
        public SelectList CourseTypeList { get; set; }

        [Required(ErrorMessage = "Please select Course series", AllowEmptyStrings = false)]
        public int? CourseSeriesId { get; set; }
        public SelectList CourseSeriesList { get; set; }

        [Required(ErrorMessage = "Please select Multiple courses", AllowEmptyStrings = false)]
        public string[] MultipleCourseId { get; set; }
        public SelectList MultipleCourseList { get; set; }
        public MultiCourse MultiCourse { get; set; }

    }
}