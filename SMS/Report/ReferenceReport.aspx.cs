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
    public partial class ReferenceReport : System.Web.UI.Page
    {
        public class clsReference
        {
            public DateTime WIDate { get; set; }
            public string StudentName { get; set; }

            public string Qualification { get; set; }
            public string RefName { get; set; }
            public string RefContactNo { get; set; }
            public string RefEmailId { get; set; }
            public StudentWalkInn StudentWalkInn { get; set; }
            public string CollegeName
            {
                get
                {
                    string _college_company_name = string.Empty;
                    if (StudentWalkInn.CandidateStatus == (int)EnumClass.StudentCategory.STUDENT || StudentWalkInn.CandidateStatus == (int)EnumClass.StudentCategory.NONEMPLOYED)
                    {
                        _college_company_name = StudentWalkInn.CollegeAddress;
                    }
                    else
                    {
                        _college_company_name = StudentWalkInn.CompanyAddress;
                    }
                    return _college_company_name;
                }
            }
            public string ReferenceStatus
            {
                get
                {
                    Common_Report _cmn = new Common_Report();
                    string _referenceDetails = string.Empty;
                    _referenceDetails = _cmn.GetReferenceWalkInnDetails(RefContactNo, RefEmailId);
                    return _referenceDetails;

                }
            }
            public string WIStatus
            {
                get
                {
                    string _WIStatus = string.Empty;
                    if (StudentWalkInn.Status == EnumClass.WalkinnStatus.REGISTERED.ToString())
                    {
                        _WIStatus = string.Join(",", StudentWalkInn.StudentRegistrations.FirstOrDefault()
                                                  .StudentRegistrationCourses.SelectMany(rc => rc.MultiCourse.MultiCourseDetails
                                                  .Select(mcd => mcd.Course.Name)));
                    }
                    else
                    {
                        _WIStatus = "NOT REGISTERED";
                    }
                    return _WIStatus;
                }
            }
            public string SalesPersonName
            {
                get
                {
                    string _salesPerson = string.Empty;
                    if (StudentWalkInn.CROCount == 1)
                    {
                        _salesPerson = StudentWalkInn.Employee1.Name;
                    }
                    else
                    {
                        _salesPerson = StudentWalkInn.Employee1.Name + "," + StudentWalkInn.Employee2.Name;
                    }
                    return _salesPerson;
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    Common_Report _cmnReport = new Common_Report();
                    Common _cmn = new Common();
                    DataTable _dTable = new DataTable();

                    int _centreId = Convert.ToInt32(Session["CentreId"]);
                    int _empId = Convert.ToInt32(Session["EmpId"]);
                    DateTime _fromDate = Convert.ToDateTime(Session["FromDate"]);
                    DateTime _toDate = Convert.ToDateTime(Session["ToDate"]);
                    string _finYearId = Session["FinYearId"].ToString();
                    int _loggedUserId = Convert.ToInt32(Session["LoggedUserId"]);

                    _dTable = GetReferenceList(_centreId, _empId, _fromDate, _toDate,_loggedUserId);

                    rptReferenceReportViewer.ProcessingMode = ProcessingMode.Local;
                    rptReferenceReportViewer.LocalReport.ReportPath = Server.MapPath("~/Report/ReferenceReport.rdlc");

                    List<ReportParameter> _lstReportParam = new List<ReportParameter>();
                    _lstReportParam.Add(new ReportParameter("FinYear", _finYearId));
                    _lstReportParam.Add(new ReportParameter("CentreName", _cmnReport.GetParam_CentreName(_centreId)));
                    _lstReportParam.Add(new ReportParameter("EmpName", _cmnReport.GetParam_EmployeeName(_empId)));
                    _lstReportParam.Add(new ReportParameter("FromDate", _fromDate.Date.ToString("dd/MM/yyyy")));
                    _lstReportParam.Add(new ReportParameter("ToDate", _toDate.Date.ToString("dd/MM/yyyy")));

                    ReportDataSource datasource = new ReportDataSource("dtReference", _dTable);
                    rptReferenceReportViewer.Width = Unit.Pixel(1200);
                    rptReferenceReportViewer.Height = Unit.Pixel(1300);
                    rptReferenceReportViewer.LocalReport.SetParameters(_lstReportParam);
                    if (_empId != (int)EnumClass.SelectAll.ALL)
                    {
                        int _currEmployeeRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));
                        if (_currEmployeeRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                        {
                            rptReferenceReportViewer.ShowExportControls = false;
                        }
                    }
                    rptReferenceReportViewer.LocalReport.DataSources.Clear();
                    rptReferenceReportViewer.LocalReport.DataSources.Add(datasource);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public DataTable GetReferenceList(int centreId, int empId, DateTime fromDate, DateTime toDate,int loggedUserId)
        {
            DataTable _dtReference = new DataTable();
            List<clsReference> _clsReference = new List<clsReference>();
            Common_Report _cmnReport = new Common_Report();
            List<int> _walkInnIDList = new List<int>();
            List<StudentRelation> _lstWalkInnRelation = new List<StudentRelation>();
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

                    //Get distinct walkinn id from studentrelation table
                    _lstWalkInnRelation = _db.StudentRelations
                                            .AsEnumerable()
                                            .Where(w => _centerIdList.Contains(w.StudentWalkInn.CenterCode.Id)
                                                        && w.StudentWalkInn.TransactionDate.Value.Date >= fromDate.Date
                                                        && w.StudentWalkInn.TransactionDate.Value.Date <= toDate.Date)
                                            .ToList();

                    if (empId != (int)EnumClass.SelectAll.ALL)
                    {
                        List<int> _cro1_WalkInnID_List = _lstWalkInnRelation
                                                            .Where(r => r.StudentWalkInn.CROCount == 1
                                                                && r.StudentWalkInn.Employee1.Id == empId)
                                                            .Select(r => r.Id).ToList();


                        List<int> _cro2_WalkInnID_List = _lstWalkInnRelation
                                                           .Where(r => r.StudentWalkInn.CROCount == 2
                                                                 && (r.StudentWalkInn.Employee1.Id == empId || r.StudentWalkInn.Employee2.Id == empId))
                                                           .Select(r => r.Id).ToList();
                        List<int> _final_WalkInnID_List = _cro1_WalkInnID_List.Concat(_cro2_WalkInnID_List).Distinct().ToList();

                        _lstWalkInnRelation = _lstWalkInnRelation
                                            .Where(w => _final_WalkInnID_List.Contains(w.Id))
                                            .ToList();
                    }

                    _clsReference = _lstWalkInnRelation
                                        .Select(w => new clsReference
                                        {
                                            Qualification = w.StudentWalkInn.QlfnType.Name + "," + w.StudentWalkInn.QlfnMain.Name,
                                            RefContactNo = w.MobileNo,
                                            RefEmailId = w.EmailId,
                                            RefName = w.Name,
                                            StudentName = w.StudentWalkInn.CandidateName,
                                            WIDate = w.StudentWalkInn.TransactionDate.Value,
                                            StudentWalkInn = w.StudentWalkInn
                                        })
                                    .OrderBy(w => w.WIDate)
                                    .ToList();

                    var _referenceList = _clsReference
                                       .Select(r => new
                                       {
                                           WIDate = r.WIDate,
                                           StudentName = r.StudentName,
                                           WIStatus = r.WIStatus,
                                           Qualification = r.Qualification,
                                           CollegeName = r.CollegeName,
                                           RefName = r.RefName,
                                           RefContactNo = r.RefContactNo,
                                           RefEmailId = r.RefEmailId,
                                           ReferenceStatus = r.ReferenceStatus,
                                           SalesPerson=r.SalesPersonName
                                       }).ToList();

                    _dtReference = ToDataTable(_referenceList);
                    return _dtReference;
                };

            }
            catch (Exception ex)
            {
                _dtReference = null;
            }
            return _dtReference;
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