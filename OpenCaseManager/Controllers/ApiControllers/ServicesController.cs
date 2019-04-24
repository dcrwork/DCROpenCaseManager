using Newtonsoft.Json;
using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Http;
using System.Xml;

namespace OpenCaseManager.Controllers.ApiControllers
{
    [Authorize]
    [RoutePrefix("api/Services")]
    public class ServicesController : ApiController
    {
        private IManager _manager;
        private IService _service;
        private IDCRService _dcrService;
        private IDataModelManager _dataModelManager;

        public ServicesController(IManager manager, IService service, IDCRService dCRService, IDataModelManager dataModelManager)
        {
            _manager = manager;
            _service = service;
            _dcrService = dCRService;
            _dataModelManager = dataModelManager;
        }

        /// <summary>
        /// Initialise Instance using DCR Active Repository
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InitializeGraph")]
        public IHttpActionResult InitializeGraph(dynamic input)
        {
            var instanceId = input["instanceId"].ToString();
            var graphId = input["graphId"].ToString();
            var responsibleId = Common.GetResponsibleId();

            var simulationId = string.Empty;
            var eventsXml = string.Empty;
            dynamic result = null;
            var instanceXml = string.Empty;

            // initialize graph/process and get pending or enabled events
            result = _dcrService.InitializeGraph(graphId);
            simulationId = result;
            eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);

            if (!string.IsNullOrEmpty(simulationId))
            {
                Common.UpdateInstance(instanceId, simulationId, instanceXml, _manager, _dataModelManager);
                Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                AutomaticEvents(instanceId, graphId, simulationId, responsibleId);
                return Ok(eventsXml);
            }

            return InternalServerError();
        }

        /// <summary>
        /// Execute a task using dcr active repository
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExecuteEvent")]
        public IHttpActionResult ExecuteEvent(dynamic input)
        {
            var graphId = input["graphId"].ToString();
            var simulationId = input["simulationId"].ToString();
            var eventId = input["eventId"].ToString();
            var instanceId = input["instanceId"].ToString();
            var title = input["title"].ToString();
            var trueEventId = input["trueEventId"].ToString();
            var responsibleId = Common.GetResponsibleId();
            var eventsXml = string.Empty;

            // execute event
            _dcrService.ExecuteEvent(graphId, simulationId, eventId);
            // get pending or enabled from active repository
            eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);

