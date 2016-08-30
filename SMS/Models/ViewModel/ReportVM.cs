using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class ReportVM
    {
        public string FinYearId { get; set; }
        public SelectList FinYearList { get; set; }
        public int? CentreId { get; set; }
        public SelectList CentreList { get; set; }

        [Required(ErrorMessage="Please select Employee")]
        public int? EmpId { get; set; }
        public SelectList EmpList { get; set; }
        public string CategoryType { get; set; }
        public SelectList CategoryTypeList { get; set; }

        [Required(ErrorMessage="Please select FromDate")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? FromDate { get; set; }

        [Required(ErrorMessage="Please select ToDate")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? ToDate { get; set; }

        public SelectList CourseCategoryList { get; set; }
        public int? CourseCategoryId { get; set; }

        public SelectList CourseList { get; set; }
        public int? CourseId { get; set; }
    }
}