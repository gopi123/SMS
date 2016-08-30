using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMS.Models;
using SMS.Models.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SMS.Controllers
{
    public class RoleController : Controller
    {
        //
        // GET: /Role/

        dbSMSNSEntities _db = new dbSMSNSEntities();

        public ActionResult Index()
        {
            return View();
        }        

      
        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
