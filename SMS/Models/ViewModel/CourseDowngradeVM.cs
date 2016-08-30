using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class CourseDowngradeVM
    {
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter EmailId", AllowEmptyStrings = false)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter MobileNo", AllowEmptyStrings = false)]
        public string MobileNo { get; set; }
        public StudentRegistration StudentRegistration { get; set; }
        public string Curr_MultiCourseCode { get; set; }
        public string Curr_CourseTitle { get; set; }
        public string Curr_SoftwareDetails { get; set; }

        [Required(ErrorMessage="Please select course to downgrade",AllowEmptyStrings=false)]
        public int CourseDowngradeId { get; set; }
        public SelectList CourseDowngradeList { get; set; }

        [Required(ErrorMessage = "Installment")]
        public EnumClass.InstallmentType InstallmentType { get; set; }

        [Required(ErrorMessage = "Please select no of installment", AllowEmptyStrings = false)]
        public int? InstallmentID { get; set; }
        public SelectList InstallmentList { get; set; }

        [Required(ErrorMessage = "Please select MultiCourse", AllowEmptyStrings = false)]
        public string[] MultiCourseId { get; set; }
        public SelectList MultiCourseList { get; set; }
        public string CourseIds { get; set; }
        public string MultiCourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string SoftwareDetails { get; set; }
        public int Duration { get; set; }

        [Range(0, 100, ErrorMessage = "Enter number between 0 to 100")]
        public int Discount { get; set; }
        public int DefaultDiscountPercentage { get; set; }
        public int CourseFee { get; set; }
        public double ST { get; set; }
        public int STAmount { get; set; }
        public int TotalFee { get; set; }
        public int RoundUpId { get; set; }
        public SelectList RoundUpList { get; set; }
        public int PrevStudentWalkinnId { get; set; }
        public List<StudentReceipt> StudentReceiptList { get; set; }
        public string DefaultEmailId { get; set; }
        public string CentreCode { get; set; }
        public string PrevCourseId { get; set; }
        public int FeeMode { get; set; }
        public string NewCourseIds { get; set; }
        
        [Required(ErrorMessage="Please enter PinNo",AllowEmptyStrings=false)]
        public int StudentPinNo { get; set; }

        [Required(ErrorMessage = "Please enter PinNo", AllowEmptyStrings = false)]
        public int ManagerPinNo { get; set; }

        public string StudentMobileNo { get; set; }
        public string CroName { get; set; }
        public string MgrMobileNo { get; set; }

        public List<string> StudentReceiptNoList
        {
            get
            {
                List<string> _studRecNoList = new List<string>();
                if (StudentReceiptList != null)
                {
                    for (int i = 0; i < StudentReceiptList.Count; i++)
                    {
                        if (StudentReceiptList[i].Status == true)
                        {
                            _studRecNoList.Add(Common.GetReceiptNo(StudentReceiptList[i].StudentReceiptNo));
                        }
                        else
                        {
                            _studRecNoList.Add(string.Empty);
                        }
                    }
                }
                else
                {
                    _studRecNoList.Add(string.Empty);
                }
              
                return _studRecNoList;
            }

        }

    }
}