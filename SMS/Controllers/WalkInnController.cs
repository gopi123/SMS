using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMS.Models;
using System.Transactions;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;

namespace SMS.Controllers
{
    public class WalkInnController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();
        //
        // GET: /WalkInn/

        public ActionResult Index()
        {
            Common _cmn = new Common();

            var _finYearList = _cmn.FinancialYearList().ToList()
                                .OrderByDescending(x => x.ToString())
                                .Select(x => new
                                {
                                    Id = x.ToString(),
                                    Name = x.ToString()
                                });
            var _studTypeList = from EnumClass.WalkinnStatus c in Enum.GetValues(typeof(EnumClass.WalkinnStatus))
                                where (c != EnumClass.WalkinnStatus.ALL)
                                select new { Id = c.ToString(), Name = c.ToString() };


            var _monthList = from EnumClass.Month c in Enum.GetValues(typeof(EnumClass.Month))
                             select new { Id = c.ToString(), Name = c.ToString() };
        

            var _indexVM = new IndexVM
            {
                FinancialYearList = new SelectList(_finYearList, "Id", "Name"),
                StudentTypeList = new SelectList(_studTypeList, "Id", "Name"),
                MonthList = new SelectList(_monthList, "Id", "Name"),            
                MonthName = Common.LocalDateTime().ToString("MMMM")
            };
            return View(_indexVM);
        }

