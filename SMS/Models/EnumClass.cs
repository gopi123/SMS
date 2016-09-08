

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.ComponentModel;


namespace SMS.Models
{
    public static class EnumClass
    {
        public enum BatchPreferred
        {
            INT = 1,
            HALFDAY_MORNING = 2,
            HALFDAY_EVENING = 3,
            PARTTIME = 4,
            WEEKEND_SATURDAY = 5,
            WEEKEND_SUNDAY = 6,
            SPECIAL = 7
        }

        public enum Placement
        {
            YES = 1,
            NO = 2,
            LATER = 3
        }

        public enum CustomerType
        {
            STUDENT = 1,
            NONEMPLOYED = 2,
            EMPLOYED = 3,
            SELFEMPLOYED = 4
        }

        public enum GetMonth
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        public enum Role
        {
            ED = 1,
            CPDHEAD = 2,
            CPD = 3,
            CENTERMANAGER = 4,
            ACCOUNTSHEAD = 5,
            MANAGER = 6,
            TECHNICALHEAD = 7,
            SALESINDIVIDUAL = 8,
            ACCOUNTS = 9,
            TECHNICALINDIVIDUAL = 10
        }

        public enum Gender
        {
            Male = 1,
            Female = 2
        }

        public enum CareGiver
        {
            FATHER = 1,
            MOTHER = 2,
            GUARDIAN = 3,
            SPOUSE = 4
        }

        public enum StudentCurrentYear
        {
            FIRST_YEAR = 1,
            SECOND_YEAR = 2,
            THIRD_YEAR = 3,
            FOURTH_YEAR = 4,
            FIFTH_YEAR = 5
        }

        public enum RegistrationVenue
        {
            CENTRE = 1,
            AT_SITE = 2
        }

        public enum CROCount
        {
            ONE = 1,
            TWO = 2
        }

        public enum InstallmentType
        {
            SINGLE = 1,
            INSTALLMENT = 2
        }

        public enum InstallmentNo
        {
            THREE = 3,
            FOUR = 4,
            FIVE = 5,
            SIX = 6,
            SEVEN = 7,
            EIGHT = 8,
            NINE = 9,
            TEN = 10
        }

        public enum RoundUpList
        {
            ROUND_UP = 1,
            ROUND_OFF = 2
        }

        public enum WalkinnStatus
        {
            WALKINN,
            REGISTERED,
            ALL

        }

        public enum DiscountPercentage
        {
            DISCOUNT = 50
        }

        public enum Designation
        {
            ED = 25,
            CPDHEAD = 1,
            SYSTEMSPECIALIST = 2,
            TECHNICALTRAINER = 3,
            GRAPHICDESIGNER = 4,
            SOFTWARESPECIALIST = 5,
            CENTREMANAGER = 6,
            MANAGERFRANCHISEDEVELOPMENT = 7,
            MANAGERSUPPORT = 8,
            ACCOUNTSASSISTANT = 9,
            OPERATIONSLOGISTICS = 10,
            MANAGERSALES = 11,
            ASSISTANTMANAGER = 12,
            TECHNICALLEADER = 13,
            CUSTOMERRELATIONSOFFICER = 14,
            TECHNOCOMMERCIALEXECUTIVE = 15,
            BUSINESSDEVELOPMENTEXECUTIVE = 16,
            ACCOUNTS = 17,
            TECHNICALFACILITATOR = 18,
            TECHNICALENGINEERNETWORKING = 19,
            TECHNICALTRAINER_TI = 20,
            TECHNICALLEADER_TI = 21,
            SOFTWARESPECIALIST_TI = 22,
            NETWORKSPECIALIST = 23,
            NETWORKTRAINER = 24,
            HR = 26
        }

        public enum PaymentMode
        {
            CASH = 1,
            CHEQUE = 2
        }

        public enum ReceiptType
        {
            ORIGINAL = 1,
            DUPLICATE = 2
        }

        public enum FinYearMonth
        {
            STARTMONTH = 4,
            ENDMONTH = 3
        }
        public enum FutureCourseJoinStatus
        {
            YES = 1,
            NO = 2
        }
        public enum PhotoMode
        {
            NEW = 1,
            VERIFIED = 2
        }

        public enum SelectAll
        {
            ALL = -1
        }

        public enum EmployeeCategory
        {
            WORKING = 1,
            NON_WORKING = 0,
            ALL=-1
        }
        public enum ReportMode
        {
            WAKLINNREPORT,
            COLLECTIONREPORT,
            PENDINGREPORT,
            REGISTRATIONREPORT
        }

        public enum CourseCategory
        {
            FOUNDATION=1,
            DIPLOMA=2,
            PROFESSIONAL=3,
            MASTERDIPLOMA=4
        }
        public enum StudentCategory
        {
            STUDENT=1,
            NONEMPLOYED=2,
            EMPLOYED=3,
            SELFEMPLOYED=4
        }
      
        public enum Month
        {

            APRIL,
            MAY ,
            JUNE ,
            JULY ,
            AUGUST ,
            SEPTEMBER ,
            OCTOBER ,
            NOVEMBER ,
            DECEMBER ,
            JANUARY,
            FEBRUARY ,
            MARCH ,
            ALL 

        }

        public enum MaritalStatus
        {
            UNMARRIED=1,
            MARRIED=2
        }

        public enum DiscountSettingRole
        {
            ED=1,
            CENTRE_MANAGER=2,
            CLUSTER_MANAGER=3,
            SALES_MANAGER=4,
            SALES_INDIVIDUAL=5
        }

        public enum SMSCATEGORY
        {
            DISCOUNTSMS=1,
            REGISTRATIONSMS=2,
            RECEIPTSMS=3
        }



    }
}