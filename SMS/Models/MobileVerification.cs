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
    
    public partial class MobileVerification
    {
        public int Id { get; set; }
        public Nullable<int> TypeId { get; set; }
        public string Type { get; set; }
        public Nullable<int> PinNo { get; set; }
        public string MobileNo { get; set; }
    }
}
