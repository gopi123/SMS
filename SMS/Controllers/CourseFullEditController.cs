using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class CourseFullEditController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();

        #region Class Defintion

        public class CourseDetails
        {
            public string CourseCode { get; set; }
            public string CourseTitle { get; set; }
            public string SoftwareUsed { get; set; }
            public int Duration { get; set; }
            public decimal Fee { get; set; }
            public string CourseIds { get; set; }
            public StudentRegistration StudentRegistration { get; set; }
            public int CourseFee_Updated
            {
                get
                {
                    int _newFee = 0;



                    if (StudentRegistration != null)
                    {
                        decimal _discount = StudentRegistration.Discount.Value;
                        decimal _existingCourseFee = StudentRegistration.TotalCourseFee.Value;
                        decimal _newCourseFee = Fee;
                        decimal _currST = currST;
                        FeeCalculation _clsFee = CalculateCourseFee(StudentRegistration, Fee, currST);
                        _newFee = _clsFee.CourseFee;
                        STAmt_Updated = _clsFee.STAmount;
                        Discount_Updated = _clsFee.DiscountPercentage;

                    }
                    else
                    {
                        _newFee = 0;
                        STAmt_Updated = 0;
                        Discount_Updated = 0;
                    }


                    return _newFee;
                }
            }
            public int STAmt_Updated { get; set; }
            public int Discount_Updated { get; set; }
            public int Discount_Allowed
            {
                get
                {
                    return Calculate_MaxDiscAllowed(StudentRegistration, Fee);
                }
            }
            public int FeePaid
            {
                get
                {
                    return StudentRegistration.StudentReceipts
                            .Where(r => r.Status == true)
                            .Sum(r => r.Fee.Value);
                }
            }
            public decimal currST { get; set; }
            public bool IsSingleAndFullyPaid
            {
                get
                {
                    if (StudentRegistration.StudentReceipts.Count() == 2 &&
                        StudentRegistration.StudentReceipts.Where(r => r.Status == true).Count() == 2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public class FeeCalculation
        {
            public int CourseFee { get; set; }
            public int STAmount { get; set; }
            public int DiscountPercentage { get; set; }
        }

        public class SuccessMessage
        {
            public int Id { get; set; }
            public string Status { get; set; }

        }

        #endregion

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /CourseFullEdit/

        public ActionResult CourseFullEdit(int regID)
        {
            try
            {
                Common _cmn = new Common();
                var _dbRegn = _db.StudentRegistrations
                            .Where(r => r.Id == regID)
                            .FirstOrDefault();

                if (_dbRegn != null)
                {
                    int loggedEmpId = Convert.ToInt32(Session["LoggedUserId"]);
                    var _employee = _db.Employees
                                   .Where(e => e.Id == loggedEmpId)
                                   .FirstOrDefault();

                    //Mask email if loggedemployee is not the cro of current student
                    var _studentEmail = _dbRegn.StudentWalkInn.EmailId;
                    if (_employee.Designation.Role.Id == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (_dbRegn.StudentWalkInn.CRO1ID != loggedEmpId || _dbRegn.StudentWalkInn.CRO2ID != loggedEmpId)
                        {
                            _studentEmail = _cmn.MaskString(_studentEmail, "email");
                        }
                    }

                    //Mask mobile if loggedemployee is not the cro of current student
                    var _studentMobile = _dbRegn.StudentWalkInn.MobileNo;
                    if (_employee.Designation.Role.Id == (int)EnumClass.Role.SALESINDIVIDUAL)
                    {
                        if (_dbRegn.StudentWalkInn.CRO1ID != loggedEmpId || _dbRegn.StudentWalkInn.CRO2ID != loggedEmpId)
                        {
                            _studentMobile = _cmn.MaskString(_studentMobile, "mobile");
                        }
                    }


                    //Courses whose feedback has been submitted cannot be downgraded
                    var _feedbackCourseIdList = _dbRegn.StudentFeedbacks
                                                .Where(f => f.IsFeedbackGiven == true)
                                                .Select(c => new
                                                {
                                                    Id = c.Course.Id,
                                                    Name = c.Course.Name
                                                }).ToList();

                    var _paidCount = _dbRegn.StudentReceipts.Where(r => r.Status == true).Count();
                    var _installmentNo = from EnumClass.InstallmentNo b in Enum.GetValues(typeof(EnumClass.InstallmentNo))
                                         where ((int)b > _paidCount)
                                         select new { Id = (int)b, Name = (int)b };
                    var _roundUpList = from EnumClass.RoundUpList r in Enum.GetValues(typeof(EnumClass.RoundUpList))
                                       select new { Id = (int)r, Name = r.ToString().Replace('_', ' ') };
                    var _taxPercentage = _db.ServiceTaxes
                                   .AsEnumerable()
                                   .Where(t => t.FromDate <= Common.LocalDateTime())
                                   .OrderByDescending(t => t.FromDate)
                                   .First().Percentage;

                    var _courseList = _db.Courses
                                   .Select(c => new
                                   {
                                       Id = c.Id,
                                       Name = c.Name
                                   }).ToList();

                    int _courseInterchangeFee = _dbRegn.StudentRegistration_History.Any() ?
                                                     _dbRegn.StudentRegistration_History.Sum(h => h.AdditionalCourseFee.Value) : 0;

                    int _courseInterchangeSTAmt = _dbRegn.StudentRegistration_History.Any() ?
                                                _dbRegn.StudentRegistration_History.Sum(h => h.AdditionalSTAmount.Value) : 0;

                    var _mdlCourseFullEdit = new CourseFullEditVM
                    {
                        StudentID = _dbRegn.RegistrationNumber,
                        Email = _studentEmail,
                        MobileNo = _studentMobile,
                        Name = _dbRegn.StudentWalkInn.CandidateName,
                        StudentRegistration = _dbRegn,
                        Curr_MultiCourseCode = string.Join(",", _dbRegn.StudentRegistrationCourses
                                            .Select(src => src.MultiCourse.CourseCode)),
                        Curr_CourseTitle = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                    .Select(src => src.MultiCourse.CourseSubTitle.Name)),
                        Curr_SoftwareDetails = string.Join(Environment.NewLine, _dbRegn.StudentRegistrationCourses
                                             .SelectMany(src => src.MultiCourse.MultiCourseDetails)
                                             .Select(mcd => mcd.Course.Name)),
                        CourseFeedbackList = new SelectList(_feedbackCourseIdList, "Id", "Name"),
                        CourseFeedbackId = _feedbackCourseIdList.Select(f => f.Id).ToArray(),
                        //InstallmentID = _dbRegn.NoOfInstallment.Value,
                        InstallmentID = _dbRegn.StudentReceipts.Where(r => r.Status == false).Count() == 0 ?
                                        _dbRegn.StudentReceipts.Count() + 1 : _dbRegn.StudentReceipts.Count(),
                        InstallmentType = (_paidCount <= 2 && _dbRegn.StudentReceipts.Count() == 2)
                                            ? EnumClass.InstallmentType.SINGLE : EnumClass.InstallmentType.INSTALLMENT,
                        InstallmentList = new SelectList(_installmentNo, "Id", "Name"),
                        MultiCourseList = new SelectList("", "Id", "CourseCode"),
                        RoundUpList = new SelectList(_roundUpList, "Id", "Name"),
                        RoundUpId = (int)EnumClass.RoundUpList.ROUND_UP,
                        ST = Convert.ToDouble(_taxPercentage),
                        CentreCode = _dbRegn.StudentWalkInn.CenterCode.CentreCode,
                        PrevCourseId = string.Join(",", _dbRegn.StudentRegistrationCourses
                                                      .Select(rc => rc.MultiCourse.Id)),
                        //CourseIds = string.Join(",", _notFeedbackCourseIds.ToList()),
                        StudentReceiptLists = _dbRegn.StudentReceipts.ToList(),
                        StudentMobileNo = _dbRegn.StudentWalkInn.MobileNo,
                        CroName = _dbRegn.StudentWalkInn.CROCount.Value == 1 ? _dbRegn.StudentWalkInn.Employee1.Name :
                                _dbRegn.StudentWalkInn.Employee1.Name + "," + _dbRegn.StudentWalkInn.Employee2.Name,
                        Feedback_CourseIds = string.Join(",", _feedbackCourseIdList.Select(f => f.Id).ToList()),//used for validating purpose
                        Curr_Discount = _dbRegn.Discount.Value,
                        CourseInterchangeFee = _courseInterchangeFee,
                        CourseInterchangeST = _courseInterchangeSTAmt,
                        CourseList = new SelectList(_courseList, "Id", "Name"),
                        Curr_CourseFee = _dbRegn.TotalCourseFee.Value - _courseInterchangeFee,
                        Curr_STAmount = _dbRegn.TotalSTAmount.Value - _courseInterchangeSTAmt,
                        TotalFeePaid = _dbRegn.StudentReceipts.Where(r => r.Status == true).Sum(r => r.Fee.Value)
                    };

                    return View(_mdlCourseFullEdit);
                }
                return RedirectToAction("Index");


            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult CourseFullEdit(CourseFullEditVM mdlCourseFullEdit)
        {
            SuccessMessage _successMsg = new SuccessMessage();
            try
            {
                if (ModelState.IsValid)
                {
                    StudentRegistration_CourseFullEdit _dbCourseFullEdit = new StudentRegistration_CourseFullEdit();
                    _dbCourseFullEdit.CROID = Convert.ToInt32(Session["LoggedUserId"]);
                    _dbCourseFullEdit.Discount = mdlCourseFullEdit.Curr_Discount;
                    _dbCourseFullEdit.Reason = mdlCourseFullEdit.CourseFullEditReason.ToUpper();
                    _dbCourseFullEdit.TotalDuration = mdlCourseFullEdit.Duration;
                    _dbCourseFullEdit.StudentRegistrationID = mdlCourseFullEdit.StudentRegistration.Id;
                    _dbCourseFullEdit.TransactionDate = Common.LocalDateTime();

                    // saving student course details
                    foreach (var courseId in mdlCourseFullEdit.MultiCourseId)
                    {
                        StudentRegistrationCourse_CourseFullEdit _studCourse = new StudentRegistrationCourse_CourseFullEdit();
                        _studCourse.MultiCourseID = Convert.ToInt32(courseId);
                        _dbCourseFullEdit.StudentRegistrationCourse_CourseFullEdit.Add(_studCourse);
                    }

                    //adding new receipt
                    foreach (var _receipt in mdlCourseFullEdit.StudentReceiptLists)
                    {

                        StudentReceipt_CourseFullEdit _studReceipt = new StudentReceipt_CourseFullEdit();
                        _studReceipt.DueDate = _receipt.DueDate;
                        _studReceipt.Fee = _receipt.Fee;
                        _studReceipt.ST = _receipt.ST;
                        _studReceipt.Status = _receipt.Status;
                        _studReceipt.Total = _receipt.Total;
                        _studReceipt.STPercentage = _receipt.STPercentage;
                        _studReceipt.StudentReceiptNo = _receipt.StudentReceiptNo;
                        _studReceipt.ModeOfPayment = _receipt.ModeOfPayment;
                        _dbCourseFullEdit.StudentReceipt_CourseFullEdit.Add(_studReceipt);
                    }

                    _db.StudentRegistration_CourseFullEdit.Add(_dbCourseFullEdit);
                    int j = _db.SaveChanges();
                    if (j > 0)
                    {
                        _successMsg.Id = _dbCourseFullEdit.Id;
                        _successMsg.Status = "success";

                        return Json(_successMsg, JsonRequestBehavior.AllowGet);
                    }
                }
               
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _successMsg.Status = ex.Message;
                return Json(_successMsg, JsonRequestBehavior.AllowGet);
            }
        }

        //Called while selecting multicourse details
        public JsonResult GetCourseDetails(int[] multiCourseId, string feeMode, int regId)
        {

            List<CourseDetails> _courseDetails = new List<CourseDetails>();
            CourseDetails _objCourseDetails = new CourseDetails();
            try
            {

                var _dbRegn = _db.StudentRegistrations
                             .Where(r => r.Id == regId)
                             .FirstOrDefault();


                List<MultiCourse> _lstMultiCourse = new List<MultiCourse>();
                _lstMultiCourse = _db.MultiCourses
                                .Where(mcd => multiCourseId.Contains(mcd.Id))
                                .ToList();
                if (_lstMultiCourse.Count > 0)
                {
                    _objCourseDetails.CourseCode = string.Join(",", _lstMultiCourse.Select(x => x.CourseCode).ToList());
                    _objCourseDetails.CourseTitle = string.Join(",", _lstMultiCourse.Select(x => x.CourseSubTitle.Name.ToUpper()).ToList());
                    _objCourseDetails.Duration = _lstMultiCourse.Sum(x => x.Duration.Value);
                    _objCourseDetails.Fee = feeMode == EnumClass.InstallmentType.SINGLE.ToString() ? _lstMultiCourse.Sum(s => s.SingleFee.Value) : _lstMultiCourse.Sum(s => s.InstallmentFee.Value);
                    _objCourseDetails.SoftwareUsed = string.Join(Environment.NewLine, _lstMultiCourse.SelectMany(x => x.MultiCourseDetails
                                                                    .Select(y => y.Course.Name).ToList()));
                    _objCourseDetails.CourseIds = string.Join(",", _lstMultiCourse.SelectMany(x => x.MultiCourseDetails
                                                                   .Select(y => y.Course.Id).ToList()));
                    _objCourseDetails.StudentRegistration = _dbRegn;
                    _objCourseDetails.currST = _db.ServiceTaxes
                                               .AsEnumerable()
                                               .Where(t => t.FromDate <= Common.LocalDateTime())
                                               .OrderByDescending(t => t.FromDate)
                                               .First().Percentage.Value;
                }
                else
                {
                    _objCourseDetails.CourseCode = "";
                    _objCourseDetails.CourseTitle = "";
                    _objCourseDetails.Duration = 0;
                    _objCourseDetails.Fee = 0;
                    _objCourseDetails.SoftwareUsed = "";
                    _objCourseDetails.CourseIds = "";
                    _objCourseDetails.StudentRegistration = null;
                }

                _courseDetails.Add(_objCourseDetails);


                var _courseUpdatedDetails = _courseDetails
                                         .Select(mc => new
                                         {
                                             CourseCode = mc.CourseCode,
                                             CourseTitle = mc.CourseTitle,
                                             Duration = mc.Duration,
                                             CourseFee = mc.CourseFee_Updated,
                                             STAmount = mc.STAmt_Updated,
                                             DiscountPercentage = mc.Discount_Updated,
                                             SoftwareUsed = mc.SoftwareUsed,
                                             CourseIds = mc.CourseIds,
                                             MaxAllowedDiscount = mc.Discount_Allowed,
                                             FeePaid = mc.FeePaid,
                                             IsSingleAndFullyPaid = mc.IsSingleAndFullyPaid
                                         }).ToList();

                return Json(_courseUpdatedDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        //Calcuation of fees by applying discount
        //Called while selcting multicourse change
        public static FeeCalculation CalculateCourseFee(StudentRegistration studReg, decimal newCourseFee, decimal currST)
        {
            FeeCalculation _clsFeeCalc = new FeeCalculation();
            try
            {

                decimal discount = 0;
                decimal existingPaidCourseFee = studReg.StudentReceipts.Where(r => r.Status == true).Sum(r => r.Fee).Value;
                decimal existingPaidSTAmount = studReg.StudentReceipts.Where(r => r.Status == true).Sum(r => r.ST).Value;
                decimal stAmt = 0;
                decimal currTotalAmt = 0;
                decimal discountAmt = 0;
                decimal newTotalAmt = 0;
                decimal newCourseFee_AftrDiscount = 0;
                decimal newSTAmt_AftrDiscount = 0;

                //TotalAmount Calculation           

                stAmt = Math.Round((newCourseFee) * (currST / 100));
                currTotalAmt = newCourseFee + stAmt;

                //Gets the new discount amount
                discountAmt = Math.Round((currTotalAmt) * (discount / 100));
                //Gets the new Total Amt
                newTotalAmt = currTotalAmt - discountAmt;

                //Applying Roundup
                newTotalAmt = Math.Ceiling((decimal)newTotalAmt / 10) * 10;

                //calculating coursefee from new total amt
                newCourseFee_AftrDiscount = Math.Round((newTotalAmt) / ((100 + currST) / 100));

                //if new fee is less than existing fee then discount will be reduced
                if (newCourseFee_AftrDiscount < existingPaidCourseFee)
                {
                    discount = (100 - ((existingPaidCourseFee * 100) / newCourseFee));
                    newCourseFee_AftrDiscount = existingPaidCourseFee;
                    newSTAmt_AftrDiscount = existingPaidSTAmount;
                }
                else
                {
                    //Gets the new st amt
                    newSTAmt_AftrDiscount = Convert.ToInt32(newTotalAmt - newCourseFee_AftrDiscount);
                }



                _clsFeeCalc.CourseFee = Convert.ToInt32(newCourseFee_AftrDiscount);
                _clsFeeCalc.DiscountPercentage = Convert.ToInt32(discount);
                _clsFeeCalc.STAmount = Convert.ToInt32(newSTAmt_AftrDiscount);
            }
            catch (Exception ex)
            {
                _clsFeeCalc.CourseFee = 0;
                _clsFeeCalc.DiscountPercentage = 0;
                _clsFeeCalc.STAmount = 0;
            }

            return _clsFeeCalc;
        }

        //Called while selecting multicourse change
        public static int Calculate_MaxDiscAllowed(StudentRegistration studReg, decimal newCourseFee)
        {
            try
            {
                decimal _existingPaidFee = studReg.StudentReceipts
                                  .Where(r => r.Status == true)
                                  .Sum(x => x.Fee.Value);

                //Round down to the nearest integer
                decimal _maxAllowedDisc = Math.Floor(((100) - ((_existingPaidFee / newCourseFee) * 100)));

                return Convert.ToInt32(_maxAllowedDisc);
            }
            catch (Exception ex)
            {
                return 0;
            }


        }

        //Called on while changing the percentage of discount
        public JsonResult GetFeeDetailsOnDiscountChange(decimal discount, decimal newCourseFee, decimal currST)
        {
            FeeCalculation _clsFeeCalc = new FeeCalculation();
            try
            {
                decimal stAmt = 0;
                decimal currTotalAmt = 0;
                decimal discountAmt = 0;
                decimal newTotalAmt = 0;
                decimal newCourseFee_AftrDiscount = 0;
                decimal newSTAmt_AftrDiscount = 0;

                //TotalAmount Calculation           

                stAmt = Math.Round((newCourseFee) * (currST / 100));
                currTotalAmt = newCourseFee + stAmt;

                //Gets the new discount amount
                discountAmt = Math.Round((currTotalAmt) * (discount / 100));
                //Gets the new Total Amt
                newTotalAmt = currTotalAmt - discountAmt;

                //Applying Roundup
                newTotalAmt = Math.Ceiling((decimal)newTotalAmt / 10) * 10;

                //calculating coursefee from new total amt
                newCourseFee_AftrDiscount = Math.Round((newTotalAmt) / ((100 + currST) / 100));

                //Gets the new st amt
                newSTAmt_AftrDiscount = Convert.ToInt32(newTotalAmt - newCourseFee_AftrDiscount);

                _clsFeeCalc.CourseFee = Convert.ToInt32(newCourseFee_AftrDiscount);
                _clsFeeCalc.DiscountPercentage = Convert.ToInt32(discount);
                _clsFeeCalc.STAmount = Convert.ToInt32(newSTAmt_AftrDiscount);
            }
            catch (Exception ex)
            {
                _clsFeeCalc.CourseFee = 0;
                _clsFeeCalc.DiscountPercentage = 0;
                _clsFeeCalc.STAmount = 0;
            }

            return Json(_clsFeeCalc, JsonRequestBehavior.AllowGet);
        }


        //send pinno to the student
        public JsonResult SendPinNoToStudent(string studMobileNo, int prevCourseFee, string prevCourseCode, int currCourseFee, string currCourseCode, string studentID, string studentName)
        {
            int pinNo = 0;
            try
            {
                Common _cmn = new Common();
                //Gets RandomNo
                int _randomNo = _cmn.GenerateRandomNo();

                string _message = "Course Full Edit \n" +
                                   studentID + "\n" +
                                   studentName.ToUpper() + "\n" +
                                   "Old: " + prevCourseCode + "," + prevCourseFee + " plus ST" + "\n" +
                                   "New: " + currCourseCode + "," + currCourseFee + " plus ST" + "\n" +
                                   "Agree: " + _randomNo;
                //Sends the 4 digitno to the sutdent mobileNo
                string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + studMobileNo + "&msg=" + _message + " &route=T");
                if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
                {
                    pinNo = _randomNo;
                    return Json(pinNo, JsonRequestBehavior.AllowGet);

                }
                return Json(pinNo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(pinNo, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }



    }
}
