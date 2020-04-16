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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace OpenCaseManager.Commons
{
    public static class Common
    {
        #region Methods
        /// <summary>
        /// Add Instance
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string AddInstance(AddInstanceModel model, IManager manager, IDataModelManager dataModelManager)
        {
            //Update Fragments in XML
            var graphXML = UpdateFragmentsInXML(model.GraphId, manager, dataModelManager);

            try
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Instance.ToString());
                dataModelManager.AddParameter(DBEntityNames.Instance.Title.ToString(), Enums.ParameterType._string, model.Title);
                dataModelManager.AddParameter(DBEntityNames.Instance.Responsible.ToString(), Enums.ParameterType._int, GetResponsibleId());
                dataModelManager.AddParameter(DBEntityNames.Instance.GraphId.ToString(), Enums.ParameterType._int, model.GraphId.ToString());
                if (!graphXML.Equals(string.Empty))
                    dataModelManager.AddParameter(DBEntityNames.Instance.DCRXML.ToString(), Enums.ParameterType._xml, graphXML);

                var instanceIdTable = manager.InsertData(dataModelManager.DataModel);

                // add Instance Roles
                if (instanceIdTable.Rows.Count > 0 && instanceIdTable.Rows[0][DBEntityNames.Instance.Id.ToString()] != null)
                {
                    var instanceId = (instanceIdTable.Rows[0][DBEntityNames.Instance.Id.ToString()]).ToString();

                    AddInstanceDescription(instanceId, model.GraphId.ToString(), manager, dataModelManager);

                    if (model.UserRoles.Count > 0)
                    {
                        AddInstanceRoles(model.UserRoles, instanceId, manager, dataModelManager);
                    }
                    return instanceId;
                }
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "AddInstance - Failed. - " + ToJson(model));
                LogError(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// Update Fragments In XML
        /// </summary>
        /// <param name="graphId"></param>
        private static string UpdateFragmentsInXML(int graphId, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.DCRXML.ToString() });
                dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, graphId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = manager.SelectData(dataModelManager.DataModel);

                if (data.Rows.Count > 0)
                {
                    var mainGraphXML = data.Rows[0][DBEntityNames.Process.DCRXML.ToString()].ToString();

                    XDocument document = XDocument.Parse(mainGraphXML);
                    IEnumerable<XElement> lst =
                        from el in document.Descendants("event")
                        where el.Attribute("fragmentId") != null
                        select el;

                    foreach (XElement item in lst)
                    {
                        var EventID = item.Attribute("id").Value;
                        var fragmentId = item.Attribute("fragmentId").Value;

                        dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Process.ToString());
                        dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.DCRXML.ToString() });
                        dataModelManager.AddFilter(DBEntityNames.Process.GraphId.ToString(), Enums.ParameterType._int, fragmentId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                        var fragmentXml = manager.SelectData(dataModelManager.DataModel);

                        if (fragmentXml.Rows.Count > 0)
                        {
                            Dictionary<string, string> substitution = new Dictionary<string, string>();
                            string JSONresult = JsonConvert.SerializeObject(substitution);
                            DCRGraph graph = new DCRGraph(mainGraphXML);
                            mainGraphXML = graph.Splice(fragmentXml.Rows[0]["DCRXML"].ToString(), EventID, JSONresult);
                        }
                        else
                        {
                            throw new Exception("Fragment not found");
                        }
                    }
                    return mainGraphXML;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "UpdateFragmentsInXML - Failed. - " + ToJson(graphId));
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Add Instance Description
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="graphId"></param>
        public static void AddInstanceDescription(string instanceId, string graphId, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddInstanceDescription.ToString());
            dataModelManager.AddParameter(DBEntityNames.AddInstanceDescription.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
            dataModelManager.AddParameter(DBEntityNames.AddInstanceDescription.GraphId.ToString(), Enums.ParameterType._int, graphId);

            manager.ExecuteStoredProcedure(dataModelManager.DataModel);
        }

        /// <summary>
        /// Add Roles to Instance
        /// </summary>
        /// <param name="UserRoles"></param>
        public static void AddInstanceRoles(List<UserRole> UserRoles, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                var xmlDoc = CreateUserRolesXml(UserRoles);

                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, DBEntityNames.StoredProcedures.AddInstanceRoles.ToString());
                dataModelManager.AddParameter(DBEntityNames.AddInstanceRoles.InstanceId.ToString(), Enums.ParameterType._string, instanceId);
                dataModelManager.AddParameter(DBEntityNames.AddInstanceRoles.UserRoles.ToString(), Enums.ParameterType._xml, xmlDoc.InnerXml);

                manager.ExecuteStoredProcedure(dataModelManager.DataModel);
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "AddInstanceRoles - Failed. - " + ToJson(UserRoles) + ",instanceId : " + instanceId);
                LogError(ex);
            }
        }

        /// <summary>
        /// Create xml for roles
        /// </summary>
        /// <param name="userRolesList"></param>
        /// <returns></returns>
        private static XmlDocument CreateUserRolesXml(List<UserRole> userRolesList)
        {
            var xmlDoc = new XmlDocument();
            var rootNode = xmlDoc.CreateElement("UserRoles");
            xmlDoc.AppendChild(rootNode);

            foreach (var userRole in userRolesList)
            {
                XmlNode userNode = xmlDoc.CreateElement("User");
                XmlAttribute attribute = xmlDoc.CreateAttribute("Id");
                attribute.Value = userRole.UserId.ToString();
                userNode.Attributes.Append(attribute);

                XmlNode roleNode = xmlDoc.CreateElement("Role");
                roleNode.InnerText = userRole.RoleId;
                userNode.AppendChild(roleNode);

                rootNode.AppendChild(userNode);
            }
            return xmlDoc;
        }

        /// <summary>
        /// Initialize Graph
        /// </summary>
        /// <param name="useProcessEngine"></param>
        /// <param name="graphId"></param>
        /// <param name="instanceId"></param>
        /// <param name="dcrService"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static InitializeGraphModel InitializeGraph(bool useProcessEngine, string graphId, string instanceId, IDCRService dcrService, IManager manager, IDataModelManager dataModelManager)
        {
            var instanceXml = string.Empty;

            string simulationId;
            string eventsXml;
            dynamic result;
            // initialize graph/process and get pending or enabled events
            if (!useProcessEngine)
            {
                result = dcrService.InitializeGraph(graphId);
                simulationId = result;
                eventsXml = dcrService.GetPendingOrEnabled(graphId, simulationId);
            }
            else
            {
                // get process xml
                //var processXML = GetProcessXML(string.Empty, graphId, manager, dataModelManager);
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                dataModelManager.AddResultSet(new List<string> { DBEntityNames.Process.DCRXML.ToString() });
                dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var data = manager.SelectData(dataModelManager.DataModel);

                if (data.Rows.Count < 0)
                    throw new Exception("No Graph XML is found");
                dynamic dcrGraph = new DCRGraph(data.Rows[0]["DCRXML"].ToString());
                result = dcrService.InitializeGraph(dcrGraph);
                simulationId = instanceId;
                eventsXml = dcrService.GetPendingOrEnabled(dcrGraph);
                simulationId = instanceId;
                instanceXml = ((DCRGraph)dcrGraph).ToXml();
            }

            return new InitializeGraphModel()
            {
                EventsXML = eventsXml,
                SimulationId = simulationId,
                InstanceXML = instanceXml
            };
        }

        /// <summary>
        /// Sync Events
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="xml"></param>
        /// <param name="responsibleId"></param>
        public static void SyncEvents(string instanceId, string xml, string responsibleId, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SP, "SyncEvents");
                dataModelManager.AddParameter("InstanceId", Enums.ParameterType._int, instanceId);
                dataModelManager.AddParameter("EventXML", Enums.ParameterType._xml, xml);
                dataModelManager.AddParameter("LoginUser", Enums.ParameterType._int, responsibleId);

                manager.ExecuteStoredProcedure(dataModelManager.DataModel);
            }
            catch (Exception ex)
            {
                var model = new
                {
                    InstanceId = instanceId,
                    XML = xml,
                    ResponsibleId = responsibleId
                };

                LogInfo(manager, dataModelManager, "SyncEvents - Failed. - " + ToJson(model));
                LogError(ex);
            }
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
            try
            {
                var identity = Thread.CurrentPrincipal.Identity;
                if (samAccountName == string.Empty)
                {
                    var splitted = identity.Name.Split('\\');
                    if (splitted.Count() > 1) {
                        samAccountName = splitted[1];
                    } else {
                        samAccountName = splitted[0];
                    }
                }

                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "UserDetail");
                dataModelManager.AddResultSet(new List<string> { "Id", "SamAccountName", "Name", "Title", "Department", "ManagerId", "IsManager", "Acadreorgid" });
                dataModelManager.AddFilter("SamAccountName", Enums.ParameterType._string, samAccountName, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var data = manager.SelectData(dataModelManager.DataModel);

                // If SamAccountName is not found in UserDetail table, create new user

                if (data.Rows.Count == 0) {
                    // Insert new user
                    dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, "User");
                    dataModelManager.AddParameter("SamAccountName", Enums.ParameterType._string, samAccountName);
                    dataModelManager.AddParameter("Name", Enums.ParameterType._string, identity.Name);
                    dataModelManager.AddParameter("Title", Enums.ParameterType._string, "DefaultTitle");
                    dataModelManager.AddParameter("Department", Enums.ParameterType._string, "DefaultDepartment");
                    manager.InsertData(dataModelManager.DataModel);

                    // Try to find new userdetails
                    dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "UserDetail");
                    dataModelManager.AddResultSet(new List<string> { "Id", "SamAccountName", "Name", "Title", "Department", "ManagerId", "IsManager", "Acadreorgid" });
                    dataModelManager.AddFilter("SamAccountName", Enums.ParameterType._string, samAccountName, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    data = manager.SelectData(dataModelManager.DataModel);
                }

                return data;
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "GetResponsibleFullDetails - Failed. - " + ToJson(samAccountName));
                LogError(ex);
                throw ex;
            }
        }


        /// <summary>
        /// Gets if a user is manager
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static DataTable GetIsManager(IManager manager, IDataModelManager dataModelManager, string samAccountName = "")
        {
            try
            {
                var identity = Thread.CurrentPrincipal.Identity;
                if (samAccountName == string.Empty)
                {
                    var splitted = identity.Name.Split('\\');
                    if (splitted.Count() > 1)
                    {
                        samAccountName = splitted[1];
                    }
                    else
                    {
                        samAccountName = splitted[0];
                    }
                }

                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "[User]");
                dataModelManager.AddResultSet(new List<string> { "IsManager"});
                dataModelManager.AddFilter("SamAccountName", Enums.ParameterType._string, samAccountName, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var data = manager.SelectData(dataModelManager.DataModel);
                
                return data;
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "GetDepartment - Failed. - " + ToJson(samAccountName));
                LogError(ex);
                throw ex;
            }
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
            try
            {
                IDatabaseHandler iDataAccess = new DataAccess(Configurations.Config.ConnectionString);
                IDBManager iDbManager = new DbManager(iDataAccess);
                IManager iManager = new Manager(iDbManager);
                IDataModelManager iDataModelManager = new DataModelManager();
                var responsilbe = GetResponsibleFullDetails(iManager, iDataModelManager);
                if (responsilbe.Rows.Count < 1)
                    throw new Exception("No User Details Found");
                return responsilbe.Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw ex;
            }
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

            manager.ExecuteStoredProcedure(dataModelManager.DataModel);
        }

        /// <summary>
        /// Get paramters from Event Type data
        /// </summary>
        /// <param name="eventTypeData"></param>
        /// <param name="instanceId"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static Dictionary<string, dynamic> GetParametersFromEventTypeData(string eventTypeData, string instanceId, IManager manager, IDataModelManager dataModelManager, string eventId)
        {
            var data = "<data>" + eventTypeData + "</data>";
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);
                XmlNodeList resources = xml.SelectNodes("data/parameter");
                var dictionary = new Dictionary<string, dynamic>();

                foreach (XmlNode node in resources)
                {
                    var key = node.Attributes["title"].Value;
                    dynamic value = node.Attributes["value"].Value;

                    value = ReplaceEventTypeKeyValues(value, instanceId, manager, dataModelManager, eventId);

                    dictionary.Add(key.ToLower(), value);
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    EventTypeData = eventTypeData,
                    InstanceId = instanceId,
                    EventId = eventId
                };
                LogInfo(manager, dataModelManager, "GetParametersFromEventTypeData - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Replace event type keys with actual values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="instanceId"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static dynamic ReplaceEventTypeKeyValues(string value, string instanceId, IManager manager, IDataModelManager dataModelManager, string eventId = "", string journalCaseId = "")
        {
            var instanceTitle = string.Empty;
            var caseForeignNo = string.Empty;
            var internalCaseId = string.Empty;
            dynamic returVal = value;

            try
            {            // make a one time call to database for case title and case foreign number
                if (!string.IsNullOrEmpty(instanceId))
                {
                    dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Instance");
                    dataModelManager.AddResultSet(new List<string>() { "Title", "CaseNoForeign", "InternalCaseID" });
                    dataModelManager.AddFilter("Id", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                    var instanceDetails = manager.SelectData(dataModelManager.DataModel);
                    if (instanceDetails.Rows.Count > 0)
                    {
                        instanceTitle = instanceDetails.Rows[0]["Title"].ToString();
                        caseForeignNo = instanceDetails.Rows[0]["CaseNoForeign"].ToString();
                        internalCaseId = instanceDetails.Rows[0]["InternalCaseID"].ToString();
                    }
                }

                if (value.Contains("$(Title)") && !string.IsNullOrEmpty(instanceId))
                {
                    var temp = value;
                    returVal = temp.Replace("$(Title)", instanceTitle);
                    value = returVal;
                }
                if (value.Contains("$(title)") && !string.IsNullOrEmpty(instanceId))
                {
                    var temp = value;
                    returVal = temp.Replace("$(title)", instanceTitle);
                    value = returVal;
                }
                if (value.ToLower().Contains("$(externalcasetitle)".ToLower()) && !string.IsNullOrEmpty(instanceId) && string.IsNullOrEmpty(journalCaseId))
                {
                    var temp = value;
                    returVal = temp.Replace("$(externalcasetitle)", instanceTitle);
                    value = returVal;
                }
                if (value.ToLower().Contains("$(externalcasetitle)".ToLower()) && !string.IsNullOrEmpty(journalCaseId))
                {
                    try
                    {
                        var child = GetChildInfo(manager, dataModelManager, int.Parse(journalCaseId));
                        if (child != null && child.SimpleChild != null)
                        {
                            var temp = value;
                            returVal = temp.Replace("$(externalcasetitle)", child.SimpleChild.FullName);
                            value = returVal;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
                }
                if (value.ToLower().Contains("$(CaseNoForeign)".ToLower()) && !string.IsNullOrEmpty(instanceId))
                {
                    var temp = value;
                    returVal = temp.Replace("$(CaseNoForeign)", caseForeignNo);
                    value = returVal;
                }
                if (value.ToLower().Contains("$(externalcaseid)".ToLower()) && !string.IsNullOrEmpty(instanceId) && string.IsNullOrWhiteSpace(journalCaseId))
                {
                    var temp = value;
                    returVal = temp.Replace("$(externalcaseid)", caseForeignNo);
                    value = returVal;
                }
                if (value.ToLower().Contains("$(externalcaseid)".ToLower()) && !string.IsNullOrEmpty(journalCaseId))
                {
                    try
                    {
                        var child = GetChildInfo(manager, dataModelManager, int.Parse(journalCaseId));
                        if (child != null)
                        {
                            var temp = value;
                            returVal = temp.Replace("$(externalcaseid)", child.CaseNumberIdentifier);
                            value = returVal;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
                }
                if (value.ToLower().Contains("$(InternalCaseId)".ToLower()) && !string.IsNullOrEmpty(instanceId))
                {
                    var temp = value;
                    returVal = temp.Replace("$(InternalCaseId)", internalCaseId);
                    value = returVal;
                }
                if (value.ToLower().Contains("$(InternalCaseId)".ToLower()) && !string.IsNullOrEmpty(instanceId))
                {
                    if (((string)returVal).ToLower().Contains("$(InternalCaseId)".ToLower()))
                    {
                        var temp = value;
                        returVal = temp.Replace("$(InternalCaseid)", internalCaseId);
                        value = returVal;
                    }
                }
                if (value.ToLower().Contains("$(employee.".ToLower()))
                {
                    var occurences = value.Occurences("$(employee.");
                    while (value.ToLower().Contains("$(employee.".ToLower()) && occurences > 0)
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
                if (value.ToLower().Contains("$(loggedInUser)".ToLower()))
                {
                    returVal = value.Replace("$(loggedInUser)", GetCurrentUserName());
                    value = returVal;
                }
                if (value.ToLower().Equals("$(now)".ToLower()))
                {
                    returVal = DateTime.Now;
                }
                else if (value.ToLower().Contains("$(now)".ToLower()))
                {
                    returVal = value.Replace("$(now)", DateTime.Now.ToString());
                    value = returVal;
                }
                if (!string.IsNullOrEmpty(eventId))
                {
                    dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Event");
                    dataModelManager.AddResultSet(new List<string>() { "EventId", "Title", "Description", "Note" });
                    dataModelManager.AddFilter("EventId", Enums.ParameterType._string, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    dataModelManager.AddFilter("InstanceId", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);

                    var selectedEvent = manager.SelectData(dataModelManager.DataModel);
                    if (selectedEvent.Rows.Count > 0)
                    {
                        if (value.ToLower().Contains("$(EventId)".ToLower()))
                        {
                            returVal = value.Replace("$(EventId)", selectedEvent.Rows[0]["EventId"].ToString());
                            value = returVal;
                        }
                        if (value.ToLower().Contains("$(EventLabel)".ToLower()))
                        {
                            returVal = value.Replace("$(EventLabel)", selectedEvent.Rows[0]["Title"].ToString());
                            value = returVal;
                        }
                        if (value.ToLower().Contains("$(EventDescription)".ToLower()))
                        {
                            returVal = value.Replace("$(EventDescription)", selectedEvent.Rows[0]["Description"].ToString());
                            value = returVal;
                        }
                        if (value.ToLower().Contains("$(Comment)".ToLower()))
                        {
                            returVal = value.Replace("$(Comment)", selectedEvent.Rows[0]["Note"].ToString());
                            returVal = returVal.Replace("$(comment)", selectedEvent.Rows[0]["Note"].ToString());
                            value = returVal;
                        }
                    }
                }
                if (value.ToLower().Contains("$(JournalCaseId)".ToLower()))
                {
                    var childId = GetInstanceChildId(manager, dataModelManager, instanceId);
                    returVal = value.Replace("$(JournalCaseId)", childId.ToString());
                    value = returVal;
                }
                if (value.ToLower().Contains("$(JournalCaseId.CPR)".ToLower()))
                {
                    try
                    {
                        var childId = GetInstanceChildId(manager, dataModelManager, instanceId);
                        var child = GetChildInfo(manager, dataModelManager, childId);
                        if (child != null && child.SimpleChild != null)
                        {
                            returVal = value.Replace("$(JournalCaseId.CPR)", child.SimpleChild.CPR);
                            value = returVal;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
                }
                if (value.ToLower().Contains("$(JournalCaseId.Name)".ToLower()))
                {
                    try
                    {
                        var childId = GetInstanceChildId(manager, dataModelManager, instanceId);
                        var child = GetChildInfo(manager, dataModelManager, childId);
                        if (child != null && child.SimpleChild != null)
                        {
                            returVal = value.Replace("$(JournalCaseId.Name)", child.SimpleChild.FullName);
                            value = returVal;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
                }
                if (value.ToLower().Contains("$(loggedInUser.".ToLower()))
                {
                    var occurences = value.Occurences("$(loggedInUser.");
                    while (value.ToLower().Contains("$(loggedInUser.".ToLower()) && occurences > 0)
                    {
                        var startIndexColumnKey = value.IndexOf("$(loggedInUser.");
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
                        var columnValue = ReplaceValueWithUserData(columnName, instanceId, manager, dataModelManager);
                        if (!string.IsNullOrEmpty(columnValue))
                        {
                            value = value.Replace(keyToReplace, columnValue);
                        }

                        occurences--;
                    }
                    returVal = value;
                }

            }
            catch (Exception ex)
            {
                var obj = new
                {
                    Value = value,
                    InstanceId = instanceId,
                    EventId = eventId,
                    JournalCaseId = journalCaseId
                };
                LogInfo(manager, dataModelManager, "ReplaceEventTypeKeyValues - Failed. - " + ToJson(obj));
                LogError(ex);
            }
            return returVal;
        }

        /// <summary>
        /// Get child id from database for selected case
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public static int GetInstanceChildId(IManager manager, IDataModelManager dataModelManager, string instanceId)
        {
            try
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.InstanceExtension.ToString());
                dataModelManager.AddResultSet(new List<string>() { DBEntityNames.InstanceExtension.ChildId.ToString() });
                dataModelManager.AddFilter(DBEntityNames.InstanceExtension.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var childId = manager.SelectData(dataModelManager.DataModel);
                if (childId.Rows.Count > 0)
                    return int.Parse(childId.Rows[0][DBEntityNames.InstanceExtension.ChildId.ToString()].ToString());
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "GetInstanceChildId - Failed. - " + ToJson(instanceId));
                LogError(ex);
            }
            return 0;
        }

        /// <summary>
        /// Get internal case Id of a case
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public static int GeInternalCaseId(IManager manager, IDataModelManager dataModelManager, string instanceId)
        {
            try
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.InternalCaseID.ToString() });
                dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var insternalCaseDataTable = manager.SelectData(dataModelManager.DataModel);
                if (insternalCaseDataTable.Rows.Count > 0)
                    return int.Parse(insternalCaseDataTable.Rows[0][DBEntityNames.Instance.InternalCaseID.ToString()].ToString());
                else
                {
                    LogError(new Exception("Internal Case Id not found against Instance Id : " + instanceId));
                }
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "GeInternalCaseId - Failed. - " + ToJson(instanceId));
                LogError(ex);
            }
            return 0;
        }

        /// <summary>
        /// Get internal case Id of a case
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public static DataTable GetInstanceDetails(IManager manager, IDataModelManager dataModelManager, string instanceId)
        {
            try
            {
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
                dataModelManager.AddResultSet(new List<string>() { "*" });
                dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var instance = manager.SelectData(dataModelManager.DataModel);
                if (instance.Rows.Count > 0)
                    return instance;
                else
                {
                    LogError(new Exception("Instance not found for Id : " + instanceId));
                }
            }
            catch (Exception ex)
            {
                LogInfo(manager, dataModelManager, "GetInstanceDetails - Failed. - " + ToJson(instanceId));
                LogError(ex);
            }
            return null;
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
                    var obj = new
                    {
                        ColumnName = columnName,
                        InstanceId = instanceId
                    };
                    LogInfo(manager, dataModelManager, "ReplaceValueWithEmployeeData - Failed. - " + ToJson(obj));
                    LogError(ex);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Replace key values with actual user values
        /// </summary>
        /// <param name="value"></param>
        private static string ReplaceValueWithUserData(string columnName, string instanceId, IManager manager, IDataModelManager dataModelManager)
        {
            // get selected employee sam account name
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.UserDetail.ToString());
            dataModelManager.AddResultSet(new List<string>() { DBEntityNames.UserDetail.Id.ToString() });
            dataModelManager.AddFilter(DBEntityNames.UserDetail.SamAccountName.ToString(), Enums.ParameterType._string, GetCurrentUserName(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            var user = manager.SelectData(dataModelManager.DataModel);

            if (user.Rows.Count > 0)
            {
                // sql query
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.UserDetail.ToString());
                dataModelManager.AddResultSet(new List<string>() { columnName });
                dataModelManager.AddFilter(DBEntityNames.UserDetail.SamAccountName.ToString(), Enums.ParameterType._string, GetCurrentUserName(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                try
                {
                    var columnValue = manager.SelectData(dataModelManager.DataModel);
                    return columnValue.Rows[0][columnName].ToString();
                }
                catch (Exception ex)
                {
                    var obj = new
                    {
                        ColumnName = columnName,
                        InstanceId = instanceId
                    };
                    LogInfo(manager, dataModelManager, "ReplaceValueWithUserData - Failed. - " + ToJson(obj));
                    LogError(ex);
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
            var temp = Thread.CurrentPrincipal.Identity.Name.Split('\\');
            if (temp.Length == 2) return temp[1];
            return temp[0];
        }

        /// <summary>
        /// Get dcr graphs url configured in ocm
        /// </summary>
        /// <returns></returns>
        public static string GetDCRGraphsURL()
        {
            return Configurations.Config.DCRPortalURL;
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
        /// Update Instance Responsible
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="responsible"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        public static void UpdateInstanceResponsible(string instanceId, string internalCaseId, string oldResponsible, string newResponsible, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "[" + DBEntityNames.Tables.User.ToString() + "]");
            dataModelManager.AddResultSet(new List<string> { DBEntityNames.User.Acadreorgid.ToString() });
            dataModelManager.AddFilter(DBEntityNames.User.SamAccountName.ToString(), Enums.ParameterType._string, newResponsible, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            var data = manager.SelectData(dataModelManager.DataModel);

            if (data.Rows.Count < 1)
            {
                throw new Exception("User not found");
            }
            else if (data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()] == null || data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()].ToString() == "0")
            {
                throw new Exception("User not assigned with proper acadre organization Id");
            }

            if (!string.IsNullOrWhiteSpace(internalCaseId))
            {
                LogInfo(manager, dataModelManager, "ChangeChildResponsible( " + oldResponsible + " , " + newResponsible + " , " + int.Parse(data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()].ToString()) + " , " + internalCaseId + " )");
                UpdateResponsibleInAcadre(oldResponsible, newResponsible, int.Parse(data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()].ToString()), int.Parse(internalCaseId));
                LogInfo(manager, dataModelManager, "Instance Responsible Changed In Acadre - InternalCaseId: " + internalCaseId + " ,InstanceId : " + instanceId + " ,ResponsibleTo: " + newResponsible + " ,ResponsibleFrom: " + oldResponsible);
            }
        }

        /// <summary>
        /// Update child responsible
        /// </summary>
        /// <param name="childId"></param>
        /// <param name="responsible"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        public static void UpdateChildResponsible(string childId, string oldResponsibleSamAccountName, string newResponsible, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "[" + DBEntityNames.Tables.User.ToString() + "]");
            dataModelManager.AddResultSet(new List<string> { DBEntityNames.User.Acadreorgid.ToString() });
            dataModelManager.AddFilter(DBEntityNames.User.SamAccountName.ToString(), Enums.ParameterType._string, newResponsible, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            var data = manager.SelectData(dataModelManager.DataModel);

            if (data.Rows.Count < 1)
            {
                throw new Exception("User not found");
            }
            else if (data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()] == null || data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()].ToString() == "0")
            {
                throw new Exception("User not assigned with proper acadre organization Id");
            }

            LogInfo(manager, dataModelManager, "ChangeChildResponsible( " + oldResponsibleSamAccountName + " , " + newResponsible + " , " + int.Parse(data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()].ToString()) + " , " + childId + " )");
            //UpdateResponsibleInAcadre(oldResponsibleSamAccountName, newResponsible, int.Parse(data.Rows[0][DBEntityNames.User.Acadreorgid.ToString()].ToString()), int.Parse(childId));
            LogInfo(manager, dataModelManager, "Instance Responsible Changed - childId : " + childId + " ,ResponsibleTo: " + newResponsible + " ,ResponsibleFrom: " + oldResponsibleSamAccountName);
        }

        /// <summary>
        /// Update Activity Responsible
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="eventId"></param>
        /// <param name="responsible"></param>
        public static void UpdateActivityResponsible(string instanceId, string eventId, string responsible, IManager manager, IDataModelManager dataModelManager)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Event.ToString());
            dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            dataModelManager.AddParameter(DBEntityNames.Event.Responsible.ToString(), Enums.ParameterType._string, responsible);

            manager.UpdateData(dataModelManager.DataModel);
            LogInfo(manager, dataModelManager, "Activity Responsible Changed - EventId : " + eventId + " InstanceId : " + instanceId + " ,ResponsibleIdTo: " + responsible);
        }

        /// <summary>
        /// Add a document
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="type"></param>
        /// <param name="link"></param>
        public static string AddDocument(string documentName, string type, string link, string instanceId, string childId, bool isDraft, DateTime eventDateTime, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                if (type == "Temp")
                {
                    dataModelManager.GetDefaultDataModel(Enums.SQLOperation.UPDATE, DBEntityNames.Tables.Document.ToString());
                    dataModelManager.AddParameter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, false.ToString());
                    dataModelManager.AddFilter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, "Temp", Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    dataModelManager.AddFilter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentName, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    dataModelManager.AddFilter(DBEntityNames.Document.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                    dataModelManager.AddFilter(DBEntityNames.Document.IsActive.ToString(), Enums.ParameterType._boolean, true.ToString(), Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                    manager.UpdateData(dataModelManager.DataModel);
                }
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Document.ToString());
                dataModelManager.AddParameter(DBEntityNames.Document.Title.ToString(), Enums.ParameterType._string, documentName);
                dataModelManager.AddParameter(DBEntityNames.Document.Type.ToString(), Enums.ParameterType._string, type);
                dataModelManager.AddParameter(DBEntityNames.Document.Link.ToString(), Enums.ParameterType._string, link);
                dataModelManager.AddParameter(DBEntityNames.Document.Responsible.ToString(), Enums.ParameterType._string, GetCurrentUserName());
                dataModelManager.AddParameter(DBEntityNames.Document.ChildId.ToString(), Enums.ParameterType._int, childId);
                dataModelManager.AddParameter(DBEntityNames.Document.UploadDate.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
                dataModelManager.AddParameter(DBEntityNames.Document.IsLocked.ToString(), Enums.ParameterType._boolean, "false");
                dataModelManager.AddParameter(DBEntityNames.Document.IsDraft.ToString(), Enums.ParameterType._boolean, isDraft.ToString());

                if (!string.IsNullOrEmpty(instanceId))
                {
                    dataModelManager.AddParameter(DBEntityNames.Document.InstanceId.ToString(), Enums.ParameterType._int, instanceId);
                }

                var dataTable = manager.InsertData(dataModelManager.DataModel);
                var documentId = dataTable.Rows[0].ItemArray[0].ToString();
                AddJournalHistory(instanceId, null, documentId, type, documentName, childId, eventDateTime, manager, dataModelManager);
                return documentId;
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    DocumentName = documentName,
                    InstanceId = instanceId,
                    Type = type,
                    Link = link,
                    IsDraft = isDraft,
                    EventDateTime = eventDateTime
                };
                LogInfo(manager, dataModelManager, "AddDocument - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get user name
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="responsible"></param>
        /// <returns></returns>
        public static string GetUserName(IManager manager, IDataModelManager dataModelManager, string responsibleId)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.UserDetail.ToString());
            dataModelManager.AddResultSet(new List<string> { "Id", "SamAccountName", "Name", "Title", "Department", "ManagerId", "IsManager", "Acadreorgid" });
            dataModelManager.AddFilter(DBEntityNames.UserDetail.Id.ToString(), Enums.ParameterType._string, responsibleId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var data = manager.SelectData(dataModelManager.DataModel);
            if (data.Rows.Count > 0)
            {
                return data.Rows[0][DBEntityNames.UserDetail.SamAccountName.ToString()].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Add a journal note to the history table
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="eventId"></param>
        /// <param name="documentId"></param>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="childId"></param>
        /// <param name="eventDate"></param>
        /// <param name="isLocked"></param>
        public static void AddJournalHistory(string instanceId, string eventId, string documentId, string type, string title, string childId, DateTime eventDateTime, IManager manager, IDataModelManager dataModelManager)
        {
            if (!string.IsNullOrEmpty(instanceId))
            {
                string responsibleInitials = string.Empty;
                string responsibleName = string.Empty;
                var responsible = GetResponsibleFullDetails(manager, dataModelManager);
                if (responsible.Rows.Count > 0)
                {
                    responsibleInitials = responsible.Rows[0]["SamAccountName"].ToString();
                    responsibleName = responsible.Rows[0]["Name"].ToString();
                }

                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.JournalHistory.ToString());
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.InstanceId.ToString(), Enums.ParameterType._int, instanceId.ToString());
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.ChildId.ToString(), Enums.ParameterType._int, childId.ToString());
                if (eventId != null) dataModelManager.AddParameter(DBEntityNames.JournalHistory.EventId.ToString(), Enums.ParameterType._int, eventId.ToString());
                if (documentId != null) dataModelManager.AddParameter(DBEntityNames.JournalHistory.DocumentId.ToString(), Enums.ParameterType._int, documentId.ToString());
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.Type.ToString(), Enums.ParameterType._string, type);
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.Title.ToString(), Enums.ParameterType._string, title);
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.EventDate.ToString(), Enums.ParameterType._datetime, eventDateTime.ToString());
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.CreationDate.ToString(), Enums.ParameterType._datetime, DateTime.Now.ToString());
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.IsLocked.ToString(), Enums.ParameterType._boolean, "False");
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.ResponsibleInitials.ToString(), Enums.ParameterType._string, responsibleInitials);
                dataModelManager.AddParameter(DBEntityNames.JournalHistory.ResponsibleName.ToString(), Enums.ParameterType._string, responsibleName);
                manager.InsertData(dataModelManager.DataModel);
            }
        }

        /// <summary>
        /// Save array bytes to specified location
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytesToWrite"></param>
        public static void SaveBytesToFile(string filePath, byte[] bytesToWrite)
        {
            try
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
            catch (Exception ex)
            {
                LogError(ex);
                throw ex;
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
            try
            {
                byte[] data = { };

                using (MemoryStream ms = new MemoryStream())
                {
                    var html = GetFormHtml(formId, manager, dataModelManager);
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
            catch (Exception ex)
            {
                var obj = new
                {
                    FormId = formId
                };
                LogInfo(manager, dataModelManager, "GetFormData - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
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
            try
            {
                var html = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "HtmlTemplates/FormTemplate.html");

                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "FormItem");
                dataModelManager.AddResultSet(new List<string> { "Id", "ItemId", "ItemText", "IsGroup", "SequenceNumber" });
                dataModelManager.AddFilter("FormId", Enums.ParameterType._int, formId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

                var formItems = manager.SelectData(dataModelManager.DataModel);

                var list = formItems.AsEnumerable().ToList();

                var groups = list.Where(x => bool.Parse(x["IsGroup"].ToString())).OrderBy(x => int.Parse(x["SequenceNumber"].ToString())).ToList();

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
            catch (Exception ex)
            {
                var obj = new
                {
                    FormId = formId
                };
                LogInfo(manager, dataModelManager, "GetFormHtml - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
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
            try
            {
                return JsonConvert.SerializeObject(input);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return string.Empty;
            }
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

        /// <summary>
        /// Get refer xml by event id
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="instanceId"></param>
        /// <param name="useProcessEngine"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static string GetReferXmlByEventId(string eventId, string instanceId, bool useProcessEngine, IManager manager, IDataModelManager dataModelManager, IDCRService service)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.Instance.ToString());
            dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Instance.DCRXML.ToString(), DBEntityNames.Instance.GraphId.ToString(), DBEntityNames.Instance.SimulationId.ToString() });
            dataModelManager.AddFilter(DBEntityNames.Instance.Id.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
            var data = manager.SelectData(dataModelManager.DataModel);
            if (data.Rows.Count < 1)
            {
                return string.Empty;
            }
            var xml = data.Rows[0][DBEntityNames.Instance.DCRXML.ToString()].ToString();
            var graphId = data.Rows[0][DBEntityNames.Instance.GraphId.ToString()].ToString();
            var simulationId = data.Rows[0][DBEntityNames.Instance.SimulationId.ToString()].ToString();
            var referXml = string.Empty;
            if (!useProcessEngine)
            {
                referXml = service.GetReferXmlByEventId(graphId, simulationId, eventId);
            }
            else
            {
                referXml = service.GetReferXmlByEventId(eventId, xml);
            }
            return referXml;
        }

        /// <summary>
        /// Convert DCR form to pdf
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="dcrFormId"></param>
        public static void ConvertDCRFormToPdf(string eventId, string instanceId, bool useProcessEngine, string formPdfPath, IManager manager, IDataModelManager dataModelManager, IDCRService service)
        {
            LogInfo(manager, dataModelManager, "DCRForm2PDF - Getting FormXML from main graph xml");
            var formXml = GetReferXmlByEventId(eventId, instanceId, useProcessEngine, manager, dataModelManager, service);
            DCRFormToPdf(formXml, formPdfPath, manager, dataModelManager);
            LogInfo(manager, dataModelManager, "DCRForm2PDF - Form converted to pdf and saved");
        }

        /// <summary>
        /// DCRFormToPdf
        /// </summary>
        /// <param name="formXML"></param>
        /// <returns></returns>
        private static void DCRFormToPdf(string formXML, string formPdfPath, IManager manager, IDataModelManager dataModelManager)
        {
            try
            {
                LogInfo(manager, dataModelManager, "DCRForm2PDF - Sending FormXML to converter api and get Pdf");
                string converterUrl = Configurations.Config.DCRConverterAppUrl;
                string getAuthURL = string.Format("{0}/api/ConvertXmlToLatex", converterUrl);

                LogInfo(manager, dataModelManager, "DCRForm2PDF - Sending FormXML to converter api and get Pdf , converter api URL : " + getAuthURL);

                using (var httpClient = new HttpClient())
                {
                    var obj = new { PostedXML = formXML, DocType = "3", RetrunType = "2" };
                    StringContent sc = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                    using (HttpResponseMessage res = httpClient.PostAsync(getAuthURL, sc).GetAwaiter().GetResult())
                    {
                        try
                        {
                            res.EnsureSuccessStatusCode();
                            if (res.IsSuccessStatusCode)
                            {
                                var entityData = res.Content.ReadAsStringAsync();
                                string link = entityData.Result;
                                string lnk = link.Replace("\"", "");
                                WebClient webClient = new WebClient();
                                {
                                    webClient.DownloadFile(lnk, formPdfPath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogInfo(manager, dataModelManager, "DCRForm2PDF - Error from converter api URL : " + getAuthURL);
                            LogError(ex);
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    FormXML = formXML,
                    FormPdfPath = formPdfPath
                };
                LogInfo(manager, dataModelManager, "DCRFormToPdf - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get RTF Document Bytes
        /// </summary>
        /// <returns></returns>
        public static byte[] GetRTFDocument(string text, IManager manager, IDataModelManager dataModelManager, string instanceId = "", string eventId = "", string childId = "")
        {
            try
            {
                var rtfTemplateFile = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\" + "rtf-template.rtf";
                var textInRTF = File.ReadAllText(rtfTemplateFile);
                textInRTF = textInRTF.Replace("$(comment)", "$(key)$(comment)");

                if (!string.IsNullOrWhiteSpace(instanceId) || !string.IsNullOrWhiteSpace(eventId) || !string.IsNullOrWhiteSpace(childId))
                {
                    textInRTF = ReplaceEventTypeKeyValues(textInRTF, instanceId, manager, dataModelManager, eventId, childId);
                }

                System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox
                {
                    Rtf = textInRTF
                };
                string plainText = rtBox.Text;
                rtBox.Text = "$(key)";

                plainText = plainText.Replace("\n", "\\par");
                textInRTF = UTF8TOSafeAsciiEncoding(plainText);
                textInRTF = textInRTF.Replace("\\\\par", "\n");

                text = text.Replace("\n", "\\par");
                text = UTF8TOSafeAsciiEncoding(text);
                text = text.Replace("\\\\par", "\\par\n");
                var newText = textInRTF.Replace("$(key)", text).Replace("$(comment)", string.Empty).Replace("$(Comment)", string.Empty).Replace("\n", "\\par\n");
                newText = rtBox.Rtf.Replace("$(key)", newText);

                var tempDirectory = AppDomain.CurrentDomain.BaseDirectory + "tmp\\rtf\\";
                var newRtfFileName = tempDirectory + DateTime.Now.ToFileTime() + ".rtf";
                if (!Directory.Exists(tempDirectory))
                {
                    Directory.CreateDirectory(tempDirectory);
                }

                var isoBytes = Encoding.GetEncoding("windows-1252").GetBytes(newText);

                File.WriteAllText(newRtfFileName, newText, Encoding.GetEncoding("windows-1252"));
                return isoBytes;
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    Text = text,
                    InstanceId = instanceId,
                    EventId = eventId,
                    ChildId = childId
                };
                LogInfo(manager, dataModelManager, "GetRTFDocument - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Escape unicode characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string UTF8TOSafeAsciiEncoding(string s)
        {
            if (s == null) return s;

            int len1;
            len1 = s.Length;
            StringBuilder sb = new StringBuilder(len1);
            for (int i = 0; i < len1; i++)
            {
                char c = s.ToCharArray()[i];
                if (c >= 0x20 && c < 0x80)
                {
                    if (c == '\\' || c == '{' || c == '}')
                    {
                        sb.Append('\\');
                    }
                    sb.Append(c);
                }
                else if (c < 0x20 || (c >= 0x80 && c <= 0xFF))
                {
                    sb.Append("\\\'");
                    Int32 ii;
                    ii = Char.ConvertToUtf32(s, i);
                    sb.Append(ii.ToString("X").ToLower());
                }
                else
                {
                    sb.Append("\\u");
                    sb.Append((short)c);
                    sb.Append("??");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts html to RTF
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static byte[] GetRtfFromHtml(string instanceId, string html, IService service, IManager manager, IDataModelManager dataModelManager, string eventId)
        {
            try
            {
                Encoding utf8 = Encoding.UTF8;

                var rtfTemplateFile = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\" + "rtf-template.rtf";
                var textInRTF = File.ReadAllText(rtfTemplateFile);
                System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox
                {
                    Rtf = textInRTF
                };
                string plainText = rtBox.Text;
                plainText = plainText.Replace("$(comment)", html).Replace("\n", "<br/>");

                if (!string.IsNullOrWhiteSpace(instanceId) || !string.IsNullOrWhiteSpace(eventId))
                {
                    plainText = ReplaceEventTypeKeyValues(plainText, instanceId, manager, dataModelManager, eventId);
                }
                else
                {
                    plainText = ReplaceEventTypeKeyValues(plainText, 0.ToString(), manager, dataModelManager, string.Empty);
                }

                var serviceModel = new ServiceModel()
                {
                    BaseUrl = Configurations.Config.NodeWordDocumentServer,
                    Body = JsonConvert.SerializeObject(new { html = plainText }),
                    MethodType = Method.POST,
                    Url = "/RTF"
                };
                var path = service.GetNodeJSServiceResponse(serviceModel).Content;
                var rtfFilePath = JsonConvert.DeserializeObject<dynamic>(path);
                Thread.Sleep(5000);
                var bytes = File.ReadAllBytes(rtfFilePath.success.ToString());
                return bytes;
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    Html = html,
                    InstanceId = instanceId,
                    EventId = eventId
                };
                LogInfo(manager, dataModelManager, "GetRtfFromHtml - Failed. - " + ToJson(obj));
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Log Info Data for Verification
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="message"></param>
        public static void LogInfo(IManager manager, IDataModelManager dataModelManager, string message)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.Log.ToString());
            dataModelManager.AddParameter("Message", Enums.ParameterType._string, message);
            dataModelManager.AddParameter("Logged", Enums.ParameterType._datetime, DateTime.Now.ToString());
            dataModelManager.AddParameter("Level", Enums.ParameterType._string, "Info");
            manager.InsertData(dataModelManager.DataModel);
        }

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="exception"></param>
        public static void LogError(Exception exception)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Error(exception, "Error");
        }

        /// <summary>
        /// Get Instance Roles
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public static DataTable GetInstanceRoles(IManager manager, IDataModelManager dataModelManager, string instanceId)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, DBEntityNames.Tables.InstanceRole.ToString());
            dataModelManager.AddResultSet(new List<string> { "Id", "InstanceId", "Role", "UserId" });
            dataModelManager.AddFilter(DBEntityNames.InstanceRole.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var data = manager.SelectData(dataModelManager.DataModel);
            return data;
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
                var personNameForAddressingName = CheckDictionaryKey(parameters, "personNameForAddressingName".ToLower(), instanceId, "CreateCaseAcadre");
                var personCivilRegistrationNumber = CheckDictionaryKey(parameters, "PersonCivilRegistrationIdentifier".ToLower(), instanceId, "CreateCaseAcadre");
                var caseFileTypeCode = CheckDictionaryKey(parameters, "caseFileTypeCode".ToLower(), instanceId, "CreateCaseAcadre");
                var accessCode = CheckDictionaryKey(parameters, "accessCode".ToLower(), instanceId, "CreateCaseAcadre");
                var caseFileTitleText = CheckDictionaryKey(parameters, "caseFileTitleText".ToLower(), instanceId, "CreateCaseAcadre");
                var journalizingCode = CheckDictionaryKey(parameters, "journalizingCode".ToLower(), instanceId, "CreateCaseAcadre");
                var facet = CheckDictionaryKey(parameters, "facet".ToLower(), instanceId, "CreateCaseAcadre");
                var caseResponsible = CheckDictionaryKey(parameters, "CaseResponsible".ToLower(), instanceId, "CreateCaseAcadre");
                var administrativeUnit = CheckDictionaryKey(parameters, "administrativeUnit".ToLower(), instanceId, "CreateCaseAcadre");
                var caseContent = CheckDictionaryKey(parameters, "caseContent".ToLower(), instanceId, "CreateCaseAcadre");
                var caseFileDisposalCode = CheckDictionaryKey(parameters, "caseFileDisposalCode".ToLower(), instanceId, "CreateCaseAcadre");
                var deletionCode = CheckDictionaryKey(parameters, "deletionCode".ToLower(), instanceId, "CreateCaseAcadre");
                var caseRestrictedFromPublicText = CheckDictionaryKey(parameters, "RestrictedFromPublicText".ToLower(), instanceId, "CreateCaseAcadre");

                var SpecialistId = string.Empty;
                var RecommendationId = string.Empty;
                var CategoryId = string.Empty;
                var SubType = string.Empty;
                if (caseFileTypeCode == "BUSAG")
                {
                    SpecialistId = CheckDictionaryKey(parameters, "SpecialistId".ToLower(), instanceId, "CreateCaseAcadre");
                    RecommendationId = CheckDictionaryKey(parameters, "RecommendationId".ToLower(), instanceId, "CreateCaseAcadre");
                    CategoryId = CheckDictionaryKey(parameters, "CategoryId".ToLower(), instanceId, "CreateCaseAcadre");
                }
                SubType = CheckDictionaryKey(parameters, "SubtypeCode".ToLower(), instanceId, "CreateCaseAcadre");

                LogInfo(manager, dataModelManager, "CreateCaseAcadre - Calling Acadre Service and Saving params to Acadre Log, InstanceId : " + instanceId);

                LogInfo(manager, dataModelManager, "CreateCase(" + personNameForAddressingName + " , " + personCivilRegistrationNumber + " , " + caseFileTypeCode + " , " + accessCode + " , " +
                    caseFileTitleText + " , " + journalizingCode + " , " + facet + " , " + caseResponsible + " , " + administrativeUnit + " , " + caseContent + " , " + caseFileDisposalCode + " , " + deletionCode + " , " + caseRestrictedFromPublicText + " , " + SpecialistId + " , " + RecommendationId + " , " + CategoryId + " , " + SubType + ")");
                // saving parameters to database
                SaveEventTypeDataParamertes(instanceId, parameters, "CreateCaseAcadre", null, manager, dataModelManager);

                // calling acadre services for my children
                var acadreService = GetAcadreService();
                var caseId = acadreService.CreateCase(personNameForAddressingName, personCivilRegistrationNumber, caseFileTypeCode, accessCode, caseFileTitleText, journalizingCode,
                                        facet, caseResponsible, administrativeUnit, caseContent, caseFileDisposalCode, deletionCode, caseRestrictedFromPublicText, SpecialistId, RecommendationId, CategoryId, SubType);
                return caseId;
            }
            catch (Exception ex)
            {
                var obj = new
                {
                    InstanceId = instanceId,
                    Params = parameters
                };

                LogInfo(manager, dataModelManager, "CreateCase - Failed. - " + ToJson(obj));
                LogError(ex);
                SaveEventTypeDataParamertes(instanceId, parameters, "CreateCaseAcadre", ex, manager, dataModelManager);
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
            try
            {
                CaseManagement.ActingFor(GetCurrentUserName());
                CaseManagement.CloseCase(caseId);
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw ex;
            }
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
                string documentCategoryCode = CheckDictionaryKey(parameters, "DocumentCategoryCode".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentTitleText = CheckDictionaryKey(parameters, "DocumentTitleText".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentStatusCode = CheckDictionaryKey(parameters, "DocumentStatusCode".ToLower(), instanceId, "UploadDocumentAcadre");
                DateTime documentDate = CheckDictionaryKey(parameters, "DocumentDate".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentAccessCode = CheckDictionaryKey(parameters, "DocumentAccessCode".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentCaseId = CheckDictionaryKey(parameters, "DocumentCaseId".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentDescriptionText = CheckDictionaryKey(parameters, "DocumentDescriptionText".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentAccessLevel = CheckDictionaryKey(parameters, "DocumentAccessLevel".ToLower(), instanceId, "UploadDocumentAcadre");
                string documentTypeCode = CheckDictionaryKey(parameters, "DocumentTypeCode".ToLower(), instanceId, "UploadDocumentAcadre");
                string recordStatusCode = CheckDictionaryKey(parameters, "RecordStatusCode".ToLower(), instanceId, "UploadDocumentAcadre");
                bool documentEvenOutRequired = bool.Parse(CheckDictionaryKey(parameters, "DocumentEvenOutRequired".ToLower(), instanceId, "UploadDocumentAcadre").ToString().ToLower());
                string documentUserId = CheckDictionaryKey(parameters, "DocumentUserId".ToLower(), instanceId, "UploadDocumentAcadre");
                string recordPublicationIndicator = CheckDictionaryKey(parameters, "PublicationIndicator".ToLower(), instanceId, "UploadDocumentAcadre");
                documentTitleText = fileName;

                LogInfo(manager, dataModelManager, "UploadDocumentAcadre - Calling Acadre Service and Saving params to Acadre Log, InstanceId : " + instanceId);
                LogInfo(manager, dataModelManager, "CreateDocumentService(" + documentCaseId + " , " + recordStatusCode + " , " + documentTypeCode + " , " + documentDescriptionText + " , " + documentAccessCode + " , " + documentStatusCode + " , " + documentTitleText + " , " + documentCategoryCode + " , " + recordPublicationIndicator + " , " + fileName + " , " + "filebytes : []" + ")");
                // saving parameters to database
                SaveEventTypeDataParamertes(instanceId, parameters, "UploadDocumentAcadre", null, manager, dataModelManager);

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
                var obj = new
                {
                    InstanceId = instanceId,
                    Params = parameters,
                    FileName = fileName
                };

                LogInfo(manager, dataModelManager, "CreateDocument - Failed. - " + ToJson(obj));
                LogError(ex);
                SaveEventTypeDataParamertes(instanceId, parameters, "UploadDocumentAcadre", ex, manager, dataModelManager);
                throw ex;
            }
        }

        /// <summary>
        /// Save parameters
        /// </summary>
        /// <param name="parameters"></param>
        public static void SaveEventTypeDataParamertes(string instanceId, Dictionary<string, dynamic> parametersXML, string method, Exception ex, IManager manager, IDataModelManager dataModelManager)
        {
            bool isSuccess = true;
            string errorStatement = string.Empty;
            string trace = string.Empty;
            if (ex != null)
            {
                errorStatement = ex.Message;
                trace = ex.StackTrace;
                isSuccess = false;
            }

            var parametesString = "{";
            foreach (var item in parametersXML)
            {
                parametesString += "\"" + item.Key + "\"" + ":\"" + item.Value + "\",";
            }

            parametesString += "}";

            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.INSERT, DBEntityNames.Tables.AcadreLog.ToString());
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.Method.ToString(), Enums.ParameterType._string, method);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.Parameters.ToString(), Enums.ParameterType._string, parametesString);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.IsSuccess.ToString(), Enums.ParameterType._boolean, isSuccess.ToString());
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.ErrorStatement.ToString(), Enums.ParameterType._string, errorStatement);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.ErrorStackTrace.ToString(), Enums.ParameterType._string, trace);
            dataModelManager.AddParameter(DBEntityNames.AcadreLog.InstanceId.ToString(), Enums.ParameterType._string, instanceId);

            manager.InsertData(dataModelManager.DataModel);
        }

        /// <summary>
        /// Create Memo
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="eventId"></param>
        /// <param name="isTasksWNote"></param>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        public static void CreateMemoAcadre(string instanceId, string eventId, bool isMemoEvent, IManager manager, IDataModelManager dataModelManager, IService service)
        {
            dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Event");
            dataModelManager.AddResultSet(new List<string>() { DBEntityNames.Event.NoteIsHtml.ToString(), DBEntityNames.Event.EventType.ToString(), DBEntityNames.Event.Note.ToString(), DBEntityNames.Event.EventTypeData.ToString() });
            dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, eventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
            dataModelManager.AddFilter(DBEntityNames.Event.InstanceId.ToString(), Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);

            var eventDetails = manager.SelectData(dataModelManager.DataModel);
            if (eventDetails.Rows.Count > 0)
            {
                var parameters = GetParametersFromEventTypeData(eventDetails.Rows[0]["EventTypeData"].ToString(), instanceId, manager, dataModelManager, eventId);

                var donotLog = false;
                if (parameters.ContainsKey("DoNotJournalEvent".ToLower()))
                {
                    if (parameters["DoNotJournalEvent".ToLower()] == true.ToString().ToLower())
                    {
                        donotLog = true;
                    }
                }

                if (donotLog)
                {
                    LogInfo(manager, dataModelManager, "CreateJournalAcadre - DonotJournal is true, Event not Logged.eventId : " + eventId);
                    return;
                }

                var eventType = eventDetails.Rows[0]["EventType"].ToString();
                var note = string.Empty;
                var isHtml = false;
                if (eventType == "TasksWNote" || eventType == "TasksWNoteFull")
                {
                    var noteIsHtml = eventDetails.Rows[0]["NoteIsHtml"].ToString();
                    isHtml = bool.Parse(noteIsHtml == "" ? "false" : noteIsHtml);
                    if (isHtml)
                        note = eventDetails.Rows[0]["Note"].ToString();
                }

                var memoEventId = isMemoEvent == false ? "Generic" : eventId;
                dataModelManager.GetDefaultDataModel(Enums.SQLOperation.SELECT, "Event");
                dataModelManager.AddResultSet(new List<string>() { "EventTypeData" });
                dataModelManager.AddFilter(DBEntityNames.Event.EventId.ToString(), Enums.ParameterType._string, memoEventId, Enums.CompareOperator.equal, Enums.LogicalOperator.and);
                dataModelManager.AddFilter("InstanceId", Enums.ParameterType._int, instanceId, Enums.CompareOperator.equal, Enums.LogicalOperator.none);
                var memoEvent = manager.SelectData(dataModelManager.DataModel);

                if (memoEvent.Rows.Count > 0 || isMemoEvent)
                {
                    LogInfo(manager, dataModelManager, "CreateJournalAcadre - Calling Acadre to  Log  eventId");
                    parameters = GetParametersFromEventTypeData(memoEvent.Rows[0]["EventTypeData"].ToString(), instanceId, manager, dataModelManager, eventId);
                    // get parameters
                    var accessCode = CheckDictionaryKey(parameters, "accessCode".ToLower(), instanceId, "CreateMemoAcadre");
                    var caseFileReference = CheckDictionaryKey(parameters, "CaseFileReference".ToLower(), instanceId, "CreateMemoAcadre");
                    var creationDate = CheckDictionaryKey(parameters, "CreationDate".ToLower(), instanceId, "CreateMemoAcadre");
                    var creator = CheckDictionaryKey(parameters, "Creator".ToLower(), instanceId, "CreateMemoAcadre");
                    string fileName = CheckDictionaryKey(parameters, "FileName".ToLower(), instanceId, "CreateMemoAcadre") == string.Empty ? eventId + ".rtf" : CheckDictionaryKey(parameters, "FileName".ToLower(), instanceId, "CreateMemoAcadre");
                    if (fileName.Equals("0") || string.IsNullOrWhiteSpace(fileName))
                    {
                        fileName = DateTime.Now.ToFileTime() + ".rtf";
                    }
                    else if (!fileName.EndsWith(".rtf"))
                    {
                        fileName += ".rtf";
                    }

                    var memoTitleText = CheckDictionaryKey(parameters, "memoTitleText".ToLower(), instanceId, "CreateMemoAcadre");
                    var memoTypeReference = CheckDictionaryKey(parameters, "memoTypeReference".ToLower(), instanceId, "CreateMemoAcadre");
                    DateTime memoEventDate = CheckDictionaryKey(parameters, "MemoEventDate".ToLower(), instanceId, "CreateMemoAcadre");
                    var memoIsLocked = bool.Parse(CheckDictionaryKey(parameters, "MemoIsLocked".ToLower(), instanceId, "CreateMemoAcadre"));
                    byte[] fileBytes;
                    if (isHtml)
                    {
                        fileBytes = GetRtfFromHtml(instanceId, note, service, manager, dataModelManager, eventId);
                    }
                    else
                    {
                        fileBytes = GetRTFDocument(note, manager, dataModelManager, instanceId, eventId);
                    }

                    if (caseFileReference != "$(InternalCaseid)")
                    {
                        try
                        {
                            LogInfo(manager, dataModelManager, "CreateMemo(" + fileName + " , " + accessCode + " , " + caseFileReference + " , " + memoTitleText + " , " + creator + " , " + memoTypeReference + " , " + memoIsLocked + " , fileBytes:[] " + " , " + memoEventDate.ToString() + ")");
                            SaveEventTypeDataParamertes(instanceId, parameters, "CreateMemoAcadre", null, manager, dataModelManager);
                            CaseManagement.ActingFor(GetCurrentUserName());
                            CaseManagement.CreateMemo(fileName, accessCode, caseFileReference, memoTitleText, creator, memoTypeReference, memoIsLocked, fileBytes, memoEventDate);
                            LogInfo(manager, dataModelManager, "CreateJournalAcadre - Success for eventId : " + eventId + ", instanceId : " + instanceId);
                        }
                        catch (Exception ex)
                        {
                            LogInfo(manager, dataModelManager, "CreateJournalAcadre - Failed for eventId : " + eventId + ", instanceId : " + instanceId);
                            LogError(ex);
                            parameters.Add("xmlBinary", Encoding.ASCII.GetString(fileBytes));
                            SaveEventTypeDataParamertes(instanceId, parameters, "CreateMemoAcadre", ex, manager, dataModelManager);
                        }
                    }
                    else
                    {
                        LogInfo(manager, dataModelManager, "CreateJournalAcadre - Case not created so event can't be logged for eventId : " + eventId + ", instanceId : " + instanceId);
                    }
                }
            }
        }

        /// <summary>
        /// Search my children from Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static List<AcadreLib.ChildCase> SeacrhChildren(IManager manager, IDataModelManager dataModelManager, AcadreLib.SearchCriterion searchCriteria)
        {
            LogInfo(manager, dataModelManager, "Calling Acadre SearchChildren Service");
            //Replace logged in user key with value
            if (searchCriteria.CaseManagerInitials != null)
            {
                searchCriteria.CaseManagerInitials = ReplaceKeyWithResponsible(searchCriteria.CaseManagerInitials);
            }

            LogInfo(manager, dataModelManager, "SearchChildren(AcadreOrgID : " + searchCriteria.AcadreOrgID + " , CaseManagerInitials : " + searchCriteria.CaseManagerInitials + " , ChildCPR : " + searchCriteria.ChildCPR + " , CaseContent : " + searchCriteria.CaseContent + " , PrimaryContactsName : " + searchCriteria.PrimaryContactsName + " , KLE : " + searchCriteria.KLE + " , IsClosed : " + searchCriteria.IsClosed + ")");

            // calling acadre services for my children
            var acadreService = GetAcadreService();
            var obj = acadreService.SearchChildren(searchCriteria).ToList();

            LogInfo(manager, dataModelManager, "Called Succeeded - Acadre MyChildren Service");
            if (obj != null)
                LogInfo(manager, dataModelManager, obj.Count.ToString());
            else
                LogInfo(manager, dataModelManager, "No children Acadre MyChildren Service");

            return obj;
        }

        /// <summary>
        /// Get Child info from Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static AcadreLib.Child GetChildInfo(IManager manager, IDataModelManager dataModelManager, int CaseID)
        {
            LogInfo(manager, dataModelManager, "Calling Acadre GetChildInfo Service");
            LogInfo(manager, dataModelManager, "GetChildInfo( " + CaseID + " )");

            // calling acadre services for my children
            var acadreService = GetAcadreService();
            var childInfo = acadreService.GetChildInfo(CaseID);

            LogInfo(manager, dataModelManager, "Called Succeeded - Calling Acadre GetChildInfo Service Against Journal Case ID ");
            if (childInfo.CustodyOwnersNames != null && childInfo.CustodyOwnersNames.ToList().Count > 0)
                LogInfo(manager, dataModelManager, "CustodyOwnersNames Count - " + childInfo.CustodyOwnersNames.ToList().Count);
            return childInfo;
        }

        /// <summary>
        /// Get Child Journal Documents from Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static IEnumerable<AcadreLib.JournalDocument> GetChildJournalDocuments(IManager manager, IDataModelManager dataModelManager, JournalDocument journalDocument)
        {
            if (journalDocument != null)
            {
                if (journalDocument.JournalCaseId == 0)
                {
                    var ex = new Exception("Input parameters failed. Journcal Case Id = 0 - GetChildJournalDocuments");
                    LogError(ex);
                    throw ex;
                }

                if (journalDocument.JournalCaseId != 0)
                {
                    LogInfo(manager, dataModelManager, "Calling Acadre GetChildJournalDocuments Service");
                    LogInfo(manager, dataModelManager, "GetChildJournalDocuments( " + journalDocument.JournalCaseId + " )");

                    var acadreService = GetAcadreService();
                    var obj = acadreService.GetChildJournalDocuments(journalDocument.JournalCaseId);

                    if (obj == null)
                        LogInfo(manager, dataModelManager, "Called Succeeded - Nothing returned from server GetChildJournalDocuments Service ");

                    LogInfo(manager, dataModelManager, "Called Succeeded - Calling Acadre GetChildJournalDocuments Service ");

                    return obj;
                }
                return null;
            }
            else
            {
                var ex = new Exception("Input parameters failed. JournalDocument is null - GetChildJournalDocuments");
                LogError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get Child info Against CPR from Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <returns></returns>
        public static AcadreLib.Child GetChildInfo(IManager manager, IDataModelManager dataModelManager, string CPR)
        {
            LogInfo(manager, dataModelManager, "Calling Acadre GetChildInfo Service Against CPR ");
            LogInfo(manager, dataModelManager, "GetChildInfo( " + CPR + " )");
            var acadreService = GetAcadreService();
            var childInfo = acadreService.GetChildInfo(CPR);
            LogInfo(manager, dataModelManager, "Called Succeeded - Acadre GetChildInfo Service Against CPR ");
            return childInfo;
        }

        /// <summary>
        /// Get Child Cases From Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="CaseID"></param>
        /// <returns></returns>
        public static IEnumerable<AcadreLib.ChildCase> GetChildCases(IManager manager, IDataModelManager dataModelManager, int CaseID)
        {
            LogInfo(manager, dataModelManager, "Journal Case ID: " + CaseID);
            LogInfo(manager, dataModelManager, "Calling Acadre GetChildCases Service Against Journal Case ID ");

            IEnumerable<AcadreLib.ChildCase> childCases = null;
            // calling acadre services for my children
            var acadreService = GetAcadreService();
            try
            {
                LogInfo(manager, dataModelManager, "GetChildCases( " + CaseID + " )");
                childCases = acadreService.GetChildCases(CaseID).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            try
            {
                LogInfo(manager, dataModelManager, childCases.Count().ToString());
                foreach (var childCase in childCases)
                {
                    LogInfo(manager, dataModelManager, "Child - caseId : " + childCase.CaseID + ", caseforeignnumber : " + childCase.CaseNumberIdentifier);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return childCases;
        }

        /// <summary>
        /// Create Child Journal From Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="CPR"></param>
        /// <param name="AcadreOrgID"></param>
        /// <param name="CaseManagerInitials"></param>       
        /// <returns></returns>
        public static int CreateChildJournal(IManager manager, IDataModelManager dataModelManager, string CPR, int AcadreOrgID, string CaseManagerInitials)
        {
            //Replace logged in user key with value
            if (CaseManagerInitials != null)
            {
                CaseManagerInitials = ReplaceKeyWithResponsible(CaseManagerInitials);
            }
            LogInfo(manager, dataModelManager, "Calling Acadre CreateChildJournal Service");

            var childJournal = 0;
            try
            {
                LogInfo(manager, dataModelManager, "CreateChildJournal( " + CPR + " , " + AcadreOrgID + " , " + CaseManagerInitials + " )");

                // calling acadre services for my children
                var acadreService = GetAcadreService();
                childJournal = acadreService.CreateChildJournal(CPR, AcadreOrgID, CaseManagerInitials);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            try
            {
                LogInfo(manager, dataModelManager, "child id : " + childJournal.ToString());
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return childJournal;
        }

        /// <summary>
        /// Get Child Case Documents from Acadre
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="caseID"></param>       
        /// <returns></returns>
        public static IEnumerable<AcadreLib.JournalDocument> GetChildCaseDocuments(IManager manager, IDataModelManager dataModelManager, int CaseID)
        {
            LogInfo(manager, dataModelManager, "Calling Acadre GetChildCaseDocuments Service");

            LogInfo(manager, dataModelManager, "GetChildCaseDocuments( " + CaseID + " )");
            var acadreService = GetAcadreService();
            var obj = acadreService.GetChildCaseDocuments(CaseID).ToList();

            LogInfo(manager, dataModelManager, "Called Succeeded - Calling Acadre GetChildCaseDocuments Service ");
            if (obj != null)
            {
                LogInfo(manager, dataModelManager, "GetChildCaseDocuments Count : " + obj.Count.ToString());
                foreach (var document in obj)
                    LogInfo(manager, dataModelManager, "GetChildCaseDocuments - CaseId : " + document.CaseID + ",CaseIdentifier : " + document.CaseNumberIdentifier + ",documentId:" + document.DocumentID);
            }
            else
                LogInfo(manager, dataModelManager, "GetChildCaseDocuments no record");

            return obj;
        }

        /// <summary>
        /// Update Child
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="dataModelManager"></param>
        /// <param name="caseID"></param>       
        /// <returns></returns>
        public static void UpdateChild(string obsText, string childId, IManager manager, IDataModelManager dataModelManager)
        {
            LogInfo(manager, dataModelManager, "Calling Acadre SetBUComment Service");
            LogInfo(manager, dataModelManager, "SetBUComment( " + childId + " , " + obsText + " )");

            var acadreService = GetAcadreService();
            acadreService.SetBUComment(int.Parse(childId), obsText);

            LogInfo(manager, dataModelManager, "Called Succeeded - Calling Acadre SetBUComment Service ");
        }

        /// <summary>
        /// Update Responsible in Acadre
        /// </summary>
        /// <param name="oldResponsibleName"></param>
        /// <param name="newResponsibleName"></param>
        /// <param name="newAcadreOrgId"></param>
        /// <param name="caseId"></param>
        public static void UpdateResponsibleInAcadre(string oldResponsibleName, string newResponsibleName, int newAcadreOrgId, int caseId)
        {
            var acadreService = GetAcadreService();
            acadreService.ChangeChildResponsible(oldResponsibleName, newResponsibleName, newAcadreOrgId, caseId);
        }

        #region Private Methods  
        /// <summary>
        /// Check if keys exists
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <param name="key"></param>
        /// <param name="id"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private static dynamic CheckDictionaryKey(Dictionary<string, dynamic> keyValuePairs, string key, string id, string functionName)
        {
            if (keyValuePairs.Any(x => x.Key.ToLower() == key))
            {
                return keyValuePairs[key.ToLower()];
            }
            else if (key == "SubtypeCode".ToLower())
            {
                return string.Empty;
            }
            var ex = new Exception("Expected EventTypeParameter '" + key + "' does not exists for instanceId " + id + " in EventType '" + functionName + "'");
            LogError(ex);
            throw ex;
        }

        /// <summary>
        /// Get Acadre Service With Configurations
        /// </summary>
        /// <returns></returns>
        public static AcadreLib.AcadreService GetAcadreService()
        {
            AcadreLib.AcadreService acadreService = new AcadreLib.AcadreService(Configurations.Config.AcadreBaseurlPWI, Configurations.Config.AcadreService, Configurations.Config.AcadreServiceUserName, Configurations.Config.AcadreServiceUserPassword, Configurations.Config.AcadreServiceUserDomain, GetCurrentUserName(), Configurations.Config.CPRBrokerEndpointURL, Configurations.Config.CPRBrokerUserToken, Configurations.Config.CPRBrokerApplicationToken);
            return acadreService;
        }
        #endregion

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