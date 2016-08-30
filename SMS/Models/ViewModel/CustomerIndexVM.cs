using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CustomerIndexVM
    {
        public class dtCustomer
        {
            public int Id { get; set; }
            public string Course { get; set; }
            public bool? IsProjectUploaded { get; set; }
            public bool? IsTrainerVerified { get; set; }
            public bool? IsFeedBackGiven { get; set; }
            public StudentProjectApproval StudentProjectApproval { get; set; }
            public bool IsPhotoRejected { get; set; }
            public bool ProjectUploadRequired
            {
                get
                {
                    //if no project is uploaded => Project upload required
                    if (IsProjectUploaded == false)
                    {
                        return true;
                    }
                    //if project is uploaded and trainer has denied the project => Project upload required
                    else if (IsProjectUploaded == true && IsTrainerVerified == true && StudentProjectApproval.IsTrainerApproved == false)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            public List<StudentFeedback> StudentFeedback { get; set; }
            public bool IsLastCourse_ProjectUpload
            {
                get
                {
                    bool _isLastCourse = false;
                    int _projectUploadCount = StudentFeedback.Select(f => f.IsProjectUploaded == true).Count();
                    if (_projectUploadCount == 1)
                    {
                        if (IsProjectUploaded == true)
                        {
                            _isLastCourse = true;
                        }
                        else
                        {
                            _isLastCourse = false;
                        }
                    }
                    else
                    {
                        _isLastCourse = false;
                    }
                    return _isLastCourse;
                }
            }
        }
        public List<StudentFeedback> StudentFeedback { get; set; }
        public bool isPhotoUploaded { get; set; }
        public string RegistraionNumber { get; set; }
        public string StudentName { get; set; }
        public bool isProjectUploaded { get; set; }
        public int RemainingFee { get; set; }
        public List<dtCustomer> dtCustomerList { get; set; }
        public bool isPhotoRejected { get; set; }
    }

}


