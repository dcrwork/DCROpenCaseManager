using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCaseManager.Managers
{
    public interface IDocumnentManager
    {
        string AddDocument(string instanceId, string fileType, string givenFileName, string fileName, string eventId, IManager manager, IDataModelManager dataModelManager);
        Tuple<string,string> AddDocument(string instanceId, string fileType, string givenFileName, string fileName, string eventId, bool isDraft, DateTime eventDateTime, IManager manager, IDataModelManager dataModelManager);
    }
}
