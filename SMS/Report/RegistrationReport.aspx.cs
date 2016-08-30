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
    public partial class RegistrationReport : System.Web.UI.Page
    {
        public class clsRegistration
        {
            public DateTime RegDate { get; set; }
            public string StudentId { get; set; }
            public string StudentName { get; set; }       
            public int Discount { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public int CourseId { get; set; }
            public int EmployeeId { get; set; }
            public int CourseCategoryId { get; set; }
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
            public string SoftwareUsed
            {
                get
                {
                    string _softwareUsed = string.Empty;
                    Common _cmn=new Common();
                    if (CourseId == (int)EnumClass.SelectAll.ALL)
                    {
                        _softwareUsed = string.Join(",", StudentRegistration.StudentRegistrationCourses
                                                        .Where(r => CourseCategoryId == (int)EnumClass.SelectAll.ALL || r.MultiCourse.CourseSubTitle.CourseSeriesType.Id == CourseCategoryId)
                                                        .SelectMany(rc => rc.MultiCourse.MultiCourseDetails
                                                        .Select(mcd => mcd.Course.Name)));
                    }
                    else
                    {
                        _softwareUsed = _cmn.GetCourseName(CourseId);
                    }
                    return _softwareUsed;
                }
            }
            public string CategoryName 
            {
                get
                {
                    string _categoryName = string.Empty;
                    _categoryName = string.Join(",", StudentRegistration.StudentRegistrationCourses
                                                .Where(r => CourseCategoryId == (int)EnumClass.SelectAll.ALL || r.MultiCourse.CourseSubTitle.CourseSeriesType.Id == CourseCategoryId)
                                                .Select(src => src.MultiCourse.CourseSubTitle.Name));
                    return _categoryName;
                }
            }
            public decimal ActualFee 
            {
                get
                {
                    decimal _actualFee=0;
                    Common _cmn=new Common();
                    int _feeMode=StudentRegistration.FeeMode.Value;
                    if (CourseId == (int)EnumClass.SelectAll.ALL)
                    {
                        if(_feeMode==(int)EnumClass.InstallmentType.SINGLE)
                        {
                            _actualFee = StudentRegistration.StudentRegistrationCourses
                                        .Where(r => CourseCategoryId == (int)EnumClass.SelectAll.ALL || r.MultiCourse.CourseSubTitle.CourseSeriesType.Id == CourseCategoryId)
                                        .Sum(rc => rc.MultiCourse.SingleFee.Value);
                        }
                        else
                        {
                            _actualFee = StudentRegistration.StudentRegistrationCourses
                                        .Where(r => CourseCategoryId == (int)EnumClass.SelectAll.ALL || r.MultiCourse.CourseSubTitle.CourseSeriesType.Id == CourseCategoryId)
                                        .Sum(rc => rc.MultiCourse.InstallmentFee.Value);
                        }
                      
                    }
                    else
                    {
                        _actualFee = _cmn.GetCourseFee(CourseId, _feeMode);
                    }
                    return _actualFee;
                }
            }
            public decimal CourseFee 
            {
                get
                {
                    decimal _courseFee = 0;
                    if (CourseId == (int)EnumClass.SelectAll.ALL)
                    {
                        List<int> _lstCourseID = StudentRegistration.StudentRegistrationCourses
                                                .Where(r => CourseCategoryId == (int)EnumClass.SelectAll.ALL || r.MultiCourse.CourseSubTitle.CourseSeriesType.Id == CourseCategoryId)
                                                .SelectMany(r => r.MultiCourse.MultiCourseDetails
                                                .Select(mcd => mcd.Course.Id)).ToList();

                        _courseFee = StudentRegistration.StudentFeedbacks
                                    .Where(f => _lstCourseID.Contains(f.Course.Id))
                                    .Sum(f => f.TotalCourseAmount.Value);
                    }
                    else
                    {
                        var _studentFeedback=StudentRegistration.StudentFeedbacks
                                            .Where(f => f.CourseId == CourseId)
                                            .FirstOrDefault();
                        if(_studentFeedback!=null)
                        {
                            _courseFee = _studentFeedback.TotalCourseAmount.Value;
                        }
                        else 
                        {
                            _courseFee = 0;
                        }
                        
                    }
                    return _courseFee;
                }
            }
            public decimal CourseFee_Final 
            {
                get
                {
                    decimal _courseFee = 0;
                    int _sharePercent = 0;

                    _courseFee = CourseFee;
                    //if walkinn is not shared
                    if (StudentRegistration.StudentWalkInn.CROCount == 1 || EmployeeId == (int)EnumClass.SelectAll.ALL)
                    {
                        _courseFee = CourseFee;
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
                        _courseFee = Convert.ToDecimal((CourseFee * _sharePercent) / 100);
                    }
                    _courseFee = Math.Round(_courseFee, 2);
                    return _courseFee;
                }
            }
            public decimal ActualFee_Final
            {
                get
                {
                    decimal _actualFee = 0;
                    int _sharePercent = 0;

                    _actualFee = ActualFee;
                    //if walkinn is not shared
                    if (StudentRegistration.StudentWalkInn.CROCount == 1 || EmployeeId == (int)EnumClass.SelectAll.ALL)
                    {
                        _actualFee = ActualFee;
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
                        _actualFee = Convert.ToDecimal((ActualFee * _sharePercent) / 100);
                    }
                    _actualFee = Math.Round(_actualFee, 2);
                    return _actualFee;
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
                    int _courseCategoryId = Convert.ToInt32(Session["CourseCategoryId"]);
                    int _courseId = Convert.ToInt32(Session["CourseId"]);
                    int _loggedUserId = Convert.ToInt32(Session["LoggedUserId"]);

                    _dTable = GetRegistrationList(_centreId, _empId, _fromDate, _toDate, _courseCategoryId, _courseId,_loggedUserId);

                    rptRegistrationReportViewer.ProcessingMode = ProcessingMode.Local;
                    rptRegistrationReportViewer.LocalReport.ReportPath = Server.MapPath("~/Report/RegistrationReport.rdlc");

                    List<ReportParameter> _lstReportParam = new List<ReportParameter>();
                    _lstReportParam.Add(new ReportParameter("FinYear", _finYearId));
                    _lstReportParam.Add(new ReportParameter("CentreName", _cmnReport.GetParam_CentreName(_centreId)));
                    _lstReportParam.Add(new ReportParameter("EmpName", _cmnReport.GetParam_EmployeeName(_empId)));
                    _lstReportParam.Add(new ReportParameter("FromDate", _fromDate.Date.ToString("dd/MM/yyyy")));
                    _lstReportParam.Add(new ReportParameter("ToDate", _toDate.Date.ToString("dd/MM/yyyy")));
                    _lstReportParam.Add(new ReportParameter("CategoryName", _cmnReport.GetCourseCategoryName(_courseCategoryId)));
                    _lstReportParam.Add(new ReportParameter("CourseName", _cmn.GetCourseName(_courseId)));

                    ReportDataSource datasource = new ReportDataSource("dtRegistration", _dTable);
                    rptRegistrationReportViewer.Width = Unit.Pixel(1200);
                    rptRegistrationReportViewer.Height = Unit.Pixel(1300);
                    rptRegistrationReportViewer.LocalReport.SetParameters(_lstReportParam);
                    if (_empId != (int)EnumClass.SelectAll.ALL)
                    {
                        int _currEmployeeRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));
                        if (_currEmployeeRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                        {
                            rptRegistrationReportViewer.ShowExportControls = false;
                        }
                    }
                    rptRegistrationReportViewer.LocalReport.DataSources.Clear();
                    rptRegistrationReportViewer.LocalReport.DataSources.Add(datasource);
                }
                catch (Exception ex)
                {

                }
              
            }
        }

        public DataTable GetRegistrationList(int centreId, int empId, DateTime fromDate, DateTime toDate, int courseCategoryId, int courseId,int loggedUserId)
        {
            DataTable _dtRegistration = new DataTable();
            List<clsRegistration> _clsRegistration = new List<clsRegistration>();
            Common_Report _cmnReport = new Common_Report();
            List<int> _empIdList = new List<int>();
            List<StudentRegistration> _lstRegistration = new List<StudentRegistration>();
            List<int> _lstMultiCourseId = new List<int>();
            List<int> _lstRegnId = new List<int>();
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

                    //Get Courses corresponding to coursecategory(F/D/P/MD)
                    _lstMultiCourseId = _db.MultiCourseDetails
                                        .Where(mcd => (courseCategoryId == (int)EnumClass.SelectAll.ALL || mcd.MultiCourse.CourseSubTitle.CourseSeriesType.Id == courseCategoryId)
                                                    && (courseId == (int)EnumClass.SelectAll.ALL || mcd.Course.Id == courseId))
                                        .Select(mcd => mcd.MultiCourse.Id)
                                        .Distinct()
                                        .ToList();

                    //Get CourseId from mutlicourselist 
                    List<int> _lstCourseID = _db.MultiCourseDetails
                                            .Where(mcd => _lstMultiCourseId.Contains(mcd.MultiCourse.Id))
                                            .Select(mcd => mcd.Course.Id)
                                            .Distinct().ToList();

                    //Get RegistrationId from studentregistrationcourses filtering by multicourse,centre,fromdate and todate
                    _lstRegnId = _db.StudentRegistrationCourses
                                      .AsEnumerable()
                                      .Where(r => _centerIdList.Contains(r.StudentRegistration.StudentWalkInn.CenterCode.Id)
                                                  && (_lstMultiCourseId.Contains(r.MultiCourse.Id))
                                                  && r.StudentRegistration.TransactionDate.Value.Date >= fromDate.Date
                                                  && r.StudentRegistration.TransactionDate.Value.Date <= toDate.Date)
                                      .Select(r => r.StudentRegistration.Id)
                                      .Distinct()
                                      .ToList();

                    //Get studentregistrationlist filtering by studentregistrationid
                    _lstRegistration = _db.StudentRegistrations
                                    .Where(r => _lstRegnId.Contains(r.Id))
                                    .ToList();

                    if (empId != (int)EnumClass.SelectAll.ALL)
                    {
                        List<int> _cro1_RegnID_List = _lstRegistration
                                                            .Where(r => r.StudentWalkInn.CROCount == 1
                                                                && r.StudentWalkInn.Employee1.Id == empId)
                                                            .Select(r => r.Id).ToList();


                        List<int> _cro2_RegnID_List = _lstRegistration
                                                           .Where(r => r.StudentWalkInn.CROCount == 2
                                                                 && (r.StudentWalkInn.Employee1.Id == empId || r.StudentWalkInn.Employee2.Id == empId))
                                                           .Select(r => r.Id).ToList();
                        List<int> _final_RegnID_List = _cro1_RegnID_List.Concat(_cro2_RegnID_List).Distinct().ToList();

                        _lstRegistration = _lstRegistration
                                            .Where(r => _final_RegnID_List.Contains(r.Id))
                                            .ToList();
                    }

                    _clsRegistration = _lstRegistration
                                        .Select(r => new clsRegistration
                                        {
                                            CourseCategoryId = courseCategoryId,
                                            CourseId = courseId,
                                            EmployeeId = empId,
                                            RegDate = r.TransactionDate.Value,
                                            StudentId = r.RegistrationNumber,
                                            StudentName = r.StudentWalkInn.CandidateName,
                                            StudentRegistration = r,
                                            Discount=r.Discount.Value
                                        })
                                    .OrderBy(r => r.RegDate)
                                    .ToList();

                    var _regnList = _clsRegistration
                                       .Select(rl => new
                                       {
                                           RegDate = rl.RegDate,
                                           StudentId = rl.StudentId,
                                           StudentName = rl.StudentName,
                                           SalesPerson = rl.SalesPerson,
                                           SoftwareUsed = rl.SoftwareUsed,
                                           Category = rl.CategoryName,
                                           ActualFee = rl.ActualFee_Final,
                                           CourseFee = rl.CourseFee_Final,
                                           Discount = rl.Discount                                         
                                       }).ToList();

                    _dtRegistration = ToDataTable(_regnList);
                    return _dtRegistration;
                };

            }
            catch (Exception ex)
            {
                _dtRegistration = null;
            }
            return _dtRegistration;
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

        //returns the coursename of given courseid
       

    }
}