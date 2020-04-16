using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class AdjunktInstanceController : Controller
    {
        // GET: AdjunktInstance
        public ActionResult Index()
        {
            return View();
        }

        // GET: AdjunktInstance
        public ActionResult Search(string query)
        {
            return View();
        }
    }
}