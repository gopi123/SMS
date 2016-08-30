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
    
    public partial class Employee
    {
        public Employee()
        {
            this.CenterCodes = new HashSet<CenterCode>();
            this.EmployeeCenters = new HashSet<EmployeeCenter>();
            this.StudentProjectApprovals = new HashSet<StudentProjectApproval>();
            this.StudentFeedbacks = new HashSet<StudentFeedback>();
            this.StudentProjectApprovals1 = new HashSet<StudentProjectApproval>();
            this.StudentWalkInns = new HashSet<StudentWalkInn>();
            this.StudentWalkInns1 = new HashSet<StudentWalkInn>();
            this.StudentWalkInns2 = new HashSet<StudentWalkInn>();
            this.Users = new HashSet<User>();
            this.StudentReceipts = new HashSet<StudentReceipt>();
        }
    
        public int Id { get; set; }
        public Nullable<int> DesignationId { get; set; }
        public Nullable<int> QlfnTypeId { get; set; }
        public Nullable<int> QlfnMainId { get; set; }
        public Nullable<int> QlfnSubId { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public string CenterCodeIds { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public Nullable<int> Gender { get; set; }
        public string Address { get; set; }
        public string Pincode { get; set; }
        public string BloodGroup { get; set; }
        public Nullable<int> MaritalStatus { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string OfficialMobileNo { get; set; }
        public string OfficialEmailId { get; set; }
        public Nullable<System.DateTime> DateOfJoin { get; set; }
        public string PhotoUrl { get; set; }
        public string AddressProofUrl { get; set; }
        public string IdProofUrl { get; set; }
        public Nullable<bool> IsEmailVerified { get; set; }
        public Nullable<bool> IsMobileVerified { get; set; }
        public Nullable<bool> Status { get; set; }
    
        public virtual ICollection<CenterCode> CenterCodes { get; set; }
        public virtual Designation Designation { get; set; }
        public virtual District District { get; set; }
        public virtual QlfnMain QlfnMain { get; set; }
        public virtual QlfnSub QlfnSub { get; set; }
        public virtual QlfnType QlfnType { get; set; }
        public virtual ICollection<EmployeeCenter> EmployeeCenters { get; set; }
        public virtual ICollection<StudentProjectApproval> StudentProjectApprovals { get; set; }
        public virtual ICollection<StudentFeedback> StudentFeedbacks { get; set; }
        public virtual ICollection<StudentProjectApproval> StudentProjectApprovals1 { get; set; }
        public virtual ICollection<StudentWalkInn> StudentWalkInns { get; set; }
        public virtual ICollection<StudentWalkInn> StudentWalkInns1 { get; set; }
        public virtual ICollection<StudentWalkInn> StudentWalkInns2 { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<StudentReceipt> StudentReceipts { get; set; }
    }
}
