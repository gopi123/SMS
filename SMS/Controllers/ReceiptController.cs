using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
//using Rotativa;
//using NumberText;
using System.IO;
//using Rotativa.Options;
using iTextSharp.text.pdf;
using Microsoft.Reporting.WebForms;
using System.ComponentModel;

namespace SMS.Controllers
{
    public class ReceiptController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();

        #region Class Definition
        // GET: /Receipt/
        public class ReceiptPaidDetails
        {           
            public int StudentRegId { get; set; }
            public string Status { get; set; }         
            public string PdfName { get; set; }

        }

        public class clsReceipt
        {
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public string CourseTitle { get; set; }
            public string Duration { get; set; }
            public string PaymentType { get; set; }
            public string PaymentMode { get; set; }
            public string CroName { get; set; }
            public string StudentMobileNo { get; set; }
            public string StudentEmail { get; set; }
            public string CentreCode { get; set; }
            public string STRegnNo { get; set; }
            public string CentreCodeAddress { get; set; }
            public decimal PaymentFeeDetails { get; set; }
            public decimal PaymentSTAmount { get; set; }
            public double PaymentSTPercentage { get; set; }
            public decimal PaymentAmountDetails { get; set; }
            public DateTime PaymentDateDetails { get; set; }
            public string ReceiptNoDetails { get; set; }
            public string CentreCodePhoneNo { get; set; }
            public List<StudentReceipt> StudentReceipt { get; set; }
            public DateTime ReceiptDate
            {
                get
                {
                    DateTime _receiptDate = StudentReceipt.Where(r => r.Status == true)
                                          .Last().DueDate.Value.Date;
                    return _receiptDate;
                }
            }
            public int ReceiptNo
            {
                get
                {
                    int _receiptNo = 0;
                    _receiptNo = StudentReceipt.Where(r => r.Status == true)
                                .Last().StudentReceiptNo.Value;
                    return _receiptNo;
                }
            }
            public int CourseFee
            {
                get
                {
                    int _courseFee = 0;
                    _courseFee = StudentReceipt.Where(r => r.Status == true)
                               .Last().Fee.Value;
                    return _courseFee;
                }
            }
            public decimal ServiceTax
            {
                get
                {
                    decimal _serviceTax = 0;
                    _serviceTax = StudentReceipt.Where(r => r.Status == true)
                               .Last().ST.Value;
                    return _serviceTax;
                }
            }
            public decimal TotalAmount
            {
                get
                {
                    decimal _totalAmount = 0;
                    _totalAmount = StudentReceipt.Where(r => r.Status == true)
                               .Last().Total.Value;
                    return _totalAmount;
                }
            }
            public string TotalAmountInWords
            {
                get
                {
                    Common _cmn = new Common();
                    string _strTotalAmount = string.Empty;
                    int _totalAmount = StudentReceipt.Where(r => r.Status == true)
                                        .Last().Total.Value;
                    _strTotalAmount = _cmn.NumbersToWords(_totalAmount);
                    return _strTotalAmount;

                }
            }
            public decimal BalancePayable
            {
                get
                {
                    decimal _balanceAmt = 0;
                    int _count = 0;
                    _count = StudentReceipt.Where(r => r.Status == false).Count();
                    if (_count > 0)
                    {
                        _balanceAmt = StudentReceipt.Where(r => r.Status == false)
                                    .Sum(r => r.Fee.Value);
                    }
                    else
                    {
                        _balanceAmt = 0;
                    }
                    return _balanceAmt;
                }
            }
            public string CrossCheckedBy
            {
                get
                {
                    string _empName = string.Empty;
                    _empName = StudentReceipt.Where(r => r.Status == true)
                                .Last().Employee.Name;
                    return _empName;
                }
            }
            public string CourseCode { get; set; }


        }

        public class clsReceipt_Print
        {
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public string STRegNo { get; set; }
            public string RecNo { get; set; }
            public string RecDate { get; set; }
            public string CourseFee { get; set; }
            public string STPercentage { get; set; }
            public string STAmount { get; set; }
            public string TotalAmount { get; set; }
        }
        #endregion

