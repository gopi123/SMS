using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMS.Models;
using SMS.Models.ViewModel;
using System.Diagnostics;
using System.Data;
using System.Transactions;
using System.Data.Entity;

namespace SMSLocal.Controllers
{    
    public class MenuSettingsController : Controller
    {
        dbSMSNSEntities _db = new dbSMSNSEntities();     
        // GET: /MenuSettings/       

       
        public ActionResult Index()
        {
            //try
            //{
            //    var _menuRoleVM = new MenuRoleVM
            //    {
            //        RoleList = new SelectList(_db.Roles.ToList(), "Id", "RoleName"),//for dropdown filling 
            //        DesignationList=new SelectList("")
            //    };
            //    return View(_menuRoleVM);
            //}
            //catch (Exception ex)
            //{
            //    return View("Error");
            //}
            return RedirectToAction("Add");

        }

        [HttpPost]
        public JsonResult LoadGridView(int designationId = 0)
        {
            var _dTableMenuRole = _db.MenuRoles
                                 .Where(m => m.DesignationID == designationId)
                                 .OrderBy(x=>x.Id)
                                 .Select(x => new MenuVM()
                                 {
                                     MenuName = x.Menu.MenuName,
                                     CanAdd = x.CanAdd,
                                     CanEdit = x.CanEdit,
                                     CanDelete = x.CanDelete,
                                     MenuRoleId = x.Id
                                 }).ToList();
            return Json(new { data = _dTableMenuRole }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Add()
        {
            Common _cmn = new Common();
            var _menuRoleVM = new MenuRoleVM
             {
                 //for dropdown filling  
                 RoleList = new SelectList(_cmn.GetRolesEmpwise(Convert.ToInt32(Session["LoggedUserId"].ToString())), "Id", "RoleName"),

                 DesignationList = new SelectList(""),
                 //for showing menus 
                 MenuList = null

             };
            return View(_menuRoleVM);

        }
        //save to MenuRole table
        [HttpPost]
        public JsonResult Add(MenuRoleVM _vmMenuRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope _ts = new TransactionScope())
                    {
                        //Check whether role already exists in RoleMenu table
                        var _roleCount = _db.MenuRoles
                                        .Where(m => m.DesignationID == _vmMenuRole.DesignationId).Distinct().Count();
                        if (_roleCount > 0)
                        {
                            //update existing record
                            //Remove existing menurole
                            foreach (var _existingMenuRole in _db.MenuRoles
                                                                .Where(e => e.DesignationID == _vmMenuRole.DesignationId)
                                                                .ToList())
                            {
                                _db.MenuRoles.Remove(_existingMenuRole);
                            }

                        }
                        for (int i = 0; i < _vmMenuRole.MenuList.Count; i++)
                        {
                            var canAdd = _vmMenuRole.MenuList[i].CanAdd;
                            var canEdit = _vmMenuRole.MenuList[i].CanEdit;
                            var canDelete = _vmMenuRole.MenuList[i].CanDelete;
                            if (canAdd == true || canEdit == true || canDelete == true)
                            {
                                MenuRole _menuRole = new MenuRole();
                                _menuRole.MenuID = _vmMenuRole.MenuList[i].MenuId;
                                _menuRole.DesignationID = _vmMenuRole.DesignationId;
                                _menuRole.CanAdd = _vmMenuRole.MenuList[i].CanAdd;
                                _menuRole.CanEdit = _vmMenuRole.MenuList[i].CanEdit;
                                _menuRole.CanDelete = _vmMenuRole.MenuList[i].CanDelete;
                                _db.MenuRoles.Add(_menuRole);
                            }

                        }
                        _db.SaveChanges();
                        _ts.Complete();                           
                        return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);

                    };

                }
                return Json(new { message = "validationError" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { message = "Error: Cannot Insert" + ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Edit(int menuRoleId = 0)
        {
            var _menuItem = _db.MenuRoles
                           .Where(r => r.Id == menuRoleId)
                           .Select(x => new MenuVM()
                           {
                               CanAdd = x.CanAdd,
                               CanEdit = x.CanEdit,
                               CanDelete = x.CanDelete,
                               MenuName = x.Menu.MenuName,
                               MenuRoleId = x.Id
                           }).FirstOrDefault();
            return PartialView("_Edit", _menuItem);
        }

        public JsonResult GetDesignation(int roleId)
        {
            try
            {
                var _designationList = _db.Designations
                                   .Where(d => d.RoleId == roleId)
                                   .Select(x => new
                                   {
                                       Id = x.Id,
                                       Name = x.DesignationName
                                   }).ToList();
                return Json(_designationList, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Edit(MenuVM _menuVM)
        {
            try
            {
                var _menuItem = _db.MenuRoles
                               .Where(r => r.Id == _menuVM.MenuRoleId).FirstOrDefault();
                if (_menuItem != null)
                {
                    _menuItem.CanAdd = _menuVM.CanAdd;
                    _menuItem.CanEdit = _menuVM.CanEdit;
                    _menuItem.CanDelete = _menuVM.CanDelete;
                    _db.Entry(_menuItem).State = EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { message = "success", JsonRequestBehavior.AllowGet });
                };
                return Json(new { message = "error", JsonRequestBehavior.AllowGet });
            }
            catch (Exception ex)
            {
                return Json(new { message = "exception", JsonRequestBehavior.AllowGet });
            }

        }
        
        
        public ActionResult MenuListDesignationwise(int designationId=0)
        {
            try
            {
                MenuRoleVM _menuRoleVM = new MenuRoleVM();
                if (designationId == 0)
                {
                    _menuRoleVM.MenuList = null;
                }
                else
                {
                    _menuRoleVM.MenuList = (from m in _db.Menus
                                            join mr in _db.MenuRoles on m.Id equals mr.MenuID into t
                                            from rt in t.Where(x => x.DesignationID == designationId).DefaultIfEmpty()
                                            where m.ParentID != 0                                          
                                            select new MenuVM
                                            {
                                                CanAdd = (rt.CanAdd == null || rt.CanAdd==false) ? false : true,
                                                CanEdit = (rt.CanEdit == null || rt.CanEdit == false) ? false : true,
                                                CanDelete = (rt.CanDelete == null || rt.CanDelete == false) ? false : true,
                                                MenuId = m.Id,
                                                MenuName = m.MenuName
                                            }).OrderBy(x=>x.MenuId).ToList();


                    
                }
                return PartialView("_MenuDesignationWise", _menuRoleVM);
            }
            catch (Exception ex)
            {
                return PartialView("_MenuDesignationWise", null);
            }

        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }       
    }
}
