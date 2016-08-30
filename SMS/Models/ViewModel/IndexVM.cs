using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models.ViewModel
{
    public class IndexVM
    {
        public SelectList FinancialYearList { get; set; }
        public SelectList StudentTypeList { get; set; }     

        public string MonthName { get; set; }
        public SelectList MonthList { get; set; }
    }
}