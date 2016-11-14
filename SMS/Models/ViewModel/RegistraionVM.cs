using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class RegistraionVM
    {
        public class RegDataTable
        {
            public int RegistrationID { get; set; }
            public string RegDate { get; set; }
            public string Centre { get; set; }
            public string SalesPerson { get; set; }
            public string StudentName { get; set; }
            public int Discount { get; set; }
            public int CourseFee { get; set; }
            public string SoftwareUsed { get; set; }
            public StudentReceipt Receipt { get; set; }
            public bool IsSalesIndividual { get; set; }
            public StudentWalkInn WalkInn { get; set; }
            public int CurrEmpId { get; set; }
            public int NextDueAmount
            {
                get { return Receipt == null ? 0 : Receipt.Total.Value; }

            }
            public string NextDueDate
            {
                get { return Receipt == null ? "" : Receipt.DueDate.Value.ToString("dd/MM/yyyy"); }
            }

            public string MobileNo
            {
                get
                {
                    if (IsSalesIndividual)
                    {
                        if ((CurrEmpId == WalkInn.CRO1ID) || (CurrEmpId == WalkInn.CRO2ID))
                        {
                            return WalkInn.MobileNo;
                        }
                        else
                        {
                            return "-";
                        }
                    }
                    else
                    {
                        return WalkInn.MobileNo;
                    }
                }
            }


        }


        public EnumClass.RegistrationVenue RegistrationVenue { get; set; }

        [Required(ErrorMessage = "Please enter Name", AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter MobileNo", AllowEmptyStrings = false)]
        [RegularExpression("^[0-9]{10,10}$", ErrorMessage = "Invalid MobileNo.Minimum 10 numbers required")]
        [Remote("IsMobileAlreadyExists", "Registration", AdditionalFields = "InitialMobile", ErrorMessage = "MobileNo already exists")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Please enter email", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Enter valid email")]
        [Remote("IsEmailAlreadyExists", "Registration", AdditionalFields = "InitialEmail", ErrorMessage = "EmailId already exists")]
        public string Email { get; set; }

        public HttpPostedFileBase PhotoUrl { get; set; }
        public string CourseTitle { get; set; }
        public string SoftwareDetails { get; set; }
        public int Duration { get; set; }
        public int CourseFee { get; set; }
        public double ST { get; set; }
        public int STAmount { get; set; }

        [Range(0, 100, ErrorMessage = "Enter number between 0 to 100")]
        public int Discount { get; set; }
        public int TotalFee { get; set; }
        public int WalkInnID { get; set; }
        public string MultiCourseCode { get; set; }
        public int DefaultDiscountPercentage { get; set; }
        public string CentreCode { get; set; }
        public int CentreId { get; set; }
        public string CROName { get; set; }
        public string CourseIds { get; set; }
        public int FeeMode { get; set; }        
        public string[] CourseId { get; set; }
        public SelectList CourseList { get; set; }
        public string DiscountReason { get; set; }
        public int? DiscountEmployeeId { get; set; }

        //For Editing purpose
        public string RegistrationNumber { get; set; }
        public int StudentRegistrationID { get; set; }
        public string FeeModeType { get; set; }
        public int InstallmentCount { get; set; }
        public double OldST
        {
            get
            {
                //if single receipt has been paid
                if (StudentReceipt.Where(r => r.Status == true).Count() >= 1)
                {
                    return StudentReceipt.Where(r => r.Status == true)
                           .Last().STPercentage.Value;
                }
                else
                {
                    return ST;
                }
            }
        }

        public int TotalCourseFeePaid
        {
            get
            {
                //if single receipt has been paid
                if (StudentReceipt.Where(r => r.Status == true).Count() >= 1)
                {
                    return StudentReceipt.Where(r => r.Status == true)
                           .Sum(r => r.Fee.Value);
                }
                else
                {
                    return 0;
                }
            }

        }

        public int TotalSTPaid
        {
            get
            {
                //if single receipt has been paid
                if (StudentReceipt.Where(r => r.Status == true).Count() >= 1)
                {
                    return StudentReceipt.Where(r => r.Status == true)
                           .Sum(r => r.ST.Value);
                }
                else
                {
                    return 0;
                }
            }

        }

        public int TotalAmountPaid
        {
            get
            {
                //if single receipt has been paid
                if (StudentReceipt.Where(r => r.Status == true).Count() >= 1)
                {
                    return StudentReceipt.Where(r => r.Status == true)
                           .Sum(r => r.Total.Value);
                }
                else
                {
                    return 0;
                }
            }

        }
        //


        [Required(ErrorMessage = "Enter PinNo")]
        public string PinNo { get; set; }

        [Required(ErrorMessage = "Installment")]
        public EnumClass.InstallmentType InstallmentType { get; set; }

        [Required(ErrorMessage = "Please select MultiCourse", AllowEmptyStrings = false)]
        public string[] MultiCourseId { get; set; }
        public SelectList MultiCourseList { get; set; }

        [Required(ErrorMessage = "Please select no of installment", AllowEmptyStrings = false)]
        public int InstallmentID { get; set; }
        public SelectList InstallmentList { get; set; }
        public int RoundUpId { get; set; }
        public SelectList RoundUpList { get; set; }
        public StudentRegistration StudentRegistration { get; set; }
        public int CourseInterchangeFee { get; set; }
        public int CourseInterchangeSTAmount { get; set; }
        public List<StudentReceipt> StudentReceipt { get; set; }
        public List<string> StudentReceiptNoList 
        {
            get
            {
                List<string> _studRecNoList = new List<string>();
                for (int i = 0; i < StudentReceipt.Count; i++)
                {
                    if (StudentReceipt[i].Status == true)
                    {
                        _studRecNoList.Add(Common.GetReceiptNo(StudentReceipt[i].StudentReceiptNo));
                    }
                    else
                    {
                        _studRecNoList.Add(string.Empty);
                    }
                }
                return _studRecNoList;
            }
        
        }
        public class clsCourseCodeSearch
        {
            public string CourseCode { get; set; }
            public decimal SingleFee { get; set; }
            public decimal InstallmentFee { get; set; }
            public string CourseCombination { get; set; }           
           
        }
    }
}