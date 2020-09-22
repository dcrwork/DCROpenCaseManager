#define ProcessEngine
//#define ActiveRepository


using System;

namespace OpenCaseManager.Managers
{
    public class DCRService : IDCRService
    {
        private IActiveRepositoryService _activeRepositoryService;
        private bool UseProcessEngine = false;
        private IProcessEngineService _processEngineService;
        private IManager _manager;

        public DCRService(IActiveRepositoryService activeRepositoryService, IProcessEngineService processEngineService, IManager manager)
        {
            _activeRepositoryService = activeRepositoryService;
            UseProcessEngine = Configurations.Config.UseProcessEngine;
            _processEngineService = processEngineService;
            _manager = manager;
        }

        #region Process Engine

        /// <summary>
        /// Initialize a graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public DCRGraph InitializeGraph(DCRGraph graphXml)
        {
            var dCRGraph = _processEngineService.InitializeProcess(graphXml);
            return dCRGraph;
        }

        /// <summary>
        /// Get Pending or Enabled Events
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <returns></returns>
        public string GetPendingOrEnabled(DCRGraph dCRGraph)
        {
            var content = string.Empty;
            content = _processEngineService.GetPendingOrEnabled(dCRGraph);
            return content;
        }

        /// <summary>
        /// Execute an event
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DCRGraph ExecuteEvent(DCRGraph dCRGraph, string eventId)
        {
            var content = string.Empty;

            var dcrGraph = _processEngineService.ExecuteEvent(dCRGraph, eventId);
            return dcrGraph;
        }

        /// <summary>
        /// Execute an event with set time
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DCRGraph ExecuteEventWithTime(DCRGraph dCRGraph, string eventId, DateTime time)
        {
            var content = string.Empty;
            var dcrGraph = _processEngineService.ExecuteEventWithTime(dCRGraph, eventId, time);
            return dcrGraph;
        }

        /// <summary>
        /// Execute an event with set time and role
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DCRGraph ExecuteEventWithTimeAndRole(DCRGraph dCRGraph, string eventId, DateTime time, string role)
        {
            var content = string.Empty;
            var dcrGraph = _processEngineService.ExecuteEventWithTimeAndRole(dCRGraph, eventId, time, role);
            return dcrGraph;
        }

        /// <summary>
        /// Get Phases of a graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="dcrXml"></param>
        /// <returns></returns>
        public string GetPhases(string dcrXml)
        {
            var phases = string.Empty;
            return _processEngineService.GetPhases(dcrXml);
        }

        /// <summary>
        /// Advance time to current time/next deadline
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public string AdvanceTime(string graphId, string simulationId, string time)
        {
            return _activeRepositoryService.AdvanceTime(graphId, simulationId, time);
        }

        /// <summary>
        /// Get refer form xml from dcr xml
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="dcrXml"></param>
        /// <returns></returns>
        public string GetReferXmlByEventId(string eventId, string dcrXml)
        {
            return _processEngineService.GetReferXmlByEventId(eventId, dcrXml);
        }

        /// <summary>
        /// Merge refer xml with main xml
        /// </summary>
        /// <param name="mainXml"></param>
        /// <param name="referXml"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string MergeReferXmlWithMainXml(string mainXml, string referXml, string eventId)
        {
            return _processEngineService.MergeReferXmlWithMainXml(mainXml, referXml, eventId);
        }

        /// <summary>
        /// Add Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="label"></param>
        /// <param name="roles"></param>
        /// <param name="description"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public string AddEvent(string eventId, string label, string roles, string description, string xml)
        {
            return _processEngineService.AddEvent(eventId, label, roles, description, xml);
        }

        /// <summary>
        /// Not applicable event
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <param name="eventId"></param>
        /// <param name="Note"></param>
        /// <returns></returns>
        public DCRGraph NotApplicable(DCRGraph dCRGraph, string eventId, string note)
        {
            var content = string.Empty;
            var dcrGraph = _processEngineService.NotApplicable(dCRGraph, eventId, note);
            return dcrGraph;
        }

        /// <summary>
        /// Remove Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public string RemoveEvent(string eventId, string xml)
        {
            return _processEngineService.RemoveEvent(eventId, xml);
        }
        #endregion

        #region Active Repository

        /// <summary>
        /// Initialize a graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string InitializeGraph(string graphId)
        {
            var simulationId = string.Empty;
            simulationId = _activeRepositoryService.InitializeGraph(graphId);
            return simulationId;
        }

        /// <summary>
        /// Get Pending or Enabled Events
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <returns></returns>
        public string GetPendingOrEnabled(string graphId, string simulationId)
        {
            var content = string.Empty;
            content = _activeRepositoryService.GetPendingOrEnabled(graphId, simulationId);
            return content;
        }

        /// <summary>
        /// Execute an event
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string ExecuteEvent(string graphId, string simulationId, string eventId)
        {
            var content = string.Empty;
            content = _activeRepositoryService.ExecuteEvent(graphId, simulationId, eventId);
            return content;
        }

        /// <summary>
        /// Execute an event with time
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public string ExecuteEventWithTime(string graphId, string simulationId, string eventId, string time)
        {
            var content = string.Empty;
            content = _activeRepositoryService.ExecuteEventWithTime(graphId, simulationId, eventId, time);
            return content;
        }

        /// <summary>
        /// Get Process Roles
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetProcessRoles(string graphId)
        {
            return _activeRepositoryService.GetProcessRoles(graphId);
        }

        /// <summary>
        /// Search Processes
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public string SearchProcess(string title)
        {
            return _activeRepositoryService.SearchProcess(title);
        }

        /// <summary>
        /// Get Process from active repository using graph id
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetProcess(string graphId)
        {
            return _activeRepositoryService.GetProcess(graphId);
        }

        /// <summary>
        /// Get refer xml from graph xml
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="simulationId"></param>
        /// <returns></returns>
        public string GetReferXmlByEventId(string graphId, string simulationId, dynamic eventId)
        {
            return _activeRepositoryService.GetReferXmlByEventId(graphId, simulationId, eventId);
        }

        /// <summary>
        /// Merge executed xml with graph
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="simulationId"></param>
        /// <param name="referXml"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public void MergeReferXmlWithMainXml(string graphId, string simulationId, dynamic referXml, dynamic eventId)
        {
            _activeRepositoryService.MergeReferXmlWithMainXml(graphId, simulationId, referXml, eventId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public string GetMajorRevisions(string graphId)
        {
            return _activeRepositoryService.GetMajorRevisions(graphId);
        }

        #endregion
    }
}