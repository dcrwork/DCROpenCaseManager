using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenCaseManager.Custom.Syddjurs
{
    public class SyddjursWork : ISyddjursWork
    {
        private IManager _manager;
        private IDataModelManager _dataModelManager;

        public SyddjursWork(IManager manager, IDataModelManager dataModelManager)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
        }

        /// <summary>
        /// Update Case Content in Acadre
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="caseContent"></param>
        public void UpdateCaseContent(int caseId, string caseContent)
        {
            try
            {
                Common.LogInfo(_manager, _dataModelManager, "Calling Acadre ChangeCaseContent Service");
                Common.LogInfo(_manager, _dataModelManager, "caseId: " + caseId + " , caseContent : " + caseContent + " - UpdateCaseContent");

                var acadreService = Common.GetAcadreService();
                acadreService.ChangeCaseContent(caseContent, caseId);

                Common.LogInfo(_manager, _dataModelManager, "Call Succeeded -  Acadre ChangeCaseContent Service");
            }
            catch (Exception ex)
            {
                Common.LogInfo(_manager, _dataModelManager, "Call Failed -  Acadre ChangeCaseContent Service : caseId: " + caseId + " , caseContent : " + caseContent + " - UpdateCaseContent");
                Common.LogError(ex);
            }
        }
    }
}