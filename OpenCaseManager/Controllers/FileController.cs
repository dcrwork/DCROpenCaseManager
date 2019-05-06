using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenCaseManager.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private IManager _manager;
        private IDataModelManager _dataModelManager;

        public FileController(IManager manager, IDataModelManager dataModelManager)
        {
            _manager = manager;
            _dataModelManager = dataModelManager;
        }
        // GET: File
        public FileResult DownloadFile(string link)
        {
            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Document.ToString());
            _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Document.Link.ToString(), DBEntityNames.Document.Title.ToString(), DBEntityNames.Document.Responsible.ToString(), DBEntityNames.Document.InstanceId.ToString(), DBEntityNames.Document.Type.ToString(), DBEntityNames.Document.ChildId.ToString() });
            _dataModelManager.AddFilter(DBEntityNames.Document.Link.ToString(), Enums.ParameterType._string, link, Enums.CompareOperator.like, Enums.LogicalOperator.none);

            var data = _manager.SelectData(_dataModelManager.DataModel);
            if (data.Rows.Count > 0)
            {
                var path = string.Empty;
                var type = data.Rows[0]["Type"].ToString();
                var currentUser = Common.GetCurrentUserName();
                var instanceId = data.Rows[0]["InstanceId"].ToString();
                var childId = data.Rows[0]["ChildId"].ToString();

                switch (type) //TODO: Maybe needs to be extended, when we are to actually show the journalnotes, and be able to download them
                {
                    case "PersonalDocument":
                        path = Configurations.Config.PersonalFileLocation + "\\" + currentUser + "\\" + data.Rows[0]["Link"].ToString();
                        break;
                    case "InstanceDocument":
                        path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + data.Rows[0]["Link"].ToString();
                        break;
                    case "Instance":
                        path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + data.Rows[0]["Link"].ToString();
                        break;
                    case "ChildDocument":
                        path = Configurations.Config.ChildFileLocation + "\\" + childId + "\\" + data.Rows[0]["Link"].ToString();
                        break;
                }

                string fileName = data.Rows[0]["Title"].ToString() + Path.GetExtension(link);
                byte[] filedata = System.IO.File.ReadAllBytes(path);
                string contentType = MimeMapping.GetMimeMapping(path);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = true,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            else
            {
                throw new Exception("File not found");
            }
        }
    }
}