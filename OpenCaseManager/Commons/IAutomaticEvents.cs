using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCaseManager.Commons
{
    public interface IAutomaticEvents
    {
        void SendEmail(string to, string subject, string body);

        void ReleaseProcess(string instanceId);
    }
}
