using SMS.Models;
using SMS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace SMS.Controllers
{
    public class GroupController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();
        public class PopulateSelectList
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        //GET: /Group/Index
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

        public JsonResult GetDataTable()
        {
            try
            {
                List<GroupVM.GroupCentreCodeList> _clsGroupCentreCode = new List<GroupVM.GroupCentreCodeList>();
                _clsGroupCentreCode = _db.Group_CentreCode_Setting
                                    .AsEnumerable()
                                    .OrderByDescending(x => x.TransactionDate)
                                    .GroupBy(g => g.GroupName)
                                    .Select(gcs => new GroupVM.GroupCentreCodeList
                                    {
                                        SlNo = "",
                                        GroupName = gcs.Key.ToUpper(),
                                        CentreCodeName = string.Join(",", gcs.Select(x => x.CenterCode.CentreCode)),
                                        GroupCreatedDate = gcs.Select(x => x.TransactionDate.Value.ToString("dd/MM/yyyy")).FirstOrDefault()
                                    }).ToList();

                return Json(new { data = _clsGroupCentreCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Group/Add
        public ActionResult Add()
        {
            try
            {
                List<PopulateSelectList> _centreCodeList = GetCentreCodeList();
                GroupVM _groupVM = new GroupVM
                {
                    CentreCodeList = new SelectList(_centreCodeList, "Id", "Name")
                };

                return View(_groupVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }

        }

        //POST : Group/Add
        [HttpPost]
        public JsonResult Add(GroupVM mdlGroupVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        foreach (var _centre in mdlGroupVM.CentreCodeId)
                        {
                            Group_CentreCode_Setting _group = new Group_CentreCode_Setting();
                            _group.CentreCodeId = Convert.ToInt32(_centre);
                            _group.GroupName = mdlGroupVM.GroupName;
                            _group.TransactionDate = Common.LocalDateTime();
                            _db.Group_CentreCode_Setting.Add(_group);
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

        // GET: /Group Name Checking/
        public JsonResult CheckGroupName(string GroupName, string InitialGroupName)
        {
            try
            {

                //For  edit purpose
                if (!string.IsNullOrEmpty(InitialGroupName))
                {
                    if (InitialGroupName.ToLower().Trim() != GroupName.ToLower().Trim())
                    {
                        var _exist = _db.Group_CentreCode_Setting.Any(g => g.GroupName == GroupName.Trim());
                        if (_exist)
                        {
                            return Json(false, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    var _exist = _db.Group_CentreCode_Setting.Any(g => g.GroupName == GroupName.Trim());
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

        //GET: / Group/Edit
        public ActionResult Edit(string GroupName)
        {
            try
            {
                List<PopulateSelectList> _centreCodeList = GetCentreCodeList();
                List<PopulateSelectList> _existingCentreCodeList = new List<PopulateSelectList>();

                List<Group_CentreCode_Setting> _group_centreCode_list = new List<Group_CentreCode_Setting>();


                _group_centreCode_list = _db.Group_CentreCode_Setting
                                          .Where(gc => gc.GroupName == GroupName)
                                          .ToList();

                //getting existing centrecode list
                _existingCentreCodeList = _group_centreCode_list
                                        .Select(gc => new PopulateSelectList
                                        {
                                            Id = gc.CenterCode.Id,
                                            Name = gc.CenterCode.CentreCode
                                        }).ToList();

                List<PopulateSelectList> _filteredCentreCodeList = _centreCodeList.Concat(_existingCentreCodeList).ToList();


                GroupVM _clsGroupVM = new GroupVM();
                _clsGroupVM.CentreCodeId = _group_centreCode_list
                                          .Select(gc => gc.CenterCode.Id)
                                          .ToArray();

                _clsGroupVM.GroupName = _group_centreCode_list
                                       .FirstOrDefault()
                                       .GroupName;

                _clsGroupVM.TransactionDate = _group_centreCode_list
                                            .FirstOrDefault()
                                            .TransactionDate.Value;

                _clsGroupVM.InitialGroupName = _group_centreCode_list
                                            .FirstOrDefault()
                                            .GroupName;

                _clsGroupVM.CentreCodeList = new SelectList(_filteredCentreCodeList, "Id", "Name");

                return View(_clsGroupVM);
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }


        //POST : Group/Edit
        [HttpPost]
        public JsonResult Edit(GroupVM mdlGroupVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        List<Group_CentreCode_Setting> _lstGroupCentreCode = new List<Group_CentreCode_Setting>();
                        _lstGroupCentreCode = _db.Group_CentreCode_Setting
                                            .Where(gcs => gcs.GroupName == mdlGroupVM.GroupName)
                                            .ToList();

                        //Removing each item from the database
                        foreach (var groupCentreCode in _lstGroupCentreCode)
                        {
                            _db.Group_CentreCode_Setting.Remove(groupCentreCode);
                        }

                        //Adding newly added item
                        foreach (var _centre in mdlGroupVM.CentreCodeId)
                        {
                            Group_CentreCode_Setting _group = new Group_CentreCode_Setting();
                            _group.CentreCodeId = Convert.ToInt32(_centre);
                            _group.GroupName = mdlGroupVM.GroupName;
                            _group.TransactionDate = Common.LocalDateTime();
                            _db.Group_CentreCode_Setting.Add(_group);
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

        //POST : Group/Delete
        [HttpPost]
        public JsonResult Delete(string groupName)
        {
            try
            {
                using (TransactionScope _ts = new TransactionScope())
                {
                    List<Group_CentreCode_Setting> _lstGroupCentreCode = new List<Group_CentreCode_Setting>();
                    _lstGroupCentreCode = _db.Group_CentreCode_Setting
                                        .Where(gcs => gcs.GroupName == groupName)
                                        .ToList();

                    //Adding newly added item
                    foreach (var _item in _lstGroupCentreCode)
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

        //Get Centrecodes that are not allotted to any group
        public List<PopulateSelectList> GetCentreCodeList()
        {
            List<PopulateSelectList> _centreCodeList = new List<PopulateSelectList>();
            try
            {

                _centreCodeList = _db.CenterCodes
                                    .Where(s => s.Status == true)
                                    .Select(c => new PopulateSelectList
                                    {
                                        Id = c.Id,
                                        Name = c.CentreCode
                                    }).ToList();

                //Getting centres allotted for all group
                List<int> _existing_group_centrecode = _db.Group_CentreCode_Setting
                                                    .Select(gc => gc.CenterCode.Id)
                                                    .ToList();

                //Removing existing centrecode from the group list
                _centreCodeList = _centreCodeList
                                .Where(c => !_existing_group_centrecode.Any(e => e == c.Id))
                                 .Select(c => new PopulateSelectList
                                 {
                                     Id = c.Id,
                                     Name = c.Name
                                 }).ToList();
            }
            catch (Exception ex)
            {
                _centreCodeList = null;
            }

            return _centreCodeList;
        }


        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }


    }
}
