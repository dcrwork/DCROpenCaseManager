using OpenCaseManager.Models;
using System;
using System.Data;

namespace OpenCaseManager.Managers
{
    public interface IManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        DataTable SelectData(DataModel dataModel);
        DataTable UpdateData(DataModel dataModel);
        DataTable InsertData(DataModel dataModel);
        DataTable ExecuteStoredProcedure(DataModel dataModel);
        void LogSQLModel(DataModel dataModel, Exception ex, string methodName);

    }
}
