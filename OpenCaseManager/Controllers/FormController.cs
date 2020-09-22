using Newtonsoft.Json;
using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using System;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class FormController : Controller
    {
        private IManager _manager;
        private IDataModelManager _dataModelManager;
        private IDocumentManager _documentManager;
        private IService _service;

        public FormController(IManager manager, IDataModelManager dataModelManager, IDocumentManager documentManager, IService service)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
            _documentManager = documentManager;
            _service = service;
        }
        // GET: Form
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetPdf(string formId, string formName)
        {
            byte[] data = { };
            data = Common.GetFormData(formId, _manager, _dataModelManager);
            return File(data, "application/pdf", String.Format("{0:yyyyMMdd}", DateTime.Now) + "_" + formName + ".pdf");
        }

        public ActionResult GetWord(string formId, string formName)
        {
            byte[] data = { };
            var html = Common.GetFormHtml(formId, _manager, _dataModelManager);
            var path = Common.GetFormWordPath(html, _service);
            var formDocumentPath = JsonConvert.DeserializeObject<dynamic>(path);
            data = System.IO.File.ReadAllBytes(formDocumentPath.success.ToString());
            return File(data, "application/octet-stream", String.Format("{0:yyyyMMdd}", DateTime.Now) + "_" + formName + ".docx");
        }

    }
}