using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CourseFullEditApprovalController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        #region Class Definition
        public class SuccessMessage
        {
            public int RegistrationId { get; set; }
            public string Status { get; set; }

        }

        public class CourseFullEditDataTable
        {
            public string RequestedDate
            {
                get
                {
                    return CourseFullEditDetails.TransactionDate.Value.ToString("dd/MM/yyyy");
                }
            }
            public string RequestedBy
            {
                get
                {
                    return CourseFullEditDetails.Employee.Name;
                }
            }
            public string RegNo { get; set; }
            public string ExistingSoftwareUsed { get; set; }
            public string NewSoftwareUsed
            {
                get
                {
                    return string.Join(",", CourseFullEditDetails.StudentRegistrationCourse_CourseFullEdit
                                    .SelectMany(rh => rh.MultiCourse.MultiCourseDetails
                                    .Select(rhmc => rhmc.Course.Name)));
                }
            }
            public int ExistingFee { get; set; }
            public int NewFee
            {
                get
                {
                    return CourseFullEditDetails.StudentReceipt_CourseFullEdit.Sum(s => s.Fee.Value);
                }

            }
            public string StudentName { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public StudentRegistration_CourseFullEdit CourseFullEditDetails
            {
                get
                {
                    StudentRegistration_CourseFullEdit _courseFullEdit = new StudentRegistration_CourseFullEdit();
                    _courseFullEdit = StudentRegistration.StudentRegistration_CourseFullEdit.FirstOrDefault();
                    return _courseFullEdit;
                }
            }
            public int Id { get; set; }
        }

        #endregion
        //
        // GET: /CourseFullEditApproval/

        public ActionResult Index()
        {
            try
            {
                Common _cmn = new Common();

                var _finYearList = _cmn.FinancialYearList().ToList()
                                    .OrderByDescending(x => x.ToString())
                                    .Select(x => new
                                    {
                                        Id = x.ToString(),
                                        Name = x.ToString()
                                    });

                var _monthList = from EnumClass.Month c in Enum.GetValues(typeof(EnumClass.Month))
                                 select new { Id = c.ToString(), Name = c.ToString() };

                var _indexVM = new IndexVM
                {
                    FinancialYearList = new SelectList(_finYearList, "Id", "Name"),
                    MonthList = new SelectList(_monthList, "Id", "Name"),
                    MonthName = "ALL"
                };
                return View(_indexVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public JsonResult GetDataTable(string finYear, string month)
        {
            try
            {

                Common _cmn = new Common();
                List<CourseFullEditDataTable> _dTableReg = new List<CourseFullEditDataTable>();
                DateTime _startFinDate = new DateTime();
                DateTime _endFinDate = new DateTime();

                var arrFinYear = finYear.Split('-');
                var _startFinYear = Convert.ToInt32(arrFinYear[0]);
                var _endFinYear = Convert.ToInt32(arrFinYear[1]);
                var _currentRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));
                int monthID = _cmn.GetMonthID(month);

                if (monthID == (int)EnumClass.SelectAll.ALL)
                {
                    _startFinDate = new DateTime(_startFinYear, (int)EnumClass.FinYearMonth.STARTMONTH, 1).Date;
                    _endFinDate = new DateTime(_endFinYear, (int)EnumClass.FinYearMonth.ENDMONTH, 31).Date;
                }
                else
                {
                    //Eg:FinYear=2016-2017
                    //if month is greater than 4 then 2016 is taken else 2017 is taken
                    if (monthID >= 4)
                    {
                        _startFinDate = new DateTime(_startFinYear, monthID, 1);
                        _endFinDate = new DateTime(_startFinYear, monthID, DateTime.DaysInMonth(_startFinYear, monthID));
                    }
                    else
                    {
                        _startFinDate = new DateTime(_endFinYear, monthID, 1);
                        _endFinDate = new DateTime(_endFinYear, monthID, DateTime.DaysInMonth(_startFinYear, monthID));
                    }

                }

                //Gets all the centerCodeIds allotted to an employee
                List<int> _centerCodeIds = _cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"]))
                                            .Select(x => x.Id).ToList();
                List<StudentRegistration> _lstStudReg = new List<StudentRegistration>();
                _lstStudReg = _db.StudentRegistrations
                            .Where(r => r.StudentRegistration_CourseFullEdit.Any(x => x.CourseFullEdit_Approval.Any(y => y.IsApproved == null && y.IsValidated == true))
                                        && _centerCodeIds.Contains(r.StudentWalkInn.CenterCode.Id))
                            .ToList();

                _dTableReg = _lstStudReg
                                .AsEnumerable()
                                .Where(r => (r.StudentRegistration_CourseFullEdit.OrderByDescending(rh => rh.Id).FirstOrDefault().TransactionDate.Value.Date >= _startFinDate
                                            && r.StudentRegistration_CourseFullEdit.OrderByDescending(rh => rh.Id).FirstOrDefault().TransactionDate.Value.Date <= _endFinDate))
                                .OrderByDescending(r => r.Id)
                                .Select(rd => new CourseFullEditDataTable
                                {
                                    StudentRegistration = rd,
                                    ExistingFee = rd.StudentReceipts.Sum(s => s.Fee.Value),
                                    ExistingSoftwareUsed = string.Join(",", rd.StudentRegistrationCourses
                                                         .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                         .Select(mc => mc.Course.Name))),
                                    RegNo = rd.RegistrationNumber,

                                    StudentName = rd.StudentWalkInn.CandidateName,
                                    Id = rd.StudentRegistration_CourseFullEdit.FirstOrDefault().Id

                                }).ToList();

                var _regList = _dTableReg
                               .Select(x => new
                               {
                                   ReqDate = x.RequestedDate,
                                   ReqBy = x.RequestedBy,
                                   RegNo = x.RegNo,
                                   ExistingSoftwareUsed = x.ExistingSoftwareUsed.ToUpper(),
                                   NewSoftwareUsed = x.NewSoftwareUsed.ToUpper(),
                                   ExistingFee = x.ExistingFee,
                                   NewFee = x.NewFee,
                                   StudentName = x.StudentName.ToUpper(),
                                   Id = x.Id
                               }).ToList();



                return Json(new { data = _regList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult CourseFullEditApproval(int courseFullEditID)
        {
            try
            {
                StudentRegistration_CourseFullEdit _dbCourseFullEdit = _db.StudentRegistration_CourseFullEdit.Find(courseFullEditID);

                CourseFullEditApprovalVM _mdlFullEditApproval = new CourseFullEditApprovalVM();
                _mdlFullEditApproval.ApprovedBy = Convert.ToInt32(Session["LoggedUserId"]);
                _mdlFullEditApproval.CourseCombination_NewValue = string.Join(",", _dbCourseFullEdit.StudentRegistrationCourse_CourseFullEdit
                                                                                .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                                .Select(mcd => mcd.Course.Name)));
                _mdlFullEditApproval.CourseCombination_OldValue = string.Join(",", _dbCourseFullEdit.StudentRegistration
                                                                                .StudentRegistrationCourses
                                                                                .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                                .Select(mcd => mcd.Course.Name)));
                _mdlFullEditApproval.CourseFee_NewValue = _dbCourseFullEdit.StudentReceipt_CourseFullEdit.Sum(r => r.Fee.Value);
                _mdlFullEditApproval.CourseFee_OldValue = _dbCourseFullEdit.StudentRegistration.StudentReceipts.Sum(r => r.Fee.Value);
                _mdlFullEditApproval.Discount_NewValue = _dbCourseFullEdit.Discount.Value;
                _mdlFullEditApproval.Discount_OldValue = _dbCourseFullEdit.StudentRegistration.Discount.Value;
                _mdlFullEditApproval.Email = _dbCourseFullEdit.StudentRegistration.StudentWalkInn.EmailId;
                _mdlFullEditApproval.IsCourseCombination_CrossChecked = false;
                _mdlFullEditApproval.IsCourseFees_CrossChecked = false;
                _mdlFullEditApproval.IsCustomer_CrossChecked = false;
                _mdlFullEditApproval.IsDiscountDetails_CrossChecked = false;
                _mdlFullEditApproval.IsReason_CrossChecked = false;
                _mdlFullEditApproval.MobileNo = _dbCourseFullEdit.StudentRegistration.StudentWalkInn.MobileNo;
                _mdlFullEditApproval.PhotoUrl = _dbCourseFullEdit.StudentRegistration.PhotoUrl;
                _mdlFullEditApproval.RegistrationNumber = _dbCourseFullEdit.StudentRegistration.RegistrationNumber;
                _mdlFullEditApproval.CourseFullEditReason = _dbCourseFullEdit.Reason;
                _mdlFullEditApproval.StudentName = _dbCourseFullEdit.StudentRegistration.StudentWalkInn.CandidateName;
                _mdlFullEditApproval.CourseFullEditID = courseFullEditID;
                _mdlFullEditApproval.RejectedReason = string.Empty;
                _mdlFullEditApproval.StudentRegistrationID = _dbCourseFullEdit.StudentRegistrationID.Value;



                return View(_mdlFullEditApproval);



            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult CourseFullEditApproval(CourseFullEditApprovalVM mdlCourseFullEditApproval)
        {
            StudentRegistration_History _dbRegn_History = new StudentRegistration_History();
            StudentRegistration_History _dbRegn_History_Existing = new StudentRegistration_History();
            int _totalCourseFee = 0;
            int _totalST = 0;
            int _totalAmount = 0;
            List<string> _lstCourseFullEditMultiCourseId = new List<string>();
            string _courseFullEditCourseIds = string.Empty;
            List<int> _newFeedbackCourseIds = new List<int>();
            decimal _newST = 0;
            SuccessMessage _successMsg = new SuccessMessage();
            try
            {
                StudentRegistration _dbRegn = _db.StudentRegistrations
                                            .Where(r => r.Id == mdlCourseFullEditApproval.StudentRegistrationID)
                                            .FirstOrDefault();

                _dbRegn_History_Existing = _dbRegn.StudentRegistration_History.FirstOrDefault();
                StudentRegistration_CourseFullEdit _dbCourseFullEdit = _dbRegn.StudentRegistration_CourseFullEdit.FirstOrDefault();

                #region Taking Backup of existing studentregistration details

                int _courseInterchangeFee = _dbRegn.StudentRegistration_History.Any() ?
                                                     _dbRegn.StudentRegistration_History.Sum(h => h.AdditionalCourseFee.Value) : 0;

                int _courseInterchangeSTAmt = _dbRegn.StudentRegistration_History.Any() ?
                                            _dbRegn.StudentRegistration_History.Sum(h => h.AdditionalSTAmount.Value) : 0;

                //Remove existing history for students
                _dbRegn.StudentRegistration_History.Remove(_dbRegn_History_Existing);

                //Back up of student registration
                _dbRegn_History.Discount = _dbRegn.Discount;
                _dbRegn_History.FeeMode = _dbRegn.FeeMode;
                _dbRegn_History.HistoryType = (int)EnumClass.STUDENT_BACKUP_TYPE.COURSE_FULL_EDIT;
                _dbRegn_History.STPercentage = _dbRegn.STPercentage;
                _dbRegn_History.TotalAmount = _dbRegn.TotalAmount;
                _dbRegn_History.TotalCourseFee = _dbRegn.TotalCourseFee;
                _dbRegn_History.TotalDuration = _dbRegn.TotalDuration;
                _dbRegn_History.TotalSTAmount = _dbRegn.TotalSTAmount;
                _dbRegn_History.TransactionDate = Common.LocalDateTime();
                _dbRegn_History.CROID = Convert.ToInt32(Session["LoggedUserId"]);
                _dbRegn_History.IsModifiedByEmployee = true;
                _dbRegn_History.AdditionalCourseFee = _courseInterchangeFee;
                _dbRegn_History.AdditionalSTAmount = _courseInterchangeSTAmt;

                //Back up of student registration courses
                foreach (var item in _dbRegn.StudentRegistrationCourses)
                {
                    StudentRegistrationCourse_History _dbRegn_Course_History = new StudentRegistrationCourse_History();
                    _dbRegn_Course_History.MultiCourseID = item.MultiCourseID;
                    _dbRegn_History.StudentRegistrationCourse_History.Add(_dbRegn_Course_History);
                }

                //Backup of student student receipt
                foreach (var item in _dbRegn.StudentReceipts)
                {
                    StudentReceipt_History _dbReceipt_History = new StudentReceipt_History();
                    _dbReceipt_History.BankName = item.BankName;
                    _dbReceipt_History.BranchName = item.BranchName;
                    _dbReceipt_History.ChDate = item.ChDate;
                    _dbReceipt_History.ChNo = item.ChNo;
                    _dbReceipt_History.CROID = item.CROID;
                    _dbReceipt_History.DueDate = item.DueDate;
                    _dbReceipt_History.Fee = item.Fee;
                    _dbReceipt_History.ModeOfPayment = item.ModeOfPayment;
                    _dbReceipt_History.PDCSlip = item.PDCSlip;
                    _dbReceipt_History.ST = item.ST;
                    _dbReceipt_History.Status = item.Status;
                    _dbReceipt_History.STPercentage = item.STPercentage;
                    _dbReceipt_History.StudentReceiptNo = item.StudentReceiptNo;
                    _dbReceipt_History.Total = item.Total;


                    _dbRegn_History.StudentReceipt_History.Add(_dbReceipt_History);
                }

                _dbRegn.StudentRegistration_History.Add(_dbRegn_History);
                #endregion

                #region Transferring data from course full edit table to student registration table

                StudentRegistration_CourseFullEdit _mdlCourseFullEdit = _dbRegn.StudentRegistration_CourseFullEdit.FirstOrDefault();

                //Removing existing course details
                foreach (var _existingCourse in _dbRegn.StudentRegistrationCourses.ToList())
                {
                    _db.StudentRegistrationCourses.Remove(_existingCourse);
                }

                //Remove existing receipts
                foreach (var _existingReceipt in _dbRegn.StudentReceipts.ToList())
                {
                    _db.StudentReceipts.Remove(_existingReceipt);
                }


                //saving student course details
                foreach (var multicourse in _mdlCourseFullEdit.StudentRegistrationCourse_CourseFullEdit)
                {
                    StudentRegistrationCourse _studCourse = new StudentRegistrationCourse();
                    _studCourse.MultiCourseID = Convert.ToInt32(multicourse.MultiCourseID);
                    _dbRegn.StudentRegistrationCourses.Add(_studCourse);

                    _lstCourseFullEditMultiCourseId.Add(multicourse.MultiCourseID.ToString());
                }

                //adding new receipt
                foreach (var _receipt in _mdlCourseFullEdit.StudentReceipt_CourseFullEdit)
                {
                    //adding coursefee and totalst details
                    _totalCourseFee = _totalCourseFee + _receipt.Fee.Value;
                    _totalST = _totalST + _receipt.ST.Value;
                    _newST = Convert.ToDecimal(_receipt.STPercentage.Value);

                    StudentReceipt _studReceipt = new StudentReceipt();
                    _studReceipt.DueDate = _receipt.DueDate;
                    _studReceipt.Fee = _receipt.Fee;
                    _studReceipt.ST = _receipt.ST;
                    _studReceipt.Status = _receipt.Status;
                    _studReceipt.Total = _receipt.Total;
                    _studReceipt.STPercentage = _receipt.STPercentage;
                    _studReceipt.StudentReceiptNo = _receipt.StudentReceiptNo;
                    _studReceipt.CROID = Convert.ToInt32(Session["LoggedUserId"]);
                    _studReceipt.ModeOfPayment = _receipt.ModeOfPayment;
                    _dbRegn.StudentReceipts.Add(_studReceipt);
                }

                _totalAmount = _totalCourseFee + _totalST;
                _dbRegn.STPercentage = Convert.ToDouble(_newST);
                _dbRegn.TotalDuration = _mdlCourseFullEdit.TotalDuration;
                _dbRegn.TotalSTAmount = _totalST;
                _dbRegn.TotalCourseFee = _totalCourseFee;
                _dbRegn.TotalAmount = _totalAmount;
                _dbRegn.FeeMode = _mdlCourseFullEdit.StudentReceipt_CourseFullEdit.Count <= 2 ? (int)EnumClass.InstallmentType.SINGLE : (int)EnumClass.InstallmentType.INSTALLMENT;
                _dbRegn.NoOfInstallment = _dbRegn.FeeMode == (int)EnumClass.InstallmentType.SINGLE ? 0 : _mdlCourseFullEdit.StudentReceipt_CourseFullEdit.Count;
                _dbRegn.MultiCourseIDs = string.Join(",", _lstCourseFullEditMultiCourseId
                                                               .Where(mc => !string.IsNullOrEmpty(mc))
                                                               .Select(mc => Int32.Parse(mc))
                                                               .OrderBy(mc => mc)
                                                               .ToList());
                _dbRegn.Discount = _mdlCourseFullEdit.Discount;

                //removing non feedback courses 
                foreach (var _existingFeedback in _dbRegn.StudentFeedbacks.Where(f => f.IsFeedbackGiven == false).ToList())
                {
                    _db.StudentFeedbacks.Remove(_existingFeedback);
                }

                //joining courses of full cours edit
                _courseFullEditCourseIds = string.Join(",", _mdlCourseFullEdit.StudentRegistrationCourse_CourseFullEdit
                                                        .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                                        .Select(rc => rc.Course.Id));


                //adding newly added course in studentfeedback
                string[] _arrCourseId = _courseFullEditCourseIds
                                        .Split(',')
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .OrderBy(x => x.ToString()).ToArray();
                foreach (var _newCourseId in _arrCourseId)
                {
                    StudentFeedback _studentFeedback = _dbRegn.StudentFeedbacks
                                                    .Where(f => f.Course.Id == Convert.ToInt32(_newCourseId))
                                                    .FirstOrDefault();
                    //adding new feedback
                    if (_studentFeedback == null)
                    {
                        _newFeedbackCourseIds.Add(Convert.ToInt32(_newCourseId));
                    }
                }

                //adding new coursedids to feedback
                foreach (var _newFeedbackCourseId in _newFeedbackCourseIds)
                {
                    StudentFeedback _newFeedback = new StudentFeedback();
                    _newFeedback.CourseId = Convert.ToInt32(_newFeedbackCourseId);
                    _dbRegn.StudentFeedbacks.Add(_newFeedback);
                }

                //updating student feedback
                var m = 0;
                var count = _dbRegn.StudentFeedbacks.Count();
                int _totalCourseAmt = 0;
                foreach (var feedback in _dbRegn.StudentFeedbacks)
                {
                    // if not is last feedback then normal procedure is followed
                    if (++m != count)
                    {
                        //Gets each course in feedback
                        var _course = _db.Courses
                                     .Where(c => c.Id == feedback.CourseId).FirstOrDefault();

                        var _currDiscount = _dbRegn_History.Discount.Value;//getting the old discount value
                        var _currDiscPercentage = Convert.ToDecimal(100 - _currDiscount) / 100;
                        var _currCourseAmt = _mdlCourseFullEdit.StudentReceipt_CourseFullEdit.Count() <= 2 ? _course.SingleFee : _course.InstallmentFee;
                        var _currTotalAmt = Convert.ToInt32(_currCourseAmt * _currDiscPercentage);
                        _totalCourseAmt = Convert.ToInt32(_totalCourseAmt + _currTotalAmt);

                        feedback.TotalCourseAmount = _currTotalAmt;
                    }
                    //if last item courseamount in feedback=TotalCourseFee-sum(individual course amount)
                    else
                    {
                        feedback.TotalCourseAmount = _totalCourseFee - _totalCourseAmt;
                    }
                }

                #endregion

                #region Adding approval

                CourseFullEdit_Approval _dbCourseFullEditApproval = _dbCourseFullEdit.CourseFullEdit_Approval.FirstOrDefault();
                _dbCourseFullEditApproval.ApprovedBy = Convert.ToInt32(Session["LoggedUserId"]);
                _dbCourseFullEditApproval.CourseCombination_CrossChecked = mdlCourseFullEditApproval.IsCourseCombination_CrossChecked;
                _dbCourseFullEditApproval.CourseFees_CrossChecked = mdlCourseFullEditApproval.IsCourseFees_CrossChecked;
                _dbCourseFullEditApproval.Customer_CrossChecked = mdlCourseFullEditApproval.IsCustomer_CrossChecked;
                _dbCourseFullEditApproval.DiscountDetails_CrossChecked = mdlCourseFullEditApproval.IsDiscountDetails_CrossChecked;
                _dbCourseFullEditApproval.Reason_CrossChecked = mdlCourseFullEditApproval.IsReason_CrossChecked;
                _dbCourseFullEditApproval.RejectedReason = mdlCourseFullEditApproval.RejectedReason;
                _dbCourseFullEditApproval.TransactionDate = Common.LocalDateTime();
                _dbCourseFullEditApproval.IsApproved = true;               

                #endregion

                _db.Entry(_dbRegn).State = EntityState.Modified;
                int j = _db.SaveChanges();
                if (j > 0)
                {
                    _successMsg.RegistrationId = _dbRegn.Id;
                    _successMsg.Status = "success";

                    return Json(_successMsg, JsonRequestBehavior.AllowGet);
                }


                _successMsg.Status = "error";
                return Json(_successMsg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _successMsg.Status = ex.InnerException.ToString();
                return Json(_successMsg, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CourseFullEditApproval_Reject(CourseFullEditApprovalVM mdlCourseFullEditApproval)
        {
            SuccessMessage _successMsg = new SuccessMessage();
            try
            {
                if (ModelState.IsValid)
                {
                    StudentRegistration_CourseFullEdit _dbCourseFullEdit = _db.StudentRegistration_CourseFullEdit
                                                                     .Where(r => r.Id == mdlCourseFullEditApproval.CourseFullEditID)
                                                                     .FirstOrDefault();

                    CourseFullEdit_Approval _dbCourseFullEditApproval = _dbCourseFullEdit.CourseFullEdit_Approval.FirstOrDefault();
                    _dbCourseFullEditApproval.ApprovedBy = Convert.ToInt32(Session["LoggedUserId"]);
                    _dbCourseFullEditApproval.CourseCombination_CrossChecked = mdlCourseFullEditApproval.IsCourseCombination_CrossChecked;
                    _dbCourseFullEditApproval.CourseFees_CrossChecked = mdlCourseFullEditApproval.IsCourseFees_CrossChecked;
                    _dbCourseFullEditApproval.Customer_CrossChecked = mdlCourseFullEditApproval.IsCustomer_CrossChecked;
                    _dbCourseFullEditApproval.DiscountDetails_CrossChecked = mdlCourseFullEditApproval.IsDiscountDetails_CrossChecked;
                    _dbCourseFullEditApproval.Reason_CrossChecked = mdlCourseFullEditApproval.IsReason_CrossChecked;
                    _dbCourseFullEditApproval.RejectedReason = mdlCourseFullEditApproval.RejectedReason;
                    _dbCourseFullEditApproval.TransactionDate = Common.LocalDateTime();
                    _dbCourseFullEditApproval.IsApproved = false;

                    

                    _db.Entry(_dbCourseFullEdit).State = EntityState.Modified;
                    int j = _db.SaveChanges();
                    if (j > 0)
                    {
                        _successMsg.RegistrationId = _dbCourseFullEdit.Id;
                        _successMsg.Status = "success";

                        return Json(_successMsg, JsonRequestBehavior.AllowGet);
                    }

                }


                _successMsg.Status = "error";
                return Json(_successMsg, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                _successMsg.Status = ex.InnerException.ToString();
                return Json(_successMsg, JsonRequestBehavior.AllowGet);
            }
        }

        //send email function
        public JsonResult Send_CourseFullEditApproval_Email(int regId)
        {
            string _tdString = string.Empty;
            string _styleContent = string.Empty;           
            string _emailSubject = string.Empty;           
            string _courseCombination_NewValue=string.Empty;
            string _courseCombination_OldValue=string.Empty;
            string _courseFee_NewValue = string.Empty;
            string _courseFee_OldValue = string.Empty;
            string _courseCode_NewValue = string.Empty;
            string _courseCode_OldValue=string.Empty;
            string _rejectedReason = string.Empty;

            List<string> _emailList = new List<string>();
            Common _cmn = new Common();
            StudentRegistration_CourseFullEdit _dbCourseFullEdit = new StudentRegistration_CourseFullEdit();
            StudentRegistration_History _dbHistory = new StudentRegistration_History();

            try
            {
                StudentRegistration _studReg = _db.StudentRegistrations
                                             .Where(r => r.Id == regId).FirstOrDefault();

                string _studentId = _studReg.RegistrationNumber;
                string _studentName = _studReg.StudentWalkInn.CandidateName;
                string _studentMobNo = _studReg.StudentWalkInn.MobileNo;
                string _croName = _studReg.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ? _studReg.StudentWalkInn.Employee1.Name :
                                _studReg.StudentWalkInn.Employee1.Name + "," + _studReg.StudentWalkInn.Employee2.Name;
                _dbCourseFullEdit = _studReg.StudentRegistration_CourseFullEdit.FirstOrDefault();
                _dbHistory = _studReg.StudentRegistration_History.FirstOrDefault();
                int _centreId = _studReg.StudentWalkInn.CenterCode.Id;

                #region Approval section
                //if course full edit is approved
                if (_dbCourseFullEdit.CourseFullEdit_Approval.FirstOrDefault().IsApproved == true)
                {
                    _emailSubject = "Course Full Edit Approved";
                    _styleContent = "display:none";
                    //Taking details from student registration
                    _courseCombination_NewValue=string.Join(",", _studReg.StudentRegistrationCourses
                                                                 .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                 .Select(mcd => mcd.Course.Name)));
                    _courseFee_NewValue = _studReg.StudentReceipts.Sum(r => r.Fee.Value).ToString();
                    _courseCode_NewValue = string.Join(",", _studReg.StudentRegistrationCourses
                                                                 .Select(c => c.MultiCourse.CourseCode));

                    //Taking details from student history
                    _courseCombination_OldValue = string.Join(",", _dbHistory.StudentRegistrationCourse_History
                                                                   .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                   .Select(mcd => mcd.Course.Name)));               
                    _courseFee_OldValue = _dbHistory.StudentReceipt_History.Sum(r => r.Fee.Value).ToString();
                    _courseCode_OldValue = string.Join(",", _dbHistory.StudentRegistrationCourse_History
                                                            .Select(c => c.MultiCourse.CourseCode));

                    #region EmailAdding section
                    string _studentEmail = _studReg.StudentWalkInn.EmailId;

                    string _croEmailId = _studReg.StudentWalkInn.CROCount == (int)EnumClass.CROCount.ONE ? _studReg.StudentWalkInn.Employee1.EmailId :
                       _studReg.StudentWalkInn.Employee1.EmailId + "," + _studReg.StudentWalkInn.Employee2.EmailId;

                    string _mgrEmailId = string.Join(",", _cmn.GetEmployee_Centrewise(_centreId, (int)EnumClass.Designation.MANAGERSALES)
                                                                        .Select(e => e.EmailId)
                                                                         .Distinct()
                                                                        .ToList());

                    string _centreMgrEmailId = string.Join(",", _cmn.GetEmployee_Centrewise(_centreId, (int)EnumClass.Designation.CENTREMANAGER)
                                                                        .Select(e => e.EmailId)
                                                                         .Distinct()
                                                                        .ToList());
                    string _centreHeadEmailId = _studReg.StudentWalkInn.CenterCode.CenterCodePartnerDetails.FirstOrDefault().EmailId;
                    string _edEmailId = string.Join(",", _cmn.GetEmployee_Centrewise(_centreId, (int)EnumClass.Designation.ED)
                                                                        .Select(e => e.EmailId)
                                                                         .Distinct()
                                                                        .ToList());
                    _emailList.Add(_studentEmail);
                    _emailList.Add(_croEmailId);
                    _emailList.Add(_mgrEmailId);
                    _emailList.Add(_centreMgrEmailId);
                    _emailList.Add(_centreHeadEmailId);
                    _emailList.Add(_edEmailId);
                    
                    
                    #endregion
                }
                #endregion

                #region Rejected Section
                else
                {
                    _emailSubject = "Course Full Edit Rejected";
                    _rejectedReason = _dbCourseFullEdit.CourseFullEdit_Approval.FirstOrDefault().RejectedReason.ToUpper();
                    //Taking details from student course full edit
                    _courseCombination_NewValue = string.Join(",", _dbCourseFullEdit.StudentRegistrationCourse_CourseFullEdit
                                                                   .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                   .Select(mcd => mcd.Course.Name)));
                    _courseFee_NewValue = _dbCourseFullEdit.StudentReceipt_CourseFullEdit.Sum(r => r.Fee.Value).ToString();
                    _courseCode_NewValue = string.Join(",", _dbCourseFullEdit.StudentRegistrationCourse_CourseFullEdit
                                                            .Select(c => c.MultiCourse.CourseCode));

                    //Taking details from student registration
                    _courseCombination_OldValue = string.Join(",", _studReg.StudentRegistrationCourses
                                                                 .SelectMany(c => c.MultiCourse.MultiCourseDetails
                                                                 .Select(mcd => mcd.Course.Name)));
                    _courseFee_OldValue = _studReg.StudentReceipts.Sum(r => r.Fee.Value).ToString();
                    _courseCode_OldValue = string.Join(",", _studReg.StudentRegistrationCourses
                                                                 .Select(c => c.MultiCourse.CourseCode));

                    #region EmailAdding section
                   
                    string _mgrEmailId = string.Join(",", _cmn.GetEmployee_Centrewise(_centreId, (int)EnumClass.Designation.MANAGERSALES)
                                                                        .Select(e => e.EmailId)
                                                                         .Distinct()
                                                                        .ToList());

                    string _centreMgrEmailId = string.Join(",", _cmn.GetEmployee_Centrewise(_centreId, (int)EnumClass.Designation.CENTREMANAGER)
                                                                        .Select(e => e.EmailId)
                                                                         .Distinct()
                                                                        .ToList());
                    string _centreHeadEmailId = _studReg.StudentWalkInn.CenterCode.CenterCodePartnerDetails.FirstOrDefault().EmailId;
                    string _edEmailId = string.Join(",", _cmn.GetEmployee_Centrewise(_centreId, (int)EnumClass.Designation.ED)
                                                                        .Select(e => e.EmailId)
                                                                         .Distinct()
                                                                        .ToList());

                    _emailList.Add(_mgrEmailId);
                    _emailList.Add(_centreMgrEmailId);
                    _emailList.Add(_centreHeadEmailId);
                    _emailList.Add(_edEmailId);
                   
                    
                    #endregion


                }
                #endregion

                #region Course Details Adding section
                //Adding coursefee section
                _tdString = _tdString + "<tr><td>Fees</td>";
                _tdString = _tdString + "<td>" + _courseFee_NewValue + "</td>";
                _tdString = _tdString + "<td>" + _courseFee_OldValue + "</td><tr>";

                //Adding coursediscount section
                _tdString = _tdString + "<tr><td>CourseCode</td>";
                _tdString = _tdString + "<td>" + _courseCode_NewValue + "</td>";
                _tdString = _tdString + "<td>" + _courseCode_OldValue + "</td><tr>";

                //Adding coursecombination section
                _tdString = _tdString + "<tr><td>Course</td>";
                _tdString = _tdString + "<td>" + _courseCombination_NewValue + "</td>";
                _tdString = _tdString + "<td>" + _courseCombination_OldValue + "</td><tr>";
                #endregion

                //Mail sending code
                string _body = string.Empty;

                using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/CourseFullEditApproval.html")))
                {
                    _body = reader.ReadToEnd();
                }

                _body = _body.Replace("{StudentID}", _studentId);
                _body = _body.Replace("{StudentName}", _studentName);
                _body = _body.Replace("{StudentMobileNo}", _studentMobNo);
                _body = _body.Replace("{CroName}", _croName);
                _body = _body.Replace("{RejectedReason}", _rejectedReason);
                _body = _body.Replace("{CourseDetailsContent}", _tdString);
                _body = _body.Replace("{styleContent}", _styleContent);
                _body = _body.Replace("{EmailSubject}", _emailSubject);
                


                //Email sending
                var isMailSend = _cmn.SendEmailViaGmail(string.Join(",", _emailList.Distinct().ToList()), _body, _emailSubject);
                if (isMailSend)
                    return Json("success", JsonRequestBehavior.AllowGet);
                else
                    return Json("error", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.Message, JsonRequestBehavior.AllowGet);
            }
        }


        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