        public ActionResult Add(int RegId, bool Redirect)
        {
            try
            {
                Common _cmn = new Common();
                bool _showData = true;

                if (CalculateSTChange(RegId))
                {
                    var _dbRegn = _db.StudentRegistrations
                           .Where(r => r.Id == RegId)
                           .FirstOrDefault();


                    //Gets the current role of employee
                    var _currentRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));

                    //if concerned cro is the logged user show mobile otherwise not
                    if (_currentRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (_dbRegn.StudentWalkInn.CRO1ID != Convert.ToInt32(Session["LoggedUserId"].ToString())
                            || _dbRegn.StudentWalkInn.CRO2ID != Convert.ToInt32(Session["LoggedUserId"].ToString()))
                        {
                            _showData = false;
                        }
                    }

                    if (_dbRegn != null)
                    {
                        var _paymentList = from EnumClass.PaymentMode b in Enum.GetValues(typeof(EnumClass.PaymentMode))
                                           select new { Id = (int)b, Name = b.ToString() };

                        var _mdlReceipt = new ReceiptVM()
                        {
                            CourseCode = String.Join(",", _dbRegn.StudentRegistrationCourses
                                                         .Select(rc => rc.MultiCourse.CourseCode)),
                            CourseTitle = String.Join(",", _dbRegn.StudentRegistrationCourses
                                                         .Select(rc => rc.MultiCourse.CourseSubTitle.Name)),
                            SoftwareUsed = String.Join(",", _dbRegn.StudentRegistrationCourses
                                                        .SelectMany(rc => rc.MultiCourse.MultiCourseDetails
                                                        .Select(c => c.Course.Name))),
                            FeeMode = _dbRegn.FeeMode.Value == (int)EnumClass.InstallmentType.SINGLE ? EnumClass.InstallmentType.SINGLE.ToString() : EnumClass.InstallmentType.INSTALLMENT.ToString(),
                            StudentRegistration = _dbRegn,
                            StudentReceipt = _dbRegn.StudentReceipts.ToList(),
                            PaymentModeID = (int)EnumClass.PaymentMode.CASH,
                            PaymentModeList = new SelectList(_paymentList, "Id", "Name"),
                            IsEmailValidated = _dbRegn.IsEmailVerified.Value == true ? "Verified" : "NotVerified",
                            Email = _showData == true ? _dbRegn.StudentWalkInn.EmailId : _cmn.MaskString(_dbRegn.StudentWalkInn.EmailId, "email"),
                            MobileNo = _showData == true ? _dbRegn.StudentWalkInn.MobileNo : _cmn.MaskString(_dbRegn.StudentWalkInn.MobileNo, "mobile"),
                        };

                        ViewBag.StepChanged = Redirect;


                        return View(_mdlReceipt);

                    }
                }
                return RedirectToAction("Index", "Registration");

            }
            catch (Exception ex)
            {
                return View("Error");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Receipt_Add(ReceiptVM mdlReceiptVM)
        {
            ReceiptPaidDetails receiptPaidDetails = new ReceiptPaidDetails();
            try
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                Common _cmn = new Common();

                using (TransactionScope _ts = new TransactionScope())
                {
                    var _startDate = _cmn.GetFinancialYear().StartDate;
                    var _endDate = _cmn.GetFinancialYear().EndDate;
                    //Gets the receipt details
                    var _receiptId = mdlReceiptVM.StudentReceipt.First(r => r.Status == false).Id;
                    var _receiptDetails = _db.StudentReceipts
                                           .Where(r => r.Id == _receiptId)
                                           .FirstOrDefault();

                    if (_receiptDetails != null)
                    {
                        //Gets the centrecode details
                        var _centreCodeId = _receiptDetails.StudentRegistration.StudentWalkInn.CenterCodeId.Value;

                        var _lastGeneratedRecDetails = _db.StudentReceipts
                                                .Where(r => r.Status == true
                                                         && r.StudentRegistration.StudentWalkInn.CenterCode.Id == _centreCodeId
                                                         && EntityFunctions.TruncateTime(r.DueDate.Value) >= _startDate.Date
                                                         && EntityFunctions.TruncateTime(r.DueDate.Value) <= _endDate.Date)
                                                .AsEnumerable()
                                                .OrderByDescending(x => x.StudentReceiptNo)
                                                .FirstOrDefault();


                        _receiptDetails.DueDate = Common.LocalDateTime();
                        _receiptDetails.Status = true;
                        _receiptDetails.ModeOfPayment = mdlReceiptVM.PaymentModeID;
                        _receiptDetails.BankName = mdlReceiptVM.PaymentModeID == (int)EnumClass.PaymentMode.CHEQUE ? mdlReceiptVM.BankName.ToUpper() : null;
                        _receiptDetails.BranchName = mdlReceiptVM.PaymentModeID == (int)EnumClass.PaymentMode.CHEQUE ? mdlReceiptVM.BranchName.ToUpper() : null;
                        _receiptDetails.ChDate = mdlReceiptVM.PaymentModeID == (int)EnumClass.PaymentMode.CHEQUE ? mdlReceiptVM.ChequeDate : (DateTime?)null;
                        _receiptDetails.ChNo = mdlReceiptVM.PaymentModeID == (int)EnumClass.PaymentMode.CHEQUE ? mdlReceiptVM.ChequeNo.ToUpper() : null;
                        _receiptDetails.CROID = Convert.ToInt32(Session["LoggedUserId"]);

                        _db.Entry(_receiptDetails).State = EntityState.Modified;
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            int l = _cmn.AddTransactions(actionName, controllerName, "");
                            if (l > 0)
                            {
                                _ts.Complete();
                                receiptPaidDetails.StudentRegId = mdlReceiptVM.StudentRegistration.Id;
                                receiptPaidDetails.Status = "success";
                                return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }
                    receiptPaidDetails.Status = "error";
                    return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                receiptPaidDetails.Status = ex.Message;               
                return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);
            }



        }

        //SMS and Email the receipt details to the student        
        public ActionResult Generate_Receipt_Email_SMS(int studentRegId)
        {
            ReceiptPaidDetails _rcptPaidDetails = new ReceiptPaidDetails();
            try
            {
                var _dbRegn = _db.StudentRegistrations
                       .Where(r => r.Id == studentRegId)
                       .FirstOrDefault();

                var _receiptNo = _dbRegn.StudentReceipts
                                .Where(r => r.Status == true)
                                .Last()
                                .StudentReceiptNo.Value;

                //Receipt is generated and saved into receipts folder
                var _isReceiptGenerated = GenerateReceipts(studentRegId);
                //Receipt for print in generated and saved into receipts_print folder
                var _isReceiptForPrintGenerated = GenerateReceiptForPrint(studentRegId);
                //Email sending for student
                var isEmailSend = SendMail(_dbRegn.Id);
                //var isEmailSend = true;
                //SMS sending to student and official
                var isSMSSend = SendSMS(studentRegId);

                if (_isReceiptGenerated)
                {
                    if (_isReceiptForPrintGenerated)
                    {
                        if (isEmailSend)
                        {
                            if (isSMSSend)
                            {

                                _rcptPaidDetails.Status = "success";
                                _rcptPaidDetails.PdfName = _dbRegn.RegistrationNumber + "_" + Common.GetReceiptNo(_receiptNo) + ".pdf";
                            }
                            else
                            {
                                _rcptPaidDetails.Status = "error_sms";
                            }
                        }
                        else
                        {
                            _rcptPaidDetails.Status = "error_email";
                        }

                    }
                    else
                    {
                        _rcptPaidDetails.Status = "error_receipt_forprint";
                    }
                }
                else
                {
                    _rcptPaidDetails.Status = "error_receipt";
                }
            }
            catch (Exception ex)
            {
                _rcptPaidDetails.Status = ex.Message;
            }

            return Json(_rcptPaidDetails, JsonRequestBehavior.AllowGet);
        }

        //This function generates the pdf
        public void PdfRequest(int regId)
        {
            try
            {
                Common _cmn = new Common();

                var _studRegn = _db.StudentRegistrations
                                .Where(r => r.Id == regId)
                                .FirstOrDefault();

                var _fileName = _studRegn.RegistrationNumber + "_" + Common.GetReceiptNo(_studRegn.StudentReceipts.Where(r => r.Status == true).Last().StudentReceiptNo.Value) + ".pdf";

                Response.ContentType = "Application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=Receipt.pdf");
                Response.TransmitFile(Server.MapPath("~/Receipt/" + _fileName));
                Response.End();

            }
            catch (Exception ex)
            {
                RedirectToAction("Add", new { RegId = regId, Redirect = true });
            }

        }

        public void PrintPdfRequest(string pdfName)
        {
            try
            {
                Common _cmn = new Common();

                Response.ContentType = "Application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=Receipt.pdf");
                Response.TransmitFile(Server.MapPath("~/Receipt_Print/" + pdfName));
                Response.End();

            }
            catch (Exception ex)
            {

            }

        }

        public bool GenerateReceipts(int studRegId)
        {
            try
            {
                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                DataTable _dTable = new DataTable();
                _dTable = GetReceiptDetails(studRegId);

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = Server.MapPath("~/Report/Receipt.rdlc"); ; //This is your rdlc name.   

                viewer.LocalReport.EnableExternalImages = true;
                string imagePath = new Uri(Server.MapPath("~/Styles/ReceiptImages/EReceipt-logo.jpg")).AbsoluteUri;
                string nsLogoPath = new Uri(Server.MapPath("~/Styles/ReceiptImages/NSlogo.jpg")).AbsoluteUri;
                ReportParameter parameter1 = new ReportParameter("ReceiptLogoPath", imagePath);
                ReportParameter parameter2 = new ReportParameter("NSLogoPath", nsLogoPath);
                viewer.LocalReport.SetParameters(parameter1);
                viewer.LocalReport.SetParameters(parameter2);

                ReportDataSource datasource = new ReportDataSource("dtReceipt", _dTable);
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(datasource);

                //FileName form=>StudentID_ReceiptNo
                var _fileName = Server.MapPath("~/Receipt/" + _dTable.Rows[0]["StudentID"].ToString() + "_"
                              + _dTable.Rows[0]["ReceiptNo"].ToString());

                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                using (FileStream stream = new FileStream(_fileName + ".pdf", FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool GenerateReceiptForPrint(int receiptID)
        {
            try
            {
                Warning[] warnings;
                string[] streamIds;
                string mimeType = string.Empty;
                string encoding = string.Empty;
                string extension = string.Empty;

                DataTable _dTable = new DataTable();
                _dTable = GetReceiptTable(receiptID);

                ReportViewer viewer = new ReportViewer();
                viewer.ProcessingMode = ProcessingMode.Local;
                viewer.LocalReport.ReportPath = Server.MapPath("~/Report/Receipt_Print.rdlc"); ; //This is your rdlc name.   

                viewer.LocalReport.EnableExternalImages = true;
                string nsLogoPath = new Uri(Server.MapPath("~/Styles/ReceiptImages/NSlogo.jpg")).AbsoluteUri;
                ReportParameter parameter2 = new ReportParameter("NSLogoPath", nsLogoPath);
                viewer.LocalReport.SetParameters(parameter2);

                ReportDataSource datasource = new ReportDataSource("dtReceipt_Print", _dTable);
                viewer.LocalReport.DataSources.Clear();
                viewer.LocalReport.DataSources.Add(datasource);

                //FileName form=>StudentID_ReceiptNo
                var _fileName = Server.MapPath("~/Receipt_Print/" + _dTable.Rows[0]["StudentID"].ToString() + "_"
                              + _dTable.Rows[0]["RecNo"].ToString());

                byte[] bytes = viewer.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamIds, out warnings);
                using (FileStream stream = new FileStream(_fileName + ".pdf", FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public DataTable GetReceiptDetails(int studentRegId)
        {
            List<clsReceipt> _clsReceipt = new List<clsReceipt>();
            DataTable _dtReceipt = new DataTable();
            Common _cmn = new Common();
            try
            {
                using (var _db = new dbSMSNSEntities())
                {
                    _clsReceipt = _db.StudentReceipts
                                .Where(r => r.StudentRegistration.Id == studentRegId)
                                .AsEnumerable()
                                .Select(r => new clsReceipt()
                                {
                                    CentreCode = r.StudentRegistration.StudentWalkInn.CenterCode.CentreCode,
                                    CentreCodeAddress = r.StudentRegistration.StudentWalkInn.CenterCode.Address.ToUpper(),
                                    CentreCodePhoneNo = r.StudentRegistration.StudentWalkInn.CenterCode.PhoneNo,
                                    CourseTitle = string.Join(",", r.StudentRegistration.StudentRegistrationCourses
                                                                .Select(sc => sc.MultiCourse.CourseSubTitle.Name)),
                                    CroName = r.StudentRegistration.StudentWalkInn.CROCount == 1 ? r.StudentRegistration.StudentWalkInn.Employee1.Name :
                                            r.StudentRegistration.StudentWalkInn.Employee1.Name + "," + r.StudentRegistration.StudentWalkInn.Employee2.Name,
                                    Duration = r.StudentRegistration.TotalDuration.ToString(),
                                    PaymentFeeDetails = r.Fee.Value,
                                    PaymentSTAmount = r.ST.Value,
                                    PaymentSTPercentage = r.STPercentage.Value,
                                    PaymentAmountDetails = r.Total.Value,
                                    PaymentDateDetails = r.DueDate.Value.Date,
                                    PaymentMode = r.StudentRegistration.FeeMode.Value == (int)EnumClass.InstallmentType.SINGLE ?
                                                  EnumClass.InstallmentType.SINGLE.ToString() : EnumClass.InstallmentType.INSTALLMENT.ToString(),
                                    PaymentType = "CASH",
                                    ReceiptNoDetails = r.StudentReceiptNo.ToString(),
                                    STRegnNo = r.StudentRegistration.StudentWalkInn.CenterCode.STRegNo,
                                    StudentEmail = _cmn.MaskString(r.StudentRegistration.StudentWalkInn.EmailId, "email"),
                                    StudentMobileNo = _cmn.MaskString(r.StudentRegistration.StudentWalkInn.MobileNo, "mobile"),
                                    StudentID = r.StudentRegistration.RegistrationNumber,
                                    StudentName = r.StudentRegistration.StudentWalkInn.CandidateName.ToUpper(),
                                    StudentReceipt = r.StudentRegistration.StudentReceipts.ToList(),
                                    CourseCode = string.Join(",", r.StudentRegistration.StudentRegistrationCourses
                                                                .Select(sc => sc.MultiCourse.CourseCode))
                                }).ToList();

                    var _clsReceipts = _clsReceipt
                                     .Select(r => new
                                     {
                                         ReceiptDate = r.ReceiptDate,
                                         ReceiptNo = Common.GetReceiptNo(r.ReceiptNo),
                                         StudentID = r.StudentID,
                                         StudentName = r.StudentName,
                                         CourseTitle = r.CourseTitle,
                                         Duration = r.Duration,
                                         PaymentType = r.PaymentType,
                                         PaymentMode = r.PaymentMode,
                                         CourseFee = r.CourseFee,
                                         ServiceTax = r.ServiceTax,
                                         TotalAmount = r.TotalAmount,
                                         TotalAmountInWords = r.TotalAmountInWords.ToString().ToUpper(),
                                         BalancePayable = r.BalancePayable,
                                         CroName = r.CroName,
                                         CrossCheckedBy = r.CrossCheckedBy,
                                         StudentMobileNo = r.StudentMobileNo,
                                         StudentEmail = r.StudentEmail,
                                         CentreCode = r.CentreCode,
                                         STRegnNo = r.STRegnNo,
                                         CentreCodeAddress = r.CentreCodeAddress.Replace(",", ", "),
                                         CentreCodePhoneNo = r.CentreCodePhoneNo,
                                         PaymentAmountDetails = r.PaymentAmountDetails,
                                         PaymentDateDetails = r.PaymentDateDetails,
                                         ReceiptNoDetails = r.ReceiptNoDetails,
                                         CourseCode = r.CourseCode,
                                         PaymentFeeDetails = r.PaymentFeeDetails,
                                         PaymentSTAmount = r.PaymentSTAmount,
                                         PaymentSTPercentage = r.PaymentSTPercentage

                                     }).ToList();
                    _dtReceipt = ToDataTable(_clsReceipts);

                }
            }
            catch (Exception ex)
            {
                _dtReceipt = null;
            }
            return _dtReceipt;
        }

        public DataTable GetReceiptTable(int studRegId)
        {
            List<clsReceipt_Print> _clsReceipt_Print = new List<clsReceipt_Print>();
            DataTable _dtReceipt = new DataTable();
            Common _cmn = new Common();
            try
            {
                using (var _db = new dbSMSNSEntities())
                {
                    var _receiptId = _db.StudentReceipts
                                   .Where(r => r.StudentRegistration.Id == studRegId && r.Status == true)
                                   .OrderByDescending(r => r.Id)
                                   .FirstOrDefault().Id;

                    _clsReceipt_Print = _db.StudentReceipts
                                      .Where(r => r.Id == _receiptId)
                                      .AsEnumerable()
                                      .Select(r => new clsReceipt_Print
                                      {
                                          CourseFee = _cmn.ConvertNumberToComma(r.Fee.Value),
                                          RecDate = r.DueDate.Value.ToString("dd/MM/yyyy"),
                                          RecNo = Common.GetReceiptNo(r.StudentReceiptNo),
                                          STAmount = _cmn.ConvertNumberToComma(r.ST.Value),
                                          STPercentage = r.STPercentage.ToString(),
                                          STRegNo = r.StudentRegistration.StudentWalkInn.CenterCode.STRegNo,
                                          StudentID = r.StudentRegistration.RegistrationNumber,
                                          StudentName = r.StudentRegistration.StudentWalkInn.CandidateName,
                                          TotalAmount = _cmn.ConvertNumberToComma(r.Total.Value)
                                      }).ToList();


                    _dtReceipt = ToDataTable(_clsReceipt_Print);

                }
            }
            catch (Exception ex)
            {
                _dtReceipt = null;
            }
            return _dtReceipt;
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

        public void DuplicatePdfReceipt(int receiptId)
        {
            try
            {               

                //var _studentReceipt = _db.StudentReceipts
                //                    .Where(r => r.Id == receiptId)
                //                    .FirstOrDefault();

                //var _fileName = _studentReceipt.StudentRegistration.RegistrationNumber + "_" + Common.GetReceiptNo(_studentReceipt.StudentReceiptNo.Value) + ".pdf";

                //Response.ContentType = "Application/pdf";
                //Response.AppendHeader("Content-Disposition", "attachment; filename=Receipt.pdf");
                //Response.TransmitFile(Server.MapPath("~/Receipt/" + _fileName));
                //Response.End();

                Common _cmn = new Common();
                byte[] _content = _cmn.MergeReceiptPdf(receiptId);
                Response.Clear();
                MemoryStream ms = new MemoryStream(_content);
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=Receipt.pdf");
                Response.Buffer = true;
                ms.WriteTo(Response.OutputStream);
                Response.End();
            }

            catch (Exception ex)
            {

            }
        }

        //send verification mail to regtistered student
        public bool SendMail(int studentRegId)
        {
            try
            {
                Common _cmn = new Common();
                var _studRegistraion = _db.StudentRegistrations
                                    .Where(r => r.Id == studentRegId).FirstOrDefault();
                if (_studRegistraion != null)
                {
                    var _latestReceiptDetails = _studRegistraion.StudentReceipts
                                                .Last(r => r.Status == true);
                    var studEmailId = _studRegistraion.StudentWalkInn.EmailId;
                    var _fileName = "Receipt/" + _latestReceiptDetails.StudentRegistration.RegistrationNumber + "_" + Common.GetReceiptNo(_latestReceiptDetails.StudentReceiptNo.Value) + ".pdf";
                    var _filePath = Path.Combine(HttpRuntime.AppDomainAppPath, _fileName);
                    byte[] _content = _cmn.MergeReceiptPdf(_latestReceiptDetails.Id);
                    string _subject = "Receipt";
                    string _body = "Please find the Receipt Attachment";
                    //Email sending
                    var isMailSend = _cmn.SendReceiptEmail(studEmailId, _body, _subject, true, _content);
                    return isMailSend;
                }
                return false;
            }
            catch
            {
                return false;
            }




        }



        //sendng SMS to student+guardian+officials
        public bool SendSMS(int studRegId)
        {
            try
            {

                Common _cmn = new Common();
                bool result = false;
                var _studRegistraion = _db.StudentRegistrations
                                    .Where(r => r.Id == studRegId).FirstOrDefault();

                var _latestReceiptDetails = _studRegistraion.StudentReceipts
                                          .Last(r => r.Status == true);


                if (_studRegistraion != null)
                {
                    string _cro1MobNo = string.Empty;
                    string _cro2MobNo = string.Empty;
                    string _feePaidCroMobNo = string.Empty;
                    int _croCount = 0;
                    var _receiptNo = Common.GetReceiptNo(_latestReceiptDetails.StudentReceiptNo.Value);
                    var _receiptDate = _latestReceiptDetails.DueDate.Value.ToString("dd/MM/yyyy");
                    var _studRegNo = _studRegistraion.RegistrationNumber;
                    var _studName = _studRegistraion.StudentWalkInn.CandidateName;
                    var _croName = _studRegistraion.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ? _studRegistraion.StudentWalkInn.Employee1.Name
                                                                                                      : _studRegistraion.StudentWalkInn.Employee1.Name + ',' + _studRegistraion.StudentWalkInn.Employee2.Name;
                    var _courseCode = string.Join(",", _studRegistraion.StudentRegistrationCourses
                                                    .Select(x => x.MultiCourse.CourseCode));
                    var _currPaid = _latestReceiptDetails.Total.Value;
                    var _sumPaid = _studRegistraion.StudentReceipts
                                 .Where(r => r.Status == true).Sum(r => r.Total);
                    var _balanceamount = _studRegistraion.TotalAmount - _sumPaid;
                    var _nextDueDetails = _studRegistraion.StudentReceipts
                                           .Where(r => r.Status == false)
                                           .FirstOrDefault();
                    var _nextDueDate = _nextDueDetails != null ? _nextDueDetails.DueDate.Value.ToString("dd/MM/yyyy") : "FULL PAID";



                    //Gets the designationids of employees to whom sms has to be send
                    List<int> _desgnList = GetEmpDesignationList(_studRegistraion.StudentWalkInn.CenterCodeId.Value);
                    //Gets the mobile no of all the employees
                    List<string> _lstMobNos = _cmn.GetEmployeeDesignationWise(_desgnList)
                                                    .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo).ToList();

                    _croCount = _studRegistraion.StudentWalkInn.CROCount.Value;

                    //if cro count is one
                    if (_croCount == (int)EnumClass.CROCount.ONE)
                    {
                        //if fee paid cro and walkinn cro are same
                        if (_latestReceiptDetails.CROID == _studRegistraion.StudentWalkInn.CRO1ID)
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                        }
                        //adding fee paid cro mobileno        
                        else
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                            _feePaidCroMobNo = _latestReceiptDetails.Employee.OfficialMobileNo != null ? _latestReceiptDetails.Employee.OfficialMobileNo :
                                                                                                   _latestReceiptDetails.Employee.MobileNo;
                        }
                    }
                    //if cro count is two
                    else
                    {
                        //adding cro1 and cro2 mobileno
                        if ((_latestReceiptDetails.CROID == _studRegistraion.StudentWalkInn.CRO1ID) || (_latestReceiptDetails.CROID == _studRegistraion.StudentWalkInn.CRO2ID))
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                            _cro2MobNo = _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee2.MobileNo;
                        }
                        //adding cro1+cro2+fee paid cro mobileno 
                        else
                        {
                            _cro1MobNo = _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee1.OfficialMobileNo :
                                                                                                       _studRegistraion.StudentWalkInn.Employee1.MobileNo;
                            _cro2MobNo = _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo != null ? _studRegistraion.StudentWalkInn.Employee2.OfficialMobileNo :
                                                                                                        _studRegistraion.StudentWalkInn.Employee2.MobileNo;
                            _feePaidCroMobNo = _latestReceiptDetails.Employee.OfficialMobileNo != null ? _latestReceiptDetails.Employee.OfficialMobileNo :
                                                                                                   _latestReceiptDetails.Employee.MobileNo;
                        }
                    }

                    //Adding student + guardian mobile no
                    string _stuMobNo = _studRegistraion.StudentWalkInn.MobileNo;
                    //string _guardianMobNo = _studRegistraion.StudentWalkInn.GuardianContactNo;

                    _lstMobNos.Add(_cro1MobNo);
                    _lstMobNos.Add(_cro2MobNo);
                    _lstMobNos.Add(_feePaidCroMobNo);
                    _lstMobNos.Add(_stuMobNo);
                    //_lstMobNos.Add(_guardianMobNo);

                    string _mobNos = string.Join(",", _lstMobNos.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList());

                    //sending message to student
                    var _message = "Rec.No:" + _receiptNo + ", " + _receiptDate + ", " + _studRegNo + ", " + _studName + ", Cro:" + _croName + ", " + _courseCode + ", Paid:Rs." + _currPaid + " including ST, BAL:Rs." +
                                    _balanceamount + ", NextDue:" + _nextDueDate;

                    string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + _mobNos + "&msg=" + _message + "&route=T");
                    if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                    {
                        result = true;

                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        //Gets the designationids of employees to whom sms has to be send
        public List<int> GetEmpDesignationList(int centreID)
        {
            List<int> _designationList = new List<int>();
            //gets the designation of current employee of a particular center(center manager)
            int _currentCMdesignationList = _db.CenterCodes
                                            .Where(c => c.Id == centreID && c.Status == true)
                                            .Select(e => e.Employee.Designation.Id).FirstOrDefault();
            //Adding current offical employee
            _designationList.Add(_currentCMdesignationList);

            //Adding ED+Manager+SalesIndividual
            _designationList.Add((int)EnumClass.Designation.ED);
            _designationList.Add((int)EnumClass.Designation.MANAGERSALES);


            return _designationList;
        }

        public bool CalculateSTChange(int registrationId)
        {
            try
            {
                //Get current student registration details
                var _dbRegistration = _db.StudentRegistrations
                                    .Where(r => r.Id == registrationId)
                                    .FirstOrDefault();
                if (_dbRegistration != null)
                {
                    //Get current tax percentage
                    var _currTaxPercentage = _db.ServiceTaxes
                                           .AsEnumerable()
                                           .Where(t => t.FromDate <= Common.LocalDateTime())
                                           .OrderByDescending(t => t.FromDate)
                                           .First().Percentage;
                    //if ST has been modified
                    if (_dbRegistration.STPercentage != Convert.ToDouble(_currTaxPercentage))
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            //Get payment not completed receipt
                            var _nonPaidReceipt = _dbRegistration.StudentReceipts
                                                          .Where(r => r.Status == false)
                                                          .ToList();
                            //if student has pending payment update the amount with current st
                            if (_nonPaidReceipt.Count > 0)
                            {
                                //Modifying receipt details
                                foreach (var _receipt in _nonPaidReceipt)
                                {
                                    var _currCourseFee = _receipt.Fee;
                                    var _currSTPercent = _currTaxPercentage;
                                    var _currSTAmount = Convert.ToInt32(_currCourseFee * (_currSTPercent / 100));
                                    var _currTotalAmount = _currCourseFee + _currSTAmount;
                                    _receipt.STPercentage = Convert.ToDouble(_currSTPercent);
                                    _receipt.ST = _currSTAmount;
                                    _receipt.Total = _currTotalAmount;
                                    _dbRegistration.StudentReceipts.Add(_receipt);
                                }

                                //Modifying registration details
                                _dbRegistration.STPercentage = Convert.ToDouble(_currTaxPercentage);
                                _dbRegistration.TotalAmount = _dbRegistration.StudentReceipts
                                                             .Sum(r => r.Total);
                                _dbRegistration.TotalSTAmount = _dbRegistration.StudentReceipts
                                                              .Sum(r => r.ST);

                                _db.Entry(_dbRegistration).State = EntityState.Modified;
                                int i = _db.SaveChanges();
                                if (i > 0)
                                {
                                    _ts.Complete();
                                    return true;
                                }

                            }
                        }

                    }
                    return true;
                }
                return false;
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
