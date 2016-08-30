using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class TrainerApprovalVM
    {
       

        [Required(ErrorMessage="Please enter reason")]
        public string SourceCodeCollectedDenyReason { get; set; }
        [Required(ErrorMessage = "Please enter reason")]
        public string CrossCheckedDenyReason { get; set; }
        [Required(ErrorMessage = "Please enter reason")]
        public string ProjectIsNotCopiedDenyReason { get; set; }
        [Required(ErrorMessage = "Please enter reason")]
        public string UploadedSameCourseProjectReportDenyReason { get; set; }
        [Required(ErrorMessage = "Please enter reason")]
        public string ProjectDifferentFromLabExerciseDenyReason { get; set; }

        [Required(ErrorMessage = "Please select YES or NO")]
        public bool? IsSourceCodeCollected { get; set; }
        [Required(ErrorMessage = "Please select YES or NO")]
        public bool? IsCrossChecked { get; set; }
        [Required(ErrorMessage = "Please select YES or NO")]
        public bool? IsProjectNotCopied { get; set; }
        [Required(ErrorMessage = "Please select YES or NO")]
        public bool? IsUploadedSameCourseProjectReport { get; set; }
        [Required(ErrorMessage = "Please select YES or NO")]
        public bool? IsProjectDifferentFromLabExercise { get; set; }  

        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public int FeedbackId { get; set; }
        public string  ProjectDownloadUrl { get; set; }
        public string CourseName { get; set; }
        public string InstructorName { get; set; }
        public StudentProjectApproval StudentProjectApprovalTrainer { get; set; }
    }
}