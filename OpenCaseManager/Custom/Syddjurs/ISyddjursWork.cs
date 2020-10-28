using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCaseManager.Custom.Syddjurs
{
    public interface ISyddjursWork
    {
        void UpdateCaseContent(int caseId, string caseContent);
    }
}
