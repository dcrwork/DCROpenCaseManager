using DCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Managers
{
    public class ProcessEngineService : IProcessEngineService
    {
        /// <summary>
        /// Initialize Graph
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <returns></returns>
        public DCRGraph InitializeProcess(DCRGraph dCRGraph)
        {
            dCRGraph.Intialize();
            return dCRGraph;
        }

        /// <summary>
        /// Get pending or enabled events
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <returns></returns>
        public string GetPendingOrEnabled(DCRGraph dCRGraph)
        {
            return dCRGraph.GetEnabledOrPending(EventType.enabledorpending);
        }

        /// <summary>
        /// Execute event
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DCRGraph ExecuteEvent(DCRGraph dCRGraph, string eventId)
        {
            dCRGraph.ExecuteEvent(eventId);
            return dCRGraph;
        }

        /// <summary>
        /// Execute event with time
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DCRGraph ExecuteEventWithTime(DCRGraph dCRGraph, string eventId, DateTime time)
        {
            dCRGraph.ExecuteEventWithTime(time, eventId);
            return dCRGraph;
        }

        /// <summary>
        /// Execute event with time and role
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <param name="eventId"></param>
        /// <returns></returns>
        public DCRGraph ExecuteEventWithTimeAndRole(DCRGraph dCRGraph, string eventId, DateTime time, string role)
        {
            dCRGraph.ExecuteEventWithTime(time, eventId);
            return dCRGraph;
        }

        /// <summary>
        /// Is Accepting
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <returns></returns>
        public DCRGraph IsAccepting(DCRGraph dCRGraph)
        {
            dCRGraph.IsAccepting();
            return dCRGraph;
        }

        /// <summary>
        /// Get Phases
        /// </summary>
        /// <param name="dcrXml"></param>
        /// <returns></returns>
        public string GetPhases(string dcrXml)
        {
            var dcrGraph = new DCRGraph(dcrXml);
            dcrGraph = InitializeProcess(dcrGraph);
            var phases = dcrGraph.GetPhases();
            return phases;
        }

        /// <summary>
        /// Get refer form xml from dcr xml
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="dCRGraph"></param>
        /// <returns></returns>
        public string GetReferXmlByEventId(string eventId, string dcrXml)
        {
            var dcrGraph = new DCRGraph(dcrXml);
            return dcrGraph.GetReferXMLbyID(eventId);
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
            var dcrGraph = new DCRGraph(mainXml);
            return dcrGraph.ExecuteRefferedXMLIntoMainXML(referXml, eventId);
        }

        /// <summary>
        /// Add Event to current instance
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <returns></returns>
        public string AddEvent(string eventId, string label, string roles, string description, string xml)
        {
            EventsParam param = new EventsParam
            {
                ID = eventId,
                Roles = roles,
                EventLabel = label,
                EventDescription = description,
                Included = "true"
            };

            DCRGraph graph = new DCRGraph(xml);
            var newXml = graph.AddEvent(param);
            return graph.ToXml();
        }

        /// <summary>
        /// Remove Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public string RemoveEvent(string eventId, string xml)
        {
            DCRGraph graph = new DCRGraph(xml);
            var newXml = graph.RemoveEvent(eventId);
            return graph.ToXml();
        }

        /// <summary>
        /// Not applicable event
        /// </summary>
        /// <param name="dCRGraph"></param>
        /// <param name="eventId"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public DCRGraph NotApplicable(DCRGraph dCRGraph, string eventId, string note)
        {
            dCRGraph.AddNote(eventId, note);
            return dCRGraph;
        }
    }
}