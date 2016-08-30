using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMS.Models;
using System.Transactions;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.Data;

namespace SMS.Controllers
{
    public class DiscountSettingController : Controller
    {

        dbSMSNSEntities _db = new dbSMSNSEntities();

        public class PopulateSelectList
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        ////GET : DiscountSetting/Add
        public class clsDiscountSettingList
        {
            public string SlNo { get; set; }
            public string GroupName { get; set; }
            public string FromDate { get; set; }
            public string CentreCode { get; set; }
        }

        //Get DiscountSetting Role List
        public List<PopulateSelectList> GetRoleList()
        {
            List<PopulateSelectList> _roleList = new List<PopulateSelectList>();
            try
            {

                IEnumerable<EnumClass.DiscountSettingRole> _roles = Enum.GetValues(typeof(EnumClass.DiscountSettingRole))
                                                                    .Cast<EnumClass.DiscountSettingRole>();
                _roleList = _roles
                          .Select(r => new PopulateSelectList()
                          {
                              Id = ((int)r).ToString(),
                              Name = r.ToString().Replace('_', ' ')
                          }).ToList();


            }
            catch (Exception ex)
            {
                _roleList = null;
            }


            return _roleList;

        }

        //Get: Group list
        public List<PopulateSelectList> GetGroupList()
        {
            List<PopulateSelectList> _groupList = new List<PopulateSelectList>();
            try
            {
                _groupList = _db.Group_CentreCode_Setting
                            .GroupBy(g => g.GroupName)
                            .Select(x => new PopulateSelectList()
                            {
                                Id = x.Key,
                                Name = x.Key.ToUpper()
                            }).ToList();


            }
            catch (Exception ex)
            {
                _groupList = null;
            }
            return _groupList;
        }

        //GET: GetDiscountSetting
        //Gets the default discount setting details based on roles
        public PartialViewResult GetDiscountSetting_Roles(List<int> roleIds)
        {
            try
            {
                Common _cmn = new Common();
                List<DiscountSettingVM.clsDiscountSettings> _lstDiscountSetting = new List<DiscountSettingVM.clsDiscountSettings>();
                foreach (var _roleId in roleIds)
                {
                    string _roleName = _cmn.GetDiscountSetting_RoleName(_roleId);

                    DiscountSettingVM.clsDiscountSettings _clsDiscountSetting = new DiscountSettingVM.clsDiscountSettings();
                    _clsDiscountSetting.RoleId = _roleId;
                    _clsDiscountSetting.RoleName = _roleName;
                    _clsDiscountSetting.Foundation = "0";
                    _clsDiscountSetting.Diploma = "0";
                    _clsDiscountSetting.MasterDiploma = "0";
                    _clsDiscountSetting.Professional = "0";
                    _lstDiscountSetting.Add(_clsDiscountSetting);
                }

                var _mdlDiscountSettings = new DiscountSettingVM
                {
                    DiscountSettingsList = _lstDiscountSetting

                };

                return PartialView("_DiscountSettingsView", _mdlDiscountSettings);
            }

            catch (Exception ex)
            {
                return PartialView("_DiscountSettingsView", null);
            }
        }

        public string GetCentreCode(string groupName)
        {
            string _centreCode = string.Empty;
            try
            {
                _centreCode = string.Join(",", _db.Group_CentreCode_Setting
                                            .Where(gcs => gcs.GroupName == groupName)
                                            .Select(gcs => gcs.CenterCode.CentreCode)
                                            .ToList());
            }
            catch (Exception ex)
            {
                _centreCode = "";
            }

            return _centreCode;
        }

        public ActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        //GET : DiscountSetting/GetDataTable
        public JsonResult GetDataTable()
        {
            List<clsDiscountSettingList> _clsDiscountSetting = new List<clsDiscountSettingList>();
            try
            {
                _clsDiscountSetting = _db.DiscountSettings
                                    .OrderByDescending(o => o.FromDate)
                                    .GroupBy(g => g.Group_CentreCode_GroupName)
                                    .AsEnumerable()
                                    .Select(d => new clsDiscountSettingList
                                    {
                                        SlNo = "",
                                        CentreCode = GetCentreCode(d.Key),
                                        FromDate = d.FirstOrDefault().FromDate.Value.ToString("dd/MM/yyyy"),
                                        GroupName = d.Key.ToUpper()
                                    }).ToList();

            }
            catch (Exception ex)
            {
                _clsDiscountSetting = null;
            }

            return Json(new { data = _clsDiscountSetting }, JsonRequestBehavior.AllowGet);
        }

