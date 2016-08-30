using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class DashboardVM
    {
        public class MenuParent
        {
            public int? MenuParentID { get; set; }
            public string MenuParentName { get; set; }
        }
        public List<MenuParent> MenuParentList { get; set; }
        public List<Menu> MenuList { get; set; }
        public User User { get; set; }

        
    }
}