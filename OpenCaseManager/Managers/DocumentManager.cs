using OpenCaseManager.Commons;
using System;
using System.IO;

namespace OpenCaseManager.Managers
{
    public class DocumentManager : IDocumnentManager
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
            Common.AddDocument(givenFileName, fileType, fileLink, instanceId, DateTime.Now, false, manager, dataModelManager);
            return filePath;
        }

        public Tuple<string, string> AddDocument(string instanceId, string fileType, string givenFileName, string fileName, string eventId, DateTime eventDateTime, bool isLocked, IManager manager, IDataModelManager dataModelManager)
        {
            CreateFileLink(fileName);
            string filePath = AddDocumentChecks(instanceId, fileType, givenFileName, fileName, eventId);
            string documentId = Common.AddDocument(givenFileName, fileType, fileLink, instanceId, eventDateTime, isLocked, manager, dataModelManager);
            Tuple<string, string> returnTuple = new Tuple<string, string>(filePath, documentId);
            return returnTuple;
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