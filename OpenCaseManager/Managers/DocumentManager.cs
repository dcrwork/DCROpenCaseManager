using OpenCaseManager.Commons;
using OpenCaseManager.Models;
using System;
using System.Data;
using System.IO;

namespace OpenCaseManager.Managers
{
    public class DocumentManager : IDocumentManager
    {
        private string fileLink;
        /// <summary>
        /// Add Document
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="fileType"></param>
        /// <param name="givenFileName"></param>
        /// <param name="fileName"></param>
        /// <param name="eventId"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public string AddDocument(string instanceId, string fileType, string givenFileName, string fileName, string eventId, IManager manager, IDataModelManager dataModelManager)
        {
            CreateFileLink(fileName);
            string filePath = AddDocumentChecks(instanceId, fileType, givenFileName, fileName, eventId);
            var isDocumentAdded = false;
            var childId = Common.GetInstanceChildId(manager, dataModelManager, instanceId);
            if (fileType == "Temp")
            {
                givenFileName = eventId;
                if (eventId.ToLower().StartsWith("global".ToLower()))
                {
                    
                    if (childId > 0)
                    {
                        dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.GetGlobalEvents.ToString());
                        dataModelManager.AddParameter(DBEntityNames.GetGlobalEvents.ChildId.ToString(), Enums.ParameterType._int, childId.ToString());
                        dataModelManager.AddParameter(DBEntityNames.GetGlobalEvents.EventId.ToString(), Enums.ParameterType._string, eventId);

                        var globalEvents = manager.ExecuteStoredProcedure(dataModelManager.DataModel);
                        isDocumentAdded = globalEvents.Rows.Count > 0 ? true : false;

                        foreach (DataRow globalEvent in globalEvents.Rows)
                        {
                            Common.AddDocument(givenFileName, fileType, fileLink, globalEvent["InstanceId"].ToString(), childId.ToString(), false, DateTime.Now, manager, dataModelManager);
                        }
                    }
                }
            }

            if (!isDocumentAdded)
            {
                Common.AddDocument(givenFileName, fileType, fileLink, instanceId, childId.ToString(),  false, DateTime.Now, manager, dataModelManager);
            }
            return filePath;
        }

        /// <summary>
        /// Add Document Journal Note
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="fileType"></param>
        /// <param name="givenFileName"></param>
        /// <param name="fileName"></param>
        /// <param name="eventId"></param>
        /// <param name="isDraft"></param>
        /// <param name="eventDateTime"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public Tuple<string, string> AddDocument(string instanceId, string fileType, string givenFileName, string fileName, string eventId, bool isDraft, string childId, DateTime eventDateTime, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                CreateFileLink(fileName);
                string filePath = AddDocumentChecks(instanceId, fileType, givenFileName, fileName, eventId);
                string documentId = Common.AddDocument(givenFileName, fileType, fileLink, instanceId, childId, isDraft, eventDateTime, manager, dataModelManager);
                Tuple<string, string> returnTuple = new Tuple<string, string>(filePath, documentId);
                return returnTuple;
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    InstanceId = instanceId,
                    FileType = fileType,
                    GivenFileName = givenFileName,
                    FileName = fileName,
                    EventId = eventId,
                    IsDraft = isDraft,
                    EventDateTime = eventDateTime
                };
                Common.LogInfo(manager, dataModelManager, "AddDocument - Failed. - " + Common.ToJson(obj));
                Common.LogError(ex);
                throw ex;
            }
        }

        public void CreateFileLink(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            fileLink = DateTime.Now.ToFileTime() + ext;
        }

        public string AddDocumentChecks(string instanceId, string fileType, string givenFileName, string fileName, string eventId)
        {
            string ext = Path.GetExtension(fileName);
            string filePath = string.Empty;

            switch (fileType)
            {
                case "PersonalDocument":
                    DirectoryInfo directoryInfo = new DirectoryInfo(Configurations.Config.PersonalFileLocation);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    string currentUser = Common.GetCurrentUserName();
                    directoryInfo = new DirectoryInfo(Configurations.Config.PersonalFileLocation + "\\" + currentUser);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    filePath = directoryInfo.FullName;
                    break;
                case "InstanceDocument":
                    directoryInfo = new DirectoryInfo(Configurations.Config.InstanceFileLocation);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    directoryInfo = new DirectoryInfo(Configurations.Config.InstanceFileLocation + "\\" + instanceId);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    filePath = directoryInfo.FullName;
                    break;
                case "JournalNoteImportant":
                    directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    filePath = directoryInfo.FullName;
                    break;
                case "JournalNoteLittle":
                    directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    filePath = directoryInfo.FullName;
                    break;
                case "JournalNoteBig":
                    directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    directoryInfo = new DirectoryInfo(Configurations.Config.JournalNoteFileLocation + "\\" + instanceId);
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    filePath = directoryInfo.FullName;
                    break;
                case "Temp":
                default:
                    directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\tmp\\" + DateTime.Now.ToFileTime());
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo.Create();
                    }
                    filePath = directoryInfo.FullName;
                    try
                    {

                        fileLink = fileName;
                        givenFileName = eventId;
                    }
                    catch (Exception)
                    {
                    }
                    break;
            }

            filePath = filePath + "\\" + fileLink;
            if (fileType == "Temp")
                fileLink = filePath;
            return filePath;
        }
    }

}