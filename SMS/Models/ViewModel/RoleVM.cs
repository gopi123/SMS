using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models.ViewModel
{
    public class RoleVM
    {
        public RoleVM()
        {
            node = new List<RoleVM>();
        }
        public string text { get; set; }
        public string icon { get; set; }
        public int roleId { get; set; }
        public Nullable<int> parentId { get; set; }
        public List<RoleVM> node { get; set; }

    }
}