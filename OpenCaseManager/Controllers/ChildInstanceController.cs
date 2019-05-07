using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class ChildInstanceController : Controller
    {
        // GET: ChildInstance
        public ActionResult Index()
        {
            return View();
        }

        // GET: ChildInstance
        public ActionResult Search(string query)
        {
            return View();
        }
    }
}
