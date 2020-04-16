namespace OpenCaseManager.Managers
{
    public interface IActiveRepositoryService
    {
        string InitializeGraph(string graphId);

        string GetPendingOrEnabled(string graphId, string simulationId);

        string ExecuteEvent(string graphId, string simulationId, string eventId);

        string ExecuteEventWithTime(string graphId, string simulationId, string eventId, string time);

        string GetProcessRoles(string graphId);

        string SearchProcess(string title);

        string GetProcess(string graphId);

        string GetProcessPhases(string graphId);

        string AdvanceTime(string graphId, string simId, string time);

        string GetReferXmlByEventId(string graphId, string simulationId, dynamic eventId);

        void MergeReferXmlWithMainXml(string graphId, string simulationId, dynamic referXml, dynamic eventId);

        string GetMajorRevisions(string graphId);
    }
}
