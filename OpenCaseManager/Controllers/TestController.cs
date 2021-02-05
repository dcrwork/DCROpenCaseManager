using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        // GET: TestInstance
        public ActionResult Instance()
        {
            return View();
        }

        public ActionResult Intro()
        {
            return View();
        }
    }
}