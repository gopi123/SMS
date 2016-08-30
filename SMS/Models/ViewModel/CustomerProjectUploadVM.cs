using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CustomerProjectUploadVM
    {
        public Course Course { get; set; }
        public int FeedbackId { get; set; }

        [Required(ErrorMessage="Please enter project title",AllowEmptyStrings=false)]
        public string ProjectTitle { get; set; }

        [Required(ErrorMessage="Please upload file",AllowEmptyStrings=false)]
        public HttpPostedFileBase ProjectUpload { get; set; }

        public string StudentEmailId { get; set; }
        public int CenterId { get; set; }
        public string CROName { get; set; }
    }
}