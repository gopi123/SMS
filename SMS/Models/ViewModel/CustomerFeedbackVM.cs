using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class CustomerFeedbackVM
    {
        public int FeedBackId { get; set; }
        public string CourseName { get; set; }
        public int Duration { get; set; }
        public bool IsHardCopyRequired { get; set; }
        public string DefaultInstructorPhoto { get; set; }

        [Required(ErrorMessage = "Please select instructor")]
        public int? InstructorID { get; set; }
        public SelectList InstuctorList { get; set; }

        [Required(ErrorMessage = "Please select course")]
        public string[] PreferredCourseID { get; set; }
        public SelectList PreferredCourseList { get; set; }

        [Required(ErrorMessage = "Please select status")]
        public EnumClass.FutureCourseJoinStatus FutureCourseJoinStatus { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string LearningEnvironmentDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string EquipmentDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string CiricullumDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string InstructorClassStartTimeDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string InstructorPreparationDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string InstructorExplanationDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string InstructorProjectSupportDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string CustomerCareDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string CourseRecommendationDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter reason")]
        public string OverallExperienceDislikeReason { get; set; }

        [Required(ErrorMessage = "Please enter CourseStart Date")]
        public DateTime? CourseStartDate { get; set; }

        [Required(ErrorMessage = "Please enter CourseEnd Date")]
        public DateTime? CourseEndDate { get; set; }

        public StudentFeedback StudentFeedback { get; set; }


        //For mail sending
        public string StudentEmailId { get; set; }
        public string CROName { get; set; }
        public int CenterId { get; set; }
      


    }
}