            Common.AddJournalHistory(instanceId, trueEventId, null, "Event", title, DateTime.Now, _manager, _dataModelManager);
            Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
            Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
            AutomaticEvents(instanceId, graphId, simulationId, responsibleId);
            return Ok(eventsXml);
        }

        /// <summary>
        /// Get All Roles for a process
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetProcessRoles")]
        public IHttpActionResult GetProcessRoles(dynamic input)
        {
            if (input["graphId"] != null)
            {
                var graphId = input["graphId"].ToString();

                // get process roles
                var content = _dcrService.GetProcessRoles(graphId);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);
                return Ok(JsonConvert.SerializeObject(xmlDoc));
            }
            return Ok(JsonConvert.SerializeObject(new { }));
        }

        /// <summary>
        /// Search for processes in dcr
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SearchProcess")]
        public IHttpActionResult SearchProcess(dynamic input)
        {
            var graphId = 0;
            var title = string.Empty;
            var content = string.Empty;
            if (input["searchText"] != null)
            {
                title = input["searchText"];
                bool parseStatus = int.TryParse(title, out graphId);
                title = "?title=" + input["searchText"];
            }

            if (graphId == 0)
            {
                // search process
                content = _dcrService.SearchProcess(title);
            }
            else
            {
                // get process
                content = _dcrService.GetProcess(graphId.ToString());
                var graphXml = new XmlDocument();
                graphXml.LoadXml(content);
                var graphTitle = string.Empty;
                if (graphXml.GetElementsByTagName("dcrgraph").Count > 0)
                {
                    graphTitle = graphXml.GetElementsByTagName("dcrgraph")[0].Attributes["title"].Value;
                }

                content = "<graphs><graph id =\"" + graphId + "\" title=\"" + graphTitle + "\"></graph></graphs>";
            }
            content = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + content;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(content);
            return Ok(JsonConvert.SerializeObject(xmlDoc));
        }

        /// <summary>
        /// Get current logged in user details
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetResponsible")]
        public IHttpActionResult GetResponsible(dynamic input)
        {
            var data = Common.GetResponsibleDetails(_manager, _dataModelManager);
            return Ok(data);
        }

        /// <summary>
        /// Get MUS GraphId
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMUSGraphId")]
        public IHttpActionResult GetMUSGraphId()
        {
            dynamic data = Configurations.Config.MUSGraphId;
            var JSONString = JsonConvert.SerializeObject(data);
            return Ok(JSONString);
        }

        /// <summary>
        /// Advance Time
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AdvanceTime")]
        public IHttpActionResult AdvanceTime()
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, "AdvanceTime");
            var data = _manager.ExecuteStoreProcedure(_dataModelManager.DataModel);

            foreach (DataRow row in data.Rows)
            {
                var instanceId = row["Id"].ToString();
                var xml = row["DCRXML"].ToString();
                var simulationId = row["SimId"].ToString();
                var graphId = row["GraphId"].ToString();
                var time = Convert.ToDateTime(row["NextTime"].ToString());

                _dcrService.AdvanceTime(graphId, simulationId, time.ToString("o"));
                var eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);

                Common.SyncEvents(instanceId, eventsXml, 0.ToString(), _manager, _dataModelManager);
                Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                AutomaticEvents(instanceId, graphId, simulationId, 0.ToString());
            }

            return Ok(data);
        }

        /// <summary>
        /// Advance Time
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDCRGraphsURL")]
        public IHttpActionResult GetDCRGraphsURL()
        {
            var url = Common.GetDCRGraphsURL();
            return Ok(url);
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetInstructionHtml")]
        public IHttpActionResult GetInstructionHtml(dynamic input)
        {
            var page = input["page"].ToString();
            var instructionHtml = Common.GetInstructionHtml(page);
            return Ok(instructionHtml);
        }

        /// <summary>
        /// Hide Document Web Part
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("HideDocumentWebPart")]
        public IHttpActionResult HideDocumentWebPart()
        {
            bool status = Common.IsHideDocumentWebpart();
            return Ok(status);
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetReferXmlByEventId")]
        public IHttpActionResult GetReferXmlByEventId(dynamic input)
        {
            return BadRequest();
            //    var eventId = input["eventId"].ToString();
            //    var instanceId = input["instanceId"].ToString();
            //    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
            //    _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString() });
            //    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            //    var data = _manager.SelectData(_dataModelManager.DataModel);
            //    if (data.Rows.Count < 1)
            //    {
            //        return NotFound();
            //    }
            //    var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
            //    //todo:Muddassar do this
            //    var referXml = string.Empty; // _dcrService.GetReferXmlByEventId(eventId, xml);
            //    return Ok(referXml);
        }

        /// <summary>
        /// Get dcr form server url
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDCRFormServerURL")]
        public IHttpActionResult GetDCRFormServerURL()
        {
            var dcrFormServerUrl = Common.GetDcrFormServerUrl();
            return Ok(dcrFormServerUrl);
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("MergeReferXmlWithMainXml")]
        public IHttpActionResult MergeReferXmlWithMainXml(dynamic input)
        {

            return BadRequest();
            //var eventId = input["eventId"].ToString();
            //var instanceId = input["instanceId"].ToString();
            //var referXml = input["referXml"].ToString();

            //_dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
            //_dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString() });
            //_dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            //var data = _manager.SelectData(_dataModelManager.DataModel);
            //if (data.Rows.Count < 1)
            //{
            //    return NotFound();
            //}
            //var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
            ////todo:Muddassar do this
            //var newMainXml = string.Empty;// _dcrService.MergeReferXmlWithMainXml(xml, referXml, eventId);

            //_dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
            //_dataModelManager.AddParameter(DBEntityNames.Instance.DCRXML.ToString(), Enums.ParameterType._xml, newMainXml);
            //_dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            //_manager.UpdateData(_dataModelManager.DataModel);

            //return Ok(Common.ToJson(new { }));
        }

        #region Private Methods
        /// <summary>
        /// Return method type
        /// </summary>
        /// <param name="methodType"></param>
        /// <returns></returns>
        private Method GetMethodType(string methodType)
        {
            switch (methodType)
            {
                case "post":
                    return Method.POST;
                case "put":
                    return Method.PUT;
                case "delete":
                    return Method.DELETE;
                case "options":
                    return Method.OPTIONS;
                case "get":
                default:
                    return Method.GET;
            }

        }

        /// <summary>
        /// Automatic Events
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="responsible"></param>
        private void AutomaticEvents(string instanceId, string graphId, string simulationId, string responsible)
        {
            for (int i = 0; i < Configurations.Config.AutomaticEventsLimit; i++)
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "InstanceAutomaticEvents");
                _dataModelManager.AddResultSet(new List<string>() { "TOP(1) EventId", "EventTitle", "EventOpen", "IsEnabled", "IsPending", "IsIncluded", "IsExecuted", "EventType", "InstanceId", "Responsible", "EventTypeData" });
                _dataModelManager.AddFilter("InstanceId", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var automaticEvents = _manager.SelectData(_dataModelManager.DataModel);
                if (automaticEvents.Rows.Count == 0)
                {
                    i = Configurations.Config.AutomaticEventsLimit;
                }
                else
                {
                    var dataRow = automaticEvents.Rows[0];
                    var eventsXml = string.Empty;
                    var instanceXml = string.Empty;

                    // execute event
                    _dcrService.ExecuteEvent(graphId, simulationId, dataRow["EventId"].ToString());
                    // get pending or enabled from active repository
                    eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);

                    Common.SyncEvents(instanceId, eventsXml, responsible, _manager, _dataModelManager);

                    #region Alec Code
                    // Alec Code will come up here
                    switch (dataRow["EventType"].ToString())
                    {
                        case "CreateCaseAcadre":
                            // get parametes for acadre
                            var parameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager);

                            // create casse in acadre
                            if (parameters.Count > 0)
                            {
                                string caseId = Common.CreateCase(instanceId, parameters, _manager, _dataModelManager);
                                string caseLink = Common.GetCaseLink(caseId);
                                string CaseIdForeign = Common.GetCaseIdForeign(caseId);

                                if (!string.IsNullOrEmpty(caseId))
                                {
                                    // update case Id and case link in open case manager
                                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, "Instance");
                                    _dataModelManager.AddParameter("CaseNoForeign", Enums.ParameterType._string, CaseIdForeign);
                                    _dataModelManager.AddParameter("CaseLink", Enums.ParameterType._string, caseLink);
                                    _dataModelManager.AddParameter("InternalCaseID", Enums.ParameterType._string, caseId);
                                    _dataModelManager.AddFilter("Id", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                                    _manager.UpdateData(_dataModelManager.DataModel);
                                }
                            }
                            break;

                        case "CloseCaseAcadre":

                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Instance");
                            _dataModelManager.AddResultSet(new List<string> { "InternalCaseID" });
                            _dataModelManager.AddFilter("Id", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                            var data = _manager.SelectData(_dataModelManager.DataModel);
                            if (data.Rows.Count > 0 && data.Rows[0]["InternalCaseID"].ToString() != "")
                            {
                                Common.CloseCase(data.Rows[0]["InternalCaseID"].ToString());
                            }
                            break;
                        case "UploadDocument":
                            // get parametes for acadre
                            var documentTitle = "";
                            parameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager);
                            if (parameters.ContainsKey("AssociatedEventId".ToLower()))
                            {
                                documentTitle = parameters["AssociatedEventId".ToLower()];
                            }
                            // get document
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
                            _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Document.Id.ToString(), DBEntityNames.Document.Title.ToString(), DBEntityNames.Document.Link.ToString() });
                            if (string.IsNullOrEmpty(documentTitle))
                            {
                                _dataModelManager.AddFilter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentTitle, Enums.CompareOperator.like, Enums.LogicalOperator.and);
                            }
                            else
                            {
                                _dataModelManager.AddFilter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentTitle, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                            }
                            _dataModelManager.AddFilter(DBEntityNames.Document.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                            _dataModelManager.AddFilter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.TrueString, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                            _dataModelManager.AddFilter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, "Temp", Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                            var document = _manager.SelectData(_dataModelManager.DataModel);

                            if (document.Rows.Count > 0)
                            {
                                var fileName = document.Rows[0][DBEntityNames.Document.Title.ToString()].ToString();
                                var filePath = document.Rows[0][DBEntityNames.Document.Link.ToString()].ToString();

                                fileName = fileName + Path.GetExtension(filePath);
                                byte[] fileBytes = File.ReadAllBytes(filePath);

                                // delete temp document
                                try
                                {
                                    string documentId = Common.CreateDocument(parameters, fileName, fileBytes, instanceId, _manager, _dataModelManager);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                                finally
                                {
                                    var fileInfo = new FileInfo(filePath);
                                    if (fileInfo.Exists)
                                    {
                                        fileInfo.Directory.Delete(true);
                                    }

                                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Document.ToString());
                                    _dataModelManager.AddParameter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.FalseString);
                                    _dataModelManager.AddFilter(DBEntityNames.Document.Id.ToString(), Enums.ParameterType._int, document.Rows[0][DBEntityNames.Document.Id.ToString()].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                                    _manager.UpdateData(_dataModelManager.DataModel);
                                }
                            }
                            break;
                    }
                    #endregion

                }
            }
        }
        #endregion
    }
}
