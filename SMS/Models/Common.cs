using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace SMS.Models
{
    public class Common : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();
        #region class

        public class EmpCenter
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class FinancialYear
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }


        #endregion

        // POST: /Transactions/Add
        public int AddTransactions(string action, string controller, string description)
        {
            try
            {
                TransactionDetail _transaction = new TransactionDetail();
                _transaction.ActionName = action;
                _transaction.ControllerName = controller;
                _transaction.Description = description;
                _transaction.TransactionTime = LocalDateTime();
                _transaction.UserId = Convert.ToInt32(System.Web.HttpContext.Current.Session["LoggedUserId"]);

                _db.TransactionDetails.Add(_transaction);
                return _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        //public List<EmpCenter> GetCenterEmployee(int employeeId)
        //{

        //    var _lstEmpCenter = new List<EmpCenter>();
        //    try
        //    {
        //        _lstEmpCenter = _db.EmployeeCenters
        //                     .Where(e => e.EmployeeId == employeeId)
        //                     .Select(x => new EmpCenter
        //                     {
        //                         Id = x.CenterCode.Id,
        //                         Name = x.CenterCode.CentreCode
        //                     }).ToList();

        //        return _lstEmpCenter;
        //    }
        //    catch (Exception ex)
        //    {
        //        return _lstEmpCenter = null;
        //    }
        //}


        //Password generator
        public string CreatePassword(int length)
        {
            StringBuilder res = new StringBuilder();
            try
            {
                const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

                Random rnd = new Random();
                while (0 < length--)
                {
                    res.Append(valid[rnd.Next(valid.Length)]);
                }
                return res.ToString();

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //Password hashing
        public string ComputePassword(string password)
        {
            StringBuilder str = new StringBuilder();
            try
            {
                byte[] bytes = Encoding.Unicode.GetBytes(password);
                var md5 = new MD5CryptoServiceProvider();
                var md5data = md5.ComputeHash(bytes);

                for (int i = 0; i < md5data.Length; i++)
                {
                    str.Append(md5data[i].ToString("x2"));
                }
                return str.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        //Email Sending
        public bool SendEmail(string recepeintEmailId, string body, string subject)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                string[] multi = recepeintEmailId.Split(',');
                foreach (string emailId in multi)
                {
                    mailMessage.To.Add(new MailAddress(emailId));
                }
                SmtpClient smtp = new SmtpClient();
                smtp.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }




        //Email Sending with attachment
        public bool SendReceiptEmail(string recepeintEmailId, string body, string subject, bool Attachment, byte[] bytes)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(recepeintEmailId));
                mailMessage.Attachments.Add(new Attachment(new MemoryStream(bytes), "Receipt.pdf"));
                SmtpClient smtp = new SmtpClient();
                smtp.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //Apicall for Mobile Verification
        public string ApiCall(string url)
        {

            HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                HttpWebResponse httpres = (HttpWebResponse)httpreq.GetResponse();
                StreamReader sr = new StreamReader(httpres.GetResponseStream());
                string results = sr.ReadToEnd();
                sr.Close();
                return results;
            }
            catch
            {
                return "0";
            }
        }

        //Generate RandomNo
        public int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        //Get Roles which are ranked below employee role
        public List<Role> GetRolesEmpwise(int empId)
        {
            List<Role> _lstRole = new List<Role>();
            //Get current employee rolewise rank
            int? _roleRank = _db.Employees
                          .Where(e => e.Id == empId)
                          .Select(e => e.Designation.Role.Rank).FirstOrDefault();

            //Get roles greater than the rank
            _lstRole = _db.Roles
                       .Where(r => r.Rank > _roleRank)
                       .ToList();

            return _lstRole;

        }

        //Get branches which are allotted to an employee
        public List<CenterCode> GetCenterEmpwise(int empId)
        {
            List<CenterCode> _lstCenterCode = new List<CenterCode>();
            List<int?> _lstEmployeeCenters = new List<int?>();

            //Get all centerIds allotted to an employee
            _lstEmployeeCenters = _db.EmployeeCenters
                                .Where(ec => ec.EmployeeId == empId)
                                .Select(ec => ec.CenterCodeId).ToList();

            //Get all center from above centerIds
            _lstCenterCode = _db.CenterCodes
                           .Where(c => _lstEmployeeCenters.Contains(c.Id)
                                && c.Status == true)
                           .ToList();

            return _lstCenterCode;

        }

        //Get LoggerUser RoleId
        public int GetLoggedUserRoleId(int empId)
        {
            int _userRoleId = 0;
            try
            {
                _userRoleId = _db.Employees
                              .Where(e => e.Id == empId)
                              .Select(e => e.Designation.Role.Id).FirstOrDefault();
                return _userRoleId;
            }
            catch
            {
                return _userRoleId;
            }
        }

        //Get all employees allotted to a particular center
        public List<Employee> GetEmployeeCenterWise(int empId)
        {
            //Get all centers allotted to a particular employee
            List<int> _centreIds = GetCenterEmpwise(empId)
                                 .Select(c => c.Id).ToList();

            //Get all employeeIds filtering by centers
            List<int> _employeeIds = _db.EmployeeCenters
                                    .Where(ec => _centreIds.Contains(ec.CenterCodeId.Value))
                                    .Select(e => e.Employee.Id).Distinct().ToList();


            //Get all employeelist filtering by employeeid
            List<Employee> _empList = _db.Employees
                                         .Where(e => _employeeIds.Contains(e.Id) && e.Status == true)
                                         .ToList();
            return _empList;
        }

        //Get concerned centermanger of a particular center
        public Employee GetCentreManager(int centreId)
        {
            Employee _employee = new Employee();
            List<int> _centerIdList = new List<int>();
            try
            {
                _employee = _db.CenterCodes
                          .Where(c => c.Id == centreId)
                          .Select(e => e.Employee).FirstOrDefault();

                return _employee;
            }
            catch (Exception ex)
            {
                return _employee;
            }
        }

        //Gets employees designationwise
        public List<Employee> GetEmployeeDesignationWise(List<int> designationIds)
        {
            //Get all employeeIds filtering by centers
            List<Employee> _empList = _db.Employees
                                    .Where(e => designationIds.Contains(e.Designation.Id) && e.Status == true)
                                    .Distinct().ToList();

            return _empList;
        }

        //Gets the current financial start date and end date
        public FinancialYear GetFinancialYear()
        {
            int currYear = 0;
            int nxtYear = 0;
            if (LocalDateTime().Date.Month >= 4)
            {
                currYear = LocalDateTime().Date.Year;
                nxtYear = LocalDateTime().Date.Year + 1;
            }
            else
            {
                currYear = LocalDateTime().Year - 1;
                nxtYear = LocalDateTime().Year;
            }

            var _financialYear = new FinancialYear()
            {
                StartDate = new DateTime(currYear, 4, 1),
                EndDate = new DateTime(nxtYear, 3, 31)
            };
            return _financialYear;
        }

        //Converts the amount into words
        public string NumbersToWords(int inputNumber)
        {
            int inputNo = inputNumber;

            if (inputNo == 0)
                return "Zero";

            int[] numbers = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (inputNo < 0)
            {
                sb.Append("Minus ");
                inputNo = -inputNo;
            }

            string[] words0 = {"" ,"One ", "Two ", "Three ", "Four ",
            "Five " ,"Six ", "Seven ", "Eight ", "Nine "};
            string[] words1 = {"Ten ", "Eleven ", "Twelve ", "Thirteen ", "Fourteen ",
            "Fifteen ","Sixteen ","Seventeen ","Eighteen ", "Nineteen "};
            string[] words2 = {"Twenty ", "Thirty ", "Forty ", "Fifty ", "Sixty ",
            "Seventy ","Eighty ", "Ninety "};
            string[] words3 = { "Thousand ", "Lakh ", "Crore " };

            numbers[0] = inputNo % 1000; // units
            numbers[1] = inputNo / 1000;
            numbers[2] = inputNo / 100000;
            numbers[1] = numbers[1] - 100 * numbers[2]; // thousands
            numbers[3] = inputNo / 10000000; // crores
            numbers[2] = numbers[2] - 100 * numbers[3]; // lakhs

            for (int i = 3; i > 0; i--)
            {
                if (numbers[i] != 0)
                {
                    first = i;
                    break;
                }
            }
            for (int i = first; i >= 0; i--)
            {
                if (numbers[i] == 0) continue;
                u = numbers[i] % 10; // ones
                t = numbers[i] / 10;
                h = numbers[i] / 100; // hundreds
                t = t - 10 * h; // tens
                if (h > 0) sb.Append(words0[h] + "Hundred ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i == 0) sb.Append("and ");
                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }
            return sb.ToString().TrimEnd();
        }

        //Masks the string except the first and last two strings
        public string MaskString(string maskString, string type)
        {
            string[] words = null;
            char _maskWord = '0';
            if (type == "email")
            {
                words = maskString.Split('@');
                maskString = words[0].ToString();
                _maskWord = 'X';

            }

            var firstDigits = maskString.Substring(0, 2);
            var lastDigits = maskString.Substring(maskString.Length - 2, 2);
            var requiredMask = new String(_maskWord, maskString.Length - firstDigits.Length - lastDigits.Length);
            var maskedString = string.Concat(firstDigits, requiredMask, lastDigits);
            //var maskedCardNumberWithSpaces = Regex.Replace(maskedString, ".{2}", "$0 ");
            if (type == "email")
            {
                maskedString = maskedString + "@" + words[1];
            }

            return maskedString;
        }


        public byte[] MergeReceiptPdf(int receiptID)
        {
            List<byte[]> arrayList = new List<byte[]>();
            arrayList.Add(Get_StudentReceiptPdf_Array(receiptID));
            arrayList.Add(Get_TermsndCondnPdf_Array());
            byte[] mergedPdf = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (Document document = new Document())
                {
                    using (PdfCopy copy = new PdfCopy(document, ms))
                    {
                        document.Open();

                        for (int i = 0; i < arrayList.Count; ++i)
                        {
                            PdfReader reader = new PdfReader(arrayList[i]);
                            // loop over the pages in that document
                            int n = reader.NumberOfPages;
                            for (int page = 0; page < n; )
                            {
                                copy.AddPage(copy.GetImportedPage(reader, ++page));
                            }

                        }
                        document.Close();
                    }
                }
                //For watermark addition
                //mergedPdf = AddWatermark(ms.ToArray(), BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED));
                //else when watermark is removed uncomment the below line and comment the above line
                mergedPdf = ms.ToArray();
                ms.Close();

            }
            return mergedPdf;
        }

        //Converting TermsNdCondition Pdf to byte array
        public byte[] Get_TermsndCondnPdf_Array()
        {
            byte[] fileContent = null;
            try
            {
                var _fileName = "Template/TC.Pdf";
                var _filePath = Path.Combine(HttpRuntime.AppDomainAppPath, _fileName);
                FileStream fs = new FileStream(_filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fs);
                long byteLength = new System.IO.FileInfo(_filePath).Length;
                fileContent = binaryReader.ReadBytes((Int32)byteLength);
                fs.Close();
                fs.Dispose();
                binaryReader.Close();
                return fileContent;
            }
            catch (Exception ex)
            {
                return fileContent;
            }

        }

        //Converting StudentPdf into byte array
        public byte[] Get_StudentReceiptPdf_Array(int receiptID)
        {
            byte[] fileContent = null;
            try
            {
                var _dbReceipt = _db.StudentReceipts
                           .Where(r => r.Id == receiptID)
                           .FirstOrDefault();
                if (_dbReceipt != null)
                {
                    var _fileName = "Receipt/" + _dbReceipt.StudentRegistration.RegistrationNumber + "_" + Common.GetReceiptNo(_dbReceipt.StudentReceiptNo.Value) + ".pdf";
                    //Reading StudentPDF File
                    var _filePath = Path.Combine(HttpRuntime.AppDomainAppPath, _fileName);
                    //var _fileName = Server.MapPath("~/Receipt/" + _dbReceipt.StudentRegistration.RegistrationNumber + "_" + _dbReceipt.ReceiptNo + ".pdf");

                    FileStream fs = new FileStream(_filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    BinaryReader binaryReader = new BinaryReader(fs);
                    long byteLength = new System.IO.FileInfo(_filePath).Length;
                    fileContent = binaryReader.ReadBytes((Int32)byteLength);
                    fs.Close();
                    fs.Dispose();
                    binaryReader.Close();
                }
                return fileContent;
            }
            catch (Exception ex)
            {
                return fileContent;
            }

        }


        public byte[] AddWatermark(byte[] bytes, BaseFont bf)
        {
            using (var ms = new MemoryStream())
            {
                using (var reader = new PdfReader(bytes))
                using (var stamper = new PdfStamper(reader, ms))
                {
                    int times = reader.NumberOfPages;
                    for (int i = 1; i <= times; i++)
                    {
                        var dc = stamper.GetOverContent(i);
                        AddWaterMark(dc, "Demo", bf, 48, 35, new BaseColor(70, 70, 255), reader.GetPageSizeWithRotation(i));
                    }
                    stamper.Close();
                }
                return ms.ToArray();
            }
        }

        public static void AddWaterMark(PdfContentByte dc, string text, BaseFont font, float fontSize, float angle, BaseColor color, Rectangle realPageSize, Rectangle rect = null)
        {
            var gstate = new PdfGState { FillOpacity = 0.1f, StrokeOpacity = 0.3f };
            dc.SaveState();
            dc.SetGState(gstate);
            dc.SetColorFill(color);
            dc.BeginText();
            dc.SetFontAndSize(font, fontSize);
            var ps = rect ?? realPageSize; /*dc.PdfDocument.PageSize is not always correct*/
            var x = (ps.Right + ps.Left) / 2;
            var y = (ps.Bottom + ps.Top) / 2;
            dc.ShowTextAligned(Element.ALIGN_CENTER, text, x, y, angle);
            dc.EndText();
            dc.RestoreState();
        }

        public static DateTime LocalDateTime()
        {
            DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            return now;
        }

        public List<string> FinancialYearList()
        {
            int startYear = 2011;
            int endYear = LocalDateTime().Year;
            int endMonth = LocalDateTime().Month;


            List<string> _periodList = new List<string>();

            for (int i = startYear; i <= endYear; i++)
            {

                if (startYear < endYear)
                {
                    int prevYear = startYear - 1;
                    int currYear = startYear;

                    _periodList.Add(Convert.ToString(prevYear + "-" + currYear));
                }
                else
                {
                    int prevYear = endYear - 1;
                    int nextYear = endYear + 1;

                    if (endMonth > 3)
                    {

                        _periodList.Add(Convert.ToString(prevYear + "-" + endYear));
                        _periodList.Add(Convert.ToString(endYear + "-" + nextYear));
                    }
                    else
                    {
                        _periodList.Add(Convert.ToString(prevYear + "-" + endYear));
                    }
                }
                startYear++;

            }

            return _periodList;
        }

        //Get Instructor List
        public List<int> GetInstructorList()
        {
            List<int> _instructorList = new List<int>();

            _instructorList.Add((int)EnumClass.Designation.CPDHEAD);
            _instructorList.Add((int)EnumClass.Designation.SYSTEMSPECIALIST);
            _instructorList.Add((int)EnumClass.Designation.TECHNICALTRAINER);
            _instructorList.Add((int)EnumClass.Designation.SOFTWARESPECIALIST);
            _instructorList.Add((int)EnumClass.Designation.TECHNICALLEADER);
            _instructorList.Add((int)EnumClass.Designation.TECHNICALFACILITATOR);
            _instructorList.Add((int)EnumClass.Designation.TECHNICALENGINEERNETWORKING);
            _instructorList.Add((int)EnumClass.Designation.TECHNICALLEADER_TI);
            _instructorList.Add((int)EnumClass.Designation.TECHNICALTRAINER_TI);
            _instructorList.Add((int)EnumClass.Designation.SOFTWARESPECIALIST_TI);
            _instructorList.Add((int)EnumClass.Designation.NETWORKSPECIALIST);
            _instructorList.Add((int)EnumClass.Designation.NETWORKTRAINER);

            return _instructorList;
        }

        //Gets the manager of a particular center
        public Employee GetManager(int centreId)
        {
            var _employee = _db.EmployeeCenters
                          .Where(ec => ec.CenterCodeId.Value == centreId && ec.Employee.Designation.Id == (int)EnumClass.Designation.MANAGERSALES)
                          .Select(ec => ec.Employee).FirstOrDefault();

            return _employee;


        }

        //Get DesignationId List by passing roleid
        public List<Designation> GetDesignationList(int roleID)
        {

            var _designationIdList = _db.Designations
                                   .Where(d => d.RoleId == roleID)
                                   .ToList();
            return _designationIdList;

        }

        //Get all employees filtering by designation ,center 
        public List<Employee> GetEmployee_Designation_CenterWise(string centerIds, int designationId)
        {
            List<int> _lstCenterId = centerIds.Split(',').Select(e => Int32.Parse(e)).ToList();
            List<Employee> _empList = new List<Employee>();
            //Filtering by centerids
            _empList = _db.EmployeeCenters
                            .Where(ec => _lstCenterId.Contains(ec.CenterCode.Id))
                            .Select(ec => ec.Employee).ToList();
            //Filtering by roles
            if (designationId != 0)
            {
                _empList = _empList.Where(e => e.DesignationId == designationId).ToList();
            }

            return _empList;

        }

        public bool SendEmailTesting(string recepeintEmailId, string body, string subject)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["UserName"]);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                string[] multi = recepeintEmailId.Split(',');
                foreach (string emailId in multi)
                {
                    mailMessage.To.Add(new MailAddress(emailId));
                }
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ConfigurationManager.AppSettings["Host"];
                smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                NetworkCred.UserName = ConfigurationManager.AppSettings["UserName"];
                NetworkCred.Password = ConfigurationManager.AppSettings["Password"];
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                smtp.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static int RoundOff(int i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        //Gets the designationids of people involved in sales
        public List<int> GetSalesTeamIdList()
        {
            List<int> _salesTeamId = new List<int>();
            _salesTeamId.Add((int)EnumClass.Designation.ED);
            _salesTeamId.Add((int)EnumClass.Designation.CENTREMANAGER);
            _salesTeamId.Add((int)EnumClass.Designation.MANAGERFRANCHISEDEVELOPMENT);
            _salesTeamId.Add((int)EnumClass.Designation.MANAGERSUPPORT);
            _salesTeamId.Add((int)EnumClass.Designation.MANAGERSALES);
            _salesTeamId.Add((int)EnumClass.Designation.ASSISTANTMANAGER);
            _salesTeamId.Add((int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER);
            _salesTeamId.Add((int)EnumClass.Designation.TECHNOCOMMERCIALEXECUTIVE);
            _salesTeamId.Add((int)EnumClass.Designation.BUSINESSDEVELOPMENTEXECUTIVE);
            return _salesTeamId;

        }

        //Gets the coursename of given courseid
        public string GetCourseName(int courseId)
        {
            string courseName = string.Empty;
            try
            {
                if (courseId == (int)EnumClass.SelectAll.ALL)
                {
                    courseName = EnumClass.SelectAll.ALL.ToString();
                }
                else
                {
                    using (var _db = new dbSMSNSEntities())
                    {
                        courseName = _db.Courses
                                     .Where(c => c.Id == courseId)
                                     .FirstOrDefault().Name;

                    }
                }
                return courseName;

            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public decimal GetCourseFee(int courseId, int feeMode)
        {
            decimal _courseFee = 0;
            try
            {
                if (feeMode == (int)EnumClass.InstallmentType.SINGLE)
                {

                    _courseFee = _db.Courses
                                .Where(c => c.Id == courseId)
                                .FirstOrDefault().SingleFee.Value;
                }
                else
                {
                    _courseFee = _db.Courses
                                .Where(c => c.Id == courseId)
                                .FirstOrDefault().InstallmentFee.Value;
                }
            }
            catch (Exception ex)
            {
                _courseFee = 0;
            }
            return _courseFee;

        }

        public int GetMonthID(string monthName)
        {
            int MonthID = -1;
            switch (monthName)
            {
                case "APRIL":
                    MonthID = 4;
                    break;

                case "MAY":
                    MonthID = 5;
                    break;

                case "JUNE":
                    MonthID = 6;
                    break;

                case "JULY":
                    MonthID = 7;
                    break;

                case "AUGUST":
                    MonthID = 8;
                    break;

                case "SEPTEMBER":
                    MonthID = 9;
                    break;

                case "OCTOBER":
                    MonthID = 10;
                    break;

                case "NOVEMBER":
                    MonthID = 11;
                    break;

                case "DECEMBER":
                    MonthID = 12;
                    break;

                case "JANUARY":
                    MonthID = 1;
                    break;

                case "FEBRUARY":
                    MonthID = 2;
                    break;

                case "MARCH":
                    MonthID = 3;
                    break;

            }

            return MonthID;

        }

        public string ConvertNumberToComma(decimal num)
        {
            NumberFormatInfo nfo = new NumberFormatInfo();
            nfo.CurrencyGroupSeparator = ",";
            // you are interested in this part of controlling the group sizes
            nfo.CurrencyGroupSizes = new int[] { 3, 2 };
            nfo.CurrencySymbol = "";

            return (num.ToString("c0", nfo)); // prints 1,50,00,000
        }

        //Converts the incoming receiptno to string
        public static string GetReceiptNo(int? receiptNo)
        {
            string _threeDigitSlNo = null;
            int _length = 3;
            _threeDigitSlNo = receiptNo != null ? receiptNo.ToString().PadLeft(_length, '0') : "";
            return _threeDigitSlNo;

        }

        //Gets the rolename based on roleid
        public string GetDiscountSetting_RoleName(int roleId)
        {
            string _roleName = string.Empty;

            switch (roleId)
            {
                case ((int)EnumClass.DiscountSettingRole.ED):
                    _roleName = EnumClass.DiscountSettingRole.ED.ToString();
                    break;

                case ((int)EnumClass.DiscountSettingRole.CENTRE_MANAGER):
                    _roleName = EnumClass.DiscountSettingRole.CENTRE_MANAGER.ToString().Replace('_', ' ');
                    break;

                case ((int)EnumClass.DiscountSettingRole.CLUSTER_MANAGER):
                    _roleName = EnumClass.DiscountSettingRole.CLUSTER_MANAGER.ToString().Replace('_', ' ');
                    break;

                case ((int)EnumClass.DiscountSettingRole.SALES_MANAGER):
                    _roleName = EnumClass.DiscountSettingRole.SALES_MANAGER.ToString().Replace('_', ' ');
                    break;

                case ((int)EnumClass.DiscountSettingRole.SALES_INDIVIDUAL):
                    _roleName = EnumClass.DiscountSettingRole.SALES_INDIVIDUAL.ToString().Replace('_', ' ');
                    break;
            }

            return _roleName;
        }

        //Get DiscountRoleDesignationId based on EmployeeRoleId
        public int GetDiscountSetting_RoleId(int employeeRoleId)
        {
            int _discountRole_Id = 0;

            if (employeeRoleId == (int)EnumClass.Role.ED)
            {
                _discountRole_Id = (int)EnumClass.DiscountSettingRole.ED;
            }
            else if (employeeRoleId == (int)EnumClass.Role.CENTERMANAGER)
            {
                _discountRole_Id = (int)EnumClass.DiscountSettingRole.CENTRE_MANAGER;
            }
            else if (employeeRoleId == (int)EnumClass.Role.MANAGER)
            {
                _discountRole_Id = (int)EnumClass.DiscountSettingRole.SALES_MANAGER;
            }
            else if (employeeRoleId == (int)EnumClass.Role.SALESINDIVIDUAL)
            {
                _discountRole_Id = (int)EnumClass.DiscountSettingRole.SALES_INDIVIDUAL;
            }
            else
            {
                _discountRole_Id = (int)EnumClass.DiscountSettingRole.CLUSTER_MANAGER;
            }

            return _discountRole_Id;

        }

        //Gets the employee based on roleid
        public List<Employee> GetEmployee_BasedOn_RoleId_CentreCodeId(int roleId, int centreCodeId)
        {
            List<Employee> _emp = new List<Employee>();
            try
            {
                CenterCode _centreCode = _db.CenterCodes
                                       .Where(c => c.Id == centreCodeId)
                                       .FirstOrDefault();

                _emp = _db.Employees
                        .Where(e => e.EmployeeCenters.Any(ec => ec.CenterCode.Id == centreCodeId)
                                    && e.Designation.Role.Id == roleId
                                    && e.Status == true)
                        .ToList();

            }
            catch (Exception ex)
            {
                _emp = null;
            }

            return _emp;
        }

        //Returns the mobileno of employees for sending sms
        public List<string> GetOfficalSMS(int smsCategory, int centreId, int? croId)
        {
            List<string> _mobileNoList = new List<string>();

            //Gets the group of corresponding centre
            string groupName = _db.Group_CentreCode_Setting
                             .Where(gcs => gcs.CenterCode.Id == centreId)
                             .FirstOrDefault().GroupName;

            if (smsCategory == (int)EnumClass.SMSCATEGORY.DISCOUNTSMS)
            {
                string _concerned_cro_mobno = GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER)
                                            .Where(e => e.Id == croId)
                                            .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                            .FirstOrDefault();

                string _concerned_centre_salesmgr_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.MANAGERSALES)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_centre_centremgr_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.CENTREMANAGER)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_centre_ed_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.ED)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                _mobileNoList.Add(_concerned_cro_mobno);
                _mobileNoList.Add(_concerned_centre_salesmgr_mobno);
                _mobileNoList.Add(_concerned_centre_centremgr_mobno);
                _mobileNoList.Add(_concerned_centre_ed_mobno);
            }

            if (smsCategory == (int)EnumClass.SMSCATEGORY.REGISTRATIONSMS)
            {
                string _concerned_centre_cro_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_group_salesmgr_mobno = string.Join(",", GetEmployee_Groupwise(groupName, (int)EnumClass.Designation.MANAGERSALES)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_group_centremgr_mobno = string.Join(",", GetEmployee_Groupwise(groupName, (int)EnumClass.Designation.CENTREMANAGER)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_group_ed_mobno = string.Join(",", GetEmployee_Groupwise(groupName, (int)EnumClass.Designation.ED)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());


                _mobileNoList.Add(_concerned_centre_cro_mobno);
                _mobileNoList.Add(_concerned_group_salesmgr_mobno);
                _mobileNoList.Add(_concerned_group_centremgr_mobno);
                _mobileNoList.Add(_concerned_group_ed_mobno);

            }

            if (smsCategory == (int)EnumClass.SMSCATEGORY.RECEIPTSMS)
            {
                string _concerned_centre_cro_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_centre_salesmgr_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.MANAGERSALES)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_centre_centremgr_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.CENTREMANAGER)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());

                string _concerned_centre_ed_mobno = string.Join(",", GetEmployee_Centrewise(centreId, (int)EnumClass.Designation.ED)
                                                                        .Select(e => e.OfficialMobileNo != null ? e.OfficialMobileNo : e.MobileNo)
                                                                         .Distinct()
                                                                        .ToList());


                _mobileNoList.Add(_concerned_centre_cro_mobno);
                _mobileNoList.Add(_concerned_centre_salesmgr_mobno);
                _mobileNoList.Add(_concerned_centre_centremgr_mobno);
                _mobileNoList.Add(_concerned_centre_ed_mobno);

            }

            return _mobileNoList.Where(m => m != null).ToList();
        }

        //Gets the employee list groupwise,designationwise is optional
        public List<Employee> GetEmployee_Groupwise(string groupName, int? designationId)
        {
            List<Employee> _employeeList = new List<Employee>();

            //gets all the centres of concerned group
            List<int> _centre_groupwise_list = _db.Group_CentreCode_Setting
                                            .Where(gcs => gcs.GroupName == groupName)
                                            .Select(gcs => gcs.CenterCode.Id).ToList();

            //gets all the employee of the centre
            _employeeList = _db.Employees
                          .Where(e => e.Status == true &&
                                 e.EmployeeCenters.Any(ec => _centre_groupwise_list.Contains(ec.CenterCode.Id)))
                          .ToList();

            if (designationId == (int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER)
            {
                _employeeList = _employeeList
                                .Where(e => e.Designation.Id == ((int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER))
                                .ToList();
            }

            if (designationId == (int)EnumClass.Designation.MANAGERSALES)
            {
                _employeeList = _employeeList
                               .Where(e => e.Designation.Id == ((int)EnumClass.Designation.MANAGERSALES))
                               .ToList();

            }

            if (designationId == (int)EnumClass.Designation.CENTREMANAGER)
            {
                _employeeList = _employeeList
                               .Where(e => e.Designation.Id == ((int)EnumClass.Designation.CENTREMANAGER))
                               .ToList();

            }

            if (designationId == (int)EnumClass.Designation.ED)
            {
                _employeeList = _employeeList
                               .Where(e => e.Designation.Id == ((int)EnumClass.Designation.ED))
                               .ToList();

            }

            return _employeeList;
        }

        //Gets the employee list centrewise,designationwise is optional
        public List<Employee> GetEmployee_Centrewise(int centreId, int? designationId)
        {

            List<Employee> _employeeList = new List<Employee>();
            //gets all the employee of the centre
            _employeeList = _db.Employees
                          .Where(e => e.Status == true &&
                                 e.EmployeeCenters.Any(ec => ec.CenterCode.Id == centreId))
                          .ToList();

            if (designationId == (int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER)
            {
                _employeeList = _employeeList
                                .Where(e => e.Designation.Id == ((int)EnumClass.Designation.CUSTOMERRELATIONSOFFICER))
                                .ToList();
            }

            if (designationId == (int)EnumClass.Designation.MANAGERSALES)
            {
                _employeeList = _employeeList
                               .Where(e => e.Designation.Id == ((int)EnumClass.Designation.MANAGERSALES))
                               .ToList();

            }

            if (designationId == (int)EnumClass.Designation.CENTREMANAGER)
            {
                _employeeList = _employeeList
                               .Where(e => e.Designation.Id == ((int)EnumClass.Designation.CENTREMANAGER))
                               .ToList();

            }

            if (designationId == (int)EnumClass.Designation.ED)
            {
                _employeeList = _employeeList
                               .Where(e => e.Designation.Id == ((int)EnumClass.Designation.ED))
                               .ToList();

            }

            return _employeeList;
        }


        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}