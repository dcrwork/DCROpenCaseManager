using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCaseManager.Managers
{
    public interface IProcessEngineService
    {
        DCRGraph InitializeProcess(DCRGraph dCRGraph);

        string GetPendingOrEnabled(DCRGraph dCRGraph);

        DCRGraph ExecuteEvent(DCRGraph dCRGraph, string eventId);

        DCRGraph NotApplicable(DCRGraph dCRGraph, string eventId, string note);

        DCRGraph ExecuteEventWithTime(DCRGraph dCRGraph, string eventId, DateTime time);

        DCRGraph ExecuteEventWithTimeAndRole(DCRGraph dCRGraph, string eventId, DateTime time, string role);

        DCRGraph IsAccepting(DCRGraph dCRGraph);

        string AddEvent(string eventId, string label, string roles, string description, string xml);

        string RemoveEvent(string eventId, string xml);

        string GetPhases(string dcrXml);

        string GetReferXmlByEventId(string eventId, string dcrXml);

        string MergeReferXmlWithMainXml(string mainXml, string referXml, string eventId);
    }
}
