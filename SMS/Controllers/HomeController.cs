using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class HomeController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();

        public class clsWalkInnCountDetails
        {
            public int EmployeeRoleId { get; set; }
            public decimal WalkInnCount
            {
                get
                {
                    decimal _count = 0;
                    if (EmployeeRoleId == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (StudentWalkInn.CROCount > 1)
                        {
                            if (EmployeeId == StudentWalkInn.Employee1.Id)
                            {
                                _count = (decimal)StudentWalkInn.CRO1Percentage.Value / 100;
                            }
                            else
                            {
                                _count = (decimal)StudentWalkInn.CRO2Percentage.Value / 100;
                            }
                        }
                        else
                        {
                            _count = 1;
                        }
                    }
                    else
                    {
                        _count = 1;
                    }
                    return _count;
                }

            }
            public StudentWalkInn StudentWalkInn { get; set; }
            public int EmployeeId { get; set; }
        }

        public class clsRegCountDetails
        {
            public int EmployeeRoleId { get; set; }
            public decimal RegCount
            {
                get
                {
                    decimal _count = 0;
                    if (EmployeeRoleId == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (StudentRegistration.StudentWalkInn.CROCount > 1)
                        {
                            if (EmployeeId == StudentRegistration.StudentWalkInn.Employee1.Id)
                            {
                                _count = (decimal)StudentRegistration.StudentWalkInn.CRO1Percentage.Value / 100;
                            }
                            else
                            {
                                _count = (decimal)StudentRegistration.StudentWalkInn.CRO2Percentage.Value / 100; ;
                            }
                        }
                        else
                        {
                            _count = 1;
                        }
                    }
                    else
                    {
                        _count = 1;
                    }
                    return _count;
                }

            }
            public StudentRegistration StudentRegistration { get; set; }
            public int EmployeeId { get; set; }
        }

        public class clsCollectionAmountDetails
        {
            public int EmployeeRoleId { get; set; }
            public decimal CollectionAmount
            {
                get
                {
                    decimal _feePaid = 0;
                    int _sharePercent = 0;

                    _feePaid = FeePaid;
                    //if walkinn is not shared
                    if (EmployeeRoleId != (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        _feePaid = FeePaid;
                    }
                    //if walkinn is shared
                    else
                    {
                        if (EmployeeId == StudentRegistration.StudentWalkInn.Employee1.Id)
                        {
                            _sharePercent = StudentRegistration.StudentWalkInn.CRO1Percentage.Value;
                        }
                        else
                        {
                            _sharePercent = StudentRegistration.StudentWalkInn.CRO2Percentage.Value;
                        }
                        _feePaid = Convert.ToDecimal((FeePaid * _sharePercent) / 100);
                    }
                    _feePaid = Math.Round(_feePaid, 2);
                    return _feePaid;
                }
            }
            public StudentRegistration StudentRegistration { get; set; }
            public decimal FeePaid { get; set; }
            public int EmployeeId { get; set; }
        }
        //
        // GET: /Home/

        public ActionResult Index()
        {
            try
            {
                Common _cmn = new Common();
                var _employee = _db.Employees.AsEnumerable().Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                       .FirstOrDefault();
                Session["EmployeeName"] = _employee.Name;
                Session["EmployeeDesignation"] = _employee.Designation.DesignationName;
                Session["EmployeeJoinDate"] = GetJoinMonth(_employee.DateOfJoin);
                Session["EmployeePhotoUrl"] = _employee.PhotoUrl;

                ViewBag.WalkInnCount_Yearly = GetWalkInnCount("year");
                ViewBag.WalkInnCount_Monthly = GetWalkInnCount("month");
                ViewBag.RegCount_Yearly = GetRegistrationCount("year");
                ViewBag.RegCount_Monthly = GetRegistrationCount("month");
                ViewBag.CollectionAmount_Monthly = _cmn.ConvertNumberToComma(GetCollectionCount("month", "COLLECTION"));
                ViewBag.CollectionAmount_Yearly = _cmn.ConvertNumberToComma(GetCollectionCount("year", "COLLECTION"));
                ViewBag.GS_Monthly = _cmn.ConvertNumberToComma(GetCollectionCount("month", "GS"));
                ViewBag.GS_Yearly = _cmn.ConvertNumberToComma(GetCollectionCount("year", "GS"));
                ViewBag.DashboardRequired = IsDashboardRequried(_employee);

                //ViewBag.WalkInnCount_Yearly = 1;
                //ViewBag.WalkInnCount_Monthly = 1;
                //ViewBag.RegCount_Yearly = 1;
                //ViewBag.RegCount_Monthly = 1;
                //ViewBag.CollectionAmount_Monthly = 1;
                //ViewBag.CollectionAmount_Yearly = 1;
                //ViewBag.GS_Monthly = 1;
                //ViewBag.GS_Yearly = 1;
                //ViewBag.DashboardRequired = false;

                return View();
            }
            catch (Exception ex)
            {
                return View("Login");
            }


        }

        [ChildActionOnly]
        public ActionResult Menu()
        {
            try
            {
                var _employee = _db.Employees.AsEnumerable().Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                   .FirstOrDefault();
                //Gets the distinct parent list
                var _parentList = _employee.Designation.MenuRoles
                                                       .Select(x => x.Menu.ParentID)
                                                       .Distinct()
                                                       .ToList();
                //Get the dashboardVM
                var _dashboardVM = new DashboardVM
                {

                    MenuParentList = _db.Menus
                                     .Where(x => _parentList.Contains(x.Id))
                                     .Select(x => new SMS.Models.ViewModel.DashboardVM.MenuParent
                                     {
                                         MenuParentID = x.Id,
                                         MenuParentName = x.MenuName
                                     })
                                     .OrderBy(x => x.MenuParentID)
                                     .ToList(),
                    MenuList = _employee.Designation.MenuRoles
                                                  .Select(x => x.Menu)
                                                  .ToList()

                };

                return PartialView("_Menu", _dashboardVM);
            }
            catch (Exception ex)
            {
                return PartialView("_Menu", null);
            }

        }

        static string GetJoinMonth(DateTime? dt)
        {
            var month = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(dt.Value.Month); ;
            var year = dt.Value.Year.ToString();
            return month + ". " + year;
        }

        //Get Total walkinncount of current month and year
        public decimal GetWalkInnCount(string type)
        {
            try
            {
                Common _cmn = new Common();
                int _empId = Convert.ToInt32(Session["LoggedUserId"]);
                int _empRoleId = _db.Employees
                                .Where(e => e.Id == _empId)
                                .Select(e => e.Designation.Role.Id)
                                .FirstOrDefault();
                List<clsWalkInnCountDetails> _clsWalkInnCountDetails = new List<clsWalkInnCountDetails>();
                List<StudentWalkInn> _walkInnList = new List<StudentWalkInn>();
                List<int> _lstCentreId = _db.EmployeeCenters
                                        .Where(ec => ec.Employee.Id == _empId)
                                        .Select(ec => ec.CenterCode.Id)
                                        .ToList();

                //Get total walkinn of currentmonth
                if (type == "month")
                {
                    int _currYear = Common.LocalDateTime().Year;
                    int _currMonth = Common.LocalDateTime().Month;
                    _walkInnList = _db.StudentWalkInns
                                   .AsEnumerable()
                                   .Where(w => w.TransactionDate.Value.Date.Year == _currYear
                                                && w.TransactionDate.Value.Month == _currMonth)
                                   .ToList();

                }
                //Get total walkinn of currentyear
                else
                {
                    var _currFinYear = _cmn.FinancialYearList().ToList()
                                     .OrderByDescending(x => x.ToString())
                                     .Select(x => new
                                     {
                                         Id = x.ToString()
                                     }).First();


                    DateTime _startYear = new DateTime(Convert.ToInt32(_currFinYear.Id.Split('-').First()), 4, 1);
                    DateTime _endYear = new DateTime(Convert.ToInt32(_currFinYear.Id.Split('-').Last()), 3, 31);

                    _walkInnList = _db.StudentWalkInns
                                 .AsEnumerable()
                                 .Where(w => w.TransactionDate.Value.Date >= _startYear.Date
                                            && w.TransactionDate.Value <= _endYear.Date)
                                 .ToList();
                }

                if (_empRoleId == (int)EnumClass.Role.SALESINDIVIDUAL)
                {

                    List<int> _cro1WalkInnIdList = _walkInnList
                                                       .Where(w => w.Employee1.Id == _empId)
                                                       .Select(w => w.Id)
                                                       .ToList();

                    List<int> _cro2WalkInnIdList = _walkInnList
                                                .Where(w => w.CROCount == 2 && w.Employee2.Id == _empId)
                                                .Select(w => w.Id)
                                                .ToList();

                    List<int> _allWalkInnIdList = _cro1WalkInnIdList.Concat(_cro2WalkInnIdList)
                                                .ToList();

                    _walkInnList = _walkInnList
                                 .Where(w => _allWalkInnIdList.Contains(w.Id))
                                 .ToList();
                }
                _clsWalkInnCountDetails = _walkInnList
                                            .Where(w => _lstCentreId.Contains(w.CenterCode.Id))
                                            .Select(w => new clsWalkInnCountDetails
                                            {
                                                EmployeeId = _empId,
                                                EmployeeRoleId = _empRoleId,
                                                StudentWalkInn = w
                                            }).ToList();
                decimal _walkInnCount = _clsWalkInnCountDetails.Sum(w => w.WalkInnCount);
                return _walkInnCount;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public decimal GetRegistrationCount(string type)
        {
            try
            {
                Common _cmn = new Common();
                int _empId = Convert.ToInt32(Session["LoggedUserId"]);
                int _empRoleId = _db.Employees
                               .Where(e => e.Id == _empId)
                               .Select(e => e.Designation.Role.Id)
                               .FirstOrDefault();
                List<clsRegCountDetails> _clsRegCountDetails = new List<clsRegCountDetails>();
                List<StudentRegistration> _regList = new List<StudentRegistration>();
                List<int> _lstCentreId = _db.EmployeeCenters
                                        .Where(ec => ec.Employee.Id == _empId)
                                        .Select(ec => ec.CenterCode.Id)
                                        .ToList();

                //Get total walkinn of currentmonth
                if (type == "month")
                {
                    int _currYear = Common.LocalDateTime().Year;
                    int _currMonth = Common.LocalDateTime().Month;
                    _regList = _db.StudentRegistrations
                                   .AsEnumerable()
                                   .Where(w => w.TransactionDate.Value.Date.Year == _currYear && w.TransactionDate.Value.Month == _currMonth)
                                   .ToList();

                }
                //Get total walkinn of currentyear
                else
                {
                    var _currFinYear = _cmn.FinancialYearList().ToList()
                                     .OrderByDescending(x => x.ToString())
                                     .Select(x => new
                                     {
                                         Id = x.ToString()
                                     }).First();

                    DateTime _startYear = new DateTime(Convert.ToInt32(_currFinYear.Id.Split('-').First()), 4, 1);
                    DateTime _endYear = new DateTime(Convert.ToInt32(_currFinYear.Id.Split('-').Last()), 3, 31);

                    _regList = _db.StudentRegistrations
                                 .AsEnumerable()
                                 .Where(w => w.TransactionDate.Value.Date >= _startYear.Date && w.TransactionDate.Value <= _endYear.Date)
                                 .ToList();
                }

                if (_empRoleId == (int)EnumClass.Role.SALESINDIVIDUAL)
                {

                    List<int> _cro1WalkInnIdList = _regList
                                                       .Where(r => r.StudentWalkInn.Employee1.Id == _empId)
                                                       .Select(w => w.Id)
                                                       .ToList();

                    List<int> _cro2WalkInnIdList = _regList
                                                .Where(r => r.StudentWalkInn.CROCount == 2 && r.StudentWalkInn.Employee2.Id == _empId)
                                                .Select(w => w.Id)
                                                .ToList();

                    List<int> _allWalkInnIdList = _cro1WalkInnIdList.Concat(_cro2WalkInnIdList)
                                                .ToList();

                    _regList = _regList
                                 .Where(w => _allWalkInnIdList.Contains(w.Id))
                                 .ToList();
                }
                _clsRegCountDetails = _regList
                                            .Where(r => _lstCentreId.Contains(r.StudentWalkInn.CenterCode.Id))
                                            .Select(r => new clsRegCountDetails
                                            {
                                                EmployeeId = _empId,
                                                EmployeeRoleId = _empRoleId,
                                                StudentRegistration = r
                                            }).ToList();
                decimal _regCount = _clsRegCountDetails.Sum(w => w.RegCount);
                return _regCount;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public decimal GetCollectionCount(string type, string dashboardType)
        {

            Common _cmn = new Common();
            List<int> _empIdList = new List<int>();
            List<StudentReceipt> _studentReceiptList = new List<StudentReceipt>();
            List<int> _centerIdList = new List<int>();
            DateTime _fromDate = new DateTime();
            DateTime _toDate = new DateTime();
            decimal _totalCollectionCount = 0;
            try
            {
                int _empId = Convert.ToInt32(Session["LoggedUserId"]);
                var _dbEmployee = _db.Employees
                                     .Where(e => e.Id == _empId)
                                     .FirstOrDefault();

                _centerIdList = _dbEmployee.EmployeeCenters
                                      .Select(ec => ec.CenterCode.Id)
                                      .ToList();

                int _empRoleId = _dbEmployee.Designation.Role.Id;

                if (type == "month")
                {
                    _fromDate = new DateTime(Common.LocalDateTime().Year, Common.LocalDateTime().Month, 1);
                    _toDate = new DateTime(Common.LocalDateTime().Year, Common.LocalDateTime().Month,
                            DateTime.DaysInMonth(Common.LocalDateTime().Year, Common.LocalDateTime().Month));
                }
                else
                {
                    var _finYearList = _cmn.FinancialYearList().ToList()
                                      .OrderByDescending(x => x.ToString())
                                      .First();
                    var arrFinYear = _finYearList.Split('-');
                    var _startFinYear = Convert.ToInt32(arrFinYear[0]);
                    var _endFinYear = Convert.ToInt32(arrFinYear[1]);

                    _fromDate = new DateTime(_startFinYear, (int)EnumClass.FinYearMonth.STARTMONTH, 1).Date;
                    _toDate = new DateTime(_endFinYear, (int)EnumClass.FinYearMonth.ENDMONTH, 31).Date;


                }

                if (dashboardType == "COLLECTION")
                {
                    _studentReceiptList = _db.StudentReceipts
                                 .AsEnumerable()
                                 .Where(r => _centerIdList.Contains(r.StudentRegistration.StudentWalkInn.CenterCode.Id)
                                         && r.DueDate.Value.Date >= _fromDate.Date
                                         && r.DueDate.Value.Date <= _toDate.Date
                                         && r.Status == true)
                                 .ToList();
                }
                else
                {
                    _studentReceiptList = _db.StudentReceipts
                                .AsEnumerable()
                                .Where(r => _centerIdList.Contains(r.StudentRegistration.StudentWalkInn.CenterCode.Id)
                                        && r.StudentRegistration.TransactionDate.Value.Date >= _fromDate.Date
                                        && r.StudentRegistration.TransactionDate.Value.Date <= _toDate.Date)
                                .ToList();
                }


                if (_empRoleId == (int)EnumClass.Role.SALESINDIVIDUAL)
                {
                    List<int> _cro1CollectionIdList = _studentReceiptList
                                                        .Where(r => r.StudentRegistration.StudentWalkInn.Employee1.Id == _empId)
                                                        .Select(r => r.Id).ToList();
                    List<int> _cro2CollectionIdList = _studentReceiptList
                                                       .Where(r => r.StudentRegistration.StudentWalkInn.CROCount == 2
                                                             && (r.StudentRegistration.StudentWalkInn.Employee2.Id == _empId))
                                                       .Select(r => r.Id).ToList();
                    List<int> _finalCollectionIdList = _cro1CollectionIdList.Concat(_cro2CollectionIdList).ToList();

                    _studentReceiptList = _studentReceiptList
                                        .Where(r => _finalCollectionIdList.Contains(r.Id))
                                        .ToList();
                }

                var _receiptList = _studentReceiptList
                                .Select(r => new clsCollectionAmountDetails
                                {
                                    EmployeeId = _empId,
                                    StudentRegistration = r.StudentRegistration,
                                    EmployeeRoleId = _empRoleId,
                                    FeePaid = r.Fee.Value
                                })
                                .ToList();

                _totalCollectionCount = _receiptList
                                        .Sum(r => r.CollectionAmount);

            }
            catch (Exception ex)
            {
                _totalCollectionCount = 0;
            }
            return _totalCollectionCount;
        }

        public bool IsDashboardRequried(Employee employee)
        {
            try
            {
                List<int> _dashboardReqdRoleList = new List<int>();
                _dashboardReqdRoleList.Add((int)EnumClass.Role.ED);
                _dashboardReqdRoleList.Add((int)EnumClass.Role.CENTERMANAGER);
                _dashboardReqdRoleList.Add((int)EnumClass.Role.MANAGER);
                _dashboardReqdRoleList.Add((int)EnumClass.Role.SALESINDIVIDUAL);

                if (_dashboardReqdRoleList.Contains(employee.Designation.Role.Id))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
