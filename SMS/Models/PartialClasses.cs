using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMS.Models
{
    [MetadataType(typeof(ServiceTaxMetadata))]
    public partial class ServiceTax
    {
    }

    [MetadataType(typeof(CourseMetadata))]
    public partial class Course
    {
    }

    [MetadataType(typeof(CourseSubTitleMetadata))]
    public partial class CourseSubTitle 
    { 
    }

    [MetadataType(typeof(CenterCodeMetaData))]
    public partial class CenterCode
    {
    }

    [MetadataType(typeof(CenterCodePartnerDetailsMetaData))]
    public partial class CenterCodePartnerDetail
    {
    }

    [MetadataType(typeof(EmployeeMetaData))]
    public partial class Employee
    {
    }

    [MetadataType(typeof(MobileVerificationMetaData))]
    public partial class MobileVerification
    {
    }

    [MetadataType(typeof(StudentWalkInnMetaData))]
    public partial class StudentWalkInn
    {
    }

    [MetadataType(typeof(StudentReceiptMetaData))]
    public partial class StudentReceipt
    {

    }

   
}