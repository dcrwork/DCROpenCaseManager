using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class AdministratorController : Controller
    {
        // GET: Administrator
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Evaluate()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult LaunchInstance()
        {
            return View();
        }
    }
}