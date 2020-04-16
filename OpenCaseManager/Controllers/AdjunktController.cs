using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class AdjunktController : Controller
    {
        // GET: Adjunkt
        public ActionResult Index()
        {
            return View();
        }
    }
}