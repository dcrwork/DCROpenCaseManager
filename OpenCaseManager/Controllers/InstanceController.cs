using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class InstanceController : Controller
    {
        // GET: Instance
        public ActionResult Index()
        {
            return View();
        }

        // GET: Instance
        public ActionResult Search(string query)
        {
            return View();
        }
    }
}