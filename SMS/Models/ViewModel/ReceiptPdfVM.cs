using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class ReceiptPdfVM
    {
        public class ReceiptDetails
        {
            public int CourseFee { get; set; }
            public string DatePaid { get; set; }
            public string ReceiptNo { get; set; }          
        }

        public string CompanyAddress { get; set; }
        public string CompanyPhoneNo { get; set; }       
        public string CourseTitle { get; set; }
        public string ReceiptDate { get; set; }
        public int Duration { get; set; }
        public string ReceiptNo { get; set; }       
        public string RegistrationNumber { get; set; }
        public string StudentName { get; set; }
        public int TotalAmount { get; set; }
        public string ServiceTaxRegistrationNo { get; set; }
        public string CentreCode { get; set; }
        public string CROName { get; set; }
        public string LoginPersonName { get; set; }
        public string StudentMaskedMobileNo { get; set; }
        public string StudentMaskedEmailId { get; set;  }
        public string TotalAmountInWords { get; set; }       
        public string FeeMode { get; set; }
        public string ModeOfPayment { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string ChequeNo { get; set; }
        public string ChequeDate { get; set; }
        public int CurrSTAmount { get; set; }
        public int CurrCourseFee { get; set; }
        public int CurrPaidTotal { get; set; }
        public int BalanceAmount { get; set; }

        

        public List<ReceiptDetails> ReceiptDetailsList { get; set; }
    }
}