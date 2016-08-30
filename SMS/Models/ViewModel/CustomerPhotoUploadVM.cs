using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CustomerPhotoUploadVM
    {
        public string PhotoUrl { get; set; }
        public int StudentRegId { get; set; }
        public bool IsPhotoVerified { get; set; }
        public bool IsPhotoRejected { get; set; }

        [Required(ErrorMessage = "Please upload file.")]
        public HttpPostedFileBase PhotoNewUrl { get; set; }
    }
}