using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
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
        private bool UseProcessEngine = false;
        private IDocumentManager _documentManager;
        private IAutomaticEvents _automaticEvents;
        private IMailRepository _mailRepository;
        private ICommons _commons;

        public ServicesController(IManager manager, IService service, IDCRService dCRService,
            IDataModelManager dataModelManager, IDocumentManager documentManager, IAutomaticEvents automaticEvents,
            IMailRepository mailRepository, ICommons commons)
        {
            _manager = manager;
            _service = service;
            _dcrService = dCRService;
            UseProcessEngine = Configurations.Config.UseProcessEngine;
            _dataModelManager = dataModelManager;
            _documentManager = documentManager;
            _automaticEvents = automaticEvents;
            _mailRepository = mailRepository;
            _commons = commons;
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
            try
            {
                var instanceId = input["instanceId"].ToString();
                var graphId = input["graphId"].ToString();
                var responsibleId = Common.GetResponsibleId();
                InitializeGraphModel model = Common.InitializeGraph(UseProcessEngine, graphId, instanceId, _dcrService, _manager, _dataModelManager);

                if (!string.IsNullOrEmpty(model.SimulationId))
                {
                    Common.UpdateInstance(instanceId, model.SimulationId, model.InstanceXML, _manager, _dataModelManager);
                    Common.SyncEvents(instanceId, model.EventsXML, responsibleId, _manager, _dataModelManager);
                    Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                    AutomaticEvents(instanceId, graphId, model.SimulationId, responsibleId);
                    return Ok(model.EventsXML);
                }
                return BadRequest("No Instance is created.");
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "InitializeGraph - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Execute a task
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExecuteEvent")]
        public IHttpActionResult ExecuteEvent(dynamic input)
        {
            try
            {
                var graphId = input["graphId"].ToString();
                var simulationId = input["simulationId"].ToString();
                var eventId = input["eventId"].ToString();
                var instanceId = input["instanceId"].ToString();
                var trueEventId = input["trueEventId"].ToString();
                var responsibleId = Common.GetResponsibleId();
                var title = string.Empty;
                var childId = Common.GetInstanceChildId(_manager, _dataModelManager, instanceId);
                

                try
                {
                    title = input["title"].ToString();
                }
                catch
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Event.ToString());
                    _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Event.Title.ToString() });
                    _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, trueEventId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    var eventTitle = _manager.SelectData(_dataModelManager.DataModel);
                    if (eventTitle.Rows.Count > 0)
                        title = eventTitle.Rows[0]["Title"].ToString();
                }

                string eventsXml;

                // execute event
                if (!UseProcessEngine)
                {
                    _dcrService.ExecuteEventWithTime(graphId, simulationId, eventId, DateTime.UtcNow.ToString("o"));
                    // get pending or enabled from active repository
                    eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);
                }
                else
                {
                    string role = _commons.GetResponsibleRoles(instanceId, trueEventId, responsibleId);

                    // get instance xml
                    var instanceXML = Common.GetInstanceXML(instanceId, _manager, _dataModelManager);
                    dynamic dcrGraph = new DCRGraph(instanceXML);
                    dcrGraph = _dcrService.ExecuteEventWithTimeAndRole(dcrGraph, eventId, DateTime.UtcNow, role);
                    string instanceXml = ((DCRGraph)dcrGraph).ToXml();
                    Common.UpdateInstance(instanceId, simulationId, instanceXml, _manager, _dataModelManager);
                    // get pending or enabled from active repository
                    eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                }

                // Mark Event as Applicable
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Event.NotApplicable.ToString(), Enums.ParameterType._boolean, false.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, trueEventId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _manager.UpdateData(_dataModelManager.DataModel);

                Common.AddJournalHistory(instanceId, trueEventId, null, "Event", title, childId.ToString(), DateTime.Now, _manager, _dataModelManager);
                Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                if (Configurations.Config.AlwaysLogExecutions)
                {
                    if (Configurations.Config.LogToAcadre)
                        Common.CreateMemoAcadre(instanceId, eventId, false, _manager, _dataModelManager, _service);
                }
                AutomaticEvents(instanceId, graphId, simulationId, responsibleId);
                return Ok(eventsXml);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "ExecuteEvent - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Sync Evnts
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SyncEvents")]
        public IHttpActionResult SyncEvents(dynamic input)
        {
            try
            {
                var eventsXML = input["xml"].ToString();
                var instanceId = input["instanceId"].ToString();
                var responsibleId = Common.GetResponsibleId();
                var title = string.Empty;

                if (eventsXML != "" && instanceId != "")
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                    _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
                    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    var data = _manager.SelectData(_dataModelManager.DataModel);
                    if (data.Rows.Count > 0)
                    {
                        try
                        {
                            var xmlDocument = new XmlDocument();
                            xmlDocument.LoadXml(eventsXML);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("events xml is not well defined\n" + ex.Message);
                        }
                        Common.SyncEvents(instanceId, eventsXML, responsibleId, _manager, _dataModelManager);
                        Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                        if (Configurations.Config.AlwaysLogExecutions)
                        {
                            if (Configurations.Config.LogToAcadre)
                                Common.CreateMemoAcadre(instanceId, "## Manaully Called Sync Events ##", false, _manager, _dataModelManager, _service);
                        }
                        AutomaticEvents(instanceId, data.Rows[0]["GraphId"].ToString(), data.Rows[0]["SimulationId"].ToString(), responsibleId);
                    }
                    else
                    {
                        throw new Exception("No Instance Found with this Id");
                    }

                }
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Not Applicable Tasks
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("NotApplicable")]
        public IHttpActionResult NotApplicableTask(dynamic input)
        {
            try
            {
                var graphId = input["graphId"].ToString();
                var simulationId = input["simulationId"].ToString();
                var eventId = input["eventId"].ToString();
                var instanceId = input["instanceId"].ToString();
                var trueEventId = input["trueEventId"].ToString();
                var note = input["note"].ToString();
                var responsibleId = Common.GetResponsibleId();
                var eventsXml = string.Empty;

                if (UseProcessEngine)
                {
                    // get instance xml
                    var instanceXML = Common.GetInstanceXML(instanceId, _manager, _dataModelManager);
                    dynamic dcrGraph = new DCRGraph(instanceXML);
                    dcrGraph = _dcrService.NotApplicable(dcrGraph, eventId, note);
                    string instanceXml = ((DCRGraph)dcrGraph).ToXml();
                    Common.UpdateInstance(instanceId, simulationId, instanceXml, _manager, _dataModelManager);
                    // get pending or enabled from active repository
                    eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                }
                // Mark Event as Not Applicable
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
                _dataModelManager.AddParameter(DBEntityNames.Event.NotApplicable.ToString(), Enums.ParameterType._boolean, true.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, trueEventId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _manager.UpdateData(_dataModelManager.DataModel);

                Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                AutomaticEvents(instanceId, graphId, simulationId, responsibleId);
                return Ok(eventsXml);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "NotApplicableTask - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Can execute global events
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CanExecuteGlobalEvent")]
        public IHttpActionResult CanExecuteGlobalEvent(dynamic input)
        {
            try
            {
                var eventId = input["eventId"].ToString();
                var childId = input["childId"].ToString();
                Common.LogInfo(_manager, _dataModelManager, "Stored Procedure called - Can Execute Global Event {eventId : " + eventId + ",childId:" + childId + "}");

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.CanExecuteGlobalEvent.ToString());
                _dataModelManager.AddParameter(DBEntityNames.CanExecuteGlobalEvent.ChildId.ToString(), Enums.ParameterType._int, childId);
                _dataModelManager.AddParameter(DBEntityNames.CanExecuteGlobalEvent.EventId.ToString(), Enums.ParameterType._string, eventId);

                var globalEvents = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                return Ok(JsonConvert.SerializeObject(globalEvents));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CanExecuteGlobalEvent - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Execute global events
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExecuteGlobalEvents")]
        public IHttpActionResult ExecuteGlobalEvents(List<GlobalEvents> globalEvents)
        {
            try
            {
                Common.LogInfo(_manager, _dataModelManager, "Starting event execution - GlobalEvents");
                foreach (var globalEvent in globalEvents)
                {
                    var responsibleId = Common.GetResponsibleId();
                    
                    var instanceXml = string.Empty;
                    dynamic dcrGraph = null;
                    var eventsXml = string.Empty;

                    // execute event
                    if (!UseProcessEngine)
                    {
                        _dcrService.ExecuteEventWithTime(globalEvent.GraphId.ToString(), globalEvent.SimulationId.ToString(), globalEvent.EventId, DateTime.UtcNow.ToString("o"));
                        // get pending or enabled from active repository
                        eventsXml = _dcrService.GetPendingOrEnabled(globalEvent.GraphId.ToString(), globalEvent.SimulationId.ToString());
                    }
                    else
                    {
                        // get instance xml
                        var instanceXML = Common.GetInstanceXML(globalEvent.InstanceId, _manager, _dataModelManager);
                        dcrGraph = new DCRGraph(instanceXML);
                        dcrGraph = _dcrService.ExecuteEventWithTime(dcrGraph, globalEvent.EventId, DateTime.UtcNow);
                        instanceXml = ((DCRGraph)dcrGraph).ToXml();
                        Common.UpdateInstance(globalEvent.InstanceId, globalEvent.SimulationId.ToString(), instanceXml, _manager, _dataModelManager);
                        // get pending or enabled from active repository
                        eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                        Common.LogInfo(_manager, _dataModelManager, "EventId '" + globalEvent.EventId + "' executed for Instance Id : " + globalEvent.InstanceId + " - GlobalEvents");
                    }

                    // Mark Event as Applicable
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Event.NotApplicable.ToString(), Enums.ParameterType._boolean, false.ToString());
                    _dataModelManager.AddFilter(DBEntityNames.Event.Id.ToString(), Enums.ParameterType._int, globalEvent.TrueEventId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    _manager.UpdateData(_dataModelManager.DataModel);
                    
                    Common.AddJournalHistory(globalEvent.InstanceId, globalEvent.TrueEventId.ToString(), null, "Event", globalEvent.EventTitle, Common.GetInstanceChildId(_manager, _dataModelManager, globalEvent.InstanceId).ToString(), DateTime.Now, _manager, _dataModelManager);
                    Common.SyncEvents(globalEvent.InstanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                    Common.UpdateEventTypeData(globalEvent.InstanceId, _manager, _dataModelManager);

                    if (Configurations.Config.AlwaysLogExecutions)
                    {
                        if (Configurations.Config.LogToAcadre)
                            Common.CreateMemoAcadre(globalEvent.InstanceId, globalEvent.EventId, false, _manager, _dataModelManager, _service);
                    }
                    AutomaticEvents(globalEvent.InstanceId, globalEvent.GraphId.ToString(), globalEvent.SimulationId.ToString(), responsibleId);
                }
                Common.LogInfo(_manager, _dataModelManager, "Event execution Completed - GlobalEvents");
                return Ok(JsonConvert.SerializeObject(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "ExecuteGlobalEvents - Failed. - " + Common.ToJson(globalEvents));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
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
            try
            {
                if (input["graphId"] != null)
                {
                    var graphId = input["graphId"].ToString();

                    // get process roles
                    var content = _dcrService.GetProcessRoles(graphId);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml("<?xml version=\"1.0\"?>" + content);
                    return Ok(JsonConvert.SerializeObject(xmlDoc));
                }
                return Ok(JsonConvert.SerializeObject(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetProcessRoles - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
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
            try
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
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "SearchProcess - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
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
            try
            {
                var data = Common.GetResponsibleDetails(_manager, _dataModelManager);
                return Ok(data);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetResponsible - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get MUS GraphId
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMUSGraphId")]
        public IHttpActionResult GetMUSGraphId()
        {
            try
            {
                dynamic data = Configurations.Config.MUSGraphId;
                var JSONString = JsonConvert.SerializeObject(data);
                return Ok(JSONString);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetMUSGraphId - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Advance Time
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AdvanceTime")]
        public IHttpActionResult AdvanceTime()
        {
            try
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, "AdvanceTime");
                var data = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                if (!UseProcessEngine)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var instanceId = row["Id"].ToString();
                        var xml = row["DCRXML"].ToString();
                        var simulationId = row["SimId"].ToString();
                        var graphId = row["GraphId"].ToString();
                        var time = Convert.ToDateTime(row["NextTime"].ToString());

                        try
                        {
                            _dcrService.AdvanceTime(graphId, simulationId, time.ToString("o"));
                            var eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);

                            Common.SyncEvents(instanceId, eventsXml, 0.ToString(), _manager, _dataModelManager);
                            Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                            AutomaticEvents(instanceId, graphId, simulationId, 0.ToString());
                        }
                        catch (Exception ex)
                        {
                            string message = "AdvanceTime Failed for graphId : " + graphId + " , instanceId : " + instanceId + " , time : " + time.ToString() + " , xml : " + xml;
                            Common.LogInfo(_manager, _dataModelManager, message);
                            Common.LogError(ex);
                        }
                    }
                }
                else
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var instanceId = row["Id"].ToString();
                        var xml = row["DCRXML"].ToString();
                        var simulationId = row["SimId"].ToString();
                        var responsibleId = Common.GetResponsibleId();
                        var graphId = row["GraphId"].ToString();
                        var time = Convert.ToDateTime(row["NextTime"].ToString());

                        try
                        {
                            var dcrGraph = new DCRGraph(xml);
                            dcrGraph.SetTime(time);
                            var instanceXml = dcrGraph.ToXml();
                            Common.UpdateInstance(instanceId, simulationId, instanceXml, _manager, _dataModelManager);
                            // get pending or enabled from active repository
                            var eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);

                            Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                            Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                            AutomaticEvents(instanceId, graphId, simulationId, responsibleId);

                        }
                        catch (Exception ex)
                        {
                            string message = "AdvanceTime Failed for graphId : " + graphId + " , instanceId : " + instanceId + " , time : " + time.ToString() + " , xml : " + xml;
                            Common.LogInfo(_manager, _dataModelManager, message);
                            Common.LogError(ex);
                        }
                    }
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AdvanceTime - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Set time for an instance
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("SetTime")]
        public IHttpActionResult SetTime(dynamic input)
        {
            try
            {
                string instanceId = input["instanceId"];
                string time = input["time"];

                var instance = Common.GetInstanceDetails(_manager, _dataModelManager, instanceId);
                if (instance.Rows.Count > 0)
                {
                    var row = instance.Rows[0];
                    if (!UseProcessEngine)
                    {
                        var xml = row["DCRXML"].ToString();
                        var simulationId = row["SimulationId"].ToString();
                        var graphId = row["GraphId"].ToString();
                        DateTime timeDateTime = Convert.ToDateTime(time.ToString()).ToUniversalTime();

                        try
                        {
                            _dcrService.AdvanceTime(graphId, simulationId, timeDateTime.ToString("o"));
                            var eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);

                            Common.SyncEvents(instanceId, eventsXml, 0.ToString(), _manager, _dataModelManager);
                            Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                            AutomaticEvents(instanceId, graphId, simulationId, 0.ToString());
                        }
                        catch (Exception ex)
                        {
                            Common.LogInfo(_manager, _dataModelManager, "SetTime - Failed. - instanceId : " + instanceId.ToString() + " graphId : " + graphId + ", simulationId : " + simulationId + ", xml : " + xml + ",time : " + time.ToString());
                            Common.LogError(ex);
                        }
                    }
                    else
                    {
                        var xml = row["DCRXML"].ToString();
                        var simulationId = row["SimulationId"].ToString();
                        var responsibleId = Common.GetResponsibleId();
                        var graphId = row["GraphId"].ToString();
                        DateTime timeDateTime = Convert.ToDateTime(time.ToString()).ToUniversalTime();

                        try
                        {
                            var dcrGraph = new DCRGraph(xml);
                            dcrGraph.SetTime(timeDateTime);
                            var instanceXml = dcrGraph.ToXml();
                            Common.UpdateInstance(instanceId, simulationId, instanceXml, _manager, _dataModelManager);
                            // get pending or enabled from active repository
                            var eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);

                            Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                            Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                            AutomaticEvents(instanceId, graphId, simulationId, responsibleId);
                        }
                        catch (Exception ex)
                        {
                            Common.LogInfo(_manager, _dataModelManager, "SetTime - Failed. - instanceId : " + instanceId.ToString() + " graphId : " + graphId + ", simulationId : " + simulationId + ", xml : " + xml + ",time : " + time.ToString());
                            Common.LogError(ex);
                        }
                    }
                }
                return Ok(instance);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "SetTime - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get DCR Graphs URL
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDCRGraphsURL")]
        public IHttpActionResult GetDCRGraphsURL()
        {
            try
            {
                var url = Common.GetDCRGraphsURL();
                return Ok(url);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetDCRGraphsURL - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetInstructionHtml")]
        public IHttpActionResult GetInstructionHtml(dynamic input)
        {
            try
            {
                var page = input["page"].ToString();
                var instructionHtml = Common.GetInstructionHtml(page);
                return Ok(instructionHtml);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetInstructionHtml - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Hide Document Web Part
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("HideDocumentWebPart")]
        public IHttpActionResult HideDocumentWebPart()
        {
            try
            {
                bool status = Common.IsHideDocumentWebpart();
                return Ok(status);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "HideDocumentWebPart - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetReferXmlByEventId")]
        public IHttpActionResult GetReferXmlByEventId(dynamic input)
        {
            try
            {
                var eventId = input["eventId"].ToString();
                var instanceId = input["instanceId"].ToString();
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString(), DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = _manager.SelectData(_dataModelManager.DataModel);
                if (data.Rows.Count < 1)
                {
                    return NotFound();
                }
                var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
                var graphId = data.Rows[0][DBEntityNames.Instance.GraphId.ToString()].ToString();
                var simulationId = data.Rows[0][DBEntityNames.Instance.SimulationId.ToString()].ToString();
                var referXml = string.Empty;
                if (!UseProcessEngine)
                {
                    referXml = _dcrService.GetReferXmlByEventId(graphId, simulationId, eventId);
                }
                else
                {
                    referXml = _dcrService.GetReferXmlByEventId(eventId, xml);
                }
                return Ok(referXml);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetReferXmlByEventId - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get dcr form server url
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDCRFormServerURL")]
        public IHttpActionResult GetDCRFormServerURL()
        {
            try
            {
                var dcrFormServerUrl = Common.GetDcrFormServerUrl();
                return Ok(dcrFormServerUrl);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetDCRFormServerURL - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("MergeReferXmlWithMainXml")]
        public IHttpActionResult MergeReferXmlWithMainXml(dynamic input)
        {
            try
            {
                var eventId = input["eventId"].ToString();
                var instanceId = input["instanceId"].ToString();
                var referXml = input["referXml"].ToString();
                var eventsXml = string.Empty;

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString(), DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = _manager.SelectData(_dataModelManager.DataModel);
                if (data.Rows.Count < 1)
                {
                    return NotFound();
                }
                var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
                var graphId = data.Rows[0][DBEntityNames.Instance.GraphId.ToString()].ToString();
                var simulationId = data.Rows[0][DBEntityNames.Instance.SimulationId.ToString()].ToString();
                if (!UseProcessEngine)
                {
                    if (!string.IsNullOrEmpty(referXml))
                    {
                        string newMainXml = string.Empty;
                        try
                        {
                            _dcrService.MergeReferXmlWithMainXml(graphId, simulationId, referXml, eventId);
                            // get pending or enabled from active repository
                            eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);
                        }
                        catch (Exception ex)
                        {
                            Common.LogInfo(_manager, _dataModelManager, "MergeReferXmlWithMainXml - Failed At MergeReferXmlWithMainXml  - eventId : " + eventId + ",referxml : " + referXml + ",mainXML : " + xml + " - " + Common.ToJson(input));
                            Common.LogError(ex);
                            throw ex;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(referXml))
                    {
                        string newMainXml = string.Empty;
                        try
                        {
                            newMainXml = _dcrService.MergeReferXmlWithMainXml(xml, referXml, eventId);
                        }
                        catch (Exception ex)
                        {
                            Common.LogInfo(_manager, _dataModelManager, "MergeReferXmlWithMainXml - Failed At MergeReferXmlWithMainXml  - eventId : " + eventId + ",referxml : " + referXml + ",mainXML : " + xml + " - " + Common.ToJson(input));
                            Common.LogError(ex);
                            throw ex;
                        }

                        if (!string.IsNullOrEmpty(newMainXml))
                        {
                            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                            _dataModelManager.AddParameter(DBEntityNames.Instance.DCRXML.ToString(), Enums.ParameterType._xml, newMainXml);
                            _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                            _manager.UpdateData(_dataModelManager.DataModel);

                            // get instance xml
                            var dcrGraph = new DCRGraph(newMainXml);
                            // get pending or enabled from active repository
                            eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(eventsXml))
                {
                    try
                    {
                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Event.ToString());
                        _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Event.Title.ToString(), DBEntityNames.Event.Id.ToString() });
                        _dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                        _dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                        var eventDetails = _manager.SelectData(_dataModelManager.DataModel);

                        Common.AddJournalHistory(instanceId, eventDetails.Rows[0]["Id"].ToString(), null, "Event", eventDetails.Rows[0]["Title"].ToString(), Common.GetInstanceChildId(_manager, _dataModelManager, instanceId).ToString(), DateTime.Now, _manager, _dataModelManager);
                        Common.SyncEvents(instanceId, eventsXml, Common.GetResponsibleId(), _manager, _dataModelManager);
                        Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                        AutomaticEvents(instanceId, graphId, simulationId, Common.GetResponsibleId());
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "MergeReferXmlWithMainXml - Failed After MergeReferXmlWithMainXml  - " + Common.ToJson(input));
                        Common.LogError(ex);
                        throw ex;
                    }
                }
                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "MergeReferXmlWithMainXml - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddTask")]
        public IHttpActionResult AddTask(dynamic input)
        {
            try
            {
                string label = input["label"].ToString();
                var role = input["role"].ToString();
                var description = input["description"].ToString();
                var instanceId = input["instanceId"].ToString();
                var eventsXml = string.Empty;

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString(), DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = _manager.SelectData(_dataModelManager.DataModel);

                var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
                var graphId = data.Rows[0][DBEntityNames.Instance.GraphId.ToString()].ToString();
                var simulationId = data.Rows[0][DBEntityNames.Instance.SimulationId.ToString()].ToString();

                if (!UseProcessEngine)
                {

                }
                else
                {
                    var eventId = new string(label.Where(c => char.IsLetter(c)).ToArray());
                    eventId += DateTime.Now.ToString("yyyyMMddHHmmss");

                    var newXml = _dcrService.AddEvent(eventId, label, role, description, xml);

                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Instance.DCRXML.ToString(), Enums.ParameterType._xml, newXml);
                    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    _manager.UpdateData(_dataModelManager.DataModel);

                    // get instance xml
                    var dcrGraph = new DCRGraph(newXml);
                    // get pending or enabled events
                    eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                }
                if (!string.IsNullOrEmpty(eventsXml))
                {
                    try
                    {
                        Common.SyncEvents(instanceId, eventsXml, Common.GetResponsibleId(), _manager, _dataModelManager);
                        Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                        AutomaticEvents(instanceId, graphId, simulationId, Common.GetResponsibleId());
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "AddTask - Failed after AddTask -  eventsXML : " + eventsXml + " - " + Common.ToJson(input));
                        Common.LogError(ex);
                        throw ex;
                    }
                }

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddTask - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Instruction Html
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("RemoveTask")]
        public IHttpActionResult RemoveTask(dynamic input)
        {
            try
            {
                var eventId = input["eventId"].ToString();
                var instanceId = input["instanceId"].ToString();
                var eventsXml = string.Empty;

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString(), DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = _manager.SelectData(_dataModelManager.DataModel);

                var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
                var graphId = data.Rows[0][DBEntityNames.Instance.GraphId.ToString()].ToString();
                var simulationId = data.Rows[0][DBEntityNames.Instance.SimulationId.ToString()].ToString();

                if (!UseProcessEngine)
                {

                }
                else
                {

                    var newXml = _dcrService.RemoveEvent(eventId, xml);

                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Instance.DCRXML.ToString(), Enums.ParameterType._xml, newXml);
                    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    _manager.UpdateData(_dataModelManager.DataModel);

                    // get instance xml
                    var dcrGraph = new DCRGraph(newXml);
                    // get pending or enabled events
                    eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                }
                if (!string.IsNullOrEmpty(eventsXml))
                {
                    try
                    {
                        Common.SyncEvents(instanceId, eventsXml, Common.GetResponsibleId(), _manager, _dataModelManager);
                        Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                        AutomaticEvents(instanceId, graphId, simulationId, Common.GetResponsibleId());
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "RemoveTask - Failed After RemoveTask - eventsXML : " + eventsXml + " - " + Common.ToJson(input));
                        Common.LogError(ex);
                        throw ex;
                    }
                }

                return Ok(Common.ToJson(new { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "RemoveTask - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get my children from Acadre
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("SearchChildren")]
        public IHttpActionResult SearchChildren(AcadreLib.SearchCriterion searchCriteria)
        {
            try
            {
                var children = Common.SeacrhChildren(_manager, _dataModelManager, searchCriteria);

                return Ok(Common.ToJson(children));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "SearchChildren - Failed. - " + Common.ToJson(searchCriteria));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Child Info
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetChildInfo")]
        public IHttpActionResult GetChildInfo(dynamic input)
        {
            try
            {
                string JournalCaseId = string.Empty;
                string CPR = string.Empty;
                AcadreLib.Child childInfo = null;
                try
                {
                    JournalCaseId = input["JournalCaseId"].ToString();
                }
                catch (Exception)
                {

                }

                try
                {
                    CPR = input["CPR"].ToString();
                }
                catch (Exception)
                {

                }

                if (!string.IsNullOrEmpty(JournalCaseId))
                {
                    var caseID = Convert.ToInt32(JournalCaseId);
                    childInfo = Common.GetChildInfo(_manager, _dataModelManager, caseID);
                }
                if (!string.IsNullOrEmpty(CPR))
                {
                    childInfo = Common.GetChildInfo(_manager, _dataModelManager, CPR);
                }
                return Ok(Common.ToJson(childInfo));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetChildInfo - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Child Journal Documents
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetChildJournalDocuments")]
        public IHttpActionResult GetChildJournalDocuments(JournalDocument journalDocument)
        {
            try
            {
                var childDocument = Common.GetChildJournalDocuments(_manager, _dataModelManager, journalDocument);
                return Ok(Common.ToJson(childDocument));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetChildJournalDocuments - Failed. - " + Common.ToJson(journalDocument));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetChildCases")]
        public IHttpActionResult GetChildCases(dynamic input)
        {
            try
            {
                string JournalCaseId = input["JournalCaseId"].ToString();
                var caseID = Convert.ToInt32(JournalCaseId);
                var childCases = Common.GetChildCases(_manager, _dataModelManager, caseID);
                return Ok(Common.ToJson(childCases));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetChildCases - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateChildJournal")]
        public IHttpActionResult CreateChildJournal(AcadreLib.SearchCriterion searchCriteria)
        {
            try
            {
                var childJournal = Common.CreateChildJournal(_manager, _dataModelManager, searchCriteria.ChildCPR, searchCriteria.AcadreOrgID, searchCriteria.CaseManagerInitials);
                return Ok(Common.ToJson(childJournal));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CreateChildJournal - Failed. - " + Common.ToJson(searchCriteria));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create Journal In Acadre
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateJournalAcadre")]
        public IHttpActionResult CreateJournalAcadre(Memo input)
        {
            try
            {
                var accessCode = input.AccessCode;
                var caseFileReference = input.CaseFileReferenceNumber;
                var creator = Common.GetCurrentUserName();
                var fileName = input.FileName == string.Empty ? DateTime.Now.ToFileTime().ToString() + ".rtf" : input.FileName;
                var memoTitleText = input.MemoTitleText;
                var memoTypeReference = input.MemoTypeReference;
                var memoIsLocked = input.IsLocked;
                var fileBytes = new byte[] { };
                var instanceId = string.Empty;

                if (fileName.Equals("0") || string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = DateTime.Now.ToFileTime() + ".rtf";
                }
                else if (!fileName.EndsWith(".rtf"))
                {
                    fileName += ".rtf";
                }

                switch (input.Html)
                {
                    case "text":
                        if (input.Type == "instance")
                            instanceId = input.InstanceId;
                        fileBytes = Common.GetRTFDocument(input.NoteText, _manager, _dataModelManager, instanceId, input.EventId);
                        break;
                    case "html":
                        if (input.Type == "instance")
                            instanceId = input.InstanceId;
                        fileBytes = Common.GetRtfFromHtml(instanceId, input.NoteText, _service, _manager, _dataModelManager, input.EventId);
                        break;
                }
                var date = input.Date.ToLocalTime();

                AcadrePWS.CaseManagement.ActingFor(Common.GetCurrentUserName());
                AcadrePWS.CaseManagement.CreateMemo(fileName, accessCode, caseFileReference, memoTitleText, creator, memoTypeReference, memoIsLocked, fileBytes, date);
                return Ok("");
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CreateJournalAcadre - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Upload Document In Acadre
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadDocumentAcadre")]
        public IHttpActionResult UploadDocumentAcadre()
        {
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime documentDate = DateTime.Now;
                var request = HttpContext.Current.Request;
                string instanceId = request.Headers["instanceId"];
                string documentCategoryCode = Common.ReplaceEventTypeKeyValues(request.Headers["documentCategoryCode"], instanceId, _manager, _dataModelManager);
                string documentTitleText = Common.ReplaceEventTypeKeyValues(request.Headers["documentTitleText"], instanceId, _manager, _dataModelManager);
                string documentStatusCode = Common.ReplaceEventTypeKeyValues(request.Headers["documentStatusCode"], instanceId, _manager, _dataModelManager);
                if (request.Headers["documentDate"].ToString() == "$(now)")
                    documentDate = Common.ReplaceEventTypeKeyValues(request.Headers["documentDate"], instanceId, _manager, _dataModelManager);
                else
                    documentDate = Convert.ToDateTime(request.Headers["documentDate"], provider);
                string documentAccessCode = Common.ReplaceEventTypeKeyValues(request.Headers["documentAccessCode"], instanceId, _manager, _dataModelManager);
                string documentCaseId = Common.ReplaceEventTypeKeyValues(request.Headers["documentCaseId"], instanceId, _manager, _dataModelManager);
                string documentDescriptionText = Common.ReplaceEventTypeKeyValues(request.Headers["documentDescriptionText"], instanceId, _manager, _dataModelManager);
                string documentAccessLevel = Common.ReplaceEventTypeKeyValues(request.Headers["documentAccessLevel"], instanceId, _manager, _dataModelManager);
                string documentTypeCode = Common.ReplaceEventTypeKeyValues(request.Headers["documentTypeCode"], instanceId, _manager, _dataModelManager);
                string recordStatusCode = Common.ReplaceEventTypeKeyValues(request.Headers["recordStatusCode"], instanceId, _manager, _dataModelManager);
                string documentUserId = Common.ReplaceEventTypeKeyValues(request.Headers["documentUserId"], instanceId, _manager, _dataModelManager);
                string recordPublicationIndicator = Common.ReplaceEventTypeKeyValues(request.Headers["recordPublicationIndicator"], instanceId, _manager, _dataModelManager);
                string fileName = Common.ReplaceEventTypeKeyValues(request.Headers["filename"], instanceId, _manager, _dataModelManager);
                byte[] fileBytes;

                using (var stream = new MemoryStream())
                {
                    request.InputStream.CopyTo(stream);
                    fileBytes = stream.ToArray();
                }

                if (fileBytes != null && documentCaseId != "$(InternalCaseId)")
                {
                    // set user
                    AcadrePWS.CaseManagement.ActingFor(Common.GetCurrentUserName());
                    var documentId = AcadrePWS.CaseManagement.CreateDocumentService(
                            documentCaseId,
                            recordStatusCode,
                            documentTypeCode,
                            documentDescriptionText,
                            documentAccessCode,
                            documentStatusCode,
                            documentTitleText,
                            documentCategoryCode,
                            recordPublicationIndicator,
                            fileName,
                            fileBytes
                        );
                    Common.LogInfo(_manager, _dataModelManager, "Document Created - document Id : " + documentId);
                    return Ok("");
                }
                else
                {
                    var newException = new Exception("No file uploaded");
                    Common.LogError(newException);
                    return InternalServerError(newException);
                }
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get value from web.config
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetKeyValue")]
        public IHttpActionResult GetKeyValue(string key)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key].ToString();
                return Ok(value);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetKeyValue - Failed. - " + Common.ToJson(key));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Child Case Documents
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetChildCaseDocuments")]
        public IHttpActionResult GetChildCaseDocuments(dynamic input)
        {
            try
            {
                string InstanceId = input["InstanceId"].ToString();
                var intrnalCaseId = Common.GeInternalCaseId(_manager, _dataModelManager, InstanceId);
                var caseDocuments = Common.GetChildCaseDocuments(_manager, _dataModelManager, intrnalCaseId);
                return Ok(Common.ToJson(caseDocuments));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetChildCaseDocuments - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Child
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateChild")]
        public IHttpActionResult UpdateChild(dynamic input)
        {
            try
            {
                string obsText = input["obsText"].ToString();
                string childId = input["childId"].ToString();
                Common.UpdateChild(obsText, childId, _manager, _dataModelManager);
                return Ok(Common.ToJson(""));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateChild - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("ReadEmail")]
        public IHttpActionResult ReadEmail()
        {
            try
            {
                var response = _mailRepository.GetUnreadMails();
                var response1 = _mailRepository.GetAllMails();
                return Ok(Common.ToJson(""));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "ReadEmail - Failed.");
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update Instance DCRXML
        /// </summary>
        /// <returns></returns>        
        [HttpPost]
        [Route("UpdateInstanceDCRXML")]
        public IHttpActionResult UpdateInstanceDCRXML(dynamic input)
        {
            string instanceId = input["instanceId"].ToString();
            string dcrXML = input["DCRXML"] == null ? "" : input["DCRXML"].ToString();

            try
            {
                if (!string.IsNullOrWhiteSpace(instanceId) && !string.IsNullOrWhiteSpace(dcrXML))
                {
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Instance.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Instance.DCRXML.ToString(), Enums.ParameterType._xml, dcrXML.ToString());
                    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    _manager.UpdateData(_dataModelManager.DataModel);

                    // get instance details
                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                    _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
                    _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    var dataTable = _manager.SelectData(_dataModelManager.DataModel);
                    string graphId = dataTable.Rows[0][DBEntityNames.Instance.GraphId.ToString()].ToString();
                    string simulationId = dataTable.Rows[0][DBEntityNames.Instance.SimulationId.ToString()].ToString();

                    string eventsXml;
                    // execute event
                    if (!UseProcessEngine)
                    {
                        // get pending or enabled from active repository
                        eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);
                    }
                    else
                    {
                        // get instance xml
                        dynamic dcrGraph = new DCRGraph(dcrXML);
                        // get pending or enabled from active repository
                        eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                    }
                    var responsibleId = Common.GetResponsibleId();

                    try
                    {
                        Common.SyncEvents(instanceId, eventsXml, responsibleId, _manager, _dataModelManager);
                        Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                        AutomaticEvents(instanceId, graphId, simulationId, responsibleId);
                    }
                    catch (Exception ex)
                    {
                        Common.LogInfo(_manager, _dataModelManager, "UpdateInstanceDCRXML - Failed after UpdateInstanceDCRXML - eventsXML : " + eventsXml + " - " + Common.ToJson(input));
                        Common.LogError(ex);
                        return InternalServerError(ex);
                    }
                }
                return Ok(Common.ToJson(new object { }));
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "UpdateInstanceDCRXML - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get Major Revisions of Processes
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetProcessMajorRevisions")]
        public IHttpActionResult GetProcessMajorRevisions(dynamic input)
        {
            try
            {
                var graphId = string.Empty;
                if (input != null)
                {
                    graphId = input["graphId"] == null ? "" : input["graphId"].ToString();
                }
                var processes = _commons.GetProcessMajorRevisions(graphId);
                return Ok(processes);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "GetProcessMajorRevisions - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Add Instance Process
        /// </summary>
        /// <returns></returns>        
        [HttpPost]
        [Route("AddProcessInstance")]
        public IHttpActionResult AddProcessInstance(dynamic input)
        {
            try
            {
                string graphId = input["graphId"].ToString();
                var graphTitle = string.Empty;

                // check if graph exists
                var graphXml = string.Empty;
                graphXml = _dcrService.GetProcess(Configurations.Config.ProcessGovernanceGraphId);

                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, Configurations.Config.ProcessGovernanceGraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.Id.ToString(), DBEntityNames.Process.GraphId.ToString(), DBEntityNames.Process.Status.ToString() });

                var processes = _manager.SelectData(_dataModelManager.DataModel);
                if (processes.Rows.Count < 1)
                {
                    var graph = new XmlDocument();
                    graph.LoadXml(graphXml);
                    if (graph.GetElementsByTagName("dcrgraph").Count > 0)
                    {
                        graphTitle = graph.GetElementsByTagName("dcrgraph")[0].Attributes["title"].Value;
                    }

                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Process.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.Process.Title.ToString(), Enums.ParameterType._string, graphTitle);
                    _dataModelManager.AddParameter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, Configurations.Config.ProcessGovernanceGraphId);
                    _dataModelManager.AddParameter(DBEntityNames.Process.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);
                    _dataModelManager.AddParameter(DBEntityNames.Process.Owner.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());

                    try
                    {
                        var processId = _manager.InsertData(_dataModelManager.DataModel);

                        var phases = _dcrService.GetPhases(graphXml);

                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddProcessPhases.ToString());
                        _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.ProcessId.ToString(), Enums.ParameterType._int, (processId.Rows[0]["Id"]).ToString());
                        _dataModelManager.AddParameter(DBEntityNames.AddProcessPhases.PhaseXml.ToString(), Enums.ParameterType._xml, phases);
                        _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);
                    }
                    catch
                    {
                    }
                }

                // check in process history
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.ProcessHistory.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, Configurations.Config.ProcessGovernanceGraphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.ProcessHistory.Id.ToString(), DBEntityNames.ProcessHistory.GraphId.ToString(), DBEntityNames.ProcessHistory.Status.ToString() });

                processes = _manager.SelectData(_dataModelManager.DataModel);
                if (processes.Rows.Count < 1)
                {
                    var graph = new XmlDocument();
                    graph.LoadXml(graphXml);
                    if (graph.GetElementsByTagName("dcrgraph").Count > 0)
                    {
                        graphTitle = graph.GetElementsByTagName("dcrgraph")[0].Attributes["title"].Value;
                    }

                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.ProcessHistory.ToString());
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.Title.ToString(), Enums.ParameterType._string, graphTitle);
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.GraphId.ToString(), Enums.ParameterType._int, Configurations.Config.ProcessGovernanceGraphId);
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.DCRXML.ToString(), Enums.ParameterType._xml, graphXml);
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.Owner.ToString(), Enums.ParameterType._int, Common.GetResponsibleId());
                    _dataModelManager.AddParameter(DBEntityNames.ProcessHistory.State.ToString(), Enums.ParameterType._int, "1");

                    try
                    {
                        _manager.InsertData(_dataModelManager.DataModel);
                    }
                    catch
                    {
                    }
                }

                // get name of graph
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, graphId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                _dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.Title.ToString(), DBEntityNames.Process.Id.ToString(), DBEntityNames.Process.GraphId.ToString(), DBEntityNames.Process.Status.ToString() });
                var process = _manager.SelectData(_dataModelManager.DataModel);
                var processName = string.Empty;

                if (process.Rows.Count > 0)
                {
                    processName = process.Rows[0][DBEntityNames.Process.Title.ToString()].ToString();
                }
                else
                {
                    var processGraphXML = _dcrService.GetProcess(graphId);
                    var processXML = new XmlDocument();
                    processXML.LoadXml(processGraphXML);
                    if (processXML.GetElementsByTagName("dcrgraph").Count > 0)
                    {
                        processName = processXML.GetElementsByTagName("dcrgraph")[0].Attributes["title"].Value;
                    }
                }

                var instanceModel = new AddInstanceModel()
                {
                    GraphId = int.Parse(Configurations.Config.ProcessGovernanceGraphId),
                    Responsible = int.Parse(Common.GetResponsibleId()),
                    Title = "Release of " + processName,
                    UserRoles = new List<UserRole>() { }
                };

                var instanceId = Common.AddInstance(instanceModel, _manager, _dataModelManager);
                _commons.AddMajorVersionIdToInstance(graphId, instanceId);
                var model = Common.InitializeGraph(UseProcessEngine, Configurations.Config.ProcessGovernanceGraphId, instanceId, _dcrService, _manager, _dataModelManager);
                Common.UpdateInstance(instanceId, model.SimulationId, model.InstanceXML, _manager, _dataModelManager);
                Common.SyncEvents(instanceId, model.EventsXML, instanceModel.Responsible.ToString(), _manager, _dataModelManager);
                Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);
                AutomaticEvents(instanceId, graphId, model.SimulationId, instanceModel.Responsible.ToString());

                return Ok(instanceId);
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "AddProcessInstance - Failed. - " + Common.ToJson(input));
                Common.LogError(ex);
                return InternalServerError(ex);
            }
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
                    string eventsXml;
                    // execute event
                    if (!UseProcessEngine)
                    {
                        _dcrService.ExecuteEventWithTime(graphId, simulationId, dataRow["EventId"].ToString(), DateTime.UtcNow.ToString("o"));
                        // get pending or enabled from active repository
                        eventsXml = _dcrService.GetPendingOrEnabled(graphId, simulationId);
                    }
                    else
                    {
                        // get instance xml
                        var instanceXML = Common.GetInstanceXML(instanceId, _manager, _dataModelManager);
                        dynamic dcrGraph = new DCRGraph(instanceXML);
                        dcrGraph = _dcrService.ExecuteEventWithTime(dcrGraph, dataRow["EventId"].ToString(), DateTime.UtcNow);
                        string instanceXml = ((DCRGraph)dcrGraph).ToXml();
                        Common.UpdateInstance(instanceId, simulationId, instanceXml, _manager, _dataModelManager);
                        // get pending or enabled from process engine
                        eventsXml = _dcrService.GetPendingOrEnabled(dcrGraph);
                    }

                    Common.SyncEvents(instanceId, eventsXml, responsible, _manager, _dataModelManager);
                    Common.UpdateEventTypeData(instanceId, _manager, _dataModelManager);

                    if (Configurations.Config.AlwaysLogExecutions)
                    {
                        if (Configurations.Config.LogToAcadre && dataRow["EventType"].ToString() != "CreateJournalAcadre")
                            Common.CreateMemoAcadre(instanceId, dataRow["EventId"].ToString(), false, _manager, _dataModelManager, _service);
                    }

                    #region Alec Code
                    // Alec Code will come up here
                    switch (dataRow["EventType"].ToString())
                    {
                        case "CreateCaseAcadre":
                            try
                            {
                                Common.LogInfo(_manager, _dataModelManager, "CreateCaseAcadre - Creating Case In Acadre for graphId : " + graphId + ", InstanceId : " + instanceId + " , eventId : " + dataRow["EventId"].ToString());
                                // get parametes for acadre
                                var createCaseAcadreParameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());

                                var isCaseCreated = CheckIfCaseExists(instanceId);

                                // create casse in acadre
                                if (createCaseAcadreParameters.Count > 0 && !isCaseCreated)
                                {
                                    string caseId = Common.CreateCase(instanceId, createCaseAcadreParameters, _manager, _dataModelManager);
                                    Common.LogInfo(_manager, _dataModelManager, "GetCaseURL(" + caseId + " )");
                                    string caseLink = Common.GetCaseLink(caseId);
                                    Common.LogInfo(_manager, _dataModelManager, "GetCaseNumber(" + caseId + " )");
                                    string CaseIdForeign = Common.GetCaseIdForeign(caseId);
                                    Common.LogInfo(_manager, _dataModelManager, "CreateCaseAcadre - Case Created for Instance Id : " + instanceId + ",caseId : " + caseId + ",caselink : " + caseLink + ",Caseforeign : " + CaseIdForeign);

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
                                else if (isCaseCreated)
                                {
                                    Common.LogInfo(_manager, _dataModelManager, "CreateCaseAcadre - Case already exists for InstanceId : " + instanceId + ", graphId:" + graphId);
                                }
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "CreateCaseAcadre - Failed for InstanceId : " + instanceId + " , graphId:" + graphId);
                                Common.LogError(ex);
                                throw ex;
                            }
                            break;
                        case "CloseCaseAcadre":
                            try
                            {
                                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Instance");
                                _dataModelManager.AddResultSet(new List<string> { "InternalCaseID" });
                                _dataModelManager.AddFilter("Id", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                                var data = _manager.SelectData(_dataModelManager.DataModel);
                                if (data.Rows.Count > 0 && data.Rows[0]["InternalCaseID"].ToString() != "")
                                {
                                    Common.LogInfo(_manager, _dataModelManager, "CloseCaseAcadre - Closing case in Acadre ,CaseId: " + data.Rows[0]["InternalCaseID"].ToString() + ",instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    Common.LogInfo(_manager, _dataModelManager, "CloseCase(" + data.Rows[0]["InternalCaseID"].ToString() + ")");
                                    Common.CloseCase(data.Rows[0]["InternalCaseID"].ToString());
                                    Common.LogInfo(_manager, _dataModelManager, "CloseCaseAcadre - Case Closed in Acadre ,instanceId : " + instanceId + ",graphId:" + graphId + ",eventId:" + dataRow["EventId"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "CloseCaseAcadre - Failed for InstanceId : " + instanceId + ", graphId:" + graphId);
                                Common.LogError(ex);
                                throw ex;
                            }
                            break;
                        case "UploadDocumentAcadre":
                            try
                            {
                                Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - Upload Document in Acadre , instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                // get parametes for acadre
                                var documentTitle = "";
                                var pFileName = "";
                                var uploadDocumentAcadreParameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());
                                if (uploadDocumentAcadreParameters.ContainsKey("AssociatedEventId".ToLower()))
                                {
                                    documentTitle = uploadDocumentAcadreParameters["AssociatedEventId".ToLower()];
                                    if (string.IsNullOrEmpty(documentTitle))
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - AssociatedEventId is not defined, Please define it eventTypeParameters, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    }
                                }
                                if (uploadDocumentAcadreParameters.ContainsKey("DocumentTitleText".ToLower()))
                                {
                                    pFileName = uploadDocumentAcadreParameters["DocumentTitleText".ToLower()];
                                    pFileName = pFileName.Trim();
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
                                    var eventId = document.Rows[0][DBEntityNames.Document.Title.ToString()].ToString().Trim();
                                    var filePath = document.Rows[0][DBEntityNames.Document.Link.ToString()].ToString();
                                    var fileName = string.Empty;
                                    if (!pFileName.ToLower().Contains("$(filename)".ToLower()))
                                    {
                                        if (string.IsNullOrWhiteSpace(pFileName))
                                        {
                                            pFileName = Path.GetFileName(filePath);
                                        }
                                        else
                                        {
                                            pFileName = pFileName + Path.GetExtension(filePath);
                                        }
                                        fileName = pFileName;
                                    }
                                    else
                                    {
                                        if (pFileName.Contains("$(FileName)"))
                                            fileName = pFileName.Replace("$(FileName)", Path.GetFileName(filePath));
                                        else if (pFileName.Contains("$(Filename)"))
                                            fileName = pFileName.Replace("$(Filename)", Path.GetFileName(filePath));
                                        else if (pFileName.Contains("$(filename)"))
                                            fileName = pFileName.Replace("$(filename)", Path.GetFileName(filePath));
                                    }
                                    if (string.IsNullOrWhiteSpace(fileName))
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - DocumentTitleText is not defined, so eventId is used as filename, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                        fileName = Path.GetFileName(filePath);
                                    }

                                    if (File.Exists(filePath))
                                    {
                                        try
                                        {
                                            byte[] fileBytes = File.ReadAllBytes(filePath);
                                            Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - Calling Acadre , instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                            string documentId = Common.CreateDocument(uploadDocumentAcadreParameters, fileName, fileBytes, instanceId, _manager, _dataModelManager);
                                            Common.LogInfo(_manager, _dataModelManager, "Document Created in Acadre, DocumentId : " + documentId);
                                        }
                                        catch (Exception ex)
                                        {
                                            Common.LogError(ex);
                                        }
                                    }
                                    else
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - Temp file not found on FileSystem, filepath : " + filePath + " instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    }
                                }
                                else
                                {
                                    Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - No related temp document found in database to upload, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "UploadDocumentAcadre - Failed for InstanceId : " + instanceId + ", graphId:" + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                Common.LogError(ex);
                                throw ex;
                            }
                            break;
                        case "UploadDocumentOCM":
                            try
                            {
                                Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - Upload Document in OCM , instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                // get parametes for acadre
                                var uploadedDocTitle = "";
                                var uploadToPersonal = false;
                                var uploadToInstance = false;

                                var uploadDocumentOCMParameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());
                                if (uploadDocumentOCMParameters.ContainsKey("AssociatedEventId".ToLower()))
                                {
                                    uploadedDocTitle = uploadDocumentOCMParameters["AssociatedEventId".ToLower()];
                                    if (string.IsNullOrEmpty(uploadedDocTitle))
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - AssociatedEventId is not defined, Please define it eventTypeParameters, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    }
                                }
                                if (uploadDocumentOCMParameters.ContainsKey("UploadToPersonal".ToLower()))
                                {
                                    if (uploadDocumentOCMParameters["UploadToPersonal".ToLower()] == "1")
                                    {
                                        uploadToPersonal = true;
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - Will be uploaded to PersonalDocuments, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    }
                                }
                                if (uploadDocumentOCMParameters.ContainsKey("UploadToInstance".ToLower()))
                                {
                                    if (uploadDocumentOCMParameters["UploadToInstance".ToLower()] == "1")
                                    {
                                        uploadToInstance = true;
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - Will be uploaded to InstanceDocuments, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    }
                                }
                                // get document
                                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
                                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Document.Id.ToString(), DBEntityNames.Document.Title.ToString(), DBEntityNames.Document.Link.ToString() });
                                if (string.IsNullOrEmpty(uploadedDocTitle))
                                {
                                    _dataModelManager.AddFilter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, uploadedDocTitle, Enums.CompareOperator.like, Enums.LogicalOperator.and);
                                }
                                else
                                {
                                    _dataModelManager.AddFilter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, uploadedDocTitle, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                                }
                                _dataModelManager.AddFilter(DBEntityNames.Document.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                                _dataModelManager.AddFilter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, bool.TrueString, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                                _dataModelManager.AddFilter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, "Temp", Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                                var uploadedDoc = _manager.SelectData(_dataModelManager.DataModel);

                                if (uploadedDoc.Rows.Count > 0)
                                {
                                    var fileName = uploadedDoc.Rows[0][DBEntityNames.Document.Title.ToString()].ToString();
                                    var filePath = uploadedDoc.Rows[0][DBEntityNames.Document.Link.ToString()].ToString();

                                    if (File.Exists(filePath))
                                    {
                                        try
                                        {
                                            byte[] fileBytes = File.ReadAllBytes(filePath);
                                            if (uploadToPersonal)
                                            {
                                                var newFilePath = _documentManager.AddDocument(instanceId, "PersonalDocument", fileName, fileName + Path.GetExtension(filePath), string.Empty, _manager, _dataModelManager);
                                                File.WriteAllBytes(newFilePath, fileBytes);
                                            }
                                            if (uploadToInstance)
                                            {
                                                var newFilePath = _documentManager.AddDocument(instanceId, "InstanceDocument", fileName, fileName + Path.GetExtension(filePath), string.Empty, _manager, _dataModelManager);
                                                File.WriteAllBytes(newFilePath, fileBytes);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Common.LogError(ex);
                                        }
                                    }
                                    else
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - Temp file not found on FileSystem, filepath : " + filePath + " instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                    }
                                }
                                else
                                {
                                    Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - No related temp document found in database to upload, instanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "UploadDocumentOCM - Failed for InstanceId : " + instanceId + ", graphId:" + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                Common.LogError(ex);
                            }
                            break;
                        case "DCRForm2PDF":
                            try
                            {
                                Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Converting to Pdf for graphId : " + graphId + ", InstanceId : " + instanceId + " , eventId : " + dataRow["EventId"].ToString());
                                var dcrFormId = string.Empty;
                                var dCRForm2PDFParameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());
                                if (dCRForm2PDFParameters.ContainsKey("DCRFormId".ToLower()))
                                {
                                    dcrFormId = dCRForm2PDFParameters["DCRFormId".ToLower()];
                                }

                                if (!string.IsNullOrWhiteSpace(dcrFormId))
                                {
                                    try
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Converting to PDF Form, dcrFormId : " + dcrFormId + ",InstanceId : " + instanceId + ",graphId : " + graphId + ",eventId : " + dataRow["EventId"].ToString());
                                        var formPdfPath = _documentManager.AddDocument(instanceId, "Temp", dcrFormId, dcrFormId + ".pdf", dcrFormId, _manager, _dataModelManager);
                                        Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Pdf will be saved at Path :  " + formPdfPath);
                                        Common.ConvertDCRFormToPdf(dcrFormId, instanceId, UseProcessEngine, formPdfPath, _manager, _dataModelManager, _dcrService);
                                        Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Pdf saved at Path :  " + formPdfPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Failed for InstanceId : " + instanceId + " , graphId:" + graphId);
                                        Common.LogError(ex);
                                        throw ex;
                                    }
                                }
                                else
                                {
                                    Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Error : DCRFormId is empty in EventTypeParameters for graphId : " + graphId + ", InstanceId : " + instanceId + " , eventId : " + dataRow["EventId"].ToString());
                                    var ex = new Exception("DCRForm2PDF - DCRFormId is empty in EventTypeParameters");
                                    Common.LogError(ex);
                                }
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "DCRForm2PDF - Failed for InstanceId : " + instanceId + " , graphId:" + graphId);
                                Common.LogError(ex);
                                throw ex;
                            }
                            break;
                        case "CreateJournalAcadre":
                            try
                            {
                                Common.LogInfo(_manager, _dataModelManager, "CreateJournalAcadre - Creating Journal in Acadre for graphId : " + graphId + ", InstanceId : " + instanceId + " , eventId : " + dataRow["EventId"].ToString());
                                Common.CreateMemoAcadre(instanceId, dataRow["EventId"].ToString(), true, _manager, _dataModelManager, _service);
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "CreateJournalAcadre - Failed for InstanceId : " + instanceId + " , graphId:" + graphId);
                                Common.LogError(ex);
                                throw ex;
                            }
                            break;
                        case "OCMSpawnChildProcess":
                            try
                            {
                                var instanceDataTable = Common.GetInstanceDetails(_manager, _dataModelManager, instanceId);
                                var UserRoles = new List<UserRole>();
                                var pGraphId = "";
                                var pTitle = "";
                                var onlycreateifnotalreadythere = "";
                                var parameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());

                                if (instanceDataTable.Rows.Count > 0)
                                {
                                    if (parameters.ContainsKey("GraphId".ToLower()))
                                    {
                                        pGraphId = parameters["GraphId".ToLower()];
                                    }
                                    if (parameters.ContainsKey("Title".ToLower()))
                                    {
                                        pTitle = parameters["Title".ToLower()];
                                    }
                                    try
                                    {
                                        if (parameters.ContainsKey("onlycreateifnotalreadythere".ToLower()))
                                        {
                                            onlycreateifnotalreadythere = parameters["onlycreateifnotalreadythere".ToLower()];
                                            onlycreateifnotalreadythere = onlycreateifnotalreadythere.ToLower();
                                        }
                                        else
                                        {
                                            onlycreateifnotalreadythere = "1";
                                        }
                                    }
                                    catch
                                    {
                                        onlycreateifnotalreadythere = "0";
                                        Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - onlycreateifnotalreadythere set to 0 because of not set, instanceId : " + instanceId + " , graphId : " + graphId);
                                    }

                                    if (!string.IsNullOrWhiteSpace(pGraphId))
                                    {
                                        var createChildInstance = true;
                                        _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                                        _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Process.Id.ToString() });
                                        _dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, pGraphId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                                        var processes = _manager.SelectData(_dataModelManager.DataModel);

                                        if (processes.Rows.Count > 0)
                                        {
                                            if ((onlycreateifnotalreadythere == "1" || onlycreateifnotalreadythere == "true"))
                                            {
                                                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.InstanceExtension.ToString());
                                                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.InstanceExtension.ChildId.ToString() });
                                                _dataModelManager.AddFilter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                                                var child = _manager.SelectData(_dataModelManager.DataModel);

                                                if (child.Rows.Count > 0)
                                                {
                                                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.ContextInstances.ToString());
                                                    _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.ContextInstances.InstanceId.ToString() });
                                                    _dataModelManager.AddFilter(DBEntityNames.ContextInstances.GraphId.ToString(), Enums.ParameterType._int, pGraphId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                                                    _dataModelManager.AddFilter(DBEntityNames.ContextInstances.ChildId.ToString(), Enums.ParameterType._int, child.Rows[0]["ChildId"].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                                                    var contextInstances = _manager.SelectData(_dataModelManager.DataModel);

                                                    if (contextInstances.Rows.Count > 0)
                                                    {
                                                        createChildInstance = false;
                                                        Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - Instance of graph : " + pGraphId + " already exists with context Id : " + child.Rows[0]["ChildId"].ToString());
                                                    }
                                                }
                                            }

                                            if (createChildInstance)
                                            {
                                                var addInstanceModel = new AddInstanceModel()
                                                {
                                                    Title = pTitle,
                                                    GraphId = int.Parse(pGraphId),
                                                    UserRoles = UserRoles
                                                };
                                                var childInstanceId = Common.AddInstance(addInstanceModel, _manager, _dataModelManager);

                                                if (!string.IsNullOrWhiteSpace(childInstanceId))
                                                {
                                                    Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - Instance Created,Title: " + pTitle + ",GraphId: " + pGraphId + ",InstanceId : " + childInstanceId + ",onlycreateifnotalreadythere:" + onlycreateifnotalreadythere);

                                                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.OCMSpawnChildProcessCopyContext.ToString());
                                                    _dataModelManager.AddParameter(DBEntityNames.OCMSpawnChildProcessCopyContext.ParentInstanceId.ToString(), Enums.ParameterType._int, instanceId);
                                                    _dataModelManager.AddParameter(DBEntityNames.OCMSpawnChildProcessCopyContext.InstanceId.ToString(), Enums.ParameterType._int, childInstanceId);

                                                    _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                                                    Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - Context copied to new instance,sp called");

                                                    var responsibleId = Common.GetResponsibleId();
                                                    InitializeGraphModel model = Common.InitializeGraph(UseProcessEngine, pGraphId, childInstanceId, _dcrService, _manager, _dataModelManager);

                                                    if (!string.IsNullOrEmpty(model.SimulationId))
                                                    {
                                                        Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - Instance Id : " + childInstanceId + " initialized , simId : " + simulationId + "");

                                                        Common.UpdateInstance(childInstanceId, model.SimulationId, model.InstanceXML, _manager, _dataModelManager);
                                                        Common.SyncEvents(childInstanceId, model.EventsXML, responsibleId, _manager, _dataModelManager);
                                                        Common.UpdateEventTypeData(childInstanceId, _manager, _dataModelManager);
                                                        AutomaticEvents(childInstanceId, pGraphId, model.SimulationId, responsibleId);
                                                        Common.LogInfo(_manager, _dataModelManager, "SyncEvents called for new instance Instanc Id : " + childInstanceId + " - OCMSpawnChildProcess");
                                                    };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - No Process found with GraphId: " + pGraphId + "");
                                        }
                                    }
                                    else if (pGraphId == string.Empty)
                                    {
                                        Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - No graph Id mention in EventTypeParameters");
                                    }
                                }
                                else
                                {
                                    Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - Instance details not found, Instance Id : " + instanceId);
                                }
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "OCMSpawnChildProcess - Failed for InstanceId : " + instanceId + " , graphId:" + graphId);
                                Common.LogError(ex);
                                throw ex;
                            }
                            break;
                        case "SendEmail":
                            try
                            {
                                var sendEmailParameters = Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());

                                string to = sendEmailParameters["to"].ToString();
                                var subject = sendEmailParameters["subject"].ToString();
                                var body = sendEmailParameters["body"].ToString();

                                if (to.StartsWith("$(") && !to.Contains("@"))
                                {
                                    to = GetValueFromForm(to, instanceId);
                                }
                                if (subject.StartsWith("$(") || subject.Contains("$("))
                                {
                                    subject = GetValueFromForm(subject, instanceId);
                                }
                                if (body.StartsWith("$(") || body.Contains("$("))
                                {
                                    body = GetValueFromForm(body, instanceId);
                                }

                                _automaticEvents.SendEmail(to, subject, body);
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "SendEmail - Failed for InstanceId : " + instanceId + " , graphId:" + graphId + ", eventId : " + dataRow["EventId"].ToString());
                                Common.LogError(ex);
                            }
                            break;
                        case "OCMReleaseGraph":
                            try
                            {
                                //Common.GetParametersFromEventTypeData(dataRow["EventTypeData"].ToString(), instanceId, _manager, _dataModelManager, dataRow["EventId"].ToString());
                                _automaticEvents.ReleaseProcess(instanceId);
                            }
                            catch (Exception ex)
                            {
                                Common.LogInfo(_manager, _dataModelManager, "OCMReleaseGraph - Failed for InstanceId : " + instanceId + " , graphId:" + graphId + ", eventId : " + dataRow["EventId"].ToString());
                                Common.LogError(ex);
                            }
                            break;
                    }
                    #endregion

                }
            }
        }

        private string GetValueFromForm(string value, string instanceId)
        {
            if (value.ToLower().Contains("$(".ToLower()))
            {
                var occurences = value.Occurences("$(");
                while (value.ToLower().Contains("$(".ToLower()) && occurences > 0)
                {
                    var startIndexColumnKey = value.IndexOf("$(");
                    var endIndexColumnKey = -1;
                    foreach (var key in value)
                    {
                        endIndexColumnKey++;
                        if (key == ')')
                        {
                            break;
                        }
                    }
                    var keyToReplace = value.Substring(startIndexColumnKey, (endIndexColumnKey + 1) - startIndexColumnKey);
                    var formName = keyToReplace.Substring(2, keyToReplace.IndexOf(".") - 2);

                    var startIndexColumnName = keyToReplace.IndexOf(".");
                    startIndexColumnName++;
                    var endIndexColumnName = -1;
                    foreach (var key in keyToReplace)
                    {
                        endIndexColumnName++;
                        if (key == ')')
                        {
                            break;
                        }
                    }
                    var columnName = keyToReplace.Substring(startIndexColumnName, endIndexColumnName - startIndexColumnName);


                    _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, "GetValueFromForm");
                    _dataModelManager.AddParameter("InstanceId", Enums.ParameterType._int, instanceId);
                    _dataModelManager.AddParameter("FormName", Enums.ParameterType._string, formName);
                    _dataModelManager.AddParameter("EventName", Enums.ParameterType._string, columnName);

                    var dataTable = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

                    value = value.Replace(keyToReplace, dataTable.Rows[0][0].ToString());
                    occurences--;
                }
            }
            return value;
        }

        /// <summary>
        /// If case exisits in acadre , dont create again
        /// </summary>
        /// <param name="instanceId"></param>
        private bool CheckIfCaseExists(string instanceId)
        {
            try
            {
                _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.InternalCaseID.ToString(), DBEntityNames.Instance.CaseNoForeign.ToString() });
                _dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var caseId = _manager.SelectData(_dataModelManager.DataModel);
                if (caseId.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(caseId.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()].ToString()))
                    {
                        Common.LogInfo(_manager, _dataModelManager, "Case is already created in acadre.");
                        return true;
                    }
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "CheckIfCaseExists - Failed. - " + Common.ToJson(instanceId));
                Common.LogError(ex);
                throw ex;
            }
        }
        #endregion
    }
}
