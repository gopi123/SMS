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
    public partial class WalkInnReport : System.Web.UI.Page
    {
        // dbSMSNSEntities _db = new dbSMSNSEntities();
        public class clsWalkInn
        {
            public DateTime WIDate { get; set; }
            public string CentreName { get; set; }
            public string SalesPerson { get; set; }
            public string StudentName { get; set; }
            public string MobileNo { get; set; }
            public string ParentNo { get; set; }
            public string CourseRecommended { get; set; }
            public string Status { get; set; }
            public string Qualification { get; set; }
            public int WalkInnId { get; set; }
            public string ExpJoinDate
            {
                get
                {
                    if (StudentWalkInn.Id == 4406)
                    {

                    }
                    //WalkInn Students
                    if (StudentWalkInn.Status.ToLower() == EnumClass.WalkinnStatus.WALKINN.ToString().ToLower())
                    {
                        if (StudentWalkInn.JoinStatus == true)
                        {
                            return StudentWalkInn.JoinDate.Value.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            return "NOT JOIN";
                        }
                    }
                    //Registered Students
                    else
                    {
                        return StudentWalkInn.StudentRegistrations.FirstOrDefault().TransactionDate.Value.ToString("dd/MM/yyyy");
                    }
                }
            }
            public StudentWalkInn StudentWalkInn { get; set; }
        }
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
                _dTable = GetWalkInnList(_centreId, _categoryType, _empId, _fromDate, _toDate, _loggedUserId);
               

                ReportViewer1.ProcessingMode = ProcessingMode.Local;
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Report/WalkInnReport.rdlc");

                List<ReportParameter> _lstReportParam = new List<ReportParameter>();
                _lstReportParam.Add(new ReportParameter("FinancialYear", _finYearId));
                _lstReportParam.Add(new ReportParameter("Centre", _cmnReport.GetParam_CentreName(_centreId)));
                _lstReportParam.Add(new ReportParameter("Employee", _cmnReport.GetParam_EmployeeName(_empId)));
                _lstReportParam.Add(new ReportParameter("FromDate", _fromDate.Date.ToString("dd/MM/yyyy")));
                _lstReportParam.Add(new ReportParameter("ToDate", _toDate.Date.ToString("dd/MM/yyyy")));
                _lstReportParam.Add(new ReportParameter("CategoryType", _categoryType));

                ReportDataSource datasource = new ReportDataSource("dtWalkInn", _dTable);
                ReportViewer1.Width = Unit.Pixel(1250);
                ReportViewer1.Height = Unit.Pixel(1000);
                ReportViewer1.LocalReport.SetParameters(_lstReportParam);
                if (_empId != (int)EnumClass.SelectAll.ALL)
                {
                    int _currEmployeeRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));
                    if (_currEmployeeRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        ReportViewer1.ShowExportControls = false;
                    }
                }
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources.Add(datasource);
                ReportViewer1.LocalReport.Refresh();
            }
        }

        public DataTable GetWalkInnList(int centreId, string categoryType, int empId, DateTime fromDate, DateTime toDate, int loggedUserId)
        {
            DataTable _dtWalkInn = new DataTable();
            List<clsWalkInn> _clsWalkInn = new List<clsWalkInn>();
            Common_Report _cmnReport = new Common_Report();
            List<int> _empIdList = new List<int>();
            List<StudentWalkInn> _walkInnList = new List<StudentWalkInn>();
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

                    _walkInnList = _db.StudentWalkInns
                                    .AsEnumerable()
                                    .Where(w => _centerIdList.Contains(w.CenterCode.Id)
                                                && (categoryType.ToLower() == EnumClass.WalkinnStatus.ALL.ToString().ToLower() ||
                                                    w.Status.ToLower() == categoryType.ToLower())
                                                && w.TransactionDate.Value.Date >= fromDate.Date
                                                && w.TransactionDate.Value.Date <= toDate.Date)
                                    .ToList();

                    if (empId != (int)EnumClass.SelectAll.ALL)
                    {
                        List<int> _cro1WalkInnIdList = _walkInnList
                                                        .Where(w => w.CROCount == 1 && w.Employee1.Id == empId)
                                                        .Select(w => w.Id)
                                                        .ToList();

                        List<int> _cro2WalkInnIdList = _walkInnList
                                                    .Where(w => w.CROCount == 2 && (w.Employee1.Id == empId || w.Employee2.Id == empId))
                                                    .Select(w => w.Id)
                                                    .ToList();

                        List<int> _allWalkInnIdList = _cro1WalkInnIdList.Concat(_cro2WalkInnIdList)
                                                    .ToList();

                        _walkInnList = _walkInnList
                                     .Where(w => _allWalkInnIdList.Contains(w.Id))
                                     .ToList();
                    }


                    _clsWalkInn = _walkInnList
                                   .AsEnumerable()
                                   .Select(w => new clsWalkInn
                                   {
                                       CentreName = w.CenterCode.CentreCode,
                                       CourseRecommended = string.Join(",", w.StudentWalkInnCourses
                                                                       .Select(c => c.Course.Name)),
                                       MobileNo = w.MobileNo,
                                       ParentNo = w.GuardianContactNo != null ? w.GuardianContactNo : "NOT GIVEN",
                                       Qualification = w.QlfnMain.Name + "," + w.QlfnSub.Name,
                                       SalesPerson = w.CROCount == 1 ? w.Employee1.Name : w.Employee1.Name + "," + w.Employee2.Name,
                                       Status = w.Status,
                                       StudentName = w.CandidateName,
                                       WIDate = w.TransactionDate.Value,
                                       WalkInnId = w.Id,
                                       StudentWalkInn = w
                                   }).ToList();

                    var _walkInnLists = _clsWalkInn
                                        .Select(w => new
                                        {
                                            CentreName = w.CentreName,
                                            CourseRecommended = w.CourseRecommended,
                                            MobileNo = w.MobileNo,
                                            ParentNo = w.ParentNo,
                                            Qualification = w.Qualification,
                                            SalesPerson = w.SalesPerson,
                                            Status = w.Status,
                                            StudentName = w.StudentName,
                                            WIDate = w.WIDate,                                         
                                            ExpJoinDate = w.ExpJoinDate
                                        })
                                        .OrderBy(w => w.WIDate)
                                        .ToList();


                    _dtWalkInn = ToDataTable(_walkInnLists);
                    return _dtWalkInn;
                };

            }
            catch (Exception ex)
            {
                _dtWalkInn = null;
            }
            return _dtWalkInn;
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

        //protected override void Dispose(bool disposing)
        //{
        //    _db.Dispose();            
        //    base.Dispose(disposing);
        //}
    }
}