using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class LeaderApprovalVM
    {
       

        [Required(ErrorMessage="Please enter reason",AllowEmptyStrings=false)]
        public string TrainerShownRunningProjectDenialReason { get; set; }
        [Required(ErrorMessage="Please enter reason",AllowEmptyStrings=false)]
        public string SouceCodeCollectedFromTrainerDenialReason { get; set; }
        [Required(ErrorMessage = "Please select YES or NO", AllowEmptyStrings = false)]
        public bool? TrainerShownRunningProject { get; set; }
        [Required(ErrorMessage = "Please select YES or NO", AllowEmptyStrings = false)]
        public bool? SourceCodeCollectedFromTrainer { get; set; }

        public string StudentRegistrationID { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public int FeedbackId { get; set; }
        public string InstructorName { get; set; }
        public string ProjectDownloadUrl { get; set; }
        public string InstructorEmail { get; set; }
        public StudentProjectApproval StudentProjectApproval { get; set; }
    }
}