using AcadrePWS;
using Newtonsoft.Json;
using NLog;
using OpenCaseManager.Managers;
using OpenCaseManager.Models;
using PdfSharp;
using Repository;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Xml;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace OpenCaseManager.Commons
{
    public static class Common
    {
        #region Methods

        /// <summary>
        /// Sync Events
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="xml"></param>
        /// <param name="responsibleId"></param>
        public static void SyncEvents(string instanceId, string xml, string responsibleId, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, "SyncEvents");
            dataModelManager.AddParameter("InstanceId", Enums.ParameterType._int, instanceId);
            dataModelManager.AddParameter("EventXML", Enums.ParameterType._xml, xml);
            dataModelManager.AddParameter("LoginUser", Enums.ParameterType._int, responsibleId);

            manager.ExecuteStoreProcedure(dataModelManager.DataModel);
        }

        /// <summary>
        /// Get Responsible Details
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static DataTable GetResponsibleDetails(IManager manager, IDataModelManager dataModelManager)
        {
            var data = GetResponsibleFullDetails(manager, dataModelManager);
            data.Columns.Remove("ManagerId");
            return data;
        }

        /// <summary>
        /// Get responsible all details with Id
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static DataTable GetResponsibleFullDetails(IManager manager, IDataModelManager dataModelManager, string samAccountName = "")
        {
            if (string.IsNullOrEmpty(samAccountName))
            {
                var claim = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity).FindFirst(ClaimTypes.Email);
                var userName = claim?.Value;
                if (userName == null)
                {
                    claim = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity).FindFirst(ClaimTypes.Upn);
                }
                samAccountName = claim?.Value;
            }

            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "UserDetail");
            dataModelManager.AddResultSet(new List<string> { "Id", "SamAccountName", "Name", "Title", "Department", "ManagerId", "IsManager" });
            dataModelManager.AddFilter("SamAccountName", Enums.ParameterType._string, samAccountName, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var data = manager.SelectData(dataModelManager.DataModel);
            return data;
        }

        /// <summary>
        /// Replace responsible key with actual value
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        public static string ReplaceKeyWithResponsible(string query)
        {
            if (query.Contains("$(loggedInUser)"))
            {
                query = query.Replace("$(loggedInUser)", GetCurrentUserName());
            }
            if (query.Contains("$(loggedInUserId)"))
            {
                var responsible = GetResponsibleId();
                query = query.Replace("$(loggedInUserId)", responsible);
            }
            return query;
        }

        /// <summary>
        /// Get Responsible Id
        /// </summary>
        /// <returns></returns>
        public static string GetResponsibleId()
        {
            IDatabaseHandler iDataAccess = new DataAccess(Configurations.Config.ConnectionString);
            IDBManager iDbManager = new DbManager(iDataAccess);
            IManager iManager = new Manager(iDbManager);
            IDataModelManager iDataModelManager = new DataModelManager();
            var responsilbe = GetResponsibleFullDetails(iManager, iDataModelManager);
            return responsilbe.Rows[0]["Id"].ToString();
        }

        /// <summary>
        /// Get process/graph xml
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="graphId"></param>
        /// <returns></returns>
        public static string GetProcessXML(string processId, string graphId, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Process");
            dataModelManager.AddResultSet(new List<string>() { "DCRXML" });

            if (!string.IsNullOrEmpty(processId))
            {
                dataModelManager.AddFilter("Id", Enums.ParameterType._int, processId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            }
            else if (!string.IsNullOrEmpty(graphId))
            {
                dataModelManager.AddFilter("graphId", Enums.ParameterType._int, graphId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            }
            else
            {
                return string.Empty;
            }

            var data = manager.SelectData(dataModelManager.DataModel);
            if (data.Rows.Count > 0)
            {
                return data.Rows[0]["DCRXML"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get Instance Xml
        /// </summary>
        /// <param name="insstanceId"></param>
        /// <returns></returns>
        public static string GetInstanceXML(string insstanceId, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Instance");
            dataModelManager.AddResultSet(new List<string>() { "DCRXML" });

            if (!string.IsNullOrEmpty(insstanceId))
            {
                dataModelManager.AddFilter("Id", Enums.ParameterType._int, insstanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            }
            else
            {
                return string.Empty;
            }

            var data = manager.SelectData(dataModelManager.DataModel);
            if (data.Rows.Count > 0)
            {
                return data.Rows[0]["DCRXML"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Update simulation Id in instance table
        /// </summary>
        /// <param name="simulationId"></param>
        public static void UpdateInstance(string instanceId, string simulationId, string xml, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, "Instance");
            dataModelManager.AddParameter("SimulationId", Enums.ParameterType._int, simulationId);
            dataModelManager.AddFilter("Id", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            if (!string.IsNullOrEmpty(xml))
            {
                dataModelManager.AddParameter("DCRXML", Enums.ParameterType._xml, xml);
            }
            manager.UpdateData(dataModelManager.DataModel);
        }

        /// <summary>
        /// Update Event Type Data
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="manager"></param>
        public static void UpdateEventTypeData(dynamic instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, "AddEventTypeData");
            dataModelManager.AddParameter("InstanceId", Enums.ParameterType._int, instanceId);

            manager.ExecuteStoreProcedure(dataModelManager.DataModel);
        }

        /// <summary>
        /// Get paramters from Event Type data
        /// </summary>
        /// <param name="eventTypeData"></param>
        /// <param name="instanceId"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static Dictionary<string, dynamic> GetParametersFromEventTypeData(string eventTypeData, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            var data = "<data>" + eventTypeData + "</data>";

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);
            XmlNodeList resources = xml.SelectNodes("data/parameter");
            var dictionary = new Dictionary<string, dynamic>();

            foreach (XmlNode node in resources)
            {
                var key = node.Attributes["title"].Value;
                dynamic value = node.Attributes["value"].Value;

                value = ReplaceEventTypeKeyValues(value, instanceId, manager, dataModelManager);

                dictionary.Add(key.ToLower(), value);
            }
            return dictionary;
        }

        /// <summary>
        /// Replace event type keys with actual values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="instanceId"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static dynamic ReplaceEventTypeKeyValues(string value, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            var instanceTitle = string.Empty;
            var caseForeignNo = string.Empty;
            var internalCaseId = string.Empty;
            dynamic returVal = value;

            // make a one time call to database for case title and case foreign number
            if ((value.Contains("$(Title)") || value.Contains("$(CaseNoForeign)") || value.Contains("$(InternalCaseId)")) && (string.IsNullOrEmpty(instanceTitle) && string.IsNullOrEmpty(caseForeignNo) && string.IsNullOrEmpty(internalCaseId)))
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Instance");
                dataModelManager.AddResultSet(new List<string>() { "Title", "CaseNoForeign", "InternalCaseID" });
                dataModelManager.AddFilter("Id", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var instanceDetails = manager.SelectData(dataModelManager.DataModel);

                instanceTitle = instanceDetails.Rows[0]["Title"].ToString();
                caseForeignNo = instanceDetails.Rows[0]["CaseNoForeign"].ToString();
                internalCaseId = instanceDetails.Rows[0]["InternalCaseID"].ToString();
            }

            if (value.Contains("$(Title)") && !string.IsNullOrEmpty(instanceTitle))
            {
                returVal = value.Replace("$(Title)", instanceTitle);
            }
            else if (value.Contains("$(CaseNoForeign)") && !string.IsNullOrEmpty(caseForeignNo))
            {
                returVal = value.Replace("$(CaseNoForeign)", caseForeignNo);
            }
            else if (value.Contains("$(InternalCaseId)") && !string.IsNullOrEmpty(internalCaseId))
            {
                returVal = value.Replace("$(InternalCaseId)", internalCaseId);
            }
            else if (value.Contains("$(employee."))
            {
                var occurences = value.Occurences("$(employee.");
                while (value.Contains("$(employee.") && occurences > 0)
                {
                    var startIndexColumnKey = value.IndexOf("$(employee.");
                    var endIndexColumnKey = -1;
                    foreach (var key in value)
                    {
                        endIndexColumnKey++;
                        if (key == ')')
                        {
                            break;
                        }
                    }
                    var keyToReplace = value.Substring(startIndexColumnKey, (endIndexColumnKey + 1) - startIndexColumnKey);

                    var startIndexColumnName = keyToReplace.IndexOf(".");
                    startIndexColumnName++;
                    var endIndexColumnName = -1;
                    foreach (var key in keyToReplace)
                    {
                        endIndexColumnName++;
                        if (key == ')')
                        {
                            break;
                        }
                    }
                    var columnName = keyToReplace.Substring(startIndexColumnName, endIndexColumnName - startIndexColumnName);
                    var columnValue = ReplaceValueWithEmployeeData(columnName, instanceId, manager, dataModelManager);
                    if (!string.IsNullOrEmpty(columnValue))
                    {
                        value = value.Replace(keyToReplace, columnValue);
                    }

                    occurences--;
                }
                returVal = value;
            }
            else if (value.Contains("$(loggedInUser)"))
            {
                returVal = value.Replace("$(loggedInUser)", GetCurrentUserName());
            }
            else if (value.Equals("$(now)"))
            {
                returVal = DateTime.Now;
            }
            else if (value.Contains("$(now)"))
            {
                returVal = value.Replace("$(now)", DateTime.Now.ToString());
            }
            return returVal;
        }

        /// <summary>
        /// Hide Document Web Part
        /// </summary>
        /// <returns></returns>
        public static bool IsHideDocumentWebpart()
        {
            return bool.Parse(Configurations.Config.HideDocumentWebpart);
        }

        /// <summary>
        /// Get instruction html
        /// </summary>
        /// <returns></returns>
        public static string GetInstructionHtml(string page)
        {
            try
            {
                var html = string.Empty;
                switch (page.ToLower())
                {
                    case "form":
                        html = File.ReadAllText(Configurations.Config.FormInstructionHtmlLocation);
                        break;
                    case "mus":
                        html = File.ReadAllText(Configurations.Config.MUSInstructionHtmlLocation);
                        break;
                }
                return html;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Replace key values with actual employee values
        /// </summary>
        /// <param name="value"></param>
        private static string ReplaceValueWithEmployeeData(string columnName, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            // get selected employee sam account name
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.InstanceExtension.ToString());
            dataModelManager.AddResultSet(new List<string>() { DBEntityNames.InstanceExtension.Employee.ToString() });
            dataModelManager.AddFilter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._string, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            var employeeId = manager.SelectData(dataModelManager.DataModel);

            if (employeeId.Rows.Count > 0)
            {
                var viewName = Configurations.Config.EmployeeView;

                // sql query
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, viewName);
                dataModelManager.AddResultSet(new List<string>() { columnName });
                dataModelManager.AddFilter("EmployeeId", Enums.ParameterType._string, employeeId.Rows[0][DBEntityNames.InstanceExtension.Employee.ToString()].ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                try
                {
                    var columnValue = manager.SelectData(dataModelManager.DataModel);
                    return columnValue.Rows[0][columnName].ToString();
                }
                catch (Exception ex)
                {
                    Logger logger = LogManager.GetCurrentClassLogger();
                    logger.Error(ex, "Error");
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get current user name from thread
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUserName()
        {
            var claim = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity).FindFirst(ClaimTypes.Email);
            var userName = claim?.Value;
            if (userName == null)
            {
                claim = ((ClaimsIdentity)Thread.CurrentPrincipal.Identity).FindFirst(ClaimTypes.Upn);
            }

            userName = claim?.Value;
            return userName;
        }

        /// <summary>
        /// Get dcr graphs url configured in ocm
        /// </summary>
        /// <returns></returns>
        public static string GetDCRGraphsURL()
        {
            return Configurations.Config.DCRActiveRepository;
        }

        /// <summary>
        /// Get sql date format for sql queries
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetSqlDateTimeFormat(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Add a document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="type"></param>
        /// <param name="link"></param>
        public static void AddDocument(string documentName, string type, string link, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Document.ToString());
            dataModelManager.AddParameter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentName);
            dataModelManager.AddParameter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, type);
            dataModelManager.AddParameter(DBEntityNames.Document.Link.ToString(), Enums.ParameterType._string, link);
            dataModelManager.AddParameter(DBEntityNames.Document.Responsible.ToString(), Enums.ParameterType._string, "$(loggedInUser)");
            dataModelManager.AddParameter(DBEntityNames.Document.UploadDate.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
            if (!string.IsNullOrEmpty(instanceId))
            {
                dataModelManager.AddParameter(DBEntityNames.Document.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
            }

            manager.InsertData(dataModelManager.DataModel);
        }

        /// <summary>
        /// Save array bytes to specified location
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytesToWrite"></param>
        public static void SaveBytesToFile(string filePath, byte[] bytesToWrite)
        {
            if (filePath != null && filePath.Length > 0 && bytesToWrite != null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                FileStream file = File.Create(filePath);

                file.Write(bytesToWrite, 0, bytesToWrite.Length);

                file.Close();
            }
        }

        /// <summary>
        /// Get Form Data
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static byte[] GetFormData(string formId, IManager manager, IDataModelManager dataModelManager)
        {
            byte[] data = { };

            using (MemoryStream ms = new MemoryStream())
            {
                var html = GetFormHtml(formId, manager, dataModelManager);
                MyFontResolverPdfSharp.Apply();

                var config = new PdfGenerateConfig()
                {
                    MarginBottom = 70,
                    MarginLeft = 20,
                    MarginRight = 20,
                    MarginTop = 70,
                };

                var pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
                pdf.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }

        /// <summary>
        /// Get formdata as html
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static string GetFormHtml(string formId, IManager manager, IDataModelManager dataModelManager)
        {
            var html = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "HtmlTemplates/FormTemplate.html");

            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "FormItem");
            dataModelManager.AddResultSet(new List<string> { "Id", "ItemId", "ItemText", "IsGroup", "SequenceNumber" });
            dataModelManager.AddFilter("FormId", Enums.ParameterType._int, formId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var formItems = manager.SelectData(dataModelManager.DataModel);

            var list = formItems.AsEnumerable().ToList();

            var groups = list.Where(x => Boolean.Parse(x["IsGroup"].ToString())).OrderBy(x => int.Parse(x["SequenceNumber"].ToString())).ToList();

            var formData = string.Empty;
            var sequenceNumber = 1;
            foreach (var group in groups)
            {
                formData += "<h3 style=\"color:rgb(64,173,72);page-break-inside: avoid;\">" + group["ItemText"].ToString() + "</h3>";
                var questions = list.Where(x => !string.IsNullOrEmpty(x["ItemId"].ToString())).Where(x => x["ItemId"].ToString() == group["Id"].ToString()).OrderBy(x => int.Parse(x["SequenceNumber"].ToString())).ToList();

                foreach (var question in questions)
                {
                    var seqNumber = string.Empty;
                    if (Boolean.Parse(question["IsGroup"].ToString()) == false)
                    {
                        seqNumber = sequenceNumber + ". ";
                    }

                    formData += "<p style=\"margin-left:20px;page-break-inside: avoid;\">" + seqNumber + question["ItemText"].ToString() + " </p>";
                    sequenceNumber++;
                }
            }
            html = html.Replace("#FormData#", formData);
            return html;
        }

        /// <summary>
        /// Get path of word as form
        /// </summary>
        /// <param name="html"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetFormWordPath(string html, IService service)
        {
            var serviceModel = new ServiceModel()
            {
                BaseUrl = Configurations.Config.NodeWordDocumentServer,
                Body = JsonConvert.SerializeObject(new { html }),
                MethodType = Method.POST
            };
            return service.GetNodeJSServiceResponse(serviceModel).Content;
        }

        /// <summary>
        /// Get Json for an object
        /// </summary>
        /// <param name="input"></param>
        public static string ToJson(object input)
        {
            return JsonConvert.SerializeObject(input);
        }

        /// <summary>
        /// Is Secure Website
        /// </summary>
        /// <returns></returns>
        public static bool IsHttps()
        {
            return HttpContext.Current.Request.IsSecureConnection;
        }

        /// <summary>
        /// Get dcr form server url
        /// </summary>
        /// <returns></returns>
        public static string GetDcrFormServerUrl()
        {
            return Configurations.Config.DcrFormServerUrl;
        }

        #region ACADRE
        /// <summary>
        /// Create Case in Acadre
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="responsible"></param>
        /// <returns></returns>
        public static string CreateCase(string instanceId, Dictionary<string, dynamic> parameters, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                // get parameters
                var personNameForAddressingName = parameters["personNameForAddressingName".ToLower()];
                var personCivilRegistrationNumber = parameters["PersonCivilRegistrationIdentifier".ToLower()];
                var caseFileTypeCode = parameters["caseFileTypeCode".ToLower()];
                var accessCode = parameters["accessCode".ToLower()];
                var caseFileTitleText = parameters["caseFileTitleText".ToLower()];
                var journalizingCode = parameters["journalizingCode".ToLower()];
                var facet = parameters["facet".ToLower()];
                var caseResponsible = parameters["CaseResponsible".ToLower()];
                var administrativeUnit = parameters["administrativeUnit".ToLower()];
                var caseContent = parameters["caseContent".ToLower()];
                var caseFileDisposalCode = parameters["caseFileDisposalCode".ToLower()];
                var deletionCode = parameters["deletionCode".ToLower()];
                var caseRestrictedFromPublicText = parameters["RestrictedFromPublicText".ToLower()];

                // set user
                CaseManagement.ActingFor(GetCurrentUserName());

                // create case in acadre
                var caseId = CaseManagement.CreateCase(
                        personNameForAddressingName,
                        personCivilRegistrationNumber,
                        caseFileTypeCode,
                        accessCode,
                        caseFileTitleText,
                       journalizingCode,
                        facet,
                        caseResponsible,
                        administrativeUnit,
                        caseContent,
                        caseFileDisposalCode,
                        deletionCode,
                        caseRestrictedFromPublicText);
                return caseId;
            }
            catch (Exception ex)
            {
                var parametersXML = GetParametersXML(parameters);
                SaveEventTypeDataParamertes(instanceId, parametersXML, "CreateCase", ex, manager, dataModelManager);
                throw ex;
            }
        }

        /// <summary>
        /// Get Case Id Foreign from Acrade
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public static string GetCaseIdForeign(string caseId)
        {
            CaseManagement.ActingFor(GetCurrentUserName());
            return CaseManagement.GetCaseNumber(caseId);
        }

        /// <summary>
        /// Get case link from acadre using case Id
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public static string GetCaseLink(string caseId)
        {
            CaseManagement.ActingFor(GetCurrentUserName());
            return CaseManagement.GetCaseURL(caseId);
        }

        /// <summary>
        /// Close case in Acadre
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        public static void CloseCase(string caseId)
        {
            CaseManagement.ActingFor(GetCurrentUserName());
            CaseManagement.CloseCase(caseId);
        }

        /// <summary>
        /// Create Document in Acadre
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="fileBytes"></param>
        /// <returns></returns>
        public static string CreateDocument(Dictionary<string, dynamic> parameters, string fileName, byte[] fileBytes, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                string documentCategoryCode = parameters["DocumentCategoryCode".ToLower()];
                string documentTitleText = parameters["DocumentTitleText".ToLower()];
                string documentStatusCode = parameters["DocumentStatusCode".ToLower()];
                DateTime documentDate = parameters["DocumentDate".ToLower()];
                string documentAccessCode = parameters["DocumentAccessCode".ToLower()];
                string documentCaseId = parameters["DocumentCaseId".ToLower()];
                string documentDescriptionText = parameters["DocumentDescriptionText".ToLower()];
                string documentAccessLevel = parameters["DocumentAccessLevel".ToLower()];
                string documentTypeCode = parameters["DocumentTypeCode".ToLower()];
                string recordStatusCode = parameters["RecordStatusCode".ToLower()];
                bool documentEvenOutRequired = bool.Parse(parameters["DocumentEvenOutRequired".ToLower()].ToString().ToLower());
                string documentUserId = parameters["DocumentUserId".ToLower()];
                string recordPublicationIndicator = parameters["PublicationIndicator".ToLower()];

                // set user
                CaseManagement.ActingFor(GetCurrentUserName());
                var documentId = CaseManagement.CreateDocumentService(
                        documentCaseId,
                        recordStatusCode,
                        documentTypeCode,
                        documentDescriptionText,
                        documentAccessCode,
                        documentStatusCode,
                        documentTitleText,
                        documentCategoryCode,
                        recordPublicationIndicator,
                        fileName,
                        fileBytes
                    );
                return documentId;
            }
            catch (Exception ex)
            {
                var parametersXML = GetParametersXML(parameters);
                SaveEventTypeDataParamertes(instanceId, parametersXML, "CreateDocument", ex, manager, dataModelManager);
                throw ex;
            }
        }

        /// <summary>
        /// Get parameter xml from dictionary
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string GetParametersXML(Dictionary<string, dynamic> parameters)
        {
            if (parameters.Count > 0)
            {
                var xml = "<data>";
                foreach (var item in parameters)
                {
                    xml += "<" + item.Key + ">" + item.Value + "</" + item.Key + ">";
                }
                xml += "</data>";
                return xml;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Save parameters
        /// </summary>
        /// <param name="parameters"></param>
        private static void SaveEventTypeDataParamertes(string instanceId, string parametersXML, string method, Exception ex, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.AcadreLog.ToString());
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.Method.ToString(), Enums.ParameterType._string, method);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.Parameters.ToString(), Enums.ParameterType._xml, parametersXML);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.IsSuccess.ToString(), Enums.ParameterType._boolean, bool.FalseString);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.ErrorStatement.ToString(), Enums.ParameterType._string, ex.Message);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.ErrorStackTrace.ToString(), Enums.ParameterType._string, ex.StackTrace);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.InstanceId.ToString(), Enums.ParameterType._string, instanceId);

            manager.InsertData(dataModelManager.DataModel);
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// String Extension Functions
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Get occurences of a string in complete string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int Occurences(this string str, string val)
        {
            int occurrences = 0;
            int startingIndex = 0;

            while ((startingIndex = str.IndexOf(val, startingIndex)) >= 0)
            {
                ++occurrences;
                ++startingIndex;
            }

            return occurrences;
        }

        /// <summary>
        /// Get indexes of all occurences in a string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException("the string to find may not be empty", "value");
            }

            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                {
                    return indexes;
                }

                indexes.Add(index);
            }
        }
    }
}