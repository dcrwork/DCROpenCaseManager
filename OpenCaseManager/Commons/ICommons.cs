using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCaseManager.Commons
{
    public interface ICommons
    {
        MajorRevision GetMajorVersion(string graphId);

        List<MajorRevision> GetProcessMajorRevisions(string graphId = "");

        void AddMajorVersionIdToInstance(string graphId, string instanceId);

        string GetResponsibleRoles(string instanceId, string eventId, string responsibleId);
    }
}
