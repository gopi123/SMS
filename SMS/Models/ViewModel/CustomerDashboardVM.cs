using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class CustomerDashboardVM
    {
        public string StudentName { get; set; }
        public string RegistrationNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string PhotoUrl { get; set; }
        public string MultiCourse { get; set; }        
        public int Duration { get; set; }
        public int TotalAmount { get; set; }

    }
}