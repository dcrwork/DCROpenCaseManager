using OpenCaseManager.Commons;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

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
            _dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Document.Link.ToString(), DBEntityNames.Document.Title.ToString(), DBEntityNames.Document.Responsible.ToString(), DBEntityNames.Document.InstanceId.ToString(), DBEntityNames.Document.Type.ToString() });
            _dataModelManager.AddFilter(DBEntityNames.Document.Link.ToString(), Enums.ParameterType._string, link, Enums.CompareOperator.like, Enums.LogicalOperator.none);

            var data = _manager.SelectData(_dataModelManager.DataModel);
            if (data.Rows.Count > 0)
            {
                var path = string.Empty;
                var type = data.Rows[0]["Type"].ToString();
                switch (type) //TODO: Maybe needs to be extended, when we are to actually show the journalnotes, and be able to download them
                {
                    case "PersonalDocument":
                        var currentUser = Common.GetCurrentUserName();
                        path = Configurations.Config.PersonalFileLocation + "\\" + currentUser + "\\" + data.Rows[0]["Link"].ToString();
                        break;
                    case "InstanceDocument":
                        var instanceId = data.Rows[0]["InstanceId"].ToString();
                        path = Configurations.Config.InstanceFileLocation + "\\" + instanceId + "\\" + data.Rows[0]["Link"].ToString();
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

        /// <summary>
        /// Download dcr xml log or XES
        /// </summary>
        /// <param name="graphId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="isAccepting"></param>
        /// <param name="toXES"></param>
        /// <returns></returns>
        public FileResult DownloadDCRXMLLog(string graphId, DateTime? from, DateTime? to, bool isAccepting, bool toXES)
        {
            var xmlString = string.Empty;

            _dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.GetDCRXMLLog.ToString());
            _dataModelManager.AddParameter(DBEntityNames.GetDCRXMLLog.GraphId.ToString(), Enums.ParameterType._int, graphId);
            if (from.HasValue)
                _dataModelManager.AddParameter(DBEntityNames.GetDCRXMLLog.From.ToString(), Enums.ParameterType._datetime, from.ToString());
            if (to.HasValue)
                _dataModelManager.AddParameter(DBEntityNames.GetDCRXMLLog.To.ToString(), Enums.ParameterType._datetime, to.ToString());
            _dataModelManager.AddParameter(DBEntityNames.GetDCRXMLLog.IsAccepting.ToString(), Enums.ParameterType._boolean, isAccepting.ToString());

            var data = _manager.ExecuteStoredProcedure(_dataModelManager.DataModel);

            if (data.Rows.Count > 0)
            {
                xmlString = data.Rows[0]["DCRXML"].ToString();
            }

            if (!toXES)
                return File(Encoding.UTF8.GetBytes(xmlString), "application/xml", graphId + "-" + DateTime.Now.ToFileTime() + ".xml");
            else
            {
                var xesString = GetXESXML(xmlString);
                return File(Encoding.UTF8.GetBytes(xesString), "application/xml", graphId + "-" + DateTime.Now.ToFileTime() + ".xes");
            }
        }

        private static string GetXESXML(string logxml)
        {
            XDocument xlsDoc = new XDocument();
            try
            {
                XDocument logxmlDoc = XDocument.Parse(logxml);
                string url = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\DCR XML Log 2 XES.xslt";
                byte[] data;
                try
                {
                    data = System.IO.File.ReadAllBytes(url);
                }
                catch (Exception ex)
                {

                    throw new Exception("Unable to find XSLT file on this location " + url);
                }
                string xslfile = Encoding.GetEncoding("UTF-8").GetString(data);
                using (XmlWriter writer = xlsDoc.CreateWriter())
                {
                    // Load the style sheet. 
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(XmlReader.Create(new StringReader(xslfile.ToString())));

                    // Execute the transform and output the results to a writer. 
                    xslt.Transform(logxmlDoc.CreateReader(), writer);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return xlsDoc.ToString();
        }
    }
}