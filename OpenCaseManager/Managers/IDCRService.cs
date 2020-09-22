using System;

namespace OpenCaseManager.Managers
{
    public interface IDCRService
    {

        #region Active Repository
        string InitializeGraph(string graphId);

        string GetPendingOrEnabled(string graphId, string simulationId);

        string ExecuteEvent(string graphId, string simulationId, string eventId);

        string ExecuteEventWithTime(string graphId, string simulationId, string eventId, string time);

        string GetProcessRoles(string graphId);

        string SearchProcess(string title);

        string GetProcess(string graphId);

        string AdvanceTime(string graphId, string simulationId, string time);

        string GetReferXmlByEventId(string graphId, string simulationId, dynamic eventId);

        void MergeReferXmlWithMainXml(string graphId, string simulationId, dynamic referXml, dynamic eventId);

        string GetMajorRevisions(string graphId);
        #endregion

        #region Process Engine
        DCRGraph InitializeGraph(DCRGraph dCRGraph);

        string GetPendingOrEnabled(DCRGraph dCRGraph);

        DCRGraph ExecuteEvent(DCRGraph dCRGraph, string eventId);

        DCRGraph NotApplicable(DCRGraph dCRGraph, string eventId, string note);

        DCRGraph ExecuteEventWithTime(DCRGraph dCRGraph, string eventId, DateTime time);

        DCRGraph ExecuteEventWithTimeAndRole(DCRGraph dCRGraph, string eventId, DateTime time, string role);

        string GetPhases(string dcrXml);

        string GetReferXmlByEventId(string eventId, string dcrXml);

        string MergeReferXmlWithMainXml(string mainXml, string referXml, string eventId);

        string AddEvent(string eventId, string label, string roles, string description, string xml);

        string RemoveEvent(string eventId, string xml);
        #endregion

    }
}
