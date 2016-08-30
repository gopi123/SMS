using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class ReceiptVM
    {
        public HttpPostedFileBase PhotoUrl { get; set; }
        public string FeeMode { get; set; }      
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public string SoftwareUsed { get; set; }
        public string IsEmailValidated { get; set; }//used for validating mail
        public string CROName { get; set; }//used for sending sms
        public string CourseId { get; set; }//used for sending sms
        public string Email { get; set; }//used for displaying in add page(condition applied)
        public string MobileNo { get; set; }//used for displaying in add page(condition applied)

        public int PaymentModeID { get; set; }
        public SelectList PaymentModeList { get; set; }

        [Required(ErrorMessage="Please enter Bank Name",AllowEmptyStrings=false)]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Please enter Branch Name", AllowEmptyStrings = false)]
        public string BranchName { get; set; }

        [Required(ErrorMessage = "Please enter Ch./DD No", AllowEmptyStrings = false)]
        public string ChequeNo { get; set; }

        [Required(ErrorMessage = "Please select Ch./DD Date", AllowEmptyStrings = false)]
        public DateTime? ChequeDate { get; set; }

        public StudentRegistration StudentRegistration { get; set; }
        public List<StudentReceipt> StudentReceipt { get; set; }

        public List<string> StudentReceiptNoList
        {
            get
            {
                List<string> _studReceiptNoList=new List<string>();
                for (int i = 0; i < StudentReceipt.Count; i++)
                {
                    if (StudentReceipt[i].Status == true)
                    {
                        _studReceiptNoList.Add(Common.GetReceiptNo(StudentReceipt[i].StudentReceiptNo));
                    }
                    else
                    {
                        _studReceiptNoList.Add(string.Empty);
                    }
                }
                return _studReceiptNoList;
            }
        }
    }
}