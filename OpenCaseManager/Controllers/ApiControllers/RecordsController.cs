using Newtonsoft.Json;
using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace OpenCaseManager.Controllers.ApiControllers
{
    [Authorize]
    [RoutePrefix("api/records")]
    public class RecordsController : ApiController
    {
        private IManager _manager;
        private IService _service;
        private IDCRService _dCRService;
        private IDataModelManager _dataModelManager;
        private IDocumnentManager _documentManager;

        public RecordsController(IManager manager, IService service, IDCRService dCRService, IDataModelManager dataModelManager, IDocumnentManager documentManager)
        {
            _manager = manager;
            _service = service;
            _dCRService = dCRService;
            _dataModelManager = dataModelManager;
            _documentManager = documentManager;

        }

        // POST api/values
        [HttpPost]
        public IHttpActionResult Post(DataModel model)
        {
            if (model.Type == Enums.SQLOperation.SELECT.ToString())
            {
                var output = _manager.SelectData(model);
                return Ok(Common.ToJson(output));
            }
            return BadRequest();
        }

        /// <summary>
        /// Add Instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addInstance")]
        // POST api/values
        public IHttpActionResult AddInstanceApi(AddInstanceModel input)
        {
            // add Instance
            var instanceId = AddInstance(input);
            if (input.ChildId != null) ConnectInstanceToChild(instanceId, input.ChildId);
            return Ok(Common.ToJson(instanceId));
        }

        /// <summary>
        /// Add Process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addProcess")]
        // POST api/values
        public IHttpActionResult AddProcess(List<Model> input)
        {
            var countAdded = 0;
            // add Instance
            foreach (var process in input)
            {
                var graphXml = string.Empty;
                graphXml = _dCRService.GetProcess(process.GraphId);

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                _dataModelManager.AddFilter(DBEntityNames.Process.Status.ToString(), Enums.ParameterType._boolean, Boolean.FalseString, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.Id.ToString(), DBEntityNames.Process.GraphId.ToString(), DBEntityNames.Process.Status.ToString() });

                var processes = _manager.SelectData(_dataModelManager.DataModel);
                if (processes.Rows.Count < 1)
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Process.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Process.Title.ToString(), Enums.ParameterType._string, process.Title);
                    _dataModelManager.AddParameter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, process.GraphId.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Process.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);

                    try
                    {
                        var processId = _manager.InsertData(_dataModelManager.DataModel);

                        var phases = _dCRService.GetProcessPhases(process.GraphId);

                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddProcessPhases.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.ProcessId.ToString(), Enums.ParameterType._int, (processId.Rows[0]["Id"]).ToString());
                        _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.PhaseXml.ToString(), Enums.ParameterType._xml, phases);

                        _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);

                        countAdded++;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    UpdateProcessAndPhases(processes.Rows[0][DBEntityNames.Process.Id.ToString()].ToString(), processes.Rows[0][DBEntityNames.Process.GraphId.ToString()].ToString(), process.Title);
                    countAdded++;
                }
            }
            if (countAdded > 0)
                return Ok(countAdded);
            return Conflict();
        }

        /// <summary>
        /// Update Process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateProcess")]
        // POST api/values
        public IHttpActionResult UpdateProcess(dynamic input)
        {
            var processId = input["processId"].ToString();
            var processTitle = input["processTitle"].ToString();
            var processStatus = input["processStatus"].ToString();
            var showOnFronPage = input["showOnFronPage"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Process.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Process.Title.ToString(), Enums.ParameterType._string, processTitle);
            _dataModelManager.AddParameter(DBEntityNames.Process.Status.ToString(), Enums.ParameterType._boolean, processStatus);
            _dataModelManager.AddParameter(DBEntityNames.Process.OnFrontPage.ToString(), Enums.ParameterType._boolean, showOnFronPage);
            _dataModelManager.AddParameter(DBEntityNames.Process.Modified.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            _dataModelManager.AddFilter(DBEntityNames.Process.Id.ToString(), Enums.ParameterType._int, processId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            _manager.UpdateData(_dataModelManager.DataModel);
            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Update Process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateProcessFromDCR")]
        // POST api/values
        public IHttpActionResult UpdateProcessFromDCR(dynamic input)
        {
            var processId = input["processId"].ToString();
            var graphId = input["graphId"].ToString();

            UpdateProcessAndPhases(processId, graphId);
            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Add a form
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddForm")]
        public IHttpActionResult AddForm(dynamic input)
        {
            var isFromTemplate = Boolean.Parse(input["isFromTemplate"].ToString());
            var templateFormId = input["templateFormId"].ToString();

            // new form data model
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Form.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Form.Title.ToString(), Enums.ParameterType._string, "Untitled");
            _dataModelManager.AddParameter(DBEntityNames.Form.IsTemplate.ToString(), Enums.ParameterType._boolean, bool.FalseString);
            _dataModelManager.AddParameter(DBEntityNames.Form.UserId.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());
            if (isFromTemplate)
            {
                _dataModelManager.AddParameter(DBEntityNames.Form.FormTemplateId.ToString(), Enums.ParameterType._string, templateFormId);
            }

            var dataTable = _manager.InsertData(_dataModelManager.DataModel);
            var formId = 0;
            if (dataTable.Rows.Count > 0)
                formId = int.Parse(dataTable.Rows[0][DBEntityNames.Form.Id.ToString()].ToString());

            if (isFromTemplate)
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.CopyFormFromTemplate.ToString());
                _dataModelManager.AddParameter(DBEntityNames.CopyFormFromTemplate.FormId.ToString(), Enums.ParameterType._int, formId.ToString());
                _dataModelManager.AddParameter(DBEntityNames.CopyFormFromTemplate.TemplateId.ToString(), Enums.ParameterType._int, templateFormId);

                _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);
            }

            return Ok(Common.ToJson(formId));
        }

        /// <summary>
        /// Update form title/IsTemplate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateForm")]
        public IHttpActionResult UpdateForm(dynamic input)
        {
            var isTemplate = input["isTemplate"].ToString();
            var title = input["title"].ToString();
            var formId = input["id"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Form.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Form.Title.ToString(), Enums.ParameterType._string, title);
            _dataModelManager.AddParameter(DBEntityNames.Form.IsTemplate.ToString(), Enums.ParameterType._boolean, isTemplate);
            _dataModelManager.AddFilter(DBEntityNames.Form.Id.ToString(), Enums.ParameterType._int, formId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var dataTable = _manager.UpdateData(_dataModelManager.DataModel);

            return Ok(Common.ToJson(formId));
        }

        /// <summary>
        /// Add a group/question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuestion")]
        public IHttpActionResult AddQuestion(dynamic input)
        {
            var formId = input["formId"].ToString();
            var itemText = input["itemText"].ToString();
            var sequenceNumber = input["sequenceNumber"].ToString();
            var itemId = input["itemId"];
            var isGroup = input["isGroup"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.FormItem.ToString());
            _dataModelManager.AddParameter(DBEntityNames.FormItem.FormId.ToString(), Enums.ParameterType._int, formId);
            _dataModelManager.AddParameter(DBEntityNames.FormItem.IsGroup.ToString(), Enums.ParameterType._boolean, isGroup);
            _dataModelManager.AddParameter(DBEntityNames.FormItem.SequenceNumber.ToString(), Enums.ParameterType._int, sequenceNumber);
            _dataModelManager.AddParameter(DBEntityNames.FormItem.ItemText.ToString(), Enums.ParameterType._string, itemText);
            if (itemId != null)
            {
                _dataModelManager.AddParameter(DBEntityNames.FormItem.ItemId.ToString(), Enums.ParameterType._int, itemId.ToString());
            }

            var dataTable = _manager.InsertData(_dataModelManager.DataModel);

            var questionId = 0;
            if (dataTable.Rows.Count > 0)
                questionId = int.Parse(dataTable.Rows[0][DBEntityNames.FormItem.Id.ToString()].ToString());

            return Ok(Common.ToJson(questionId));
        }

        /// <summary>
        /// Set sequence number of group/question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetQuestionSequence")]
        public IHttpActionResult SetQuestionSequence(dynamic input)
        {
            var itemId = input["itemId"].ToString();
            var parentId = input["targetId"].ToString();
            var position = input["position"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.SetFormItemSequence.ToString());
            _dataModelManager.AddParameter(DBEntityNames.SetFormItemSequence.Source.ToString(), Enums.ParameterType._int, itemId);
            _dataModelManager.AddParameter(DBEntityNames.SetFormItemSequence.Target.ToString(), Enums.ParameterType._int, parentId);
            _dataModelManager.AddParameter(DBEntityNames.SetFormItemSequence.Position.ToString(), Enums.ParameterType._int, position);

            var dataTable = _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);

            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Delete a question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteQuestion")]
        public IHttpActionResult DeleteQuestion(dynamic input)
        {
            var itemId = input["itemId"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.DeleteFormItem.ToString());
            _dataModelManager.AddParameter(DBEntityNames.DeleteFormItem.FormItemId.ToString(), Enums.ParameterType._int, itemId);

            var dataTable = _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);

            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Update a question
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateQuestion")]
        public IHttpActionResult UpdateQuestion(dynamic input)
        {
            var itemId = input["itemId"].ToString();
            var itemText = input["itemText"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.FormItem.ToString());
            _dataModelManager.AddParameter(DBEntityNames.FormItem.ItemText.ToString(), Enums.ParameterType._string, itemText);
            _dataModelManager.AddFilter(DBEntityNames.FormItem.Id.ToString(), Enums.ParameterType._int, itemId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var dataTable = _manager.UpdateData(_dataModelManager.DataModel);

            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Add custom values for an instance
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddInstanceCustomAttributes")]
        public IHttpActionResult AddInstanceCustomAttributes(dynamic input)
        {
            var instanceId = input["instanceId"].ToString();
            var employee = input["employee"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.InstanceExtension.ToString());
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.Employee.ToString(), Enums.ParameterType._string, employee);
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.Year.ToString(), Enums.ParameterType._int, DateTime.Now.Year.ToString());

            var dataTable = _manager.InsertData(_dataModelManager.DataModel);

            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Add Document
        /// </summary>
        /// <returns></returns>
        [Route("AddDocument")]
        [HttpPost]
        public IHttpActionResult AddDocument()
        {
            var request = HttpContext.Current.Request;
            var givenFileName = request.Headers["givenFileName"];
            var fileName = request.Headers["filename"];
            var fileType = request.Headers["type"];
            var instanceId = request.Headers["instanceId"];
            var eventId = string.Empty;
            try
            {
                eventId = request.Headers["eventId"];
            }
            catch (Exception) { }
            var eventTime = DateTime.Now;
            try
            {
                eventTime = request.Headers["eventTime"].parseDanishDateToDate();
            }
            catch (Exception) { }
            var filePath = string.Empty;
            var documentId = string.Empty;
            if (fileType == "JournalNoteBig")
            {
                var addedDocument = _documentManager.AddDocument(instanceId, fileType, givenFileName, fileName, eventId, eventTime, _manager, _dataModelManager);
                filePath = addedDocument.Item1;
                documentId = addedDocument.Item2;
            }
            else filePath = _documentManager.AddDocument(instanceId, fileType, givenFileName, fileName, eventId, _manager, _dataModelManager);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    request.InputStream.CopyTo(fs);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Ok(Common.ToJson(Convert.ToInt32(documentId)));
        }

        /// <summary>
        /// Update Document
        /// </summary>
        /// <returns></returns>
        [Route("UpdateDocument")]
        [HttpPost]
        public IHttpActionResult UpdateDocument()
        {
            var request = HttpContext.Current.Request;
            var id = request.Headers["id"];
            var givenFileName = request.Headers["givenFileName"];
            var fileType = request.Headers["type"];
            var instanceId = request.Headers["instanceId"];
            var isNewFileAdded = bool.Parse(request.Headers["isNewFileAdded"]);
            var fileLink = string.Empty;
            var eventTime = DateTime.Now;
            try
            {
                eventTime = Convert.ToDateTime(request.Headers["eventTime"]);
            }
            catch (Exception) { }
            if (isNewFileAdded)
            {
                DeleteFileFromFileSystem(id, fileType, instanceId);

                var fileName = request.Headers["filename"];
                string ext = Path.GetExtension(fileName);
                fileLink = DateTime.Now.ToFileTime() + ext;
                var filePath = string.Empty;

                switch (fileType)
                {
                    case "PersonalDocument":
                        var directoryInfo = new DirectoryInfo(Configurations.Config.PersonalFileLocation);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        var currentUser = Common.GetCurrentUserName();
                        directoryInfo = new DirectoryInfo(Configurations.Config.PersonalFileLocation + "\\" + currentUser);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        filePath = directoryInfo.FullName;
                        break;
                    case "InstanceDocument":
                        directoryInfo = new DirectoryInfo(Configurations.Config.InstanceFileLocation);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        directoryInfo = new DirectoryInfo(Configurations.Config.InstanceFileLocation + "\\" + instanceId);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        filePath = directoryInfo.FullName;
                        break;
                    case "JournalNoteImportant":
                        directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        filePath = directoryInfo.FullName;
                        break;
                    case "JournalNoteLittle": //Should only temporarily be allowed to be edited
                        directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        filePath = directoryInfo.FullName;
                        break;
                    case "JournalNoteBig": //Should only temporarily be allowed to be edited
                        directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        filePath = directoryInfo.FullName;
                        break;
                }
                filePath = filePath + "\\" + fileLink;

                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        request.InputStream.CopyTo(fs);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            UpdateDocument(id, givenFileName, fileLink);
            if (fileType == "JournalNoteBig") UpdateJournalHistoryDocument(id, givenFileName, eventTime);

            return Ok(Common.ToJson(new { }));
        }


        /// <summary>
        /// Delete Document
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("DeleteDocument")]
        [HttpPost]
        public IHttpActionResult DeleteDocument(dynamic input)
        {
            var id = input["Id"].ToString();
            var type = input["Type"].ToString();
            var instanceId = input["InstanceId"].ToString();

            // delete document from filesystem
            var isDeleted = DeleteFileFromFileSystem(id, type, instanceId);
            if (isDeleted)
            {
                // delete document from DB
                DeleteDocument(id);
            }
            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Get Document URL
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("GetDocumentsUrl")]
        [HttpPost]
        public IHttpActionResult GetDocumentsUrl(dynamic input)
        {
            var type = input["Type"].ToString();
            var instanceId = input["InstanceId"].ToString();
            var documentsUrl = CopyToTempFolder(type, instanceId);

            return Ok(Common.ToJson(documentsUrl));
        }

        /// <summary>
        /// Get Document URL
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("CleanUpTempDocuments")]
        [HttpPost]
        public IHttpActionResult CleanUpTempDocuments(dynamic input)
        {
            var urls = new List<string>();
            for (var i = 0; i < input["docsUrl"].Count; i++)
            {
                urls.Add(AppDomain.CurrentDomain.BaseDirectory + "tmp" + input["docsUrl"][i].ToString().Split(new string[] { "tmp" }, StringSplitOptions.None)[1]);
            }

            foreach (var url in urls)
            {
                var fileInfo = new FileInfo(url);
                if (fileInfo.Exists)
                {
                    fileInfo.Directory.Delete(true);
                }
            }
            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Replace values in event type parameters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("ReplaceEventTypeParamsKeys")]
        [HttpPost]
        public IHttpActionResult ReplaceEventTypeParamsKeys(dynamic input)
        {
            var eventTypeValue = input["eventTypeValue"].ToString();
            var instanceId = input["instanceId"].ToString();
            var actualValue = Common.ReplaceEventTypeKeyValues(eventTypeValue, instanceId, _manager, _dataModelManager);
            return Ok(actualValue);
        }

        /// <summary>
        /// Upload form to personal folder
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("UploadFormToPersonalFolder")]
        [HttpPost]
        public IHttpActionResult UploadFormToPersonalFolder(dynamic input)
        {
            var formId = input["formId"].ToString();
            var formName = input["formName"].ToString();
            var documentType = input["type"].ToString();

            var givenFileName = String.Format("{0:yyyyMMdd}", DateTime.Now) + "_" + formName;
            var fileName = givenFileName;
            byte[] data = { };

            if (documentType == "word")
            {
                fileName += ".docx";

                var html = Common.GetFormHtml(formId, _manager, _dataModelManager);
                var path = Common.GetFormWordPath(html, _service);
                var formDocumentPath = JsonConvert.DeserializeObject<dynamic>(path);
                data = File.ReadAllBytes(formDocumentPath.success.ToString());
            }
            else if (documentType == "pdf")
            {
                fileName += ".pdf";
                data = Common.GetFormData(formId, _manager, _dataModelManager);
            }

            var filePath = _documentManager.AddDocument(string.Empty, "Personal", givenFileName, fileName, string.Empty, _manager, _dataModelManager);
            Common.SaveBytesToFile(filePath, data);

            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Replace values in event type parameters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("LogJsError")]
        [HttpPost]
        public IHttpActionResult LogJsError(dynamic input)
        {
            var message = input["message"].ToString();
            var source = input["source"].ToString();
            var stack = input["stack"].ToString();

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Log.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Log.Level.ToString(), Enums.ParameterType._string, "JsError");
            _dataModelManager.AddParameter(DBEntityNames.Log.UserName.ToString(), Enums.ParameterType._string, Common.GetCurrentUserName());
            _dataModelManager.AddParameter(DBEntityNames.Log.ServerName.ToString(), Enums.ParameterType._string, Request.RequestUri.Host);
            _dataModelManager.AddParameter(DBEntityNames.Log.Port.ToString(), Enums.ParameterType._string, Request.RequestUri.Port.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Log.Url.ToString(), Enums.ParameterType._string, source);
            _dataModelManager.AddParameter(DBEntityNames.Log.Https.ToString(), Enums.ParameterType._boolean, Common.IsHttps().ToString());
            _dataModelManager.AddParameter(DBEntityNames.Log.Message.ToString(), Enums.ParameterType._string, message);
            _dataModelManager.AddParameter(DBEntityNames.Log.Exception.ToString(), Enums.ParameterType._string, stack);
            _manager.InsertData(_dataModelManager.DataModel);

            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Add MUS Role
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("AddMUSRole")]
        [HttpPost]
        public IHttpActionResult AddMUSRole(dynamic input)
        {
            var instanceId = input["instanceId"].ToString();
            var employee = input["employee"].ToString();
            var roles = new List<string>();
            for (var i = 0; i < input["roles"].Count; i++)
            {
                roles.Add(input["roles"][i].ToString());
            }
            var userRoles = new List<UserRole>();
            foreach (var role in roles)
            {
                if (role == Configurations.Config.MUSLeaderRole)
                {
                    userRoles.Add(new UserRole()
                    {
                        RoleId = role,
                        UserId = int.Parse(Common.GetResponsibleId())
                    });
                }
                else if (role == Configurations.Config.MUSEmployeeRole)
                {
                    userRoles.Add(new UserRole()
                    {
                        RoleId = role,
                        UserId = int.Parse(Common.GetResponsibleFullDetails(_manager, _dataModelManager, employee).Rows[0]["Id"].ToString())
                    });
                }
                else
                {
                    userRoles.Add(new UserRole()
                    {
                        RoleId = role,
                        UserId = int.Parse(Common.GetResponsibleId())
                    });
                }
            }
            AddInstanceRoles(userRoles, instanceId);
            return Ok(Common.ToJson(new { }));
        }

        /// <summary>
        /// Copy all files to a temp folder
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        private List<string> CopyToTempFolder(string type, string instanceId)
        {
            var documentUrl = new List<string>();
            var selectedDocuments = SelectPersonDocumentByPerson(new List<string> { "Title", "Link" });
            if (selectedDocuments.Rows.Count > 0)
            {
                var dirPath = "tmp\\" + DateTime.Now.ToFileTime();

                foreach (DataRow document in selectedDocuments.Rows)
                {
                    var path = string.Empty;
                    // delete document from file system
                    switch (type)
                    {
                        case "PersonalDocument":
                            var currentUser = Common.GetCurrentUserName();
                            path = Configurations.Config.PersonalFileLocation + "\\" + currentUser + "\\" + document["Link"].ToString();
                            break;
                        case "InstanceDocument":
                            path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + document["Link"].ToString();
                            break;
                    }

                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        try
                        {
                            var destFilePath = dirPath + "\\" + document["Title"].ToString() + Path.GetExtension(path);
                            var destPath = AppDomain.CurrentDomain.BaseDirectory + destFilePath;
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + dirPath);

                            var destFileInfor = new FileInfo(destPath);
                            if (destFileInfor.Exists)
                            {
                                destFileInfor.Delete();
                            }
                            fileInfo.CopyTo(destPath);
                            documentUrl.Add(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/" + destFilePath);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            return documentUrl;
        }

        /// <summary>
        /// Create xml for roles
        /// </summary>
        /// <param name="userRolesList"></param>
        /// <returns></returns>
        private XmlDocument CreateUserRolesXml(List<UserRole> userRolesList)
        {
            var xmlDoc = new XmlDocument();
            var rootNode = xmlDoc.CreateElement("UserRoles");
            xmlDoc.AppendChild(rootNode);

            foreach (var userRole in userRolesList)
            {
                XmlNode userNode = xmlDoc.CreateElement("User");
                XmlAttribute attribute = xmlDoc.CreateAttribute("Id");
                attribute.Value = userRole.UserId.ToString();
                userNode.Attributes.Append(attribute);

                XmlNode roleNode = xmlDoc.CreateElement("Role");
                roleNode.InnerText = userRole.RoleId;
                userNode.AppendChild(roleNode);

                rootNode.AppendChild(userNode);
            }
            return xmlDoc;
        }

        /// <summary>
        /// Add Instance
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string AddInstance(AddInstanceModel model)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Instance.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Instance.Title.ToString(), Enums.ParameterType._string, model.Title);
            _dataModelManager.AddParameter(DBEntityNames.Instance.Responsible.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());
            _dataModelManager.AddParameter(DBEntityNames.Instance.GraphId.ToString(), Enums.ParameterType._int, model.GraphId.ToString());

            var instanceIdTable = _manager.InsertData(_dataModelManager.DataModel);

            // add Instance Roles
            if (instanceIdTable.Rows.Count > 0 && instanceIdTable.Rows[0][DBEntityNames.Instance.Id.ToString()] != null)
            {
                var instanceId = (instanceIdTable.Rows[0][DBEntityNames.Instance.Id.ToString()]).ToString();

                AddInstanceDescription(instanceId, model.GraphId.ToString());

                if (model.UserRoles.Count > 0)
                {
                    AddInstanceRoles(model.UserRoles, instanceId);
                }
                return instanceId;
            }
            return string.Empty;
        }

        /// <summary>
        /// Add Roles to Instance
        /// </summary>
        /// <param name="UserRoles"></param>
        private void AddInstanceRoles(List<UserRole> UserRoles, string instanceId)
        {
            var xmlDoc = CreateUserRolesXml(UserRoles);

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddInstanceRoles.ToString());
            _dataModelManager.AddParameter(DBEntityNames.AddInstanceRoles.InstanceId.ToString(), Enums.ParameterType._string, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.AddInstanceRoles.UserRoles.ToString(), Enums.ParameterType._xml, xmlDoc.InnerXml);

            _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Add Instance Description
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="graphId"></param>
        private void AddInstanceDescription(string instanceId, string graphId)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddInstanceDescription.ToString());
            _dataModelManager.AddParameter(DBEntityNames.AddInstanceDescription.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.AddInstanceDescription.GraphId.ToString(), Enums.ParameterType._int, graphId);

            _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Update instance after event Log
        /// </summary>
        /// <param name="instanceId"></param>
        private void UpdateEventLogInstance(string instanceId, string xml)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.UpdateEventLogInstance.ToString());
            _dataModelManager.AddParameter(DBEntityNames.UpdateEventLogInstance.instanceId.ToString(), Enums.ParameterType._int, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.UpdateEventLogInstance.xml.ToString(), Enums.ParameterType._xml, xml);

            _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Update process and phases
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="graphId"></param>
        private void UpdateProcessAndPhases(string processId, string graphId, string title = "")
        {
            var graphXml = _dCRService.GetProcess(graphId);

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Process.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Process.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);
            _dataModelManager.AddParameter(DBEntityNames.Process.Modified.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Process.Status.ToString(), Enums.ParameterType._boolean, Boolean.TrueString);
            if (!string.IsNullOrEmpty(title))
            {
                _dataModelManager.AddParameter(DBEntityNames.Process.Title.ToString(), Enums.ParameterType._string, title);
            }
            _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, graphId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            _manager.UpdateData(_dataModelManager.DataModel);

            var phases = _dCRService.GetProcessPhases(graphId);

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddProcessPhases.ToString());
            _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.ProcessId.ToString(), Enums.ParameterType._int, processId);
            _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.PhaseXml.ToString(), Enums.ParameterType._xml, phases);

            _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Update a document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="type"></param>
        /// <param name="link"></param>
        private void UpdateDocument(string id, string documentName, string link)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentName);
            if (!string.IsNullOrEmpty(link))
            {
                _dataModelManager.AddParameter(DBEntityNames.Document.Link.ToString(), Enums.ParameterType._string, link);
            }
            _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        private void UpdateJournalHistoryDocument(string id, string documentName, DateTime eventTime)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.JournalHistory.ToString());
            _dataModelManager.AddParameter(DBEntityNames.JournalHistory.Title.ToString(), Enums.ParameterType._string, documentName);
            _dataModelManager.AddParameter(DBEntityNames.JournalHistory.EventDate.ToString(), Enums.ParameterType._datetime, eventTime.ToString());
            _dataModelManager.AddFilter(DBEntityNames.JournalHistory.DocumentId.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Select a document using Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        private DataTable SelectDocumentById(string id, List<string> resultSet)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddResultSet(resultSet);
            _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Select a document using Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        private DataTable SelectPersonDocumentByPerson(List<string> resultSet)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddResultSet(resultSet);
            _dataModelManager.AddFilter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, "Personal", Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            _dataModelManager.AddFilter(DBEntityNames.Document.Responsible.ToString(), Enums.ParameterType._string, Common.GetCurrentUserName(), Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            _dataModelManager.AddFilter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.TrueString, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            return _manager.SelectData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Delete a document using Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private void DeleteDocument(string id)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddParameter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.FalseString);
            _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, id, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            _manager.UpdateData(_dataModelManager.DataModel);
        }

        /// <summary>
        /// Delete a file from file system
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        private bool DeleteFileFromFileSystem(string id, string type, string instanceId)
        {
            // select document from Db
            DataTable document = SelectDocumentById(id, new List<string>() { DBEntityNames.Document.Link.ToString() });
            if (document.Rows.Count > 0)
            {
                var path = string.Empty;
                // delete document from file system
                switch (type)
                {
                    case "PersonalDocument":
                        var currentUser = Common.GetCurrentUserName();
                        path = Configurations.Config.PersonalFileLocation + "\\" + currentUser + "\\" + document.Rows[0]["Link"].ToString();
                        break;
                    case "InstanceDocument":
                        path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + document.Rows[0]["Link"].ToString();
                        break;
                    case "JournalNoteBig":
                        path = Configurations.Config.JournalNoteFileLocation + "\\" + instanceId + "\\" + document.Rows[0]["Link"].ToString();
                        break;
                }

                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Connect an instance to a child
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="childId"></param>
        private void ConnectInstanceToChild(string instanceId, int? childId)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.InstanceExtension.ToString());
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
            _dataModelManager.AddParameter(DBEntityNames.InstanceExtension.ChildId.ToString(), Enums.ParameterType._int, childId.ToString());

            _manager.InsertData(_dataModelManager.DataModel);
        }
    }
}