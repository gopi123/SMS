
using iTextSharp.text;
using iTextSharp.text.pdf;
using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class ProjectReportApprovalController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();

        #region TrainerApproval

        public class TrainerDenyReason
        {
            public string QuestionNo { get; set; }
            public string Question { get; set; }
            public string Reason { get; set; }
        }

        public class dtApprovalList
        {
            public string SlNo { get; set; }
            public string UploadedDate { get; set; }
            public string StudentID { get; set; }
            public string StudentName { get; set; }
            public string Course { get; set; }
            public int Id { get; set; }

        }

        //Get Trainer Approval List
        public ActionResult TrainerApprovalList()
        {
            return View();
        }

        //GET DataTable
        public JsonResult GetDataTable()
        {

            try
            {
                Common _cmn = new Common();
                var _currEmpId = Convert.ToInt32(Session["LoggedUserId"].ToString());
                List<dtApprovalList> _approvalList = new List<dtApprovalList>();
                List<int> _waitingFeedbackIds = new List<int>();
                List<int> _deniedFeedbackIds = new List<int>();
                List<int> _designationIdList = new List<int>();
                List<int> _centreIdList = new List<int>();
                List<int> _empIdList = new List<int>();

                var _empDesignationId = _db.Employees
                                        .Where(e => e.Id == _currEmpId)
                                        .FirstOrDefault().DesignationId;

                //Get all centers allotted to an employee
                _centreIdList = _cmn.GetCenterEmpwise(_currEmpId)
                                   .Select(c => c.Id).ToList();

                if (_empDesignationId == (int)EnumClass.Designation.TECHNICALLEADER_TI || _empDesignationId == (int)EnumClass.Designation.CPDHEAD)
                {
                    //if loggined user designation is cpdhead
                    if (_empDesignationId == (int)EnumClass.Designation.CPDHEAD)
                    {

                        _designationIdList = _cmn.GetDesignationList((int)EnumClass.Role.CPD)
                                            .Select(d => d.Id).ToList();

                    }
                    //if loggined user is technical leader
                    else
                    {
                        _designationIdList = _cmn.GetDesignationList((int)EnumClass.Role.TECHNICALINDIVIDUAL)
                                           .Select(d => d.Id).ToList();

                    }

                    //Get all (technical individuals or cpd) of a concerned center who is resigned
                    _empIdList = _db.EmployeeCenters
                                          .Where(ec => _centreIdList.Contains(ec.CenterCode.Id) && ec.Employee.Status == false && _designationIdList.Contains(ec.Employee.Designation.Id))
                                          .Select(ec => ec.Employee.Id).ToList();

                    //Adding current loggined user employeeid to the list for getting his student feedback details
                    _empIdList.Add(_currEmpId);

                    //All waiting feedbackids
                    _waitingFeedbackIds = _db.StudentFeedbacks
                                          .Where(f => _empIdList.Contains(f.InstructorID.Value) && f.IsProjectUploaded == true && f.IsTrainerVerified == false)
                                         .Select(f => f.Id).ToList();

                    //All denied feedbackIds
                    _deniedFeedbackIds = _db.StudentFeedbacks
                                          .Where(f => _empIdList.Contains(f.InstructorID.Value) && f.IsLeaderVerified == true && f.StudentProjectApprovals.FirstOrDefault().IsLeaderApproved == false)
                                         .Select(f => f.Id).ToList();

                    //Joining waiting feedbackIds with denied feedbackIds
                    _waitingFeedbackIds.AddRange(_deniedFeedbackIds);

                    _approvalList = _db.StudentFeedbacks
                                       .AsEnumerable()
                                       .Where(f => _waitingFeedbackIds.Contains(f.Id))
                                       .OrderByDescending(x => x.Id)
                                       .Select(f => new dtApprovalList
                                       {
                                           SlNo = "",
                                           UploadedDate = f.ProjectUploadDate.Value.Date.ToString("dd/MM/yyyy"),
                                           StudentID = f.StudentRegistration.RegistrationNumber,
                                           StudentName = f.StudentRegistration.StudentWalkInn.CandidateName,
                                           Course = f.Course.Name,
                                           Id = f.Id
                                       }).ToList();

                }

                else
                {
                    //getting waiting feedbackIds
                    _waitingFeedbackIds = _db.StudentFeedbacks
                                        .Where(f => f.InstructorID == _currEmpId && f.IsProjectUploaded == true && f.IsTrainerVerified == false)
                                        .Select(f => f.Id).ToList();

                    //getting denied feedbackIds
                    _deniedFeedbackIds = _db.StudentFeedbacks
                                        .Where(f => f.InstructorID == _currEmpId && f.IsLeaderVerified == true && f.StudentProjectApprovals.FirstOrDefault().IsLeaderApproved == false)
                                        .Select(f => f.Id).ToList();

                    //Merging waiting feedbackIds with denied feedbackIds
                    _waitingFeedbackIds.AddRange(_deniedFeedbackIds);

                    _approvalList = _db.StudentFeedbacks
                                        .AsEnumerable()
                                        .Where(f => _waitingFeedbackIds.Contains(f.Id))
                                        .OrderByDescending(x => x.Id)
                                        .Select(f => new dtApprovalList
                                        {
                                            SlNo = "",
                                            UploadedDate = f.ProjectUploadDate.Value.Date.ToString("dd/MM/yyyy"),
                                            StudentID = f.StudentRegistration.RegistrationNumber,
                                            StudentName = f.StudentRegistration.StudentWalkInn.CandidateName,
                                            Course = f.Course.Name,
                                            Id = f.Id
                                        }).ToList();
                }

                return Json(new { data = _approvalList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TrainerApproval(int feedbackId)
        {
            try
            {
                var _dbFeedback = _db.StudentFeedbacks
                                .Where(f => f.Id == feedbackId)
                                .FirstOrDefault();
                var _dbEmployee = _db.Employees
                                .AsEnumerable()
                                .Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                .FirstOrDefault();
                var _trainerApproval = new TrainerApprovalVM
                {
                    FeedbackId = _dbFeedback.Id,
                    StudentName = _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName,
                    StudentID = _dbFeedback.StudentRegistration.RegistrationNumber,
                    ProjectDownloadUrl = _dbFeedback.ProjectUploadURL,
                    CourseName = _dbFeedback.Course.Name,
                    InstructorName = _dbEmployee.Name

                };
                return View(_trainerApproval);
            }
            catch (Exception ex)
            {
                return RedirectToAction("TrainerApprovalList");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrainerApproval(TrainerApprovalVM mdlTrainerApprovalVM)
        {
            try
            {

                if (mdlTrainerApprovalVM.IsCrossChecked == true)
                {
                    ModelState.Remove("CrossCheckedDenyReason");
                }
                if (mdlTrainerApprovalVM.IsProjectDifferentFromLabExercise == true)
                {
                    ModelState.Remove("ProjectDifferentFromLabExerciseDenyReason");
                }
                if (mdlTrainerApprovalVM.IsProjectNotCopied == true)
                {
                    ModelState.Remove("ProjectIsNotCopiedDenyReason");
                }
                if (mdlTrainerApprovalVM.IsSourceCodeCollected == true)
                {
                    ModelState.Remove("SourceCodeCollectedDenyReason");
                }
                if (mdlTrainerApprovalVM.IsUploadedSameCourseProjectReport == true)
                {
                    ModelState.Remove("UploadedSameCourseProjectReportDenyReason");
                }
                if (ModelState.IsValid)
                {
                    Common _cmn = new Common();
                    List<TrainerDenyReason> _trainerDenyList = new List<TrainerDenyReason>();
                    List<string> _emailList = new List<string>();

                    string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                    string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                    bool isApproved = true;
                    int i = 0;
                    string _higherRankEmail = string.Empty;


                    var _dbFeedback = _db.StudentFeedbacks
                             .Where(f => f.Id == mdlTrainerApprovalVM.FeedbackId)
                             .FirstOrDefault();

                    if (_dbFeedback != null)
                    {

                        using (TransactionScope _ts = new TransactionScope())
                        {
                            //if student feedback is denied by leader remove that row
                            var _studentProjectApproval = _dbFeedback.StudentProjectApprovals.FirstOrDefault();
                            if (_studentProjectApproval != null)
                            {
                                _db.StudentProjectApprovals.Remove(_studentProjectApproval);
                            }


                            StudentProjectApproval _dbApprovalTrainer = new StudentProjectApproval();
                            _dbApprovalTrainer.StudentFeedbackID = mdlTrainerApprovalVM.FeedbackId;
                            _dbApprovalTrainer.TrainerID = Convert.ToInt32(Session["LoggedUserId"].ToString());
                            _dbApprovalTrainer.IsCrossChecked = mdlTrainerApprovalVM.IsCrossChecked;
                            _dbApprovalTrainer.IsProjectDifferentFromLabExercise = mdlTrainerApprovalVM.IsProjectDifferentFromLabExercise;
                            _dbApprovalTrainer.IsProjectNotCopied = mdlTrainerApprovalVM.IsProjectNotCopied;
                            _dbApprovalTrainer.IsSourceCodeCollected = mdlTrainerApprovalVM.IsSourceCodeCollected;
                            _dbApprovalTrainer.IsUploadedSameCourseProjectReport = mdlTrainerApprovalVM.IsUploadedSameCourseProjectReport;
                            _dbApprovalTrainer.TrainerFeedbackDate = Common.LocalDateTime();
                            

                            if (mdlTrainerApprovalVM.IsCrossChecked == false)
                            {
                                i = i + 1;
                                isApproved = false;
                                _dbApprovalTrainer.CrossCheckedDenyReason = mdlTrainerApprovalVM.CrossCheckedDenyReason.ToUpper();
                                TrainerDenyReason _trainerDenyReason = new TrainerDenyReason();
                                _trainerDenyReason.QuestionNo = i.ToString();
                                _trainerDenyReason.Question = "Cross checked project and found working".ToUpper();
                                _trainerDenyReason.Reason = mdlTrainerApprovalVM.CrossCheckedDenyReason.ToUpper();
                                _trainerDenyList.Add(_trainerDenyReason);

                            }
                            if (mdlTrainerApprovalVM.IsProjectDifferentFromLabExercise == false)
                            {
                                i = i + 1;
                                isApproved = false;
                                _dbApprovalTrainer.ProjectDifferentFromLabExerciseDenyReason = mdlTrainerApprovalVM.ProjectDifferentFromLabExerciseDenyReason.ToUpper();
                                TrainerDenyReason _trainerDenyReason = new TrainerDenyReason();
                                _trainerDenyReason.QuestionNo = i.ToString();
                                _trainerDenyReason.Question = "Project is different from lab exercise".ToUpper();
                                _trainerDenyReason.Reason = mdlTrainerApprovalVM.ProjectDifferentFromLabExerciseDenyReason.ToUpper();
                                _trainerDenyList.Add(_trainerDenyReason);
                            }
                            if (mdlTrainerApprovalVM.IsProjectNotCopied == false)
                            {
                                i = i + 1;
                                isApproved = false;
                                _dbApprovalTrainer.ProjectIsNotCopiedDenyReason = mdlTrainerApprovalVM.ProjectIsNotCopiedDenyReason.ToUpper();
                                TrainerDenyReason _trainerDenyReason = new TrainerDenyReason();
                                _trainerDenyReason.QuestionNo = i.ToString();
                                _trainerDenyReason.Question = "Project is not copied from internet/others".ToUpper();
                                _trainerDenyReason.Reason = mdlTrainerApprovalVM.ProjectIsNotCopiedDenyReason.ToUpper();
                                _trainerDenyList.Add(_trainerDenyReason);
                            }
                            if (mdlTrainerApprovalVM.IsSourceCodeCollected == false)
                            {
                                i = i + 1;
                                isApproved = false;
                                _dbApprovalTrainer.SourceCodeCollectedDenyReason = mdlTrainerApprovalVM.SourceCodeCollectedDenyReason.ToUpper();
                                TrainerDenyReason _trainerDenyReason = new TrainerDenyReason();
                                _trainerDenyReason.QuestionNo = i.ToString();
                                _trainerDenyReason.Question = "Collected source code from student project".ToUpper();
                                _trainerDenyReason.Reason = mdlTrainerApprovalVM.SourceCodeCollectedDenyReason.ToUpper();
                                _trainerDenyList.Add(_trainerDenyReason);
                            }
                            if (mdlTrainerApprovalVM.IsUploadedSameCourseProjectReport == false)
                            {
                                i = i + 1;
                                isApproved = false;
                                _dbApprovalTrainer.UploadedSameCourseProjectReportDenyReason = mdlTrainerApprovalVM.UploadedSameCourseProjectReportDenyReason.ToUpper();
                                TrainerDenyReason _trainerDenyReason = new TrainerDenyReason();
                                _trainerDenyReason.QuestionNo = i.ToString();
                                _trainerDenyReason.Question = "Uploaded the same course project report".ToUpper();
                                _trainerDenyReason.Reason = mdlTrainerApprovalVM.UploadedSameCourseProjectReportDenyReason.ToUpper();
                                _trainerDenyList.Add(_trainerDenyReason);
                            }
                            _dbApprovalTrainer.IsTrainerApproved = isApproved;
                            //if the project report is denied then send email to the students
                            if (!isApproved)
                            {

                                var _loggedEmpDesignationId = _db.Employees
                                                             .AsEnumerable()
                                                             .Where(e => e.Id == Int32.Parse(Session["LoggedUserId"].ToString()))
                                                             .FirstOrDefault().DesignationId;
                                //If leader has rejected mail should be send to current manager
                                if (_loggedEmpDesignationId == (int)EnumClass.Designation.TECHNICALLEADER_TI)
                                {
                                    _higherRankEmail = _cmn.GetEmployee_Designation_CenterWise(_dbFeedback.StudentRegistration.StudentWalkInn.CenterCode.Id.ToString(), (int)EnumClass.Designation.MANAGERSALES)
                                                      .FirstOrDefault().OfficialEmailId;
                                }
                                //if trainer has rejected mail should be send to current leader
                                else
                                {
                                    _higherRankEmail = _cmn.GetEmployee_Designation_CenterWise(_dbFeedback.StudentRegistration.StudentWalkInn.CenterCode.Id.ToString(), (int)EnumClass.Designation.TECHNICALLEADER_TI)
                                                      .FirstOrDefault().OfficialEmailId;
                                }

                                _emailList.Add(_dbFeedback.StudentRegistration.StudentWalkInn.EmailId);
                                _emailList.Add(_higherRankEmail);

                                var isEmailSend = SendProjectDeniedMail(_trainerDenyList, _dbFeedback.Course.Name, _dbFeedback.ProjectTitle.ToUpper(), _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName,
                                                                      _dbFeedback.StudentRegistration.RegistrationNumber, _emailList, mdlTrainerApprovalVM.InstructorName);
                                if (!isEmailSend)
                                {
                                    return Json("email_error", JsonRequestBehavior.AllowGet);
                                }
                            }

                            _db.StudentProjectApprovals.Add(_dbApprovalTrainer);
                            int k = _db.SaveChanges();
                            if (k > 0)
                            {
                                //updating student feedback project verified section
                                _dbFeedback.IsTrainerVerified = true;
                                _dbFeedback.IsLeaderVerified = false;
                                _db.Entry(_dbFeedback).State = EntityState.Modified;
                                int l = _db.SaveChanges();
                                if (l > 0)
                                {

                                    int j = _cmn.AddTransactions(actionName, controllerName, "");
                                    if (j > 0)
                                    {
                                        _ts.Complete();
                                        return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                    }
                                }


                            }

                        }

                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public bool SendProjectDeniedMail(List<TrainerDenyReason> lstDenyReasons, string courseName, string projectTitle, string studName, string studID, List<string> studentEmail,
                                           string instructorName)
        {
            string _body = string.Empty;
            var tdString = "";

            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/StudentProjectDeniedTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }

            foreach (var item in lstDenyReasons)
            {
                tdString = tdString + "<tr><td>" + item.QuestionNo + "</td>";
                tdString = tdString + "<td>" + item.Question + "</td>";
                tdString = tdString + "<td style='text-align:center'>DENIED</td>";
                tdString = tdString + "<td>" + item.Reason + "</td></tr>";
            }
            _body = _body.Replace("{StudentID}", studID);
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{CourseName}", courseName);
            _body = _body.Replace("{ProjectReportTitle}", projectTitle);
            _body = _body.Replace("{InstructorName}", instructorName);
            _body = _body.Replace("{DeniedContent}", tdString);

            Common _cmn = new Common();
            //Email sending
            var isMailSend = _cmn.SendEmail(string.Join(",", studentEmail), _body, "Project Report Status");
            return isMailSend;
        }

        public ActionResult ProjectDownload(string pjtDwnldUrl)
        {
            try
            {
                string filePath = Server.MapPath(pjtDwnldUrl);
                var contentType = "application/docx";
                var currentFileName = pjtDwnldUrl.Split('/').Last();
                return File(filePath, contentType, currentFileName);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        #endregion


        #region LeaderApproval
        public class LeaderDenyReason
        {
            public string QuestionNo { get; set; }
            public string Question { get; set; }
            public string Reason { get; set; }
        }

        public ActionResult LeaderApprovalList()
        {
            return View();
        }

        public ActionResult GetDataTable_Leader()
        {
            try
            {
                Common _cmn = new Common();
                int _currEmpId = Convert.ToInt32(Session["LoggedUserId"].ToString());
                var _currEmployee=_db.Employees
                                    .Where(e=>e.Id==_currEmpId)
                                    .FirstOrDefault();
                //Centres allotted to the employee
                List<int> _empCentreId = _currEmployee.EmployeeCenters
                                        .Select(c => c.CenterCode.Id).ToList();
                //Get rank of the employee
                int _currEmpRank = _currEmployee.Designation.Role.Rank.Value;              


                //Get employeeids where rank is greater than or equal to loggined user     

                var _employeeIds=_db.EmployeeCenters
                                .Where(ec=>_empCentreId.Contains(ec.CenterCode.Id) && ec.Employee.Designation.Role.Rank>=_currEmpRank)
                                .Select(ec=>ec.Employee.Id).ToList();

                var _trainerApprovalList = _db.StudentFeedbacks
                                         .AsEnumerable()
                                         .Where(f => f.InstructorID != null && _employeeIds.Contains(f.InstructorID.Value) && f.IsProjectUploaded == true && f.IsTrainerVerified == true && f.IsLeaderVerified==false
                                                && f.StudentProjectApprovals.FirstOrDefault().IsTrainerApproved == true)
                                        .Select(f => new
                                         {
                                             SlNo = "",
                                             UploadedDate = f.ProjectUploadDate.Value.ToString("dd/MM/yyyy"),
                                             VerifiedDate = f.StudentProjectApprovals.FirstOrDefault().TrainerFeedbackDate.Value.ToString("dd/MM/yyyy"),
                                             StudentID = f.StudentRegistration.RegistrationNumber,
                                             StudentName = f.StudentRegistration.StudentWalkInn.CandidateName,
                                             CourseName = f.Course.Name,
                                             InstructorName = f.StudentProjectApprovals.FirstOrDefault().Trainer.Name,
                                             Id = f.Id
                                         }).ToList();

                return Json(new { data = _trainerApprovalList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LeaderApproval(int feedBackId)
        {
            try
            {
                var _dbFeedback = _db.StudentFeedbacks
                                .Where(f => f.Id == feedBackId)
                                .FirstOrDefault();
                var _mdlLeaderApproval = new LeaderApprovalVM
                {
                    ProjectDownloadUrl = _dbFeedback.ProjectUploadURL,
                    CourseName = _dbFeedback.Course.Name,
                    FeedbackId = _dbFeedback.Id,
                    InstructorName = _dbFeedback.StudentProjectApprovals.FirstOrDefault().Trainer.Name,
                    StudentName = _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName,
                    StudentRegistrationID = _dbFeedback.StudentRegistration.RegistrationNumber,
                    StudentProjectApproval = _dbFeedback.StudentProjectApprovals.FirstOrDefault(),
                    InstructorEmail = _dbFeedback.StudentProjectApprovals.FirstOrDefault().Trainer.OfficialEmailId
                };
                return View(_mdlLeaderApproval);
            }
            catch (Exception ex)
            {
                return RedirectToAction("LeaderApprovalList");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaderApproval(LeaderApprovalVM mdlLeaderApproval)
        {
            try
            {
                var isApproved = true;
                List<LeaderDenyReason> _leaderDenyList = new List<LeaderDenyReason>();
                int i = 0;
                Common _cmn = new Common();
                var status = string.Empty;

                if (mdlLeaderApproval.SourceCodeCollectedFromTrainer == true)
                {
                    ModelState.Remove("SouceCodeCollectedFromTrainerDenialReason");
                }
                if (mdlLeaderApproval.TrainerShownRunningProject == true)
                {
                    ModelState.Remove("TrainerShownRunningProjectDenialReason");
                }

                if (ModelState.IsValid)
                {
                    var _dbStudentProjectApproval = _db.StudentProjectApprovals
                                                .Where(f => f.StudentFeedbackID == mdlLeaderApproval.FeedbackId)
                                                .FirstOrDefault();

                    if (_dbStudentProjectApproval != null)
                    {
                        using (TransactionScope _ts = new TransactionScope())
                        {
                            _dbStudentProjectApproval.LeaderID = Convert.ToInt32(Session["LoggedUserId"].ToString());
                            _dbStudentProjectApproval.SourceCodeCollectedFromTrainer = mdlLeaderApproval.SourceCodeCollectedFromTrainer;
                            _dbStudentProjectApproval.TrainerShownRunningProject = mdlLeaderApproval.TrainerShownRunningProject;
                            _dbStudentProjectApproval.LeaderFeedbackDate = Common.LocalDateTime();

                            if (_dbStudentProjectApproval.SourceCodeCollectedFromTrainer == false)
                            {
                                _dbStudentProjectApproval.SouceCodeCollectedFromTrainerDenialReason = mdlLeaderApproval.SouceCodeCollectedFromTrainerDenialReason.ToUpper();
                                i = i + 1;
                                isApproved = false;
                                LeaderDenyReason _leaderDenyReason = new LeaderDenyReason();
                                _leaderDenyReason.QuestionNo = i.ToString();
                                _leaderDenyReason.Question = "Source Code Collected".ToUpper();
                                _leaderDenyReason.Reason = mdlLeaderApproval.SouceCodeCollectedFromTrainerDenialReason.ToUpper();
                                _leaderDenyList.Add(_leaderDenyReason);
                            }
                            if (_dbStudentProjectApproval.TrainerShownRunningProject == false)
                            {
                                _dbStudentProjectApproval.TrainerShownRunningProjectDenialReason = mdlLeaderApproval.TrainerShownRunningProjectDenialReason.ToUpper();
                                i = i + 1;
                                isApproved = false;
                                LeaderDenyReason _leaderDenyReason = new LeaderDenyReason();
                                _leaderDenyReason.QuestionNo = i.ToString();
                                _leaderDenyReason.Question = "Trainer has shown the running project".ToUpper();
                                _leaderDenyReason.Reason = mdlLeaderApproval.TrainerShownRunningProjectDenialReason.ToUpper();
                                _leaderDenyList.Add(_leaderDenyReason);
                            }
                            _dbStudentProjectApproval.IsLeaderApproved = isApproved;

                            _db.Entry(_dbStudentProjectApproval).State = EntityState.Modified;
                            int j = _db.SaveChanges();
                            if (j > 0)
                            {
                                //Modifying dbFeedback details
                                var _dbFeedback = _dbStudentProjectApproval.StudentFeedback;
                                _dbFeedback.IsLeaderVerified = true;
                                _db.Entry(_dbFeedback).State = EntityState.Modified;
                                int k = _db.SaveChanges();
                                if (k > 0)
                                {
                                    //If leader has denied the project send email to concerned manager and instructor
                                    if (!isApproved)
                                    {
                                        List<string> _emailList = new List<string>();
                                        string instructorEmail = mdlLeaderApproval.InstructorEmail;
                                        string managerEmail = _cmn.GetEmployee_Designation_CenterWise(_dbFeedback.StudentRegistration.StudentWalkInn.CenterCode.Id.ToString(), (int)EnumClass.Designation.MANAGERSALES)
                                                            .FirstOrDefault().OfficialEmailId;
                                        _emailList.Add(instructorEmail);
                                        _emailList.Add(managerEmail);

                                        var isEmailSend = SendProjectDeniedEmail_Leader(_leaderDenyList, _dbFeedback.Course.Name, _dbFeedback.ProjectTitle, _dbFeedback.StudentRegistration.RegistrationNumber
                                                                                    , _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName, _dbFeedback.StudentProjectApprovals.FirstOrDefault().Trainer.Name, _emailList);
                                        if (!isEmailSend)
                                        {
                                            status = "error_SendProjectDeniedEmail_Leader";                                            
                                        }
                                        else
                                        {
                                            status = "success";
                                        }

                                    }
                                    //If leader approved the project        
                                    else
                                    {
                                        //if this is the last proje.ct then send certificate to the concerned student
                                        if(IsLastCourseApproved(_dbFeedback.StudentRegistrationId.Value))
                                        {
                                            if (GenerateCertificate(_dbFeedback.StudentRegistrationId.Value))
                                            {
                                                string attachmentLink=Server.MapPath("~/Certificates/"+_dbFeedback.StudentRegistration.RegistrationNumber+".pdf");

                                                if(SendCertificate(_dbFeedback.StudentRegistration.StudentWalkInn.CandidateName.ToUpper(),
                                                                _dbFeedback.StudentRegistration.StudentWalkInn.EmailId, attachmentLink))
                                                {
                                                    if (SendSMS(_dbFeedback.StudentRegistration.StudentWalkInn.MobileNo, _dbFeedback.StudentRegistration.StudentWalkInn.CandidateName.ToUpper()))
                                                    {
                                                        var _dbRegistration = _dbFeedback.StudentRegistration;
                                                        _dbRegistration.IsCertificateIssued = true;
                                                        _db.Entry(_dbRegistration).State = EntityState.Modified;
                                                        int l = _db.SaveChanges();
                                                        if (l > 0)
                                                        {
                                                            status = "success";
                                                        }
                                                        else
                                                        {
                                                            status = "error_RegistrationUpdation";
                                                        }                                               
                                                    }                                                   
                                                }
                                                else
                                                {
                                                    status = "error_email";
                                                }
                                            }
                                            else
                                            {
                                                status = "error_GenerateCertificate";
                                            }
                                        }
                                        else
                                        {
                                            status = "success";                                            
                                        }
                                    }
                                    if (status == "success")
                                    {
                                        _ts.Complete();                                       
                                    }
                                    return Json(new { message = status }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        };

                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }           
        }

        
        private bool IsLastCourseApproved(int registrationId)
        {
            bool isLastProject;

            var _dbFeedback = _db.StudentFeedbacks
                            .Where(f => f.StudentRegistrationId == registrationId)
                            .ToList();
            //gets the number of course submitted by the student
            var _feedbackCount = _dbFeedback.Count();

            //gets the number of project report approved by the leader of that student
            var _isLeaderApprovedCount = _dbFeedback
                                        .Where(f =>  f.IsLeaderVerified == true && f.StudentProjectApprovals.FirstOrDefault().IsLeaderApproved == true)
                                        .Count();

            if (_feedbackCount == _isLeaderApprovedCount)
            {
                isLastProject = true;
            }
            else
            {
                isLastProject = false;
            }
            return isLastProject;
        }

        private bool GenerateCertificate(int registrationId)
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

                _startDate = _dbRegistration.StudentFeedbacks
                              .OrderBy(f => f.CourseStartDate)
                              .First()
                              .CourseStartDate.Value.ToString("dd/MM/yyyy");

                _endDate = _dbRegistration.StudentFeedbacks
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
                        document.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
       
        #endregion


        public bool SendProjectDeniedEmail_Leader(List<LeaderDenyReason> _leaderDenyReason, string courseName, string projectTitle, string studID, string studName, string instructorName, List<string> emailList)
        {
            string _body = string.Empty;
            var tdString = "";

            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/StudentProjectDeniedTemplate_Trainer.html")))
            {
                _body = reader.ReadToEnd();
            }

            foreach (var item in _leaderDenyReason)
            {
                tdString = tdString + "<tr><td>" + item.QuestionNo + "</td>";
                tdString = tdString + "<td>" + item.Question + "</td>";
                tdString = tdString + "<td style='text-align:center'>DENIED</td>";
                tdString = tdString + "<td>" + item.Reason + "</td></tr>";
            }
            _body = _body.Replace("{StudentID}", studID);
            _body = _body.Replace("{StudentName}", studName);
            _body = _body.Replace("{CourseName}", courseName);
            _body = _body.Replace("{ProjectReportTitle}", projectTitle);
            _body = _body.Replace("{InstructorName}", instructorName);
            _body = _body.Replace("{DeniedContent}", tdString);

            Common _cmn = new Common();
            //Email sending
            var isMailSend = _cmn.SendEmail(string.Join(",", emailList), _body, "Project Report Status");
            return isMailSend;
        }

        public bool SendCertificate(string studentName,string studentEmail,string attachment)
        {
            string _body = string.Empty;           

            using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/CertificateMailTemplate.html")))
            {
                _body = reader.ReadToEnd();
            }
            _body = _body.Replace("{StudentName}", studentName);            

            //Email sending
            var isMailSend = SendEmail(studentEmail,_body,attachment);
            return isMailSend;
        }

        public bool SendEmail(string recepeintEmailId, string body, string attachmentLink)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.Subject = "Certificate Generated";
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(recepeintEmailId));
                mailMessage.Attachments.Add(new Attachment(attachmentLink));
                SmtpClient smtp = new SmtpClient();
                smtp.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendEmailTesting(string recepeintEmailId, string body,  string attachmentLink)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["UserName"]);
                mailMessage.Subject = "Certificate Generated";
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                string[] multi = recepeintEmailId.Split(',');
                foreach (string emailId in multi)
                {
                    mailMessage.To.Add(new MailAddress(emailId));
                }
                mailMessage.Attachments.Add(new Attachment(attachmentLink, "Certificate.pdf"));
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

        public bool SendSMS(string mobileNo,string studentName)
        {
            bool result = false;
            //sending message to student
            var _message = "Dear " + studentName + ". Greetings from NetworkzSystems. Your certificate has been processed and you can download it from your registered emailId. ";                      
            Common _cmn = new Common();
            string _result = _cmn.ApiCall("http://sms.networkzsystems.com/sendsms?uname=networkcorp&pwd=netsys123&senderid=NETSYS&to=" + mobileNo + "&msg=" + _message + "&route=T");
            if (!_result.StartsWith("Invalid Username/password") || !_result.StartsWith("Enter valid MobileNo"))
            {
                result = true;
                return result;
            }
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
