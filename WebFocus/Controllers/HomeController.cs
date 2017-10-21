using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebFocus.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetComInit()
        {
            var db = new WebFocus.Models.FPWorkEntities();
            var comInit = db.tbl_ComInit.ToList();
            return Json(comInit, JsonRequestBehavior.AllowGet);
        }
    }
}