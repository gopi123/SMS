using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Models
{
    public class Common_Report : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        public List<Employee> GetEmployeeList_Walkinn(int centerId, string categoryType, int empId, DateTime startDate, DateTime endDate)
        {
            List<Employee> _empList = new List<Employee>();
            try
            {
                Common _cmn = new Common();
                List<int> _centreIdList = new List<int>();
                List<int> _salesRoleIdList = new List<int>();
                int _empId = empId;
                List<StudentWalkInn> _walkInnList = new List<StudentWalkInn>();

                //Get Loggin Employee
                var _employee = _db.Employees
                                  .Where(e => e.Id == _empId)
                                  .FirstOrDefault();


                if (centerId == (int)EnumClass.SelectAll.ALL)
                {
                    //Get all centers allotted to an employee
                    _centreIdList = _employee.EmployeeCenters
                                    .Select(ec => ec.CenterCode.Id)
                                    .ToList();
                }
                else
                {
                    _centreIdList.Add(centerId);
                }


                //Get CurrentRole of the employee
                int _currRole = _employee.Designation.Role.Id;

                //If Employee is sales individual populate only his/her name
                if (_currRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                {
                    _empList.Add(_employee);
                }
                else
                {

                    //Filter walkinn by centre and status
                    _walkInnList = _db.StudentWalkInns
                                 .AsEnumerable()
                                 .Where(w => _centreIdList.Contains(w.CenterCode.Id)
                                             && (categoryType.ToLower() == EnumClass.SelectAll.ALL.ToString().ToLower() ||
                                                 categoryType.ToLower() == w.Status.ToString().ToLower())
                                             && w.TransactionDate.Value.Date >= startDate.Date
                                             && w.TransactionDate.Value.Date <= endDate.Date)
                                 .ToList();

                    List<int> _cro1WalkInnId = _walkInnList
                                               .Select(w => w.Employee1.Id).ToList();

                    List<int> _cro2WalkInnId = _walkInnList
                                                 .Where(w => w.CROCount.Value == 2)
                                                 .Select(w => w.Employee2.Id).ToList();

                    List<int> _walkinnCroIds = _cro1WalkInnId.Concat(_cro2WalkInnId).Distinct().ToList();

                    _empList = _db.Employees
                                .Where(e => _walkinnCroIds.Contains(e.Id))
                                .ToList();
                }
                return _empList;
            }
            catch (Exception ex)
            {
                return _empList = null;
            }
        }
        public List<Employee> GetEmployeeList_Collection(int centreId, int empId, DateTime startDate, DateTime endDate, string reportMode)
        {
            List<Employee> _empList = new List<Employee>();
            List<StudentReceipt> _receiptList = new List<StudentReceipt>();
            try
            {
                Common _cmn = new Common();
                List<int> _centreIdList = new List<int>();
                List<int> _salesRoleIdList = new List<int>();
                int _empId = empId;

                //Get Loggin Employee
                var _employee = _db.Employees
                                  .Where(e => e.Id == _empId)
                                  .FirstOrDefault();

                //Get CurrentRole of the employee
                int _currRole = _employee.Designation.Role.Id;

                if (centreId == (int)EnumClass.SelectAll.ALL)
                {
                    //Get all centers allotted to an employee
                    _centreIdList = _employee.EmployeeCenters
                                    .Select(ec => ec.CenterCode.Id)
                                    .ToList();
                }
                else
                {
                    _centreIdList.Add(centreId);
                }


                //If Employee is sales individual populate only his/her name
                if (_currRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                {
                    _empList.Add(_employee);
                }
                else
                {
                    //if collection report is taken then take all paid receipt
                    if (reportMode == EnumClass.ReportMode.COLLECTIONREPORT.ToString())
                    {
                        _receiptList = _db.StudentReceipts
                                        .AsEnumerable()
                                        .Where(r => r.Status == true
                                           && (_centreIdList.Contains(r.StudentRegistration.StudentWalkInn.CenterCode.Id))
                                           && r.DueDate.Value.Date >= startDate.Date
                                           && r.DueDate.Value.Date <= endDate.Date)
                                        .ToList();
                    }
                    //if pending report is taken then take all unpaid receipt
                    else
                    {
                        _receiptList = _db.StudentReceipts
                                        .AsEnumerable()
                                        .Where(r => r.Status == false
                                            && (_centreIdList.Contains(r.StudentRegistration.StudentWalkInn.CenterCode.Id))
                                           && r.DueDate.Value.Date >= startDate.Date
                                           && r.DueDate.Value.Date <= endDate.Date)
                                        .ToList();
                    }

                    //if wakinn is not a shared one
                    List<int> _cro1ReceiptEmpId = _receiptList
                                                    .Select(r => r.StudentRegistration.StudentWalkInn.Employee1.Id)
                                                    .ToList();
                    //if walkinn is shared one                 

                    List<int> _cro2ReceiptEmpId = _receiptList
                                                   .Where(r => r.StudentRegistration.StudentWalkInn.CROCount == 2)
                                                   .Select(r => r.StudentRegistration.StudentWalkInn.Employee2.Id)
                                                   .ToList();

                    List<int> _allReceiptEmpId = _cro1ReceiptEmpId.Concat(_cro2ReceiptEmpId).Distinct().ToList();

                    //Get all employees who has taken collection or receipt and disabled viewing details of higher rank employee 
                    _empList = _db.Employees
                              .Where(e => _allReceiptEmpId.Contains(e.Id)
                                    && e.Designation.Role.Rank >= _employee.Designation.Role.Rank)
                              .ToList();
                }
                return _empList;
            }
            catch (Exception ex)
            {
                return _empList = null;
            }
        }

        //List all employees from studentregistration table
        public List<Employee> GetEmployeeList_Registration(int centreId, int empId, DateTime startDate, DateTime endDate)
        {
            List<Employee> _empList = new List<Employee>();
            try
            {
                Common _cmn = new Common();
                List<int> _centreIdList = new List<int>();
                List<int> _salesRoleIdList = new List<int>();
                int _empId = empId;
                List<StudentRegistration> _regnList = new List<StudentRegistration>();

                //Get Loggin Employee
                var _employee = _db.Employees
                                  .Where(e => e.Id == _empId)
                                  .FirstOrDefault();

                //Get CurrentRole of the employee
                int _currRole = _employee.Designation.Role.Id;

                if (centreId == (int)EnumClass.SelectAll.ALL)
                {
                    //Get all centers allotted to an employee
                    _centreIdList = _employee.EmployeeCenters
                                    .Select(ec => ec.CenterCode.Id)
                                    .ToList();
                }
                else
                {
                    _centreIdList.Add(centreId);
                }


                //If Employee is sales individual populate only his/her name
                if (_currRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                {
                    _empList.Add(_employee);
                }
                else
                {
                    //Filter walkinn by centre and status
                    _regnList = _db.StudentRegistrations
                                 .AsEnumerable()
                                 .Where(w => (_centreIdList.Contains(w.StudentWalkInn.CenterCode.Id))
                                             && w.TransactionDate.Value.Date >= startDate.Date
                                             && w.TransactionDate.Value.Date <= endDate.Date)
                                 .ToList();

                    List<int> _cro1RegId = _regnList
                                               .Select(w => w.StudentWalkInn.Employee1.Id).ToList();

                    List<int> _cro2RegId = _regnList
                                                 .Where(w => w.StudentWalkInn.CROCount.Value == 2)
                                                 .Select(w => w.StudentWalkInn.Employee2.Id).ToList();

                    List<int> _regCroIds = _cro1RegId.Concat(_cro2RegId).Distinct().ToList();

                    _empList = _db.Employees
                                .Where(e => _regCroIds.Contains(e.Id) && e.Designation.Role.Rank >= _employee.Designation.Role.Rank)
                                .ToList();
                }
                return _empList;
            }
            catch (Exception ex)
            {
                return _empList = null;
            }
        }
        public string GetParam_CentreName(int centreId)
        {
            string _centreName = String.Empty;
            if (centreId == (int)EnumClass.SelectAll.ALL)
            {
                _centreName = EnumClass.SelectAll.ALL.ToString();
            }
            else
            {
                _centreName = _db.CenterCodes
                           .Where(c => c.Id == centreId)
                           .FirstOrDefault().CentreCode;
            }
            return _centreName;
        }
        public string GetParam_EmployeeName(int empId)
        {
            string _empName = String.Empty;
            if (empId == (int)EnumClass.SelectAll.ALL)
            {
                _empName = EnumClass.SelectAll.ALL.ToString();
            }
            else
            {
                _empName = _db.Employees
                           .Where(e => e.Id == empId)
                           .FirstOrDefault().Name;
            }
            return _empName;
        }
        public string GetCourseCategoryName(int courseCategoryId)
        {
            string _categoryName = string.Empty;
            if (courseCategoryId == (int)EnumClass.CourseCategory.FOUNDATION)
            {
                _categoryName = EnumClass.CourseCategory.FOUNDATION.ToString();
            }
            else if (courseCategoryId == (int)EnumClass.CourseCategory.DIPLOMA)
            {
                _categoryName = EnumClass.CourseCategory.DIPLOMA.ToString();
            }
            else if (courseCategoryId == (int)EnumClass.CourseCategory.PROFESSIONAL)
            {
                _categoryName = EnumClass.CourseCategory.PROFESSIONAL.ToString();
            }
            else if (courseCategoryId == (int)EnumClass.CourseCategory.MASTERDIPLOMA)
            {
                _categoryName = EnumClass.CourseCategory.MASTERDIPLOMA.ToString();
            }
            else
            {
                _categoryName = EnumClass.SelectAll.ALL.ToString();
            }
            return _categoryName;
        }
        public string GetReferenceWalkInnDetails(string _mobNo, string _email)
        {
            string _WIDetails = string.Empty;
            try
            {
                var _studentWalkInn = _db.StudentWalkInns
                                       .Where(w => w.MobileNo == _mobNo || w.EmailId == _email)
                                       .FirstOrDefault();
                if (_studentWalkInn != null)
                {
                    if (_studentWalkInn.Status == EnumClass.WalkinnStatus.WALKINN.ToString())
                    {
                        _WIDetails = EnumClass.WalkinnStatus.WALKINN.ToString() + ","
                                    + _studentWalkInn.TransactionDate.Value.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        _WIDetails = EnumClass.WalkinnStatus.REGISTERED.ToString() + ","
                                   + _studentWalkInn.StudentRegistrations.FirstOrDefault().TransactionDate.Value.ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    _WIDetails = "";
                }
            }
            catch (Exception ex)
            {
                _WIDetails = "";
            }

            return _WIDetails;

        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}