using Newtonsoft.Json;
using OpenCaseManager.Commons;
using OpenCaseManager.Controllers.ApiControllers;
using OpenCaseManager.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IManager _manager;
        private IDataModelManager _dataModelManager;
        private IDocumentManager _documentManager;
        private IService _service;

        public HomeController(IManager manager, IDataModelManager dataModelManager, IDocumentManager documentManager, IService service)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
            _documentManager = documentManager;
            _service = service;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";


            /* Hvis man ikke vil bruge forsiden kan man bruge dette kode til at redirecte til MineAdjunkter hvis man er manager og til MineAktiviteter hvis man ikke er.
            var data = Common.GetIsManager(_manager, _dataModelManager);
            if (data.Rows.Count < 1) return Redirect("~/MineAktiviteter");

            bool.TryParse(data.Rows[0].ItemArray[0].ToString(), out bool isManager);
            if (isManager) return Redirect("~/MineAdjunkter");
            return Redirect("~/MineAktiviteter");
            */
            return View();
        }

        [AllowAnonymous]
        public ActionResult UnAuthorized()
        {
            ViewBag.Title = "Un Authorized";

            return View();
        }
    }
}
