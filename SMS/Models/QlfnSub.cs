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
    
    public partial class QlfnSub
    {
        public QlfnSub()
        {
            this.Employees = new HashSet<Employee>();
            this.StudentWalkInns = new HashSet<StudentWalkInn>();
        }
    
        public int Id { get; set; }
        public Nullable<int> QlfnMainId { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual QlfnMain QlfnMain { get; set; }
        public virtual ICollection<StudentWalkInn> StudentWalkInns { get; set; }
    }
}
