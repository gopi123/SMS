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
    
    public partial class StudentRegistrationCourse
    {
        public int Id { get; set; }
        public Nullable<int> StudentRegistrationID { get; set; }
        public Nullable<int> MultiCourseID { get; set; }
    
        public virtual MultiCourse MultiCourse { get; set; }
        public virtual StudentRegistration StudentRegistration { get; set; }
    }
}