        //GET : DiscountSetting/Add
        public ActionResult Add()
        {
            try
            {
                var _roleList = GetRoleList();
                var _groupList = GetGroupList();

                List<int> _rolesToAdd = new List<int>();
                _rolesToAdd.Add((int)EnumClass.DiscountSettingRole.ED);
                _rolesToAdd.Add((int)EnumClass.DiscountSettingRole.CENTRE_MANAGER);
                _rolesToAdd.Add((int)EnumClass.DiscountSettingRole.SALES_MANAGER);
                _rolesToAdd.Add((int)EnumClass.DiscountSettingRole.SALES_INDIVIDUAL);

                //filtering grouplist by existing groupname
                List<string> _existingGroupList = _db.DiscountSettings
                                                .Select(d => d.Group_CentreCode_GroupName)
                                                .Distinct()
                                                .ToList();

                _groupList = _db.Group_CentreCode_Setting
                            .Where(g => !_existingGroupList.Contains(g.GroupName))
                            .GroupBy(g => g.GroupName)
                            .Select(x => new PopulateSelectList()
                            {
                                Id = x.Key,
                                Name = x.Key.ToUpper()
                            }).ToList();


                var _discountSettingsVM = new DiscountSettingVM()
                {
                    RoleList = new SelectList(_roleList, "Id", "Name"),
                    GroupNameList = new SelectList(_groupList, "Id", "Name"),
                    DiscountFromDate = null,
                    RoleId = _rolesToAdd.ToArray(),
                    CentreCodeList = new SelectList("")
                };

                return View(_discountSettingsVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        //POST : DiscountSetting/Add
        [HttpPost]
        public ActionResult Add(DiscountSettingVM mdlDiscountSettings)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    List<string> _groupNameList = new List<string>();
                    _groupNameList = mdlDiscountSettings.GroupName.ToList();

                    //iterating through each groupname
                    foreach (var _groupName in _groupNameList)
                    {

                        ///removing existing discount values of the same date
                        var _existingDiscountList = _db.DiscountSettings
                                                      .Where(d => d.Group_CentreCode_GroupName == _groupName.ToLower().Trim())
                                                      .ToList();
                        if (_existingDiscountList != null && _existingDiscountList.Count > 0)
                        {
                            foreach (var _discSettings in _existingDiscountList)
                            {
                                _db.Entry(_discSettings).State = EntityState.Deleted;

                            }
                        }


                        //iterating through each course category
                        foreach (var category in Enum.GetValues(typeof(EnumClass.CourseCategory)))
                        {

                            //iterating through each role
                            foreach (var item in mdlDiscountSettings.DiscountSettingsList)
                            {
                                DiscountSetting _discountSetting = new DiscountSetting();
                                _discountSetting.Group_CentreCode_GroupName = _groupName.ToLower().Trim();
                                _discountSetting.CourseSeriesTypeId = (int)category;
                                _discountSetting.FromDate = mdlDiscountSettings.DiscountFromDate;
                                if (category.ToString() == EnumClass.CourseCategory.FOUNDATION.ToString())
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.Foundation.ToString());
                                }
                                else if (category.ToString() == EnumClass.CourseCategory.DIPLOMA.ToString())
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.Diploma.ToString());
                                }
                                else if (category.ToString() == EnumClass.CourseCategory.PROFESSIONAL.ToString())
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.Professional.ToString());
                                }
                                else
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.MasterDiploma.ToString());
                                }
                                _discountSetting.RoleId = item.RoleId;
                                _db.DiscountSettings.Add(_discountSetting);
                            }
                        }
                    }
                    int i = _db.SaveChanges();
                    if (i > 0)
                    {
                        _ts.Complete();
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //GET : DiscountSetting/Edit
        public ActionResult Edit(string groupName)
        {
            try
            {
                DiscountSettingVM _mdlDiscountSetting = new DiscountSettingVM();
                List<string> _currGroupName = new List<string>();
                List<int> _currRoleId = new List<int>();
                List<DiscountSetting> _discSettingList = _db.DiscountSettings
                                                        .Where(d => d.Group_CentreCode_GroupName == groupName.ToLower().Trim())
                                                        .ToList();
                var _roleList = GetRoleList();
                var _groupList = GetGroupList();
                _currGroupName.Add(_discSettingList.FirstOrDefault().Group_CentreCode_GroupName.ToUpper());
                _currRoleId = _discSettingList
                            .Select(d => d.Role.Id)
                            .Distinct().ToList();


                _mdlDiscountSetting.DiscountFromDate = _discSettingList.FirstOrDefault().FromDate;
                _mdlDiscountSetting.GroupName = _currGroupName.ToArray();
                _mdlDiscountSetting.GroupNameList = new SelectList(_groupList, "Id", "Name");
                _mdlDiscountSetting.RoleId = _currRoleId.ToArray();
                _mdlDiscountSetting.RoleList = new SelectList(_roleList, "Id", "Name");

                return View(_mdlDiscountSetting);

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        //POST :  DiscountSetting/Edit
        [HttpPost]
        public ActionResult Edit(DiscountSettingVM mdlDiscountSetting)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    if (mdlDiscountSetting != null && mdlDiscountSetting.DiscountSettingsList != null)
                    {

                        var groupName = mdlDiscountSetting.GroupName[0].ToLower().Trim();
                        //removing existing discount values of the same date
                        foreach (var _discSettings in _db.DiscountSettings
                                                      .Where(d => d.Group_CentreCode_GroupName == groupName)
                                                      .ToList())
                        {
                            _db.Entry(_discSettings).State = EntityState.Deleted;

                        }

                        //iterating through each course category
                        foreach (var category in Enum.GetValues(typeof(EnumClass.CourseCategory)))
                        {

                            //iterating through each role
                            foreach (var item in mdlDiscountSetting.DiscountSettingsList)
                            {
                                DiscountSetting _discountSetting = new DiscountSetting();
                                _discountSetting.CourseSeriesTypeId = (int)category;
                                if (category.ToString() == EnumClass.CourseCategory.FOUNDATION.ToString())
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.Foundation.ToString());
                                }
                                else if (category.ToString() == EnumClass.CourseCategory.DIPLOMA.ToString())
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.Diploma.ToString());
                                }
                                else if (category.ToString() == EnumClass.CourseCategory.PROFESSIONAL.ToString())
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.Professional.ToString());
                                }
                                else
                                {
                                    _discountSetting.Discount = Convert.ToInt32(item.MasterDiploma.ToString());
                                }
                                _discountSetting.RoleId = item.RoleId;
                                _discountSetting.Group_CentreCode_GroupName = groupName;
                                _discountSetting.FromDate = mdlDiscountSetting.DiscountFromDate;
                                _db.DiscountSettings.Add(_discountSetting);
                            }

                        }
                        int i = _db.SaveChanges();
                        if (i > 0)
                        {
                            _ts.Complete();
                            return Json("success", JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return Json("error", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //called while clicking next of edit discount settings
        public PartialViewResult GetDiscountSetting_Edit(string groupName, List<int> roleIds)
        {
            try
            {
                var _spGetDiscountSetting = _db.Edit_Get_DiscountSettings(groupName);
                Common _cmn = new Common();
                List<DiscountSettingVM.clsDiscountSettings> _lstDiscountSetting = new List<DiscountSettingVM.clsDiscountSettings>();
                if (_spGetDiscountSetting != null)
                {
                    foreach (var discountSetting in _spGetDiscountSetting)
                    {
                        DiscountSettingVM.clsDiscountSettings _clsDiscountSetting = new DiscountSettingVM.clsDiscountSettings();
                        _clsDiscountSetting.RoleId = Convert.ToInt32(discountSetting.RoleId);
                        _clsDiscountSetting.RoleName = _cmn.GetDiscountSetting_RoleName(Convert.ToInt32(discountSetting.RoleId));
                        _clsDiscountSetting.Foundation = discountSetting.Foundation == null ? "NA" : discountSetting.Foundation.Value.ToString();
                        _clsDiscountSetting.Diploma = discountSetting.Diploma == null ? "NA" : discountSetting.Diploma.Value.ToString();
                        _clsDiscountSetting.MasterDiploma = discountSetting.MasterDiploma == null ? "NA" : discountSetting.MasterDiploma.Value.ToString();
                        _clsDiscountSetting.Professional = discountSetting.Professional == null ? "NA" : discountSetting.Professional.Value.ToString();
                        _lstDiscountSetting.Add(_clsDiscountSetting);

                    }

                    //if selected roles are greater than roles in the database
                    //then add new role to the list
                    if (roleIds.Count > _lstDiscountSetting.Count)
                    {
                        List<int> _allottedRoleId = _lstDiscountSetting.Select(d => d.RoleId).ToList();
                        List<int> _xtraRoleId = roleIds.Where(r => !_allottedRoleId.Contains(r)).Select(r => r).ToList();

                        foreach (var roleId in _xtraRoleId)
                        {
                            string _roleName = _cmn.GetDiscountSetting_RoleName(roleId);

                            DiscountSettingVM.clsDiscountSettings _clsDiscountSetting = new DiscountSettingVM.clsDiscountSettings();
                            _clsDiscountSetting.RoleId = roleId;
                            _clsDiscountSetting.RoleName = _roleName;
                            _clsDiscountSetting.Foundation = "0";
                            _clsDiscountSetting.Diploma = "0";
                            _clsDiscountSetting.MasterDiploma = "0";
                            _clsDiscountSetting.Professional = "0";

                            _lstDiscountSetting.Add(_clsDiscountSetting);

                        }
                    }
                    //if selected roles are lesser than roles in the database
                    //then removing  roles from the list
                    else if (roleIds.Count < _lstDiscountSetting.Count)
                    {
                        List<int> _allottedRoleId = _lstDiscountSetting.Select(d => d.RoleId).ToList();
                        List<int> _xtraRoleId = _allottedRoleId.Where(ar => !roleIds.Contains(ar)).Select(ar => ar).ToList();

                        foreach (var roleId in _xtraRoleId)
                        {
                            DiscountSettingVM.clsDiscountSettings _clsDiscountSetting = _lstDiscountSetting.Where(r => r.RoleId == roleId).FirstOrDefault();
                            _lstDiscountSetting.Remove(_clsDiscountSetting);

                        }
                    }
                }
                else
                {
                    _lstDiscountSetting = null;
                }

                var _mdlDiscountSettings = new DiscountSettingVM
                {
                    DiscountSettingsList = _lstDiscountSetting.OrderBy(r => r.RoleId).ToList()

                };

                return PartialView("_DiscountSettingsView", _mdlDiscountSettings);
            }
            catch (Exception ex)
            {
                return PartialView("");
            }
        }

        //POST : DiscountSetting/Delete
        [HttpPost]
        public JsonResult Delete(string groupName)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    List<DiscountSetting> _lstDiscountSetting = new List<DiscountSetting>();
                    _lstDiscountSetting = _db.DiscountSettings
                                        .Where(d => d.Group_CentreCode_GroupName == groupName.ToLower().Trim())
                                        .ToList();

                    //Adding newly added item
                    foreach (var _item in _lstDiscountSetting)
                    {
                        _db.Entry(_item).State = EntityState.Deleted;
                    }
                    int i = _db.SaveChanges();
                    if (i > 0)
                    {
                        _ts.Complete();
                        return Json("success", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("error", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //checks whether any fresh group exist
        //used on while adding
        public JsonResult GetGroupCount()
        {
            int _count = 0;
            try
            {
                var _groupList = GetGroupList();

                List<string> _existingGroupList = _db.DiscountSettings
                                               .Select(d => d.Group_CentreCode_GroupName)
                                               .Distinct()
                                               .ToList();

                _count = _db.Group_CentreCode_Setting
                           .Where(g => !_existingGroupList.Contains(g.GroupName))
                           .GroupBy(g => g.GroupName)
                           .Count();

            }
            catch (Exception ex)
            {
                _count = -1;
            }
            return Json(new { message = _count }, JsonRequestBehavior.AllowGet);
        }

        //Get Centrecode list on groupchange
        public JsonResult GetCentreCode_On_GroupChange(List<string> groupName)
        {
            List<PopulateSelectList> _lstCentreCode = new List<PopulateSelectList>();
            try
            {

                _lstCentreCode = _db.Group_CentreCode_Setting
                               .Where(gcs => groupName.Contains(gcs.GroupName))
                               .AsEnumerable()
                               .Select(x => new PopulateSelectList()
                               {
                                   Id = x.CenterCode.Id.ToString() ,
                                   Name = x.CenterCode.CentreCode
                               }).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _lstCentreCode = null;
            }
            return Json(_lstCentreCode, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }

    }
}
