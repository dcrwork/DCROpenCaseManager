using OpenCaseManager.Commons;
using System;
using System.IO;

namespace OpenCaseManager.Managers
{
    public class DocumentManager : IDocumnentManager
    {
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
            string ext = Path.GetExtension(fileName);
            string fileLink = DateTime.Now.ToFileTime() + ext;
            string filePath = string.Empty;

            switch (fileType)
            {
                case "Personal":
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
                case "Instance":
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
                case "JournalNote":
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

            Common.AddDocument(givenFileName, fileType, fileLink, instanceId, manager, dataModelManager);
            return filePath;
        }
    }
}