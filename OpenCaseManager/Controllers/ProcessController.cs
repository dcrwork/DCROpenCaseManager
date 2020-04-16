using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class ProcessController : Controller
    {
        // GET: Process
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SearchProcess()
        {
            return View();
        }

        public ActionResult Processes()
        {
            return View();
        }

        public ActionResult ProcessHistory(int graphId)
        {
            return View();
        }
    }
}