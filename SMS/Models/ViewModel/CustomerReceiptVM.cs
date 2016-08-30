using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CustomerReceiptVM
    {
        public string ReceiptNo { get; set; }
        public int Fee { get; set; }
        public double STPercentage { get; set; }
        public int ST { get; set; }
        public int Total { get; set; }
        public string DueDate { get; set; }
        public int ReceiptId { get; set; }
        public bool Status { get; set; }



    }
}