        public JsonResult GetDataTable(string finYear, string studType, string month)
        {
            try
            {
                Common _cmn = new Common();
                var arrFinYear = finYear.Split('-');
                var _startFinYear = Convert.ToInt32(arrFinYear[0]);
                var _endFinYear = Convert.ToInt32(arrFinYear[1]);
                DateTime _startFinDate = new DateTime();
                DateTime _endFinDate = new DateTime();
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

                IEnumerable<WalkInnVM.WalkInnDataTable> _dTableWalkInn = new List<WalkInnVM.WalkInnDataTable>();

                //Gets the current role of employee
                var _currentRole = _cmn.GetLoggedUserRoleId(Convert.ToInt32(Session["LoggedUserId"]));

                //Salesindividual can see only the walkinn he/she is added
                if (_currentRole == (int)EnumClass.Role.SALESINDIVIDUAL)
                {
                    _dTableWalkInn = _db.StudentWalkInns
                                    .AsEnumerable()
                                    .Where(x => x.Status == studType
                                        && (x.CRO1ID == Int32.Parse(Session["LoggedUserId"].ToString()) || (x.CRO2ID == Int32.Parse(Session["LoggedUserId"].ToString())))
                                        && (x.TransactionDate.Value.Date >= _startFinDate.Date && x.TransactionDate.Value.Date <= _endFinDate.Date)
                                        && (x.JoinStatus == true))
                                    .OrderByDescending(x => x.Id)
                                    .Select(x => new WalkInnVM.WalkInnDataTable
                                    {
                                        WalkInnDate = x.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        Name = x.CandidateName,
                                        Center = x.CenterCode.CentreCode,
                                        Mobile = x.MobileNo,
                                        CareGiverMobileNo = x.GuardianType == (int)EnumClass.CareGiver.FATHER ? "FATHER :" + x.GuardianContactNo :
                                                          x.GuardianType == (int)EnumClass.CareGiver.GUARDIAN ? "GUARDIAN :" + x.GuardianContactNo :
                                                          x.GuardianType == (int)EnumClass.CareGiver.MOTHER ? "MOTHER :" + x.GuardianContactNo :
                                                                          "SPOUSE :" + x.GuardianContactNo,
                                        SalesPerson = x.CROCount == (int)EnumClass.CROCount.ONE ? x.Employee1.Name : x.Employee1.Name + "," + x.Employee2.Name,
                                        CourseRecommended = x.StudentWalkInnCourses
                                                         .Select(c => c.Course.Name)
                                                         .Aggregate((m, n) => m + "," + n),
                                        ExpJoinDate = x.JoinDate,
                                        Id = x.Id,
                                        Email = x.EmailId,
                                        Status = x.Status

                                    })
                                    .ToList();

                    return Json(new { data = _dTableWalkInn }, JsonRequestBehavior.AllowGet);
                }
                //All others can view walkinn according to the branches allotted to them
                else
                {
                    //Gets all the centerCodeIds allotted to an employee
                    List<int> _centerCodeIds = _cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"]))
                                            .Select(x => x.Id).ToList();


                    _dTableWalkInn = _db.StudentWalkInns
                                    .AsEnumerable()
                                    .Where(x => (x.Status == studType)
                                                && _centerCodeIds.Contains(x.CenterCodeId.Value)
                                                && (x.TransactionDate.Value.Date >= _startFinDate && x.TransactionDate.Value.Date <= _endFinDate))
                                    .OrderByDescending(x => x.Id)
                                    .Select(x => new WalkInnVM.WalkInnDataTable
                                    {
                                        WalkInnDate = x.TransactionDate.Value.ToString("dd/MM/yyyy"),
                                        Name = x.CandidateName,
                                        Center = x.CenterCode.CentreCode,
                                        Mobile = x.MobileNo,
                                        Email = x.EmailId,
                                        CareGiverMobileNo = x.GuardianType == (int)EnumClass.CareGiver.FATHER ? "FATHER :" + x.GuardianContactNo :
                                                          x.GuardianType == (int)EnumClass.CareGiver.GUARDIAN ? "GUARDIAN :" + x.GuardianContactNo :
                                                          x.GuardianType == (int)EnumClass.CareGiver.MOTHER ? "MOTHER :" + x.GuardianContactNo :
                                                                          "SPOUSE :" + x.GuardianContactNo,
                                        SalesPerson = x.CROCount == (int)EnumClass.CROCount.ONE ? x.Employee1.Name : x.Employee1.Name + "," + x.Employee2.Name,
                                        CourseRecommended = x.StudentWalkInnCourses
                                                         .Select(c => c.Course.Name)
                                                         .Aggregate((m, n) => m + "," + n),
                                        Id = x.Id,
                                        ExpJoinDate = x.JoinDate,
                                        Status = x.Status
                                    })
                                    .ToList();


                }

                return Json(new { data = _dTableWalkInn }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Add/
        public ActionResult Add()
        {

            try
            {
                Common _cmn = new Common();
                var _batchPreferred = from EnumClass.BatchPreferred b in Enum.GetValues(typeof(EnumClass.BatchPreferred))
                                      select new { Id = (int)b, Name = b.ToString().Replace('_', ' ') };

                var _placementRequired = from EnumClass.Placement p in Enum.GetValues(typeof(EnumClass.Placement))
                                         select new { Id = (int)p, Name = p.ToString() };

                var _customerType = from EnumClass.CustomerType c in Enum.GetValues(typeof(EnumClass.CustomerType))
                                    select new { Id = (int)c, Name = c.ToString() };

                var _careGiverList = from EnumClass.CareGiver c in Enum.GetValues(typeof(EnumClass.CareGiver))
                                     select new { Id = (int)c, Name = c.ToString() };

                var _studentCurrYear = from EnumClass.StudentCurrentYear s in Enum.GetValues(typeof(EnumClass.StudentCurrentYear))
                                       select new { Id = (int)s, Name = s.ToString().Replace('_', ' ') };

                var _demoEmpList = GetDemoGivenEmployeeList();

                var _cro1EmpList = GetCROEmployeeList();

                var _cro2EmpList = GetCROEmployeeList();

                var _walkInnVM = new WalkInnVM
                {
                    BatchPrefferedList = new SelectList(_batchPreferred, "Id", "Name"),
                    CourseList = new SelectList(_db.Courses.ToList(), "Id", "Name"),
                    DistrictList = new SelectList(""),
                    DemoGivenEmpList = new SelectList(_demoEmpList, "Id", "Name"),
                    KnowHowList = new SelectList(_db.KnowHowNS.ToList(), "Id", "KnowHowEvent"),
                    PlacementList = new SelectList(_placementRequired, "Id", "Name"),
                    CRO1EmpList = new SelectList(_cro1EmpList, "Id", "Name"),
                    CRO1ID = Convert.ToInt32(Session["LoggedUserId"].ToString()),
                    CRO2EmpList = new SelectList(_cro2EmpList, "Id", "Name"),
                    QlfnTypeList = new SelectList(_db.QlfnTypes.ToList(), "Id", "Name"),
                    QlfnMainList = new SelectList(""),
                    QlfnSubList = new SelectList(""),
                    StateList = new SelectList(_db.States.ToList(), "StateId", "StateName"),
                    WhyNSList = new SelectList(_db.WhyNS.ToList(), "Id", "Reason"),
                    customerTypeList = new SelectList(_customerType, "Id", "Name"),
                    CareGiverList = new SelectList(_careGiverList, "Id", "Name"),
                    StudentCurrentYearList = new SelectList(_studentCurrYear, "Id", "Name"),
                    CenterList = new SelectList(_cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"].ToString())), "Id", "CentreCode"),
                    HasPrevExp = true,
                    JoinStatus = true,
                    IsEquipmentDemoGiven = true,
                    CROCount = EnumClass.CROCount.ONE,
                    CRO1Percentage = 100


                };

                return View(_walkInnVM);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }

        }

        // POST: /Add/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(WalkInnVM mdlWalkInn)
        {
            try
            {
                if (mdlWalkInn.CROCount == EnumClass.CROCount.ONE)
                {
                    ModelState.Remove("CRO2ID");
                }
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        StudentWalkInn _studentWalkInn = new StudentWalkInn();
                        _studentWalkInn.Address = mdlWalkInn.StudentWalkInn.Address.ToUpper();
                        _studentWalkInn.BatchPreferred = mdlWalkInn.JoinStatus ? mdlWalkInn.BatchPrefferedID : null;
                        _studentWalkInn.CandidateName = mdlWalkInn.StudentWalkInn.CandidateName.ToUpper();
                        _studentWalkInn.CandidateStatus = mdlWalkInn.customerTypeID;
                        if (mdlWalkInn.customerTypeID == 1 || mdlWalkInn.customerTypeID == 2)
                        {
                            _studentWalkInn.CollegeAddress = mdlWalkInn.CollegeAddress.ToUpper();
                        }
                        _studentWalkInn.CenterCodeId = mdlWalkInn.CenterID;
                        _studentWalkInn.Comments = mdlWalkInn.StudentWalkInn.Comments != null ? mdlWalkInn.StudentWalkInn.Comments.ToUpper() : null;
                        _studentWalkInn.CROCount = mdlWalkInn.CROCount == EnumClass.CROCount.ONE ? (int)EnumClass.CROCount.ONE : (int)EnumClass.CROCount.TWO;
                        _studentWalkInn.CRO1ID = mdlWalkInn.CRO1ID;
                        _studentWalkInn.CRO1Percentage = mdlWalkInn.CRO1Percentage;

                        if (mdlWalkInn.CROCount == EnumClass.CROCount.TWO)
                        {
                            _studentWalkInn.CRO2ID = mdlWalkInn.CRO2ID;
                            _studentWalkInn.CRO2Percentage = mdlWalkInn.CRO2Percentage;
                        }
                        else
                        {
                            _studentWalkInn.CRO2Percentage = 0;
                        }

                        if (mdlWalkInn.customerTypeID == 3 || mdlWalkInn.customerTypeID == 4)
                        {
                            _studentWalkInn.CompanyAddress = mdlWalkInn.CompanyAddress.ToUpper();
                        }
                        _studentWalkInn.CourseRecommendedIDs = String.Join(",", mdlWalkInn.CourseID);

                        _studentWalkInn.CurrentYear = (mdlWalkInn.customerTypeID == 1) && (mdlWalkInn.QlfnTypeId != 4) ? mdlWalkInn.StudentCurrentYear : null;
                        _studentWalkInn.DemoGivenEmpID = mdlWalkInn.IsEquipmentDemoGiven ? mdlWalkInn.DemoGivenEmpId : null;
                        _studentWalkInn.DistrictID = mdlWalkInn.DistrictId;
                        _studentWalkInn.DOB = mdlWalkInn.StudentWalkInn.DOB;
                        _studentWalkInn.EmailId = mdlWalkInn.EmailId.ToUpper();
                        if (mdlWalkInn.customerTypeID == 3 || mdlWalkInn.customerTypeID == 4)
                        {
                            _studentWalkInn.ExpInYears = mdlWalkInn.ExpInYears;
                        }
                        _studentWalkInn.Feedback = mdlWalkInn.IsEquipmentDemoGiven ? mdlWalkInn.Feedback.ToUpper() : null;
                        _studentWalkInn.Gender = mdlWalkInn.Gender ? (int)EnumClass.Gender.Female : (int)EnumClass.Gender.Male;
                        _studentWalkInn.GuardianOccupation = mdlWalkInn.StudentWalkInn.GuardianOccupation != null ? mdlWalkInn.StudentWalkInn.GuardianOccupation.ToUpper() : null;
                        _studentWalkInn.GuardianContactNo = mdlWalkInn.StudentWalkInn.GuardianContactNo;
                        _studentWalkInn.GuardianName = mdlWalkInn.StudentWalkInn.GuardianName != null ? mdlWalkInn.StudentWalkInn.GuardianName.ToUpper() : null;
                        _studentWalkInn.GuardianType = mdlWalkInn.CareGiverID;
                        if (mdlWalkInn.customerTypeID == 3 || mdlWalkInn.customerTypeID == 4)
                        {
                            _studentWalkInn.IndustryType = mdlWalkInn.IndustryType.ToUpper();
                        }
                        _studentWalkInn.IsDemoGiven = mdlWalkInn.IsEquipmentDemoGiven;
                        _studentWalkInn.IsPlacementReqd = mdlWalkInn.PlacementID;
                        _studentWalkInn.JoinStatus = mdlWalkInn.JoinStatus;
                        _studentWalkInn.JoinDate = mdlWalkInn.JoinStatus == true ? mdlWalkInn.JoinDate : null;
                        _studentWalkInn.KnowHowNSIDs = String.Join(",", mdlWalkInn.KnowHowID);
                        _studentWalkInn.LandlineNo = mdlWalkInn.StudentWalkInn.LandlineNo;
                        _studentWalkInn.Location = mdlWalkInn.StudentWalkInn.Location.ToUpper();
                        _studentWalkInn.MobileNo = mdlWalkInn.MobileNo;
                        _studentWalkInn.NotJoiningReason = mdlWalkInn.JoinStatus ? null : mdlWalkInn.NotJoiningReason.ToUpper();
                        _studentWalkInn.Pincode = mdlWalkInn.StudentWalkInn.Pincode;
                        _studentWalkInn.HasPrevExp = mdlWalkInn.HasPrevExp;
                        if (mdlWalkInn.HasPrevExp)
                        {
                            _studentWalkInn.PrevExpPlace = mdlWalkInn.StudentWalkInn.PrevExpPlace.ToUpper();
                            _studentWalkInn.PrevExpYear = mdlWalkInn.StudentWalkInn.PrevExpYear.ToUpper();
                            _studentWalkInn.PrevExpTrainer = mdlWalkInn.StudentWalkInn.PrevExpTrainer.ToUpper();
                        }
                        _studentWalkInn.QlfnMainID = mdlWalkInn.QlfnMainId;
                        _studentWalkInn.QlfnSubID = mdlWalkInn.QlfnSubId;
                        _studentWalkInn.QlfnTypeID = mdlWalkInn.QlfnTypeId;
                        _studentWalkInn.Status = EnumClass.WalkinnStatus.WALKINN.ToString();
                        _studentWalkInn.TransactionDate = Common.LocalDateTime();
                        _studentWalkInn.WhyNSID = mdlWalkInn.WhyNSID;
                        _studentWalkInn.YearOfCompletion = mdlWalkInn.customerTypeID == 2 ? mdlWalkInn.YearOfCompletion : null;

                        //Adding Course Details
                        if (mdlWalkInn.CourseID.Length > 0)
                        {
                            foreach (var courseId in mdlWalkInn.CourseID)
                            {
                                StudentWalkInnCourse _studentCourse = new StudentWalkInnCourse();
                                _studentCourse.CourseID = Convert.ToInt32(courseId);
                                _studentWalkInn.StudentWalkInnCourses.Add(_studentCourse);
                            }

                        }

                        //if any relation is added
                        if (mdlWalkInn.StudentRelation.Count > 0)
                        {
                            foreach (var relation in mdlWalkInn.StudentRelation)
                            {
                                if (relation.Name != null && relation.Relation != null && relation.MobileNo != null)
                                {
                                    StudentRelation _studentRelation = new StudentRelation();
                                    _studentRelation.EmailId = relation.EmailId != null ? relation.EmailId.ToUpper() : null;
                                    _studentRelation.MobileNo = relation.MobileNo;
                                    _studentRelation.Name = relation.Name.ToUpper();
                                    _studentRelation.Relation = relation.Relation.ToUpper();
                                    _studentWalkInn.StudentRelations.Add(_studentRelation);
                                }


                            }
                        }

                        //Saving WalkInn Details
                        _db.StudentWalkInns.Add(_studentWalkInn);
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            Common _cmn = new Common();
                            int k = _cmn.AddTransactions(actionName, controllerName, "");
                            if (k > 0)
                            {
                                _ts.Complete();
                                return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return Json(new { message = "exception" }, JsonRequestBehavior.AllowGet);
            }
        }

        //GET:/Edit/
        public ActionResult Edit(int walkInnId)
        {
            Common _cmn = new Common();
            var _walkInnVM = new WalkInnVM();
            try
            {

                var _mdlWalkInn = _db.StudentWalkInns.Where(x => x.Id == walkInnId).FirstOrDefault();
                if (_mdlWalkInn != null)
                {
                    var _batchPreferred = from EnumClass.BatchPreferred b in Enum.GetValues(typeof(EnumClass.BatchPreferred))
                                          select new { Id = (int)b, Name = b.ToString().Replace('_', ' ') };

                    var _careGiverList = from EnumClass.CareGiver c in Enum.GetValues(typeof(EnumClass.CareGiver))
                                         select new { Id = (int)c, Name = c.ToString() };

                    var _customerType = from EnumClass.CustomerType c in Enum.GetValues(typeof(EnumClass.CustomerType))
                                        select new { Id = (int)c, Name = c.ToString() };

                    var _demoEmpList = GetDemoGivenEmployeeList();

                    var _placementRequired = from EnumClass.Placement p in Enum.GetValues(typeof(EnumClass.Placement))
                                             select new { Id = (int)p, Name = p.ToString() };

                    var _studentCurrYear = from EnumClass.StudentCurrentYear s in Enum.GetValues(typeof(EnumClass.StudentCurrentYear))
                                           select new { Id = (int)s, Name = s.ToString().Replace('_', ' ') };

                    var _cro1EmpList = GetCROEmployeeList();

                    var _cro2EmpList = GetCROEmployeeList();

                    _walkInnVM = new WalkInnVM
                    {
                        BatchPrefferedID = _mdlWalkInn.BatchPreferred,
                        BatchPrefferedList = new SelectList(_batchPreferred, "Id", "Name"),
                        CareGiverID = _mdlWalkInn.GuardianType,
                        CareGiverList = new SelectList(_careGiverList, "Id", "Name"),
                        CenterID = _mdlWalkInn.CenterCodeId,
                        CenterList = new SelectList(_cmn.GetCenterEmpwise(Convert.ToInt32(Session["LoggedUserId"].ToString())), "Id", "CentreCode"),
                        CollegeAddress = _mdlWalkInn.CollegeAddress,
                        CompanyAddress = _mdlWalkInn.CompanyAddress,
                        CourseID = _mdlWalkInn.CourseRecommendedIDs.Split(','),
                        CourseList = new SelectList(_db.Courses.ToList(), "Id", "Name"),
                        customerTypeID = _mdlWalkInn.CandidateStatus,
                        customerTypeList = new SelectList(_customerType, "Id", "Name"),
                        DemoGivenEmpId = _mdlWalkInn.DemoGivenEmpID,
                        DemoGivenEmpList = new SelectList(_demoEmpList, "Id", "Name"),
                        DistrictId = _mdlWalkInn.DistrictID,
                        DistrictList = new SelectList(_db.Districts
                                                    .Where(x => x.StateId == _mdlWalkInn.District.StateId)
                                                    .ToList(), "DistrictId", "DistrictName"),
                        ExpInYears = _mdlWalkInn.ExpInYears,
                        Feedback = _mdlWalkInn.Feedback,
                        Gender = _mdlWalkInn.Gender == 1 ? false : true,
                        HasPrevExp = _mdlWalkInn.HasPrevExp,
                        IndustryType = _mdlWalkInn.IndustryType,
                        IsEquipmentDemoGiven = _mdlWalkInn.IsDemoGiven,
                        JoinDate = _mdlWalkInn.JoinDate,
                        JoinStatus = _mdlWalkInn.JoinStatus,
                        KnowHowID = _mdlWalkInn.KnowHowNSIDs.Split(','),
                        KnowHowList = new SelectList(_db.KnowHowNS.ToList(), "Id", "KnowHowEvent"),
                        NotJoiningReason = _mdlWalkInn.NotJoiningReason,
                        PlacementID = _mdlWalkInn.IsPlacementReqd,
                        PlacementList = new SelectList(_placementRequired, "Id", "Name"),
                        CROCount = _mdlWalkInn.CROCount == 1 ? EnumClass.CROCount.ONE : EnumClass.CROCount.TWO,
                        CRO1ID = _mdlWalkInn.CRO1ID,
                        CRO1EmpList = new SelectList(_cro1EmpList, "Id", "Name"),
                        CRO2ID = _mdlWalkInn.CRO2ID,
                        CRO1Percentage = _mdlWalkInn.CRO1Percentage.Value,
                        CRO2Percentage = _mdlWalkInn.CRO2Percentage.Value,
                        CRO2EmpList = new SelectList(_cro2EmpList, "Id", "Name"),
                        QlfnMainId = _mdlWalkInn.QlfnMainID,
                        QlfnMainList = new SelectList(_db.QlfnMains
                                                    .Where(m => m.QlfnTypeId == _mdlWalkInn.QlfnTypeID)
                                                    .ToList(), "Id", "Name"),
                        QlfnSubId = _mdlWalkInn.QlfnSubID,
                        QlfnSubList = new SelectList(_db.QlfnSubs
                                                    .Where(s => s.QlfnMainId == _mdlWalkInn.QlfnMainID)
                                                    .ToList(), "Id", "Name"),
                        QlfnTypeId = _mdlWalkInn.QlfnTypeID,
                        QlfnTypeList = new SelectList(_db.QlfnTypes.ToList(), "Id", "Name"),
                        StateId = _mdlWalkInn.District.StateId,
                        StateList = new SelectList(_db.States.ToList(), "StateId", "StateName"),
                        StudentCurrentYear = _mdlWalkInn.CurrentYear,
                        StudentCurrentYearList = new SelectList(_studentCurrYear, "Id", "Name"),
                        StudentRelation = _mdlWalkInn.StudentRelations.ToList(),
                        StudentWalkInn = _mdlWalkInn,
                        WhyNSID = _mdlWalkInn.WhyNSID,
                        WhyNSList = new SelectList(_db.WhyNS.ToList(), "Id", "Reason"),
                        YearOfCompletion = _mdlWalkInn.YearOfCompletion,
                        MobileNo = _mdlWalkInn.MobileNo,
                        EmailId = _mdlWalkInn.EmailId
                    };
                }
                return View(_walkInnVM);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        //POST: /Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(WalkInnVM mdlWalkInn)
        {
            try
            {
                if (mdlWalkInn.CROCount == EnumClass.CROCount.ONE)
                {
                    ModelState.Remove("CRO2ID");
                    ModelState.Remove("EmailId");
                    ModelState.Remove("MobileNo");

                }
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                        string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

                        StudentWalkInn _studentWalkInn = _db.StudentWalkInns
                                                        .Where(s => s.Id == mdlWalkInn.StudentWalkInn.Id).FirstOrDefault();

                        if (_studentWalkInn != null)
                        {
                            _studentWalkInn.Address = mdlWalkInn.StudentWalkInn.Address.ToUpper();
                            _studentWalkInn.BatchPreferred = mdlWalkInn.JoinStatus ? mdlWalkInn.BatchPrefferedID : null;
                            _studentWalkInn.CandidateName = mdlWalkInn.StudentWalkInn.CandidateName.ToUpper();
                            _studentWalkInn.CandidateStatus = mdlWalkInn.customerTypeID;
                            if (mdlWalkInn.customerTypeID == 1 || mdlWalkInn.customerTypeID == 2)
                            {
                                _studentWalkInn.CollegeAddress = mdlWalkInn.CollegeAddress.ToUpper();
                            }
                            else
                            {
                                _studentWalkInn.CollegeAddress = null;
                            }

                            _studentWalkInn.CenterCodeId = mdlWalkInn.CenterID;
                            _studentWalkInn.Comments = mdlWalkInn.StudentWalkInn.Comments != null ? mdlWalkInn.StudentWalkInn.Comments.ToUpper() : null;
                            if (mdlWalkInn.customerTypeID == 3 || mdlWalkInn.customerTypeID == 4)
                            {
                                _studentWalkInn.CompanyAddress = mdlWalkInn.CompanyAddress.ToUpper();
                            }
                            else
                            {
                                _studentWalkInn.CompanyAddress = null;
                            }
                            _studentWalkInn.CourseRecommendedIDs = String.Join(",", mdlWalkInn.CourseID);
                            _studentWalkInn.CRO1ID = mdlWalkInn.CRO1ID;
                            _studentWalkInn.CRO1Percentage = mdlWalkInn.CRO1Percentage;
                            if (mdlWalkInn.CROCount == EnumClass.CROCount.TWO)
                            {
                                _studentWalkInn.CRO2ID = mdlWalkInn.CRO2ID;
                                _studentWalkInn.CRO2Percentage = mdlWalkInn.CRO2Percentage;
                            }
                            else
                            {
                                _studentWalkInn.CRO2ID = null;
                                _studentWalkInn.CRO2Percentage = 0;
                            }
                            _studentWalkInn.CROCount = mdlWalkInn.CROCount == EnumClass.CROCount.ONE ? 1 : 2;
                            _studentWalkInn.CurrentYear = (mdlWalkInn.customerTypeID == 1) && (mdlWalkInn.QlfnTypeId != 4) ? mdlWalkInn.StudentCurrentYear : null;
                            _studentWalkInn.DemoGivenEmpID = mdlWalkInn.IsEquipmentDemoGiven ? mdlWalkInn.DemoGivenEmpId : null;
                            _studentWalkInn.DistrictID = mdlWalkInn.DistrictId;
                            _studentWalkInn.DOB = mdlWalkInn.StudentWalkInn.DOB;
                            if (mdlWalkInn.EmailId != null)
                            {
                                _studentWalkInn.EmailId = mdlWalkInn.EmailId.ToUpper();
                            }
                            if (mdlWalkInn.customerTypeID == 3 || mdlWalkInn.customerTypeID == 4)
                            {
                                _studentWalkInn.ExpInYears = mdlWalkInn.ExpInYears;
                            }
                            else
                            {
                                _studentWalkInn.ExpInYears = null;
                            }
                            _studentWalkInn.Feedback = mdlWalkInn.IsEquipmentDemoGiven ? mdlWalkInn.Feedback.ToUpper() : null;
                            _studentWalkInn.Gender = mdlWalkInn.Gender ? (int)EnumClass.Gender.Female : (int)EnumClass.Gender.Male;
                            _studentWalkInn.GuardianOccupation = mdlWalkInn.StudentWalkInn.GuardianOccupation != null ? mdlWalkInn.StudentWalkInn.GuardianOccupation.ToUpper() : null;
                            _studentWalkInn.GuardianContactNo = mdlWalkInn.StudentWalkInn.GuardianContactNo;
                            _studentWalkInn.GuardianName = mdlWalkInn.StudentWalkInn.GuardianName != null ? mdlWalkInn.StudentWalkInn.GuardianName.ToUpper() : null;
                            _studentWalkInn.GuardianType = mdlWalkInn.CareGiverID;
                            if (mdlWalkInn.customerTypeID == 3 || mdlWalkInn.customerTypeID == 4)
                            {
                                _studentWalkInn.IndustryType = mdlWalkInn.IndustryType.ToUpper();
                            }
                            else
                            {
                                _studentWalkInn.IndustryType = null;
                            }
                            _studentWalkInn.IsDemoGiven = mdlWalkInn.IsEquipmentDemoGiven;
                            _studentWalkInn.IsPlacementReqd = mdlWalkInn.PlacementID;
                            _studentWalkInn.JoinStatus = mdlWalkInn.JoinStatus;
                            _studentWalkInn.JoinDate = mdlWalkInn.JoinStatus == true ? mdlWalkInn.JoinDate : null;
                            _studentWalkInn.KnowHowNSIDs = String.Join(",", mdlWalkInn.KnowHowID);
                            _studentWalkInn.LandlineNo = mdlWalkInn.StudentWalkInn.LandlineNo;
                            _studentWalkInn.Location = mdlWalkInn.StudentWalkInn.Location.ToUpper();
                            if (mdlWalkInn.MobileNo != null)
                            {
                                _studentWalkInn.MobileNo = mdlWalkInn.MobileNo;
                            }
                            _studentWalkInn.NotJoiningReason = mdlWalkInn.JoinStatus ? null : mdlWalkInn.NotJoiningReason.ToUpper();
                            _studentWalkInn.Pincode = mdlWalkInn.StudentWalkInn.Pincode;
                            _studentWalkInn.HasPrevExp = mdlWalkInn.HasPrevExp;
                            if (mdlWalkInn.HasPrevExp)
                            {
                                _studentWalkInn.PrevExpPlace = mdlWalkInn.StudentWalkInn.PrevExpPlace.ToUpper();
                                _studentWalkInn.PrevExpYear = mdlWalkInn.StudentWalkInn.PrevExpYear;
                                _studentWalkInn.PrevExpTrainer = mdlWalkInn.StudentWalkInn.PrevExpTrainer.ToUpper();
                            }
                            else
                            {
                                _studentWalkInn.PrevExpPlace = null;
                                _studentWalkInn.PrevExpYear = null;
                                _studentWalkInn.PrevExpTrainer = null;
                            }
                            _studentWalkInn.CRO1ID = mdlWalkInn.CRO1ID;
                            _studentWalkInn.QlfnMainID = mdlWalkInn.QlfnMainId;
                            _studentWalkInn.QlfnSubID = mdlWalkInn.QlfnSubId;
                            _studentWalkInn.QlfnTypeID = mdlWalkInn.QlfnTypeId;
                            //_studentWalkInn.TransactionDate = Common.LocalDateTime();
                            _studentWalkInn.WhyNSID = mdlWalkInn.WhyNSID;
                            _studentWalkInn.YearOfCompletion = mdlWalkInn.customerTypeID == 2 ? mdlWalkInn.YearOfCompletion : null;

                            //Removing Existing Course Details                            
                            foreach (var _existingCourse in _db.StudentWalkInnCourses
                                                            .Where(e => e.StudentWalkInnID == mdlWalkInn.StudentWalkInn.Id)
                                                            .ToList())
                            {
                                _db.StudentWalkInnCourses.Remove(_existingCourse);
                            }

                            //Removing Existing Relation Details
                            foreach (var _existingRelation in _db.StudentRelations
                                                               .Where(e => e.WalkInnID == mdlWalkInn.StudentWalkInn.Id)
                                                               .ToList())
                            {
                                _db.StudentRelations.Remove(_existingRelation);
                            }

                            //Adding new Course Details
                            if (mdlWalkInn.CourseID.Length > 0)
                            {
                                foreach (var courseId in mdlWalkInn.CourseID)
                                {
                                    StudentWalkInnCourse _studentCourse = new StudentWalkInnCourse();
                                    _studentCourse.CourseID = Convert.ToInt32(courseId);
                                    _studentWalkInn.StudentWalkInnCourses.Add(_studentCourse);
                                }

                            }

                            //iadding new relation is added
                            if (mdlWalkInn.StudentRelation.Count > 0)
                            {
                                foreach (var relation in mdlWalkInn.StudentRelation)
                                {
                                    if (relation.Name != null && relation.Relation != null && relation.MobileNo != null)
                                    {
                                        StudentRelation _studentRelation = new StudentRelation();
                                        _studentRelation.EmailId = relation.EmailId != null ? relation.EmailId.ToUpper() : null;
                                        _studentRelation.MobileNo = relation.MobileNo;
                                        _studentRelation.Name = relation.Name.ToUpper();
                                        _studentRelation.Relation = relation.Relation.ToUpper();
                                        _studentWalkInn.StudentRelations.Add(_studentRelation);
                                    }


                                }
                            }

                            //Saving WalkInn Details
                            _db.Entry(_studentWalkInn).State = EntityState.Modified;
                            int i = _db.SaveChanges();
                            if (i > 0)
                            {
                                Common _cmn = new Common();
                                int k = _cmn.AddTransactions(actionName, controllerName, "");
                                if (k > 0)
                                {
                                    _ts.Complete();
                                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }

                    }
                }
                return Json(new { message = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return Json(new { message = "exception" }, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /District/       
        public JsonResult GetDistrict(int stateId)
        {
            try
            {
                var _districtList = _db.Districts
                                        .Where(x => x.StateId == stateId)
                                        .Select(x => new
                                        {
                                            Id = x.DistrictId,
                                            Name = x.DistrictName
                                        }).ToList();
                return Json(_districtList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Course/       
        public JsonResult GetCourse(int qlfnTypeId)
        {
            try
            {
                var _designationList = _db.QlfnMains
                                        .Where(x => x.QlfnTypeId == qlfnTypeId)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        }).ToList();
                return Json(_designationList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Stream/       
        public JsonResult GetStream(int qlfnMainId)
        {
            try
            {
                var _designationList = _db.QlfnSubs
                                        .Where(x => x.QlfnMainId == qlfnMainId)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            Name = x.Name
                                        }).ToList();
                return Json(_designationList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        private int GetMonth(string month)
        {
            int i = 0;
            switch (month)
            {
                case "January":
                    i = 1;
                    break;
                case "February":
                    i = 2;
                    break;
                case "March":
                    i = 3;
                    break;
                case "April":
                    i = 4;
                    break;
                case "May":
                    i = 5;
                    break;
                case "June":
                    i = 6;
                    break;
                case "July":
                    i = 7;
                    break;
                case "August":
                    i = 8;
                    break;
                case "September":
                    i = 9;
                    break;
                case "October":
                    i = 10;
                    break;
                case "November":
                    i = 11;
                    break;
                case "December":
                    i = 12;
                    break;

            }
            return i;

        }

        //Get all employees with Role Technical Head + Technical Individual
        public List<Employee> GetDemoGivenEmployeeList()
        {
            //Get details from role table
            List<int> _empRoleTechnicalIds = _db.Roles
                                            .Where(x => (x.Id == (int)EnumClass.Role.TECHNICALHEAD || x.Id == (int)EnumClass.Role.TECHNICALINDIVIDUAL))
                                            .Select(x => x.Id).ToList();

            //Get details from designation table
            List<int> _empDesignationIds = _db.Designations
                                        .AsEnumerable()
                                        .Where(d => _empRoleTechnicalIds.Contains(d.RoleId.Value))
                                        .Select(d => d.Id).ToList();

            //Get details from employee table
            List<Employee> _demoGiveEmpList = _db.Employees
                                            .Where(e => _empDesignationIds.Contains(e.DesignationId.Value) && e.Status == true)
                                            .ToList();

            return _demoGiveEmpList;
        }

        //Get all employees with Role Technical Head + Technical Individual + Sales Head + Sales Individual cooresponding to a center
        public List<Employee> GetCROEmployeeList()
        {
            Common _cmn = new Common();

            int _loggedUserId = Convert.ToInt32(Session["LoggedUserId"]);

            //Get all EmployeeIds allotted to a particular center       
            List<int> _empIds = _cmn.GetEmployeeCenterWise(_loggedUserId)
                                .Select(e => e.Id).ToList();

            //Get all employees with EmployeeId filtering and except loggeduserid
            List<Employee> _cro2EmpList = _db.Employees
                                            .Where(e => (e.Designation.Role.Id == (int)EnumClass.Role.TECHNICALHEAD ||
                                                        e.Designation.Role.Id == (int)EnumClass.Role.TECHNICALINDIVIDUAL ||
                                                        e.Designation.Role.Id == (int)EnumClass.Role.MANAGER ||
                                                        e.Designation.Role.Id == (int)EnumClass.Role.SALESINDIVIDUAL) &&
                                                        _empIds.Contains(e.Id))
                                            .ToList();

            return _cro2EmpList;
        }

        // GET: /Check EmailId/
        public JsonResult IsEamilAlreadyExists(string EmailId, string InitialEmail)
        {
            try
            {
                //For  addition
                if (InitialEmail == "")
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.EmailId == EmailId);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                //For Edition
                else if (InitialEmail != EmailId)
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.EmailId == EmailId);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Check MobileNo/
        public JsonResult IsMobileAlreadyExists(string MobileNo, string InitialMobile)
        {
            try
            {
                //For  addition
                if (InitialMobile == "")
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.MobileNo == MobileNo);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                //For Edition
                else if (InitialMobile != MobileNo)
                {
                    var _exist = _db.StudentWalkInns.Any(w => w.MobileNo == MobileNo);
                    if (_exist)
                    {
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
