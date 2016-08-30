using Microsoft.Reporting.WebForms;
using SMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SMS.Report
{
    public class clsCollectionReport
    {
        public DateTime RecDate { get; set; }
        public int RecNo { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string SoftwareUsed { get; set; }
        public decimal FeePaid { get; set; }
        public decimal STAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public StudentRegistration StudentRegistration { get; set; }
        public List<StudentReceipt> StudentReceipt { get; set; }
        public int EmployeeId { get; set; }
        public decimal ST { get; set; }
        public string Status
        {
            get
            {
                if (RecNo == StudentReceipt.Last().StudentReceiptNo)
                {
                    return "FULL PAID";
                }
                else
                {
                    return "PENDING";
                }
            }

        }
        public string SalesPerson
        {
            get
            {
                string _salesPerson = string.Empty;
                // if walkinn is not shared by employees return sales employee name
                if (StudentRegistration.StudentWalkInn.CROCount == 1)
                {
                    _salesPerson = StudentRegistration.StudentWalkInn.Employee1.Name;
                }
                else
                {
                    //if walkinn is shared and is filtering by all
                    if (EmployeeId == (int)EnumClass.SelectAll.ALL)
                    {
                        _salesPerson = StudentRegistration.StudentWalkInn.Employee1.Name + "," +
                                       StudentRegistration.StudentWalkInn.Employee2.Name;
                    }
                    //if walkinn is shared and is filtering by employeeid
                    else
                    {
                        if (EmployeeId == StudentRegistration.StudentWalkInn.Employee1.Id)
                        {
                            _salesPerson = StudentRegistration.StudentWalkInn.Employee1.Name;
                        }
                        else
                        {
                            _salesPerson = StudentRegistration.StudentWalkInn.Employee2.Name;
                        }
                    }
                }
                return _salesPerson;
            }
        }
        public decimal FeePaid_Final
        {
            get
            {
                decimal _feePaid = 0;
                int _sharePercent = 0;
               
                _feePaid = FeePaid;
                //if walkinn is not shared
                if (StudentRegistration.StudentWalkInn.CROCount == 1 || EmployeeId == (int)EnumClass.SelectAll.ALL)
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
        public decimal STAmount_Final
        {
            get
            {
                decimal _stAmount = 0;
                int _sharePercent = 0;
                //if walkinn is not shared
                if (StudentRegistration.StudentWalkInn.CROCount == 1 || EmployeeId == (int)EnumClass.SelectAll.ALL)
                {
                    _stAmount = STAmount;
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
                    _stAmount = Convert.ToDecimal((STAmount * _sharePercent) / 100);
                }
                _stAmount = Math.Round(_stAmount, 2);
                return _stAmount;
            }
        }
        public decimal TotalAmount_Final
        {
            get
            {
                decimal _totalAmount = 0;
                int _sharePercent = 0;
                //if walkinn is not shared
                if (StudentRegistration.StudentWalkInn.CROCount == 1 || EmployeeId == (int)EnumClass.SelectAll.ALL)
                {
                    _totalAmount = TotalAmount;
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
                    _totalAmount = Convert.ToDecimal((TotalAmount * _sharePercent) / 100);
                }
                _totalAmount = Math.Round(_totalAmount, 2);
                return _totalAmount;
            }
        }
        public int Discount { get; set; }
    }
    public partial class CollectionReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int _centreId = Convert.ToInt32(Session["CentreId"]);
                string _categoryType = Session["CategoryType"].ToString();
                int _empId = Convert.ToInt32(Session["EmpId"]);
                DateTime _fromDate = Convert.ToDateTime(Session["FromDate"]);
                DateTime _toDate = Convert.ToDateTime(Session["ToDate"]);
                string _finYearId = Session["FinYearId"].ToString();
                int _loggedUserId = Convert.ToInt32(Session["LoggedUserId"]);
                Common_Report _cmnReport = new Common_Report();
                Common _cmn = new Common();

                DataTable _dTable = new DataTable();
                _dTable = GetCollectionList(_centreId, _empId, _fromDate, _toDate,_loggedUserId);

                rptCollectionReportViewer.ProcessingMode = ProcessingMode.Local;
                rptCollectionReportViewer.LocalReport.ReportPath = Server.MapPath("~/Report/CollectionReport.rdlc");

                List<ReportParameter> _lstReportParam = new List<ReportParameter>();
                _lstReportParam.Add(new ReportParameter("FinancialYear", _finYearId));
                _lstReportParam.Add(new ReportParameter("Centre", _cmnReport.GetParam_CentreName(_centreId)));
                _lstReportParam.Add(new ReportParameter("EmployeeName", _cmnReport.GetParam_EmployeeName(_empId)));
                _lstReportParam.Add(new ReportParameter("FromDate", _fromDate.Date.ToString("dd/MM/yyyy")));
                _lstReportParam.Add(new ReportParameter("ToDate", _toDate.Date.ToString("dd/MM/yyyy")));                

                ReportDataSource datasource = new ReportDataSource("dtCollection", _dTable);
                rptCollectionReportViewer.Width = Unit.Pixel(1100);
                rptCollectionReportViewer.Height = Unit.Pixel(1300);
                rptCollectionReportViewer.LocalReport.SetParameters(_lstReportParam);
                if (_empId != (int)EnumClass.SelectAll.ALL)
                {
                    int _currEmployeeRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));
                    if (_currEmployeeRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        rptCollectionReportViewer.ShowExportControls = false;
                    }
                }
                rptCollectionReportViewer.LocalReport.DataSources.Clear();
                rptCollectionReportViewer.LocalReport.DataSources.Add(datasource);
            }
        }

        public DataTable GetCollectionList(int centreId, int empId, DateTime fromDate, DateTime toDate,int loggedUserId)
        {
            DataTable _dtCollection = new DataTable();
            List<clsCollectionReport> _clsWalkInn = new List<clsCollectionReport>();
            Common_Report _cmnReport = new Common_Report();
            List<int> _empIdList = new List<int>();
            List<StudentReceipt> _studentReceiptList = new List<StudentReceipt>();
            List<clsCollectionReport> _receiptList = new List<clsCollectionReport>();
            List<int> _centerIdList = new List<int>();
            try
            {

                using (var _db = new dbSMSNSEntities())
                {
                    //Gets the logged employee center details
                    var _dbEmployee = _db.Employees
                                    .Where(e => e.Id == loggedUserId)
                                    .FirstOrDefault();

                    if (centreId == (int)EnumClass.SelectAll.ALL)
                    {
                        _centerIdList = _dbEmployee.EmployeeCenters
                                         .Select(ec => ec.CenterCode.Id)
                                         .ToList();
                    }
                    else
                    {
                        _centerIdList.Add(centreId);
                    }

                    if (empId != (int)EnumClass.SelectAll.ALL)
                    {
                        _empIdList.Add(empId);
                    }
                    //_empIdList = _cmnReport.GetWalkInn_Employees_Centrewise(centreId, categoryType, fromDate, toDate)
                    //              .Select(e => e.Id).ToList();
                    _studentReceiptList = _db.StudentReceipts
                                        .AsEnumerable()
                                        .Where(r => _centerIdList.Contains(r.StudentRegistration.StudentWalkInn.CenterCode.Id)
                                                && r.DueDate.Value.Date >= fromDate.Date
                                                && r.DueDate.Value.Date <= toDate.Date
                                                && r.Status == true)
                                        .ToList();

                    if (empId != (int)EnumClass.SelectAll.ALL)
                    {
                        List<int> _cro1CollectionIdList = _studentReceiptList
                                                            .Where(r => r.StudentRegistration.StudentWalkInn.CROCount == 1
                                                                && r.StudentRegistration.StudentWalkInn.Employee1.Id == empId)
                                                            .Select(r => r.Id).ToList();
                        List<int> _cro2CollectionIdList = _studentReceiptList
                                                           .Where(r => r.StudentRegistration.StudentWalkInn.CROCount == 2
                                                                 && (r.StudentRegistration.StudentWalkInn.Employee1.Id == empId || r.StudentRegistration.StudentWalkInn.Employee2.Id == empId))
                                                           .Select(r => r.Id).ToList();
                        List<int> _finalCollectionIdList = _cro1CollectionIdList.Concat(_cro2CollectionIdList).ToList();

                        _studentReceiptList = _studentReceiptList
                                            .Where(r => _finalCollectionIdList.Contains(r.Id))
                                            .ToList();
                    }

                    _receiptList = _studentReceiptList
                                    .Select(r => new clsCollectionReport
                                    {
                                        EmployeeId = empId,
                                        RecDate = r.DueDate.Value,
                                        RecNo = r.StudentReceiptNo.Value,
                                        SoftwareUsed = string.Join(",", r.StudentRegistration.StudentRegistrationCourses
                                                                        .SelectMany(rc => rc.MultiCourse.MultiCourseDetails
                                                                        .Select(c => c.Course.Name))),
                                        StudentReceipt = r.StudentRegistration.StudentReceipts.ToList(),
                                        StudentId = r.StudentRegistration.RegistrationNumber,
                                        StudentRegistration = r.StudentRegistration,
                                        FeePaid = Convert.ToDecimal(r.Fee.Value),
                                        ST = Convert.ToDecimal(r.STPercentage),
                                        STAmount = Convert.ToDecimal(r.ST),
                                        TotalAmount = Convert.ToDecimal(r.Total),
                                        StudentName = r.StudentRegistration.StudentWalkInn.CandidateName,
                                        Discount=r.StudentRegistration.Discount.Value
                                    })
                                    .OrderByDescending(r => r.RecNo)
                                    .ToList();

                    var _receiptLists = _receiptList
                                       .Select(rl => new
                                       {
                                           RecDate = rl.RecDate,
                                           RecNo = Common.GetReceiptNo(rl.RecNo),
                                           StudentId = rl.StudentId,
                                           StudentName = rl.StudentName,
                                           SalesPerson = rl.SalesPerson,
                                           SoftwareUsed = rl.SoftwareUsed,
                                           FeePaid = rl.FeePaid_Final,
                                           ST = rl.STAmount_Final,
                                           Total = rl.TotalAmount_Final,
                                           Status = rl.Status,
                                           Discount=rl.Discount                                           
                                       }).ToList();

                    _dtCollection = ToDataTable(_receiptLists);
                    return _dtCollection;
                };

            }
            catch (Exception ex)
            {
                _dtCollection = null;
            }
            return _dtCollection;
        }

        public DataTable ToDataTable<T>(IList<T> data)// T is any generic type
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}