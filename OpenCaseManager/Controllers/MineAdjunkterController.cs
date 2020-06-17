using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class MineAdjunkterController : Controller
    {
        private IManager _manager;
        private IDataModelManager _dataModelManager;
        private IDocumentManager _documentManager;
        private IService _service;

        public MineAdjunkterController(IManager manager, IDataModelManager dataModelManager, IDocumentManager documentManager, IService service)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
            _documentManager = documentManager;
            _service = service;
        }


        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            var data = Common.GetIsManager(_manager, _dataModelManager);
            bool.TryParse(data.Rows[0].ItemArray[0].ToString(), out bool isManager);
            if (isManager) return View();
            return Redirect("~/MineAktiviteter");
        }
    }
}
