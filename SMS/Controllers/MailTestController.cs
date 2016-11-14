
using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using iTextSharp.tool.xml;
using System.Transactions;
using System.Data.Objects;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.ComponentModel;
using System.Globalization;
using DotNetIntegrationKit;


namespace SMS.Controllers
{
    public class MailTestController : Controller
    {
        #region classDefinition
        public class clsReceipt
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

        public class SkippingMonth
        {
            public int Fee { get; set; }
            public int ST { get; set; }
            public int Amount { get; set; }
            public string DueDate { get; set; }
            public string RecNo { get; set; }
            public bool Status { get; set; }

        }

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

        public class ReceiptPaidDetails
        {
            public string StudentRegNo { get; set; }
            public string StudentName { get; set; }
            public string ReceiptDate { get; set; }
            public string ReceiptNo { get; set; }
            public int ReceiptAmount { get; set; }
            public int StudentRegId { get; set; }
            public string Status { get; set; }
            public string ExceptionMsg { get; set; }

        }

        #endregion


        dbSMSNSEntities _db = new dbSMSNSEntities();



        public void generatePassword()
        {
            Common _cmn = new Common();
            string value = _cmn.ComputePassword("abcd1234");
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
        public string UpdateFeedback(int regId)
        {
            try
            {
                var _studentReg = _db.StudentRegistrations
                          .Where(r => r.Id == regId)
                          .FirstOrDefault();
                var _totalCourseFee = _studentReg.TotalCourseFee.Value;
                var _currDiscount = _studentReg.Discount.Value;
                int _totalCourseAmt = 0;
                var i = 0;

                foreach (var feedback in _studentReg.StudentFeedbacks)
                {
                    i++;
                    if (i != _studentReg.StudentFeedbacks.Count())
                    {
                        var _course = feedback.Course;
                        var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                        var _currCourseAmt = _studentReg.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? _course.SingleFee : _course.InstallmentFee;
                        var _currTotalAmt = _currDiscount != 0 ? Convert.ToInt32(_currCourseAmt * _currDiscPercentage) : _currCourseAmt;
                        _totalCourseAmt = Convert.ToInt32(_totalCourseAmt + _currTotalAmt);

                        feedback.TotalCourseAmount = Convert.ToInt32(_currTotalAmt);
                    }
                    else
                    {
                        feedback.TotalCourseAmount = _totalCourseFee - _totalCourseAmt;
                    }

                }
                _db.Entry(_studentReg).State = EntityState.Modified;
                int j = _db.SaveChanges();
                if (j > 0)
                {
                    return "success";
                }
                else
                {
                    return "failed";
                }


            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public void Amount(int regId)
        {
            try
            {
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regId)
                            .FirstOrDefault();

                var _receiptList = _dbRegn.StudentReceipts.ToList();

                var _groupbymonth_receiptList = _receiptList
                                              .GroupBy(g => g.DueDate.Value.ToString("yyyy.MM"))
                                              .Select(r => new
                                              {
                                                  readMonth = r.Key,
                                                  Amount = r.Sum(x => x.Total)
                                              });
            }
            catch (Exception ex)
            {

            }
        }

        //Calculates if there is any difference b/w feedbackcourseamount and totalreceiptfeeamount studentwise
        public string AmountCheck()
        {
            try
            {
                var startDate = new DateTime(2016, 04, 01);
                var endDate = new DateTime(2017, 03, 31);
                var returnString = "";
                var regList = _db.StudentRegistrations
                                    .AsEnumerable()
                                   .Where(r => r.TransactionDate.Value.Date >= startDate.Date && r.TransactionDate.Value.Date <= endDate.Date)
                                   .ToList();
                foreach (var item in regList)
                {
                    var receiptAmount = item.StudentReceipts.Sum(r => r.Fee.Value);
                    var feedbackAmount = item.StudentFeedbacks.Sum(f => f.TotalCourseAmount.Value);

                    if (receiptAmount != feedbackAmount)
                    {
                        returnString = returnString + "," + item.Id + "," + item.RegistrationNumber;
                    }
                }
                return returnString;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        public ActionResult Print()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Print(int receiptID)
        {
            ReceiptPaidDetails receiptPaidDetails = new ReceiptPaidDetails();
            try
            {
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                Common _cmn = new Common();
                using (TransactionScope _ts = new TransactionScope())
                {
                    //Gets the receipt details
                    //var _receiptId = mdlReceiptVM.StudentReceipt.First(r => r.Status == false).Id;
                    var _receiptDetails = _db.StudentReceipts
                                           .Where(r => r.Id == receiptID)
                                           .FirstOrDefault();


                    if (_receiptDetails != null)
                    {
                        //Gets the centrecode details
                        var _centreCodeId = _receiptDetails.StudentRegistration.StudentWalkInn.CenterCodeId.Value;
                        //Gets the unique receiptno
                        var _receiptNo = GetReceiptNo(_centreCodeId);
                        //if receiptno exists
                        if (_receiptNo != null)
                        {

                            //_receiptDetails.ReceiptNo = _receiptNo;
                            _receiptDetails.DueDate = Common.LocalDateTime();
                            _receiptDetails.Status = true;
                            _receiptDetails.ModeOfPayment = 1;
                            _receiptDetails.BankName = null;
                            _receiptDetails.BranchName = null;
                            _receiptDetails.ChDate = (DateTime?)null;
                            _receiptDetails.ChNo = null;
                            _receiptDetails.CROID = Convert.ToInt32(Session["LoggedUserId"]);

                            _db.Entry(_receiptDetails).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {
                                //Receipt is generated and saved into receipts folder
                                var _isPrintReceiptGenerated = printPDF(receiptID);
                                if (_isPrintReceiptGenerated)
                                {
                                    int l = _cmn.AddTransactions(actionName, controllerName, "");
                                    if (l > 0)
                                    {
                                        receiptPaidDetails.ReceiptAmount = _receiptDetails.Total.Value;
                                        receiptPaidDetails.ReceiptDate = Common.LocalDateTime().Date.ToString("dd/MM/yyyy");
                                        receiptPaidDetails.ReceiptNo = _receiptNo;
                                        receiptPaidDetails.StudentRegNo = _receiptDetails.StudentRegistration.RegistrationNumber;
                                        receiptPaidDetails.StudentName = _receiptDetails.StudentRegistration.StudentWalkInn.CandidateName;
                                        receiptPaidDetails.StudentRegId = _receiptDetails.StudentRegistration.Id;
                                        receiptPaidDetails.Status = "success";
                                        _ts.Complete();
                                        return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);

                                    }

                                }
                                else
                                {
                                    receiptPaidDetails.Status = "receipterror";
                                    return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);
                                }

                            }
                        }
                    }
                    receiptPaidDetails.Status = "error";
                    return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                receiptPaidDetails.Status = "exception";
                receiptPaidDetails.ExceptionMsg = ex.Message;
                return Json(receiptPaidDetails, JsonRequestBehavior.AllowGet);
            }

        }

        public bool printPDF(int receiptID)
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

        public DataTable GetReceiptTable(int receiptID)
        {
            List<clsReceipt> _clsReceipt = new List<clsReceipt>();
            DataTable _dtReceipt = new DataTable();
            Common _cmn = new Common();
            try
            {
                using (var _db = new dbSMSNSEntities())
                {
                    _clsReceipt = _db.StudentReceipts
                                      .Where(r => r.Id == receiptID)
                                      .AsEnumerable()
                                      .Select(r => new clsReceipt
                                      {
                                          CourseFee = _cmn.ConvertNumberToComma(r.Fee.Value),
                                          RecDate = r.DueDate.Value.ToString("dd/MM/yyyy"),
                                          RecNo = Common.GetReceiptNo(r.StudentReceiptNo.Value),
                                          STAmount = _cmn.ConvertNumberToComma(r.ST.Value),
                                          STPercentage = r.STPercentage.ToString(),
                                          STRegNo = r.StudentRegistration.StudentWalkInn.CenterCode.STRegNo,
                                          StudentID = r.StudentRegistration.RegistrationNumber,
                                          StudentName = r.StudentRegistration.StudentWalkInn.CandidateName,
                                          TotalAmount = _cmn.ConvertNumberToComma(r.Total.Value)
                                      }).ToList();


                    _dtReceipt = ToDataTable(_clsReceipt);

                }
            }
            catch (Exception ex)
            {
                _dtReceipt = null;
            }
            return _dtReceipt;
        }

        private string GetReceiptNo(int centreCodeId)
        {
            int _length = 3;
            int _currSerialNo = 0;
            string _threeDigitSlNo = null;
            Common _cmn = new Common();
            var _startDate = _cmn.GetFinancialYear().StartDate;
            var _endDate = _cmn.GetFinancialYear().EndDate;

            try
            {

                _startDate = _cmn.GetFinancialYear().StartDate;
                _endDate = _cmn.GetFinancialYear().EndDate;

                var _lastGeneratedRecDetails = _db.StudentReceipts
                                                .Where(r => r.Status == true
                                                         && r.StudentRegistration.StudentWalkInn.CenterCode.Id == centreCodeId
                                                         && EntityFunctions.TruncateTime(r.DueDate.Value) >= _startDate.Date
                                                         && EntityFunctions.TruncateTime(r.DueDate.Value) <= _endDate.Date)
                                                .AsEnumerable()
                                                .OrderByDescending(x => x.StudentReceiptNo)
                                                .FirstOrDefault();
                //if receipt is generated with in the current financial year
                if (_lastGeneratedRecDetails != null)
                {
                    _currSerialNo = Convert.ToInt32(_lastGeneratedRecDetails.StudentReceiptNo) + 1;
                }
                //increment the serial no
                else
                {
                    _currSerialNo = _currSerialNo + 1;
                }

                _threeDigitSlNo = _currSerialNo.ToString().PadLeft(_length, '0');
                return _threeDigitSlNo;

                //var _receiptSerialNo = _db.StudentReceiptSerialNoes
                //                       .Where(s => (s.CenterCodeID.Value == centreCodeId) &&
                //                                   EntityFunctions.TruncateTime(s.ReceiptDate.Value) >= _startDate.Date &&
                //                                   EntityFunctions.TruncateTime(s.ReceiptDate.Value) <= _endDate.Date)
                //                       .FirstOrDefault();
                //if (_receiptSerialNo != null)
                //{
                //    _currSerialNo = _receiptSerialNo.SerialNo.Value + 1;

                //}
                ////financialyear expired updating the date,resetting serialno     
                //else
                //{
                //    _receiptSerialNo = _db.StudentReceiptSerialNoes
                //                       .Where(s => (s.CenterCodeID.Value == centreCodeId))
                //                       .FirstOrDefault();

                //    _currSerialNo = _currSerialNo + 1;
                //    _receiptSerialNo.SerialNo = 0;
                //    _receiptSerialNo.ReceiptDate = Common.LocalDateTime();
                //    _db.Entry(_receiptSerialNo).State = EntityState.Modified;
                //    _db.SaveChanges();

                //}
                ////gets the three digit serialnumber
                //_threeDigitSlNo = _currSerialNo.ToString().PadLeft(_length, '0');
                //return _threeDigitSlNo;

            }
            catch (Exception ex)
            {
                return _threeDigitSlNo;
            }
        }


        public string GetReceiptNoss(int centreCodeId)
        {
            int _length = 3;
            int _currSerialNo = 0;
            string _threeDigitSlNo = null;
            Common _cmn = new Common();
            var _startDate = new DateTime(2016, 06, 01);
            var _endDate = new DateTime(2016, 06, 30);

            try
            {



                var _lastGeneratedRecDetails = _db.StudentReceipts
                                                .Where(r => r.Status == true
                                                         && r.StudentRegistration.StudentWalkInn.CenterCode.Id == centreCodeId
                                                         && EntityFunctions.TruncateTime(r.DueDate.Value) >= _startDate.Date
                                                         && EntityFunctions.TruncateTime(r.DueDate.Value) <= _endDate.Date)
                                                .AsEnumerable()
                                                .OrderByDescending(x => x.StudentReceiptNo)
                                                .Select(x => x.StudentReceiptNo).ToList();

                return string.Join(",", _lastGeneratedRecDetails);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GenerateCertificate(int registrationId)
        {
            try
            {
                string _startDate = string.Empty;
                string _endDate = string.Empty;

                var _dbRegistration = _db.StudentRegistrations
                                    .Where(r => r.Id == registrationId)
                                    .FirstOrDefault();

                var _nsDistrict = _dbRegistration.StudentWalkInn.CenterCode.Address.Split(',').Last().ToUpper();

                var _studentFeedbackCount = _dbRegistration.StudentFeedbacks.Count();

                _startDate = _dbRegistration
                              .StudentFeedbacks
                              .OrderBy(f => f.CourseStartDate)
                              .First()
                              .CourseStartDate.Value.ToString("dd/MM/yyyy");

                _endDate = _dbRegistration
                         .StudentFeedbacks
                         .OrderByDescending(f => f.CourseEndDate)
                         .First()
                         .CourseEndDate.Value.ToString("dd/MM/yyyy");

                //Certificate template file
                string formFile = Server.MapPath("~/Template/CertificateTemplate.pdf");
                //Place where generated certificate is to be stored
                string newFile = Server.MapPath("~/Certificates/" + _dbRegistration.RegistrationNumber + ".pdf");

                //Loading the candidate image
                string imagePath = String.Empty;
                imagePath = Server.MapPath(_dbRegistration.PhotoUrl);

                using (Document document = new Document())
                {
                    using (PdfSmartCopy copy = new PdfSmartCopy(document, new FileStream(newFile, FileMode.Create)))
                    {
                        document.Open();
                        foreach (var registrationCourse in _dbRegistration.StudentRegistrationCourses)
                        {
                            PdfReader reader = new PdfReader(formFile);
                            using (var ms = new MemoryStream())
                            {
                                using (PdfStamper stamper = new PdfStamper(reader, ms))
                                {
                                    var pdfContentBuffer = stamper.GetOverContent(1);
                                    AcroFields fields = stamper.AcroFields;
                                    fields.SetField("Awarded to", _dbRegistration.StudentWalkInn.CandidateName.ToUpper());
                                    fields.SetField("In", registrationCourse.MultiCourse.CourseSubTitle.Name);
                                    fields.SetField("At", "NETWORKZ SYSTEMS, " + _nsDistrict + ", INDIA");
                                    fields.SetField("During", _startDate + " To " + _endDate);
                                    fields.SetField("Duration", _dbRegistration.TotalDuration + " Hrs");
                                    fields.SetField("Issued On", Common.LocalDateTime().ToString("dd/MM/yyyy"));
                                    fields.SetField("CourseName", string.Join(",", registrationCourse.MultiCourse.MultiCourseDetails
                                                                               .Select(mc => mc.Course.Name)));
                                    fields.SetField("ID", _dbRegistration.RegistrationNumber);
                                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imagePath);
                                    img.SetAbsolutePosition(50, 370);
                                    img.ScaleToFit(100, 100);
                                    pdfContentBuffer.AddImage(img, true);
                                    stamper.FormFlattening = true;
                                }
                                reader = new PdfReader(ms.ToArray());
                                copy.AddPage(copy.GetImportedPage(reader, 1));
                            }
                        }
                    }
                }
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public string ChangeReceiptDate()
        {
            try
            {
                var _startDate = new DateTime(2016, 4, 1);
                var _endDate = new DateTime(2017, 3, 31);
                var _receiptList = _db.StudentReceipts
                             .Where(r => r.StudentRegistration.StudentWalkInn.CandidateName.StartsWith("test")
                             && r.DueDate.Value >= _startDate
                             && r.DueDate.Value <= _endDate)
                             .ToList();
                var _returnString = string.Empty;
                foreach (var _receipt in _receiptList)
                {
                    var _receiptDate = new DateTime(2015, 01, 01);
                    _receipt.DueDate = _receiptDate;
                    _db.Entry(_receipt).State = EntityState.Modified;
                    int i = _db.SaveChanges();
                    if (i > 0)
                    {
                        _returnString = _returnString == string.Empty ? "success-" + _receipt.Id.ToString() : _returnString + "," + "success-" + _receipt.Id.ToString();
                    }
                    else
                    {
                        _returnString = _returnString == string.Empty ? "error-" + _receipt.Id.ToString() : _returnString + "," + "error-" + _receipt.Id.ToString();
                    }
                }
                return _returnString;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public string ChangeRegistrationDate()
        {
            try
            {
                var _startDate = new DateTime(2016, 4, 1);
                var _endDate = new DateTime(2017, 3, 31);
                var _regList = _db.StudentRegistrations
                             .Where(r => r.StudentWalkInn.CandidateName.StartsWith("test")
                              && r.TransactionDate.Value >= _startDate
                             && r.TransactionDate.Value <= _endDate)
                             .ToList();
                var _returnString = string.Empty;
                foreach (var _reg in _regList)
                {
                    var _regDate = new DateTime(2015, 01, 01);
                    _reg.TransactionDate = _regDate;
                    _db.Entry(_reg).State = EntityState.Modified;
                    int i = _db.SaveChanges();
                    if (i > 0)
                    {
                        _returnString = _returnString == string.Empty ? "success-" + _reg.Id.ToString() : _returnString + "," + "success-" + _reg.Id.ToString();
                    }
                    else
                    {
                        _returnString = _returnString == string.Empty ? "error-" + _reg.Id.ToString() : _returnString + "," + "error-" + _reg.Id.ToString();
                    }
                }
                return _returnString;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public string ChangeWalkinnDate()
        {
            try
            {
                var _walkInnList = _db.StudentWalkInns
                             .Where(r => r.CandidateName.StartsWith("test"))
                             .ToList();
                var _returnString = string.Empty;
                foreach (var _walkinn in _walkInnList)
                {
                    var _walkInnDate = new DateTime(2015, 01, 01);
                    _walkinn.TransactionDate = _walkInnDate;
                    _db.Entry(_walkinn).State = EntityState.Modified;
                    int i = _db.SaveChanges();
                    if (i > 0)
                    {
                        _returnString = _returnString == string.Empty ? "success-" + _walkinn.Id.ToString() : _returnString + "," + "success-" + _walkinn.Id.ToString();
                    }
                    else
                    {
                        _returnString = _returnString == string.Empty ? "error-" + _walkinn.Id.ToString() : _returnString + "," + "error-" + _walkinn.Id.ToString();
                    }
                }
                return _returnString;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }


        public string ChangeReceiptNo(int centreCodeId)
        {

            try
            {
                var _startDate = new DateTime(2016, 4, 1);
                var _endDate = new DateTime(2017, 3, 31);
                var _returnString = string.Empty;

                var _dbReceiptList = _db.StudentReceipts
                                    .Where(r => r.StudentRegistration.StudentWalkInn.CenterCode.Id == centreCodeId &&
                                    r.DueDate.Value >= _startDate &&
                                    r.DueDate.Value <= _endDate &&
                                    r.Status == true)
                                    .OrderBy(r => r.DueDate)
                                    .ToList();
                for (int i = 0; i < _dbReceiptList.Count; i++)
                {
                    var _receipt = _dbReceiptList[i];
                    _receipt.StudentReceiptNo = i + 1;
                    _db.Entry(_receipt).State = EntityState.Modified;
                    int j = _db.SaveChanges();
                    if (j > 0)
                    {
                        _returnString = _returnString == string.Empty ? "success-" + _receipt.Id.ToString() : _returnString + "," + "success-" + _receipt.Id.ToString();
                    }
                    else
                    {
                        _returnString = _returnString == string.Empty ? "error-" + _receipt.Id.ToString() : _returnString + "," + "error-" + _receipt.Id.ToString();
                    }

                }
                return _returnString;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public void TestOffSMS(int cat, int centre)
        {
            Common _cmn = new Common();
            int _testcro = 36;

            List<string> MobileNo = _cmn.GetOfficalSMS(cat, centre, _testcro);
        }

        public string NetBankingWithEmptyITC()
        {
            string str = string.Empty;
            string TXT_requesttype = "T";
            try
            {
                RequestURL objRequestURL = new RequestURL();
                String response = objRequestURL.SendRequest
                                              (
                                                "T"
                                              , "T45649"
                                              , "16072014185132"
                                              , ""
                                              , "1.00"
                                              , "INR"
                                              , ""
                                              , "http://www.networkzsystems.com/sms/mailtest/netbankingresponse"
                                              , ""
                                              , ""
                                              , "test_1.0_0.0"
                                              , "18-09-2016"
                                              , "gopikshnan@gmail.com"
                                              , "9995220869"
                                              , "470"
                                              , "GopiKrishnan"
                                              , ""
                                              , ""
                                              , "1848660477BEFDXF"
                                              , "8023485116PXTNYT"
                                              );

                String strResponse = response.ToUpper();

                if (strResponse.StartsWith("ERROR"))
                {
                    str = response;
                }
                else
                {
                    if (TXT_requesttype.ToUpper() == "T")
                    {

                        Response.Write("<form name='s1_2' id='s1_2' action='" + response + "' method='post'> ");

                        Response.Write("<script type='text/javascript' language='javascript' >document.getElementById('s1_2').submit();");

                        Response.Write("</script>");
                        Response.Write("<script language='javascript' >");
                        Response.Write("</script>");
                        Response.Write("</form> ");

                    }

                }

            }
            catch (Exception ex)
            {
                str = ex.Message;
            }

            return str;
        }

        public string NetBankingWithDummyITC()
        {
            string str = string.Empty;
            string TXT_requesttype = "T";
            try
            {
                RequestURL objRequestURL = new RequestURL();
                String response = objRequestURL.SendRequest
                                              (
                                                "T"
                                              , "T45649"
                                              , "16072014185132"
                                              , "dummy123"
                                              , "1.00"
                                              , "INR"
                                              , ""
                                              , "http://www.networkzsystems.com/sms/mailtest/netbankingresponse"
                                              , ""
                                              , ""
                                              , "test_1.0_0.0"
                                              , "18-09-2016"
                                              , "gopikshnan@gmail.com"
                                              , "9995220869"
                                              , "470"
                                              , "GopiKrishnan"
                                              , ""
                                              ,""
                                              , "1848660477BEFDXF"
                                              , "8023485116PXTNYT"
                                              );

                String strResponse = response.ToUpper();

                if (strResponse.StartsWith("ERROR"))
                {
                    str = response;
                }
                else
                {
                    if (TXT_requesttype.ToUpper() == "T")
                    {

                        Response.Write("<form name='s1_2' id='s1_2' action='" + response + "' method='post'> ");

                        Response.Write("<script type='text/javascript' language='javascript' >document.getElementById('s1_2').submit();");

                        Response.Write("</script>");
                        Response.Write("<script language='javascript' >");
                        Response.Write("</script>");
                        Response.Write("</form> ");

                    }

                }

            }
            catch (Exception ex)
            {
                str = ex.Message;
            }

            return str;
        }


        public class PopulateSelectList
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public void GetFinancialYearList()
        {
            List<PopulateSelectList> _finYearList = new List<PopulateSelectList>();
            try
            {
                Common _cmn = new Common();

                _finYearList = _cmn.FinancialYearList().ToList()
                                    .OrderByDescending(x => x.ToString())
                                    .Select(x => new PopulateSelectList
                                    {
                                        Id = x.ToString(),
                                        Name = x.ToString()
                                    }).ToList();

                var _currFinYear = _finYearList.First().Id;
                var _currFin_startYr = Convert.ToInt32(_currFinYear.Split('-').First());
                var _currFin_endYr = Convert.ToInt32(_currFinYear.Split('-').Last());

                var _nxtFinYear = new PopulateSelectList
                {
                    Id = (_currFin_startYr + 1) + "-" + (_currFin_endYr + 1),
                    Name = (_currFin_startYr + 1) + "-" + (_currFin_endYr + 1)
                };

                _finYearList.Insert(0, _nxtFinYear);





                //Adding next financial year 


                //_finYearList.Insert(_finYearList.Count, new PopulateSelectList { Id = "-1", Name = "All" });

                //return _finYearList;
            }
            catch (Exception ex)
            {
                //return _finYearList = null;
            }
        }

        public string DiscountSMS()
        {
            Common _cmn = new Common();
            var _mobNo = _cmn.GetOfficalSMS((int)EnumClass.SMSCATEGORY.DISCOUNTSMS, 5, null);
            return string.Join(",", _mobNo);

        }

        public void RegEmailTest()
        {

        }
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
