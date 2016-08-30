//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class District
    {
        public District()
        {
            this.CenterCodes = new HashSet<CenterCode>();
            this.Employees = new HashSet<Employee>();
            this.StudentWalkInns = new HashSet<StudentWalkInn>();
        }
    
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public int StateId { get; set; }
    
        public virtual ICollection<CenterCode> CenterCodes { get; set; }
        public virtual State State { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<StudentWalkInn> StudentWalkInns { get; set; }
    }
}
